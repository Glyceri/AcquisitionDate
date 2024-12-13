using AcquisitionDate.LodestoneNetworking.Enums;
using AcquisitionDate.LodestoneRequests.Interfaces;
using HtmlAgilityPack;
using System;

namespace AcquisitionDate.LodestoneNetworking.Interfaces;

internal interface ILodestoneNetworker : IDisposable
{
    LodestoneRegion PreferredRegion { get; set; }

    void AddElementToQueue(ILodestoneRequest request);
    void AddElementToQueue(Action<HtmlDocument> onSuccess, Action<Exception> onFailure, string URL);
    void Update(float deltaTime);
}
