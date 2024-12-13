using AcquisitionDate.LodestoneNetworking.Queue.Enums;
using System;

namespace AcquisitionDate.LodestoneNetworking.Queue.Interfaces;

internal interface ILodestoneQueueElement : IDisposable
{
    float TimeInQueue { get; }
    QueueState QueueState { get; }
    void SendRequest();
    void MarkAsInQueue();

    void Tick(float deltaTime);
}
