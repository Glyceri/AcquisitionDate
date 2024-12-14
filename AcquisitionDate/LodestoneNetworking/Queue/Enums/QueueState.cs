namespace AcquisitionDate.LodestoneNetworking.Queue.Enums;

internal enum QueueState
{
    Idle = 0,
    InQueue,
    BeingProcessed,
    Success,
    Failure,
    Cancelled,
    Stale,
}
