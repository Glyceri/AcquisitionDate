using AcquisitionDate.LodestoneNetworking.Queue.Enums;
using AcquisitionDate.LodestoneNetworking.Structs;
using System;
using System.Threading;

namespace AcquisitionDate.LodestoneNetworking.Queue.Interfaces;

internal interface ILodestoneQueueElement : IDisposable
{
    float TimeInQueue { get; }
    QueueState QueueState { get; }

    bool DISPOSED { get; }

    void SendRequest();
    void MarkAsInQueue();
    void Cancel();

    bool IsDone();

    void Tick(float deltaTime);

    CancellationToken GetToken();

    LodestoneQueueElementData GetQueueData();
}
