using AcquisitionDate.Database.Enums;
using AcquisitionDate.Database.Interfaces;
using AcquisitionDate.Database.Structs;
using AcquisitionDate.DirtySystem.Interfaces;
using AcquisitionDate.Serializiation;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AcquisitionDate.Database;

internal class DatableList : IDatableList
{
    public uint[] IDs = [];
    public DateTime?[] LodestoneTimes = [];
    public DateTime?[] ManualTimes = [];

    readonly IDirtySetter DirtySetter;
    readonly Configuration Configuration;

    public int Length => IDs.Length;

    DateTime? lowestDateTime = null;
    bool isDirty = true;

    public DatableList(IDirtySetter dirtySetter, Configuration configuration)
    {
        DirtySetter = dirtySetter;
        Configuration = configuration;
    }

    public DatableList(IDirtySetter dirtySetter, SerializableList list, Configuration configuration) : this(dirtySetter, configuration)
    {
        IDs = list.IDS;
        LodestoneTimes = list.LodestoneTimes;
        ManualTimes = list.ManualTimes;
    }

    public UnlockedDate? GetDate(uint ID)
    {
        int? index = GetIndex(ID);
        if (index == null) return null;

        DateTime? lowestDateTime = GetLowestDateTime();

        DateTime? manualTime = ManualTimes[index.Value];
        DateTime? lodestoneTime = LodestoneTimes[index.Value];

        AcquisitionDateTime? acqManualTime = null;
        AcquisitionDateTime? acqLodestoneTime = null;

        bool manualTimeIsLowest = false;
        bool lodestoneTimeIsLowest = false;

        if (lowestDateTime != null)
        {
            if (manualTime != null)
            {
                manualTimeIsLowest = lowestDateTime.Value.Ticks == manualTime.Value.Ticks;
            }
            if (lodestoneTime != null)
            {
                lodestoneTimeIsLowest = lowestDateTime.Value.Ticks == lodestoneTime.Value.Ticks;
            }
        }

        if (manualTime != null)
        {
            acqManualTime = new AcquisitionDateTime(manualTime.Value, manualTimeIsLowest);
        }

        if (lodestoneTime != null)
        {
            acqLodestoneTime = new AcquisitionDateTime(lodestoneTime.Value, lodestoneTimeIsLowest);
        }

        return new UnlockedDate(Configuration, acqManualTime, acqLodestoneTime);
    }

    public bool RemoveDate(uint ID, AcquiredDateType dateType)
    {
        int? index = GetIndex(ID);
        if (index == null) return false;

        int i = index.Value;

        if (dateType == AcquiredDateType.Lodestone) LodestoneTimes[i] = null;
        if (dateType == AcquiredDateType.Manual)    ManualTimes[i] = null;

        if (LodestoneTimes[i] != null || ManualTimes[i] != null) 
        { 
            SetDirty(); 
            return true; 
        }

        return RemoveDate(ID);
    }

    public bool RemoveDate(uint ID)
    {
        int? index = GetIndex(ID);
        if (index == null) return false;

        RemoveAt(ref IDs, index.Value);
        RemoveAt(ref LodestoneTimes, index.Value);
        RemoveAt(ref ManualTimes, index.Value);

        SetDirty();

        return true;
    }

    public void SetDate(uint ID, DateTime? value, AcquiredDateType dateType)
    {
        int? index = GetIndex(ID);
        if (index == null)
        {
            if (value == null) return;

            IDs = [.. IDs, ID];
            LodestoneTimes = [.. LodestoneTimes, dateType == AcquiredDateType.Lodestone ? value : null];
            ManualTimes = [.. ManualTimes, dateType == AcquiredDateType.Manual ? value : null];
            SetDirty();
            return;
        }

        int i = index.Value;

        if (dateType == AcquiredDateType.Lodestone) LodestoneTimes[i] = value;
        if (dateType == AcquiredDateType.Manual) ManualTimes[i] = value;

        if (LodestoneTimes[i] != null || ManualTimes[i] != null)
        {
            SetDirty();
            return;
        }

        RemoveDate(ID);
    }

    void SetDirty()
    {
        isDirty = true;
        DirtySetter.NotifyDirtyDatabase();
    }

    DateTime? GetLowestDateTime()
    {
        if (!isDirty) return lowestDateTime;

        isDirty = false;

        int count = 0;
        long totalMinTime = long.MaxValue;

        for (int i = 0; i < Length; i++)
        {
            DateTime? lTime = LodestoneTimes[i];
            DateTime? mTime = ManualTimes[i];

            long lValue = lTime?.Ticks ?? long.MaxValue;
            long mValue = mTime?.Ticks ?? long.MinValue;

            long minTime = lValue;
            if (mValue < minTime) minTime = mValue;

            if (minTime < totalMinTime)
            {
                totalMinTime = minTime;
                count = 1;
            }
            else if (minTime == totalMinTime)
            {
                count++;
            }
        }

        if (count <= 1)
        {
            lowestDateTime = null;
        }
        else
        {
            lowestDateTime = new DateTime(totalMinTime);
        }

        return lowestDateTime;
    }

    int? GetIndex(uint ID)
    {
        for (int i = 0; i < IDs.Length; i++)
        {
            if (IDs[i] != ID) continue;

            return i;
        }

        return null;
    }

    void RemoveAt<T>(ref T?[] array, int index)
    {
        List<T?> list = array.ToList();
        list.RemoveAt(index);
        array = list.ToArray();
    }

    public uint GetID(int index) => IDs[index];

    public SerializableList Serialise() => new SerializableList(this);
}
