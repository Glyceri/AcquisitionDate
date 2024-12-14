using AcquisitionDate.LodestoneNetworking.Enums;
using AcquisitionDate.LodestoneNetworking.Queue.Interfaces;
using AcquisitionDate.LodestoneRequests.Interfaces;
using HtmlAgilityPack;
using System;

namespace AcquisitionDate.LodestoneNetworking.Interfaces;

internal interface ILodestoneNetworker : IDisposable
{
    LodestoneRegion PreferredRegion { get; set; }

    ILodestoneQueueElement AddElementToQueue(ILodestoneRequest request);
    ILodestoneQueueElement AddElementToQueue(Action<HtmlDocument> onSuccess, Action<Exception> onFailure, string URL);
    void Update(float deltaTime);
}
