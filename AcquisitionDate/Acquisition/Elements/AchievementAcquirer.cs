using AcquisitionDate.LodestoneData;
using AcquisitionDate.LodestoneNetworking.Interfaces;
using AcquisitionDate.LodestoneRequests.Requests;
using System;

namespace AcquisitionDate.Acquisition.Elements;

internal class AchievementAcquirer : AcquirerBase
{
    public static AchievementAcquirer Instance { get; private set; }

    public AchievementAcquirer(ILodestoneNetworker networker) : base(networker)
    {
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
        RegisterQueueElement(new AchievementPageCountRequest(_currentUser, OnPageCountData));
    }

    void OnPageCountData(PageCountData? data)
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

    void OnAchievementData(AchievementData data) 
    {
        _currentUser.AchievementList.SetDate(data.AchievementID, data.AchievedDate, Database.Enums.AcquiredDateType.Lodestone);
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

        RegisterQueueElement(new AchievementDateRequest(_currentUser, internalCounter, OnAchievementData, OnAchievementPageComplete, OnFailure));
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
