using System;

namespace AcquisitionDate.LodestoneData;

internal struct QuestData
{
    public readonly uint QuestID;
    public readonly DateTime AchievedDate;

    public QuestData(uint questID, DateTime achievedDate)
    {
        QuestID = questID;
        AchievedDate = achievedDate;
    }
}
