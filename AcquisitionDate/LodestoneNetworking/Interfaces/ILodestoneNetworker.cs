using AcquisitionDate.LodestoneNetworking.Enums;
using AcquisitionDate.LodestoneNetworking.Queue.Interfaces;
using AcquisitionDate.LodestoneNetworking.Structs;
using AcquisitionDate.LodestoneRequests.Interfaces;
using AcquisitionDate.Updating.Interfaces;
using System;

namespace AcquisitionDate.LodestoneNetworking.Interfaces;

internal interface ILodestoneNetworker : IDisposable, IUpdatable
{
    LodestoneRegion PreferredRegion { get; }

    ILodestoneQueueElement AddElementToQueue(ILodestoneRequest request);

    LodestoneQueueElementData[] GetAllQueueData();

    void SetSessionToken(string sessionToken);
    string GetSessionToken();
}
