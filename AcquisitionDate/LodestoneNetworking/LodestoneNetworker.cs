using AcquisitionDate.LodestoneNetworking.Enums;
using AcquisitionDate.LodestoneNetworking.Interfaces;
using AcquisitionDate.LodestoneNetworking.Queue;
using AcquisitionDate.LodestoneNetworking.Queue.Enums;
using AcquisitionDate.LodestoneNetworking.Queue.Interfaces;
using AcquisitionDate.LodestoneRequests.Interfaces;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Net.Http;

namespace AcquisitionDate.LodestoneNetworking;

internal class LodestoneNetworker : ILodestoneNetworker
{
    // 1.5 seconds between release.
    // SE times out your request if you send more than 1 a second
    const float TIME_BETWEEN_RELEASES = 1.5f;

    readonly HttpClient HttpClient = new HttpClient();

    public LodestoneRegion PreferredRegion { get; set; } = LodestoneRegion.Europe;

    List<ILodestoneQueueElement> _queueElements = new List<ILodestoneQueueElement>();

    float lodestoneQueueReleaseTimer = 0;

    public void AddElementToQueue(ILodestoneRequest request)
    {
        ILodestoneQueueElement queueElement = new LodestoneQueueElement(HttpClient, request.HandleSuccess, request.HandleFailure, GetBaseString() + request.GetURL());
        queueElement.MarkAsInQueue();
        _queueElements.Add(queueElement);
    }

    /// <summary>
    /// Adds an element to the queue that will handle itself.
    /// </summary>
    /// <param name="onSuccess">Gives the HTML page back upon success.</param>
    /// <param name="onFailure">Gives the exception back upon failure.</param>
    /// <param name="URL">The lodestone URL WITHOUT the ##.finalfantasyxiv.com part (Note this does NOT end with a /)</param>
    public void AddElementToQueue(Action<HtmlDocument> onSuccess, Action<Exception> onFailure, string URL)
    {
        ILodestoneQueueElement queueElement = new LodestoneQueueElement(HttpClient, onSuccess, onFailure, GetBaseString() + URL);
        queueElement.MarkAsInQueue();
        _queueElements.Add(queueElement);
    }

    string GetBaseString()
    {
        string prefix = string.Empty;

        prefix = PreferredRegion switch
        {
            LodestoneRegion.Europe => "eu",
            LodestoneRegion.America => "na",
            LodestoneRegion.Japan => "jp",
            LodestoneRegion.Germany => "de",
            LodestoneRegion.France => "fr",
            _ => "eu"
        };

        return $"https://{prefix}.finalfantasyxiv.com";
    }

    public void Update(float deltaTime)
    {
        int queueCount = _queueElements.Count;

        lodestoneQueueReleaseTimer += deltaTime;

        for (int i = 0; i < queueCount; i++)
        {
            ILodestoneQueueElement queueElement = _queueElements[i];
            queueElement.Tick(deltaTime);
        }

        if (lodestoneQueueReleaseTimer < TIME_BETWEEN_RELEASES) return;
        if (lodestoneQueueReleaseTimer >= TIME_BETWEEN_RELEASES) lodestoneQueueReleaseTimer = TIME_BETWEEN_RELEASES;

        for (int i = 0; i < _queueElements.Count; i++)
        {
            ILodestoneQueueElement element = _queueElements[i];
            if (element.QueueState != QueueState.InQueue) continue;

            element.SendRequest();
            lodestoneQueueReleaseTimer = 0;
            break;
        }

        for (int i = _queueElements.Count - 1; i >= 0; i--)
        {
            ILodestoneQueueElement queueElement = _queueElements[i];
            if (queueElement.QueueState != QueueState.Failure && 
                queueElement.QueueState != QueueState.Success && 
                queueElement.QueueState != QueueState.InQueue) continue;

            queueElement.Dispose();
            _queueElements.RemoveAt(i);
        }
    }

    public void Dispose()
    {
        int queueCount = _queueElements.Count;

        for (int i = 0; i < queueCount; i++)
        {
            ILodestoneQueueElement queueElement = _queueElements[i];
            queueElement.Dispose(); // This also calls the cancellation token c:
        }

        _queueElements.Clear();

        HttpClient.CancelPendingRequests();
        HttpClient.Dispose();
    }
}
