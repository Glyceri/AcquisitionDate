using AcquisitionDate.Database.Enums;
using AcquisitionDate.Database.Interfaces;
using AcquisitionDate.Serializiation;
using AcquisitionDate.Serializiation.DirtySystem.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AcquisitionDate.Database;

internal class DatableList : IDatableList
{
    public uint[] IDs = [];
    public DateTime?[] LodestoneTimes = [];
    public DateTime?[] CopyPasteTimes = [];
    public DateTime?[] ManualTimes = [];

    readonly IDirtySetter DirtySetter;

    public DatableList(IDirtySetter dirtySetter)
    {
        DirtySetter = dirtySetter;
    }

    public DatableList(IDirtySetter dirtySetter, SerializableList list) : this(dirtySetter)
    {
        IDs = list.IDS;
        LodestoneTimes = list.LodestoneTimes;
        CopyPasteTimes = list.CopyPasteTimes;
        ManualTimes = list.ManualTimes;
    }

    public DateTime? GetDate(uint ID)
    {
        DateTime? lodestoneTime = GetDate(ID, AcquiredDateType.Lodestone);
        if (lodestoneTime != null) return lodestoneTime;

        DateTime? copyPasteTime = GetDate(ID, AcquiredDateType.CopyPaste);
        if (copyPasteTime != null) return copyPasteTime;

        DateTime? manualTime = GetDate(ID, AcquiredDateType.Manual);
        if (manualTime != null) return manualTime;

        return null;
    }

    public DateTime? GetDate(uint ID, AcquiredDateType dateType)
    {
        int? index = GetIndex(ID);
        if (index == null) return null;

        if (dateType == AcquiredDateType.Lodestone) return LodestoneTimes[index.Value];
        if (dateType == AcquiredDateType.CopyPaste) return CopyPasteTimes[index.Value];
        if (dateType == AcquiredDateType.Manual) return ManualTimes[index.Value];

        return null;
    }

    public bool RemoveDate(uint ID, AcquiredDateType dateType)
    {
        int? index = GetIndex(ID);
        if (index == null) return false;

        int i = index.Value;

        if (dateType == AcquiredDateType.Lodestone) LodestoneTimes[i] = null;
        if (dateType == AcquiredDateType.CopyPaste) CopyPasteTimes[i] = null;
        if (dateType == AcquiredDateType.Manual)    ManualTimes[i] = null;

        if (LodestoneTimes[i] != null || CopyPasteTimes[i] != null || ManualTimes[i] != null) 
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
        RemoveAt(ref CopyPasteTimes, index.Value);
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
            CopyPasteTimes = [.. CopyPasteTimes, dateType == AcquiredDateType.CopyPaste ? value : null];
            ManualTimes = [.. ManualTimes, dateType == AcquiredDateType.Manual ? value : null];
            SetDirty();
            return;
        }

        int i = index.Value;

        if (dateType == AcquiredDateType.Lodestone) LodestoneTimes[i] = value;
        if (dateType == AcquiredDateType.CopyPaste) CopyPasteTimes[i] = value;
        if (dateType == AcquiredDateType.Manual) ManualTimes[i] = value;

        if (LodestoneTimes[i] != null || CopyPasteTimes[i] != null || ManualTimes[i] != null)
        {
            SetDirty();
            return;
        }

        RemoveDate(ID);
    }

    void SetDirty()
    {
        DirtySetter.NotifyDirty();
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
}
