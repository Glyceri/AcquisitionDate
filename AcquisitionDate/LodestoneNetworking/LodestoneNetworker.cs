using AcquisitionDate.Core.Handlers;
using AcquisitionDate.DirtySystem.Interfaces;
using AcquisitionDate.LodestoneNetworking.Enums;
using AcquisitionDate.LodestoneNetworking.Interfaces;
using AcquisitionDate.LodestoneNetworking.Queue;
using AcquisitionDate.LodestoneNetworking.Queue.Enums;
using AcquisitionDate.LodestoneNetworking.Queue.Interfaces;
using AcquisitionDate.LodestoneRequests.Interfaces;
using Dalamud.Game;
using HtmlAgilityPack;
using Lumina.Excel.Sheets;
using System;
using System.Collections.Generic;
using System.Net.Http;

namespace AcquisitionDate.LodestoneNetworking;

internal class LodestoneNetworker : ILodestoneNetworker
{
    readonly HttpClient HttpClient = new HttpClient();

    public LodestoneRegion PreferredRegion { get; private set; } = LodestoneRegion.Germany;

    List<ILodestoneQueueElement> _queueElements = new List<ILodestoneQueueElement>();

    float lodestoneQueueReleaseTimer = 0;

    readonly IDirtyListener DirtyListener;
    readonly Configuration Configuration;

    string _sessionToken = string.Empty;

    public LodestoneNetworker(IDirtyListener dirtyListener, Configuration configuration)
    {
        DirtyListener = dirtyListener;
        Configuration = configuration;

        PreferredRegion = PluginHandlers.ClientState.ClientLanguage switch
        {
            ClientLanguage.Japanese => LodestoneRegion.Japan,
            ClientLanguage.German => LodestoneRegion.Germany,
            ClientLanguage.French => LodestoneRegion.France,
            ClientLanguage.English => GetLodestoneRegion(),
            _ => LodestoneRegion.Germany,
        };

        DirtyListener.RegisterDirtyUser(OnDirty);
    }

    LodestoneRegion GetLodestoneRegion()
    {
        World? currentWorld = PluginHandlers.ClientState.LocalPlayer?.CurrentWorld.Value;
        if (currentWorld == null) return LodestoneRegion.America;
        if (currentWorld.Value.DataCenter.Value.PvPRegion == 3) return LodestoneRegion.Europe;

        return LodestoneRegion.America;
    }

    void OnDirty()
    {
        SetSessionToken(string.Empty);

        ClearQueue();
        CancelRequests();
    }

    public void SetSessionToken(string sessionToken)
    {
        _sessionToken = sessionToken;
        HttpClient.DefaultRequestHeaders.Remove("Cookie");
        HttpClient.DefaultRequestHeaders.Add("Cookie", $"ldst_sess={sessionToken}");
    }

    public string GetSessionToken()
    {
        return _sessionToken;
    }

    public ILodestoneQueueElement AddElementToQueue(ILodestoneRequest request)
    {
        ILodestoneQueueElement queueElement = new LodestoneQueueElement(HttpClient, request.HandleSuccess, request.HandleFailure, GetBaseString() + request.GetURL());
        queueElement.MarkAsInQueue();

        _queueElements.Add(queueElement);

        return queueElement;
    }

    /// <summary>
    /// Adds an element to the queue that will handle itself.
    /// </summary>
    /// <param name="onSuccess">Gives the HTML page back upon success.</param>
    /// <param name="onFailure">Gives the exception back upon failure.</param>
    /// <param name="URL">The lodestone URL WITHOUT the ##.finalfantasyxiv.com part (Note this does NOT end with a /)</param>
    public ILodestoneQueueElement AddElementToQueue(Action<HtmlDocument> onSuccess, Action<Exception> onFailure, string URL)
    {
        ILodestoneQueueElement queueElement = new LodestoneQueueElement(HttpClient, onSuccess, onFailure, GetBaseString() + URL);
        queueElement.MarkAsInQueue();

        _queueElements.Add(queueElement);

        return queueElement;
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

        if (lodestoneQueueReleaseTimer < Configuration.FetchDelayInSeconds) return;
        lodestoneQueueReleaseTimer = 0;

        for (int i = 0; i < _queueElements.Count; i++)
        {
            ILodestoneQueueElement element = _queueElements[i];
            if (element.QueueState != QueueState.InQueue) continue;

            element.SendRequest();
            break;
        }

        for (int i = _queueElements.Count - 1; i >= 0; i--)
        {
            ILodestoneQueueElement queueElement = _queueElements[i];
            if (queueElement.QueueState != QueueState.Failure &&
                queueElement.QueueState != QueueState.Success &&
                queueElement.QueueState != QueueState.Cancelled) continue;

            queueElement.Dispose();
            _queueElements.RemoveAt(i);
        }
    }

    void ClearQueue()
    {
        int queueCount = _queueElements.Count;

        for (int i = 0; i < queueCount; i++)
        {
            ILodestoneQueueElement queueElement = _queueElements[i];
            try
            {
                queueElement.Dispose(); // This also calls the cancellation token c:
            }
            catch (Exception ex)
            {
                PluginHandlers.PluginLog.Error("Error in disposal of queue element", ex);
            }
        }

        _queueElements.Clear();
    }

    void CancelRequests()
    {
        try
        {
            HttpClient.CancelPendingRequests();
        }
        catch (Exception e)
        {
            PluginHandlers.PluginLog.Error(e, "Couldn't cancel pending requests.");
        }
    }

    public void Dispose()
    {
        DirtyListener.UnregisterDirtyUser(OnDirty);

        ClearQueue();
        CancelRequests();

        try
        {
            HttpClient.Dispose();
        }
        catch (Exception e)
        {
            PluginHandlers.PluginLog.Error(e, "Couldn't dispose HTTP client :c.");
        }
    }
}
