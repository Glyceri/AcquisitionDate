using AcquisitionDate.LodestoneNetworking.Queue.Enums;

namespace AcquisitionDate.LodestoneNetworking.Structs;

internal readonly struct LodestoneQueueElementData
{
    public readonly float TimeInQueue;
    public readonly QueueState QueueState;
    public readonly bool Disposed;
    public readonly string URL;

    public readonly int TickCount;
    public readonly int AtTick;

    public LodestoneQueueElementData(float timeInQueue, QueueState queueState, bool disposed, string url, int tickCout, int atTick)
    {
        TimeInQueue = timeInQueue;
        QueueState = queueState;
        Disposed = disposed;
        URL = url;
        TickCount = tickCout;
        AtTick = atTick;
    }
}
