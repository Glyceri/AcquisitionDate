using AcquisitionDate.Core.Handlers;
using AcquisitionDate.LodestoneData;
using AcquisitionDate.LodestoneNetworking.Interfaces;
using AcquisitionDate.LodestoneRequests.Interfaces;
using System;

namespace AcquisitionDate.Acquisition.Elements.Bases;

internal abstract class AcquirerCounter : AcquirerBase
{
    int internalCounter = 0;
    int maxCounter = 0;

    protected AcquirerCounter(ILodestoneNetworker networker) : base(networker)
    {
    }

    protected abstract ILodestoneRequest PageCountRequest();
    protected abstract ILodestoneRequest DataRequest(int page);

    protected override void OnAcquire()
    {
        ResetCounter();
        GatherMaxCounter();
    }

    protected override void OnCancel()
    {
        ResetCounter();
    }

    void ResetCounter()
    {
        internalCounter = 0;
        maxCounter = 0;
    }

    void GatherMaxCounter()
    {
        SetPercentage(1);
        RegisterQueueElement(PageCountRequest());
    }

    protected void OnPageCountData(PageCountData? data)
    {
        SetPercentage(10);
        if (data == null)
        {
            Failed();
            return;
        }

        maxCounter = data.Value.PageCount;

        UpCounterAndActivate();
    }

    void UpCounterAndActivate()
    {
        internalCounter++;

        if (internalCounter > maxCounter)
        {
            Success();
            return;
        }

        float percentageAmount = internalCounter / (float)maxCounter;
        float mappedPerc = 10.0f + (percentageAmount * 0.9f * 100);

        SetPercentage(mappedPerc);

        if (!IsAcquiring)
        {
            Failed();
            return;
        }

        RegisterQueueElement(DataRequest(internalCounter));
    }

    protected void OnPageComplete()
    {
        UpCounterAndActivate();
    }

    protected void OnFailure(Exception e)
    {
        PluginHandlers.PluginLog.Error(e, "Acquirer encountered an error.");
        Failed();
    }
}
