using System;

namespace AcquisitionDate.LodestoneData;

internal struct AchievementData
{
    public readonly uint AchievementID;
    public readonly DateTime AchievedDate;

    public AchievementData(uint achievementID, DateTime achievedDate)
    {
        AchievementID = achievementID;
        AchievedDate = achievedDate;
    }
}
