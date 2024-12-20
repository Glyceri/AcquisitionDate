using System;

namespace AcquisitionDate.LodestoneData;

internal struct ItemData
{
    public readonly uint ItemID;
    public readonly DateTime AchievedDate;

    public ItemData(uint itemID, DateTime achievedDate)
    {
        ItemID = itemID;
        AchievedDate = achievedDate;
    }
}
