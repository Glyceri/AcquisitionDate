using AcquisitionDate.Core.Handlers;
using AcquisitionDate.LodestoneData;
using AcquisitionDate.LodestoneNetworking.Enums;
using AcquisitionDate.LodestoneNetworking.Interfaces;
using AcquisitionDate.LodestoneRequests.Interfaces;
using AcquisitionDate.LodestoneRequests.Requests;
using AcquisitionDate.Parser.Interfaces;
using System;

namespace AcquisitionDate.Acquisition.Elements.Bases;

internal abstract class AcquirerCounter : AcquirerBase
{
    protected int internalCounter = 0;
    protected int maxCounter = 0;
    protected LodestoneRegion pageRegion = LodestoneRegion.Europe;

    protected AcquirerCounter(ILodestoneNetworker networker, IAcquisitionParser acquistionParser) : base(networker, acquistionParser)
    {
    }

    ILodestoneRequest PageLanguageRequest() => new LodestoneLanguageRequest
    (
        AcquisitionParser.LodestonePageLanguageParser,
        _currentUser,
        OnPageLanguageData
    );

    protected abstract ILodestoneRequest PageCountRequest();
    protected abstract ILodestoneRequest DataRequest(int page);
    

    protected override void OnAcquire()
    {
        ResetCounter();
        GatherLanguage();
    }

    protected override void OnCancel()
    {
        ResetCounter();
    }

    protected virtual void ResetCounter()
    {
        internalCounter = 0;
        maxCounter = 0;
    }

    void GatherLanguage()
    {
        SetPercentage(1);
        RegisterQueueElement(PageLanguageRequest());
    }

    void GatherMaxCounter()
    {
        RegisterQueueElement(PageCountRequest());
    }

    protected void OnPageCountData(PageCountData? data)
    {
        SetPercentage(10);
        if (data == null)
        {
            Failed("Page count data is NULL. (this probably means you didn't put in a session token)");
            return;
        }

        maxCounter = data.Value.PageCount;

        UpCounterAndActivate();
    }

    protected void OnPageLanguageData(LodestoneRegion lodestoneRegion)
    {
        pageRegion = lodestoneRegion;

        SetPercentage(5);

        GatherMaxCounter();
    }

    protected void UpCounterAndActivate()
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
            Failed("Acquisition Got Cancelled Early.");
            return;
        }

        if (ExtraCheck())
        {
            Success();
            return;
        }

        RegisterQueueElement(DataRequest(internalCounter));
    }

    protected virtual bool ExtraCheck() => false;

    protected void OnPageComplete()
    {
        UpCounterAndActivate();
    }

    protected void OnFailure(Exception e)
    {
        PluginHandlers.PluginLog.Error(e, "Acquirer encountered an error.");
        Failed(e.Message);
    }
}
