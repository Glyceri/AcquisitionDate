using AcquisitionDate.Core.Handlers;
using AcquisitionDate.DirtySystem.Interfaces;
using AcquisitionDate.LodestoneNetworking.Enums;
using AcquisitionDate.LodestoneNetworking.Interfaces;
using AcquisitionDate.LodestoneNetworking.Queue;
using AcquisitionDate.LodestoneNetworking.Queue.Enums;
using AcquisitionDate.LodestoneNetworking.Queue.Interfaces;
using AcquisitionDate.LodestoneNetworking.Structs;
using AcquisitionDate.LodestoneRequests.Interfaces;
using Dalamud.Game;
using Lumina.Excel.Sheets;
using System;
using System.Collections.Generic;

namespace AcquisitionDate.LodestoneNetworking;

internal class LodestoneNetworker : ILodestoneNetworker
{
    readonly INetworkClient NetworkClient;

    public LodestoneRegion PreferredRegion { get; private set; } = LodestoneRegion.Germany;

    List<ILodestoneQueueElement> _queueElements = new List<ILodestoneQueueElement>();

    float lodestoneQueueReleaseTimer = 0;

    readonly IDirtyListener DirtyListener;
    readonly Configuration Configuration;

    public LodestoneNetworker(INetworkClient networkClient, IDirtyListener dirtyListener, Configuration configuration)
    {
        NetworkClient = networkClient;

        DirtyListener = dirtyListener;
        Configuration = configuration;

        PreferredRegion = PluginHandlers.ClientState.ClientLanguage switch
        {
            ClientLanguage.Japanese => LodestoneRegion.Japan,
            ClientLanguage.German   => LodestoneRegion.Germany,
            ClientLanguage.French   => LodestoneRegion.France,
            ClientLanguage.English  => LodestoneRegion.America,
            _ => LodestoneRegion.Germany,
        };

        DirtyListener.RegisterDirtyUser(OnDirty);
    }

    void OnDirty()
    {
        SetSessionToken(string.Empty);

        ClearQueue();
        NetworkClient.CancelPendingRequests();
    }

    public void SetSessionToken(string sessionToken)
    {
        NetworkClient.SetSessionToken(sessionToken);
    }

    public string GetSessionToken()
    {
        return NetworkClient.GetSessionToken(); ;
    }

    public ILodestoneQueueElement AddElementToQueue(ILodestoneRequest request)
    {
        ILodestoneQueueElement queueElement = new LodestoneQueueElement(NetworkClient, request.HandleSuccess, request.HandleFailure, request.OnTick, GetBaseString() + request.GetURL(), request.TickCount);
        request.SetCancellationToken(queueElement.GetToken());
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

            if (queueElement.QueueState == QueueState.Success)
            {
                if (!queueElement.IsDone()) continue;
            }

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

    public LodestoneQueueElementData[] GetAllQueueData()
    {
        LodestoneQueueElementData[] queueData = new LodestoneQueueElementData[_queueElements.Count];

        for (int i = 0; i < _queueElements.Count; i++)
        {
            queueData[i] = _queueElements[i].GetQueueData();
        }

        return queueData;
    }

    public void Dispose()
    {
        DirtyListener.UnregisterDirtyUser(OnDirty);

        ClearQueue();
    }


}
