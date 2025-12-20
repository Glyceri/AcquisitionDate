using AcquisitionDate.Core.Handlers;
using AcquisitionDate.LodestoneNetworking.Interfaces;
using AcquisitionDate.LodestoneNetworking.Queue.Enums;
using AcquisitionDate.LodestoneNetworking.Queue.Interfaces;
using AcquisitionDate.LodestoneNetworking.Structs;
using HtmlAgilityPack;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace AcquisitionDate.LodestoneNetworking.Queue;

internal class LodestoneQueueElement : ILodestoneQueueElement
{
    public float TimeInQueue { get; private set; }
    public QueueState QueueState { get; private set; }
    public bool DISPOSED { get; private set; } = false;

    readonly Action<HtmlDocument> OnSuccess;
    readonly Action<Exception> OnFailure;
    readonly Action<int, Action> OnTick;

    readonly string URL;

    readonly INetworkClient NetworkClient;

    NetworkClientRequest? request = null;
    readonly CancellationTokenSource CancellationTokenSource = new CancellationTokenSource();

    readonly int TickCount = 1;
    int atTick = 0;

    public LodestoneQueueElement(INetworkClient networkClient, Action<HtmlDocument> onSuccess, Action<Exception> onFailure, Action<int, Action> onTick, string actionURL)
    {
        URL = actionURL;

        NetworkClient = networkClient;
        OnSuccess = onSuccess;
        OnFailure = onFailure;
        OnTick = onTick;

        QueueState = QueueState.Idle;
    }

    public LodestoneQueueElement(INetworkClient networkClient, Action<HtmlDocument> onSuccess, Action<Exception> onFailure, Action<int, Action> onTick, string actionURL, int tickCount) 
        : this (networkClient, onSuccess, onFailure, onTick, actionURL)
    {
        TickCount = tickCount;
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
        TimeInQueue = 0;
    }

    public bool IsDone()
    {
        if (atTick >= TickCount) return true;

        MarkAsInQueue();

        return false;
    }

    public void SendRequest()
    {
        atTick++;

        if (atTick > 1)
        {
            SidewalkRequest();
            return;
        }

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

        request = NetworkClient.SendRequest
        (
            URL,
            (response) =>
            {
                Task.Run
                (
                    async () => await OnResponse(response),
                    CancellationTokenSource.Token
                ).ConfigureAwait(false);
            },
            HandleFailure
        );
    }

    void SidewalkRequest()
    {
        QueueState = QueueState.BeingProcessed;

        OnTick?.Invoke(atTick, OnTickComplete);
    }

    void OnTickComplete()
    {
        QueueState = QueueState.Success;
    }

    public CancellationToken GetToken() => CancellationTokenSource.Token;

    async Task OnResponse(HttpResponseMessage response)
    {
        HtmlDocument document = new HtmlDocument();

        try
        {
            document.LoadHtml(await response.Content.ReadAsStringAsync(CancellationTokenSource.Token).ConfigureAwait(false));
        }
        catch (Exception ex)
        {
            HandleFailure(ex);
            return;
        }

        QueueState = QueueState.Success;
        await PluginHandlers.Framework.Run(() => OnSuccess.Invoke(document), CancellationTokenSource.Token).ConfigureAwait(false);
    }

    void HandleFailure(Exception ex)
    {
        QueueState = QueueState.Failure;
        PluginHandlers.Framework.Run(() => OnFailure?.Invoke(ex), CancellationTokenSource.Token).ConfigureAwait(false);
    }

    public void Cancel()
    {
        QueueState = QueueState.Cancelled;
        try
        {
            CancellationTokenSource?.Cancel();
        }catch(Exception e)
        {
            PluginHandlers.PluginLog.Error(e, "Failed to cancel cancellationTokenSource.");
        }
    }

    public LodestoneQueueElementData GetQueueData()
    {
        return new LodestoneQueueElementData(TimeInQueue, QueueState, DISPOSED, URL, TickCount, atTick);
    }

    public void Dispose()
    {
        Cancel();

        try
        {
            request?.Dispose();
            CancellationTokenSource?.Dispose();
        }
        catch (Exception e)
        {
            PluginHandlers.PluginLog.Error(e, "Failed to Dispose request.");
        }

        DISPOSED = true;
    }
}
