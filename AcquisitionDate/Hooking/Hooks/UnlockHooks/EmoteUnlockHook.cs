using AcquisitionDate.Database.Enums;
using AcquisitionDate.DatableUsers.Interfaces;
using AcquisitionDate.Services.Interfaces;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using Lumina.Excel.Sheets;
using System;
using System.Collections.Generic;

namespace AcquisitionDate.Hooking.Hooks.UnlockHooks;

internal unsafe class EmoteUnlockHook : UnlockHook
{
    const float checkTimer = 5;

    float easeTimer = 0;

    readonly List<bool> EmoteUnlockStatus = new List<bool>();

    public EmoteUnlockHook(IUserList userList, ISheets sheets) : base(userList, sheets) { }

    public override void Reset()
    {
        EmoteUnlockStatus.Clear();

        foreach (Emote emote in Sheets.AllEmotes)
        {
            ushort emoteID = (ushort)emote.RowId;

            bool canUseEmote = AgentEmote.Instance()->CanUseEmote(emoteID);

            EmoteUnlockStatus.Add(canUseEmote);
        }
    }

    public override void Update(float deltaTime)
    {
        easeTimer += deltaTime;

        // This timer makes sure emotes only get checked once every 5 seconds
        if (easeTimer < checkTimer) return;
        
        easeTimer -= checkTimer;

        int index = 0;

        foreach (Emote emote in Sheets.AllEmotes)
        {
            ushort emoteID = (ushort)emote.RowId;

            bool canUseEmote = AgentEmote.Instance()->CanUseEmote(emoteID);

            bool cachedUnlockedStatus = EmoteUnlockStatus[index];

            index++;

            if (cachedUnlockedStatus) continue;
            if (!canUseEmote) continue;

            EmoteUnlockStatus[index] = canUseEmote;

            UserList.ActiveUser?.Data.EmoteList.SetDate(emoteID, DateTime.Now, AcquiredDateType.Manual);
        }
    }

    public override void Init() { } // No init needed for this one
    public override void Dispose() { } // No dispose needed for this one
}
