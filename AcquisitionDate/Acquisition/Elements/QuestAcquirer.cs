using AcquisitionDate.LodestoneData;
using AcquisitionDate.LodestoneNetworking.Interfaces;
using AcquisitionDate.LodestoneRequests.Requests;
using AcquisitionDate.Services.Interfaces;
using System;
using System.Diagnostics;

namespace AcquisitionDate.Acquisition.Elements;

internal class QuestAcquirer : AcquirerBase
{
    public static QuestAcquirer Instance { get; private set; }

    readonly IAcquisitionServices Services;

    public QuestAcquirer(IAcquisitionServices services, ILodestoneNetworker networker) : base(networker)
    {
        Services = services;
        Instance = this;
    }

    int internalCounter = 0;
    int maxCounter = 0;

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
        RegisterQueueElement(new QuestPageCountRequest(_currentUser, OnPageCountData));
    }

    void OnPageCountData(PageCountData? data)
    {
        SetPercentage(10);
        if (data == null)
        {
            Services.PetLog.Log("Data is null!");
            Failed();
            return;
        }

        maxCounter = data.Value.PageCount;

        UpCounterAndActivate();

    }

    void OnQuestData(QuestData data)
    {
        _currentUser.QuestList.SetDate(data.QuestID, data.AchievedDate, Database.Enums.AcquiredDateType.Lodestone);
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

        RegisterQueueElement(new QuestDataRequest(Services.Sheets, _currentUser, internalCounter, OnQuestData, OnAchievementPageComplete, OnFailure));
    }

    void OnAchievementPageComplete()
    {
        UpCounterAndActivate();
    }

    void OnFailure(Exception e)
    {
        Failed();
    }

    protected override void OnDispose()
    {

    }
}
