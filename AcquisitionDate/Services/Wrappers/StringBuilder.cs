using System;
using AcquisitionDate.Database.Structs;
using AcquisitionDate.Hooking.Hooks;
using AcquisitionDate.Services.Interfaces;
using Dalamud.Utility;

namespace AcquisitionDate.Services.Wrappers;

internal class StringBuilder : IStringBuilder
{
    private readonly Configuration Configuration;
    
    public StringBuilder(Configuration configuration)
    {
        Configuration = configuration;
    }
    
    /// <summary>
    /// If namestring == null, that means the dateString is for the localplayer, if it is NOT null, that means the datestring is for an alt.
    /// </summary>
    public bool CreateDateString(TypedDatabaseEntry[] entries, uint entryId, DrawTextFlags drawTextFlags, out string? dateString, out string? nameString, out string? homeworldString)
    {
        dateString      = null;
        nameString      = null;
        homeworldString = null;
        
        if (!drawTextFlags.HasFlag(DrawTextFlags.DisallowQuestionmarks))
        {
            dateString = Configuration.DateStandinString();
        }
        
        int  entriesLength = entries.Length;
        bool allowAlts     = drawTextFlags.HasFlag(DrawTextFlags.AllowAlts);
        
        allowAlts &= Configuration.ShowDatesFromAlts;
        
        if (!allowAlts)
        {
            entriesLength = Math.Min(entriesLength, 1);
        }
        
        for (int i = 0; i < entriesLength; i++)
        {
            TypedDatabaseEntry entry        = entries[i];
            UnlockedDate?      unlockedDate = entry.List.GetDate(entryId);
            
            if (unlockedDate == null)
            {
                continue;
            }
            
            AcquisitionDateTime? dateTime = unlockedDate.Value.GetDateTime();
            
            if (dateTime == null)
            {
                continue;
            }
            
            if (entry.Data.Name.IsNullOrWhitespace())
            {
                continue;
            }
            
            if (entry.Data.HomeworldName.IsNullOrWhitespace())
            {
                continue;
            }
            
            dateString = dateTime.Value.GetTimeString();
            
            if (!entry.Data.IsLocalPlayer)
            {
                nameString      = entry.Data.Name;
                homeworldString = entry.Data.HomeworldName;
            }
            
            return true;
        }
        
        return !dateString.IsNullOrWhitespace();
    }
}