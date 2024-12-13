using AcquisitionDate.LodestoneNetworking.Queue.Enums;
using AcquisitionDate.LodestoneNetworking.Queue.Interfaces;
using HtmlAgilityPack;
using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace AcquisitionDate.LodestoneNetworking.Queue;

internal class LodestoneQueueElement : ILodestoneQueueElement
{
    public float TimeInQueue { get; private set; }
    public QueueState QueueState { get; private set; }

    readonly CancellationTokenSource CancellationTokenSource;
    readonly Action<HtmlDocument> OnSuccess;
    readonly Action<Exception> OnFailure;
    readonly HttpRequestMessage MessageRequest;

    readonly HttpClient _HttpClient;

    public LodestoneQueueElement(HttpClient httpClient, Action<HtmlDocument> onSuccess, Action<Exception> onFailure, string actionURL)
    {
        _HttpClient = httpClient;
        OnSuccess = onSuccess;
        OnFailure = onFailure;
        MessageRequest = new HttpRequestMessage(HttpMethod.Get, actionURL);
        CancellationTokenSource = new CancellationTokenSource();

        QueueState = QueueState.Idle;
    }

    public void Tick(float deltaTime)
    {
        TimeInQueue += deltaTime;

        if (TimeInQueue >= 30 && QueueState == QueueState.InQueue)
        {
            QueueState = QueueState.Stale;
            OnFailure.Invoke(new Exception("Request has been in queue for over 30 seconds"));
            return;
        }
    }

    public void MarkAsInQueue()
    {
        QueueState = QueueState.InQueue;
    }

    public void SendRequest()
    {
        if (QueueState != QueueState.InQueue)
        {
            OnFailure.Invoke(new Exception("Queue Request send whilst element is being handled or not in queue"));
            return;
        }

        if (TimeInQueue >= 30 && QueueState == QueueState.InQueue)
        {
            QueueState = QueueState.Stale;
            OnFailure.Invoke(new Exception("Request has been in queue for over 30 seconds"));
            return;
        }

        QueueState = QueueState.BeingProcessed;

        Task.Run(async () => await SendLodestoneRequest(), CancellationTokenSource.Token);
    }

    async Task SendLodestoneRequest()
    {
        HttpResponseMessage? response = null;

        try
        {
            response = await _HttpClient.SendAsync(MessageRequest);
            response.EnsureSuccessStatusCode();
        }
        catch (Exception ex)
        {
            HandleFailure(ex);
            return;
        }

        if (response == null)
        {
            HandleFailure(new Exception("Response is NULL."));
            return;
        }

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            HandleFailure(new Exception("Response not found."));
            return;
        }

        HtmlDocument document = new HtmlDocument();

        try
        {
            document.LoadHtml(await response.Content.ReadAsStringAsync());
        }
        catch(Exception ex)
        {
            HandleFailure(ex);
            return;
        }

        QueueState = QueueState.Success;
        OnSuccess.Invoke(document);
    }

    void HandleFailure(Exception ex)
    {
        QueueState = QueueState.Failure;
        CancellationTokenSource.Cancel();
        OnFailure?.Invoke(ex);
    }

    public void Dispose()
    {
        CancellationTokenSource.Cancel();
        CancellationTokenSource.Dispose();

        MessageRequest.Dispose();
    }
}
