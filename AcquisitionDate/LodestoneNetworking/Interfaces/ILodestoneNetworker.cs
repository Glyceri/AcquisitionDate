using AcquisitionDate.LodestoneNetworking.Enums;
using AcquisitionDate.LodestoneNetworking.Queue.Interfaces;
using AcquisitionDate.LodestoneRequests.Interfaces;
using AcquisitionDate.Updating.Interfaces;
using HtmlAgilityPack;
using System;

namespace AcquisitionDate.LodestoneNetworking.Interfaces;

internal interface ILodestoneNetworker : IDisposable, IUpdatable
{
    LodestoneRegion PreferredRegion { get; }

    ILodestoneQueueElement AddElementToQueue(ILodestoneRequest request);
    ILodestoneQueueElement AddElementToQueue(Action<HtmlDocument> onSuccess, Action<Exception> onFailure, string URL);
}
