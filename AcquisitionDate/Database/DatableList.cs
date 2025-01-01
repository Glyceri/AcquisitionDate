using AcquisitionDate.Database.Enums;
using AcquisitionDate.Database.Interfaces;
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

    public int Length => IDs.Length;

    public DateTime? LowestDateTime => GetLowestDateTime();

    DateTime? lowestDateTime = null;
    bool isDirty = true;

    public DatableList(IDirtySetter dirtySetter)
    {
        DirtySetter = dirtySetter;
    }

    public DatableList(IDirtySetter dirtySetter, SerializableList list) : this(dirtySetter)
    {
        IDs = list.IDS;
        LodestoneTimes = list.LodestoneTimes;
        ManualTimes = list.ManualTimes;
    }

    public DateTime? GetDate(uint ID)
    {
        DateTime? manualTime = GetDate(ID, AcquiredDateType.Manual);
        if (manualTime != null) return manualTime;

        DateTime? lodestoneTime = GetDate(ID, AcquiredDateType.Lodestone);
        if (lodestoneTime != null) return lodestoneTime;

        return null;
    }

    public DateTime? GetDate(uint ID, AcquiredDateType dateType)
    {
        int? index = GetIndex(ID);
        if (index == null) return null;

        if (dateType == AcquiredDateType.Lodestone) return LodestoneTimes[index.Value];
        if (dateType == AcquiredDateType.Manual) return ManualTimes[index.Value];

        return null;
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
}
