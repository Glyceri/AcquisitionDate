using AcquisitionDate.Core.Handlers;
using AcquisitionDate.Database.Enums;
using AcquisitionDate.DatableUsers.Interfaces;
using AcquisitionDate.DirtySystem.Interfaces;
using AcquisitionDate.Services.Interfaces;
using FFXIVClientStructs.FFXIV.Client.Game;
using Lumina.Excel.Sheets;
using System;
using System.Collections.Generic;

namespace AcquisitionDate.Hooking.Hooks.UnlockHooks;

internal unsafe class QuestUnlockHook : UnlockHook
{
    readonly List<uint> QuestsCompleted = new List<uint>();
    byte lastAcceptedQuestCount = 0;

    readonly ISheets Sheets;

    public QuestUnlockHook(ISheets sheets, IUserList userList, IDirtyListener dirtyListener) : base(dirtyListener, userList)
    {
        Sheets = sheets;
    }

    public override void Init()
    {
        Reset();
    }

    public override void Reset()
    {
        QuestsCompleted.Clear();
        lastAcceptedQuestCount = QuestManager.Instance()->NumAcceptedQuests;

        foreach (Quest quest in Sheets.AllQuests)
        {
            if (!QuestManager.IsQuestComplete(quest.RowId)) continue;

            QuestsCompleted.Add(quest.RowId);
        }
    }

    public override void OnUpdate(float deltaTime)
    {
        HandleQuestDiffCheck();
    }

    void HandleQuestDiffCheck()
    {
        byte numAcceptedQuests = QuestManager.Instance()->NumAcceptedQuests;
        if (numAcceptedQuests == lastAcceptedQuestCount) return;

        lastAcceptedQuestCount = numAcceptedQuests;
        CheckQuests();
    }

    void CheckQuests()
    {
        foreach (Quest quest in Sheets.AllQuests)
        {
            uint questRowID = quest.RowId;
            if (!QuestManager.IsQuestComplete(questRowID)) continue;
            if (QuestsCompleted.Contains(questRowID)) continue;

            QuestsCompleted.Add(questRowID);
            PluginHandlers.PluginLog.Verbose($"Quest with ID {questRowID} and name {quest.Name.ExtractText()} has been found.");
            UserList.ActiveUser?.Data.QuestList.SetDate(questRowID, DateTime.Now, AcquiredDateType.Manual);
        }
    }

    public override void Dispose() { }
}
