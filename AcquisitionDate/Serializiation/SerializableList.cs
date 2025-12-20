using AcquisitionDate.Database;
using AcquisitionDate.Database.Interfaces;
using Newtonsoft.Json;
using System;

namespace AcquisitionDate.Serializiation;

[Serializable]
internal class SerializableList
{
    [JsonProperty] public uint[] IDS { get; set; }                  = [];
    [JsonProperty] public DateTime?[] LodestoneTimes { get; set; }  = [];
    [JsonProperty] public DateTime?[] ManualTimes { get; set; }     = [];

    public SerializableList()
    {
        IDS             = [];
        LodestoneTimes  = [];
        ManualTimes     = [];
    }

    public SerializableList(uint[] ids, DateTime?[] lodestoneTimes, DateTime?[] manualTimes)
    {
        IDS             = ids;
        LodestoneTimes  = lodestoneTimes;
        ManualTimes     = manualTimes;
    }

    public SerializableList(IDatableList list)
    {
        if (list is not DatableList dList) return;

        IDS             = dList.IDs;
        LodestoneTimes  = dList.LodestoneTimes;
        ManualTimes     = dList.ManualTimes;
    }

    public static SerializableList CreateEmpty() => new SerializableList([], [], []);
}
