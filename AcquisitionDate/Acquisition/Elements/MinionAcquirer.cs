using AcquiryDate.PetNicknames.Services.ServiceWrappers.Interfaces;
using AcquisitionDate.Core.Handlers;
using AcquisitionDate.LodestoneData;
using AcquisitionDate.LodestoneNetworking.Interfaces;
using AcquisitionDate.LodestoneRequests.Requests;
using AcquisitionDate.Services.Interfaces;
using System;

namespace AcquisitionDate.Acquisition.Elements;

internal class MinionAcquirer : AcquirerBase
{
    // TEMPORARY
    public static MinionAcquirer Instance { get; private set; }

    readonly ISheets Sheets;

    public MinionAcquirer(ISheets sheets, ILodestoneNetworker networker) : base(networker)
    {
        Sheets = sheets;
        Instance = this;
    }

    int internalCounter = 0;
    int maxCounter = 0;

    string[] urls = [];

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
        urls = [];
    }

    void GatherMaxCounter()
    {
        SetPercentage(1);
        RegisterQueueElement(new MinionPageListRequest(_currentUser, OnPageURLList));
    }

    void OnPageURLList(PageURLListData? data)
    {
        SetPercentage(10);
        if (data == null)
        {
            Failed();
            return;
        }

        maxCounter = data.Value.URLs.Length;
        urls = data.Value.URLs;

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

        if (internalCounter < 0 || internalCounter >= urls.Length)
        {
            Success();
            return;
        }

        RegisterQueueElement(new MinionDataRequest(Sheets, urls[internalCounter], OnItemData, UpCounterAndActivate, OnFailure));
    }

    void OnFailure(Exception e)
    {
        Failed();
    }

    void OnItemData(ItemData itemData)
    {
        _currentUser.MinionList.SetDate(itemData.ItemID, itemData.AchievedDate, Database.Enums.AcquiredDateType.Lodestone);
    }

    protected override void OnDispose()
    {

    }
}
