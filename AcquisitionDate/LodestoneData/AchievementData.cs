using System;

namespace AcquisitionDate.LodestoneData;

internal struct AchievementData
{
    public readonly int AchievementID;
    public readonly DateTime AchievedDate;

    public AchievementData(int achievementID, DateTime achievedDate)
    {
        AchievementID = achievementID;
        AchievedDate = achievedDate;
    }
}
