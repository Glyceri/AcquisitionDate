using AcquisitionDate.Database;
using AcquisitionDate.Database.Interfaces;
using Newtonsoft.Json;
using System;

namespace AcquisitionDate.Serializiation;

[Serializable]
internal class SerializableList
{
    public readonly uint[] IDS = [];
    public readonly DateTime?[] LodestoneTimes = [];
    public readonly DateTime?[] CopyPasteTimes = [];
    public readonly DateTime?[] ManualTimes = [];

    [JsonConstructor]
    public SerializableList(uint[] ids, DateTime?[] lodestoneTimes, DateTime?[] copyPasteTimes, DateTime?[] manualTimes)
    {
        IDS = ids;
        LodestoneTimes = lodestoneTimes;
        CopyPasteTimes = copyPasteTimes;
        ManualTimes = manualTimes;
    }

    public SerializableList(IDatableList list)
    {
        if (list is not DatableList dList) return;

        IDS = dList.IDs;
        LodestoneTimes = dList.LodestoneTimes;
        CopyPasteTimes = dList.CopyPasteTimes;
        ManualTimes = dList.ManualTimes;
    }
}
