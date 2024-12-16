using AcquisitionDate.Core.Handlers;
using AcquisitionDate.Database.Enums;
using AcquisitionDate.Database.Interfaces;
using AcquisitionDate.DatableUsers.Interfaces;
using AcquisitionDate.DirtySystem.Interfaces;
using AcquisitionDate.Hooking.Hooks.Interfaces;
using AcquisitionDate.Services.Enums;
using AcquisitionDate.Services.Interfaces;
using Dalamud.Hooking;
using Dalamud.Utility.Signatures;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Component.Exd;
using Lumina.Excel.Sheets;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UIAlias = FFXIVClientStructs.FFXIV.Client.Game.UI;

namespace AcquisitionDate.Hooking.Hooks;

internal unsafe class UnlocksHook : HookableElement, IUnlocksHook
{
    readonly List<uint> QuestsCompleted = new List<uint>();
    readonly List<uint> InstancedContentCompleted = new List<uint>();
    readonly List<uint> UnlockedItems = new List<uint>();
    short[] currentClassJobLevels = [];

    delegate void OnAchievementUnlockDelegate(UIAlias.Achievement* achievement, uint achievementID);
    delegate void RaptureAtkModuleUpdateDelegate(RaptureAtkModule* ram, float deltaTime);

    [Signature("81 FA ?? ?? ?? ?? 0F 87 ?? ?? ?? ?? 53", DetourName = nameof(AchievementUnlockedDetour))]
    readonly Hook<OnAchievementUnlockDelegate>? AchievementUnlockHook;
    readonly Hook<RaptureAtkModuleUpdateDelegate>? RaptureAtkModuleUpdateHook;

    byte lastAcceptedQuestCount = 0;

    readonly ISheets Sheets;
    readonly IUserList UserList;
    readonly IDirtyListener DirtyListener;

    public UnlocksHook(ISheets sheets, IUserList userList, IDirtyListener dirtyListener)
    {
        Sheets = sheets;
        UserList = userList;
        DirtyListener = dirtyListener;

        RaptureAtkModuleUpdateHook = PluginHandlers.Hooking.HookFromFunctionPointerVariable<RaptureAtkModuleUpdateDelegate>(new nint(&RaptureAtkModule.StaticVirtualTablePointer->Update), RaptureAtkModule_UpdateDetour);
    }

    public override void Init()
    {
        Reset();
        DirtyListener.RegisterDirtyUser(Reset);

        RaptureAtkModuleUpdateHook?.Enable();
        AchievementUnlockHook?.Enable();

        PluginHandlers.DutyState.DutyCompleted += OnDutyCompleted;
        PluginHandlers.ClientState.LevelChanged += OnLevelChanged;
    }

    public void Reset()
    {
        HandleUnlockedQuests();
        HandleUnlockedItems();
        HandleUnlockedInstances();
        HandleClassJobLevels();
    }

    void HandleClassJobLevels()
    {
        currentClassJobLevels = PlayerState.Instance()->ClassJobLevels.ToArray();
    }

    void HandleUnlockedQuests()
    {
        QuestsCompleted.Clear();
        lastAcceptedQuestCount = QuestManager.Instance()->NumAcceptedQuests;

        foreach (Quest quest in Sheets.AllQuests)
        {
            if (!QuestManager.IsQuestComplete(quest.RowId)) continue;

            QuestsCompleted.Add(quest.RowId);
        }
    }

    void HandleUnlockedItems()
    {
        UnlockedItems.Clear();

        foreach (Item item in Sheets.AllItems)
        {
            if (!IsUnlocked(item, out bool isUnlocked)) continue;
            if (!isUnlocked) continue;

            UnlockedItems.Add(item.RowId);
        }
    }

    void HandleUnlockedInstances()
    {
        InstancedContentCompleted.Clear();

        foreach (ContentFinderCondition iContent in Sheets.AllContentFinderConditions)
        {
            uint contentRowID = iContent.Content.RowId;
            if (!UIState.IsInstanceContentCompleted(contentRowID)) continue;
            
            InstancedContentCompleted.Add(contentRowID);
        }
    }

    public void Update()
    {
        if (UserList.ActiveUser == null) return;

        HandleQuestDiffCheck();
    }

    public List<Item> GetNewlyUnlockedItems(bool addToList = true)
    {
        List<Item> freshlyUnlockedItems = new List<Item>();

        foreach (Item item in Sheets.AllItems)
        {
            if (!IsUnlocked(item, out bool isUnlocked)) continue;
            if (!isUnlocked) continue;

            uint itemID = item.RowId;
            if (UnlockedItems.Contains(itemID)) continue;

            freshlyUnlockedItems.Add(item);

            if (!addToList) continue;
            UnlockedItems.Add(item.RowId);
        }

        return freshlyUnlockedItems;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)] ushort GetCompanionID(Item item) => GetItemActionID(item);
    [MethodImpl(MethodImplOptions.AggressiveInlining)] ushort GetBuddyEquipID(Item item) => GetItemActionID(item);
    [MethodImpl(MethodImplOptions.AggressiveInlining)] ushort GetMountID(Item item) => GetItemActionID(item);
    [MethodImpl(MethodImplOptions.AggressiveInlining)] ushort GetSecretRecipeID(Item item) => GetItemActionID(item);
    [MethodImpl(MethodImplOptions.AggressiveInlining)] ushort GetUnlockLinkID(Item item) => GetItemActionID(item);
    [MethodImpl(MethodImplOptions.AggressiveInlining)] ushort GetFolkloreID(Item item) => GetItemActionID(item);
    [MethodImpl(MethodImplOptions.AggressiveInlining)] ushort GetFramerKitID(Item item) => GetItemActionID(item);
    [MethodImpl(MethodImplOptions.AggressiveInlining)] ushort GetOrnamentID(Item item) => GetItemActionID(item);
    [MethodImpl(MethodImplOptions.AggressiveInlining)] uint GetGlassesID(Item item) => GetItemAdditionalDataID(item);
    [MethodImpl(MethodImplOptions.AggressiveInlining)] uint GetTrippleTriadID(Item item) => GetItemAdditionalDataID(item);
    [MethodImpl(MethodImplOptions.AggressiveInlining)] uint GetOrchestrionID(Item item) => GetItemAdditionalDataID(item);

    [MethodImpl(MethodImplOptions.AggressiveInlining)] ushort GetItemActionID(Item item) => item.ItemAction.Value.Data[0];
    [MethodImpl(MethodImplOptions.AggressiveInlining)] uint GetItemAdditionalDataID(Item item) => item.AdditionalData.RowId;

    unsafe bool IsUnlocked(Item item, out bool itemIsUnlocked)
    {
        itemIsUnlocked = false;

        if (item.ItemAction.RowId == 0) return false;

        switch ((ItemActionType)item.ItemAction.Value.Type)
        {
            case ItemActionType.Companion:
                itemIsUnlocked = UIAlias.UIState.Instance()->IsCompanionUnlocked(GetCompanionID(item));
                return true;

            case ItemActionType.BuddyEquip:
                itemIsUnlocked = UIAlias.UIState.Instance()->Buddy.CompanionInfo.IsBuddyEquipUnlocked(GetBuddyEquipID(item));
                return true;

            case ItemActionType.Mount:
                itemIsUnlocked = UIAlias.PlayerState.Instance()->IsMountUnlocked(GetMountID(item));
                return true;

            case ItemActionType.SecretRecipeBook:
                itemIsUnlocked = UIAlias.PlayerState.Instance()->IsSecretRecipeBookUnlocked(GetSecretRecipeID(item));
                return true;

            case ItemActionType.UnlockLink:
                itemIsUnlocked = UIAlias.UIState.Instance()->IsUnlockLinkUnlocked(GetUnlockLinkID(item));
                return true;

            case ItemActionType.TripleTriadCard when item.AdditionalData.Is<TripleTriadCard>():
                itemIsUnlocked = UIAlias.UIState.Instance()->IsTripleTriadCardUnlocked((ushort)GetTrippleTriadID(item));
                return true;

            case ItemActionType.FolkloreTome:
                itemIsUnlocked = UIAlias.PlayerState.Instance()->IsFolkloreBookUnlocked(GetFolkloreID(item));
                return true;

            case ItemActionType.OrchestrionRoll when item.AdditionalData.Is<Orchestrion>():
                itemIsUnlocked = UIAlias.PlayerState.Instance()->IsOrchestrionRollUnlocked(GetOrchestrionID(item));
                return true;

            case ItemActionType.FramersKit:
                itemIsUnlocked = UIAlias.PlayerState.Instance()->IsFramersKitUnlocked(GetFramerKitID(item));
                return true;

            case ItemActionType.Ornament:
                itemIsUnlocked = UIAlias.PlayerState.Instance()->IsOrnamentUnlocked(GetOrnamentID(item));
                return true;

            case ItemActionType.Glasses:
                itemIsUnlocked = UIAlias.PlayerState.Instance()->IsGlassesUnlocked((ushort)GetGlassesID(item));
                return true;
        }

        void* row = ExdModule.GetItemRowById(item.RowId);
        if (row == null) return false;

        itemIsUnlocked = UIAlias.UIState.Instance()->IsItemActionUnlocked(row) == 1;
        return true;
    }

    void StoreItemUnlock(Item item)
    {
        if (item.ItemAction.RowId == 0) return;

        IDatableUser? localUser = UserList.ActiveUser;
        if (localUser == null) return;

        IDatableData data = localUser.Data;

        PluginHandlers.PluginLog.Verbose($"Detected Item Completion with ID: {item.RowId}");

        switch ((ItemActionType)item.ItemAction.Value.Type)
        {
            case ItemActionType.Companion:
                data.MinionList.SetDate(GetCompanionID(item), DateTime.Now, AcquiredDateType.Manual);
                break;

            case ItemActionType.BuddyEquip:
                data.BuddyEquipList.SetDate(GetBuddyEquipID(item), DateTime.Now, AcquiredDateType.Manual);
                break;

            case ItemActionType.Mount:
                data.MountList.SetDate(GetMountID(item), DateTime.Now, AcquiredDateType.Manual);
                break;

            case ItemActionType.SecretRecipeBook:
                data.SecretRecipeBookList.SetDate(GetSecretRecipeID(item), DateTime.Now, AcquiredDateType.Manual);
                break;

            case ItemActionType.UnlockLink:
                data.UnlockLinkList.SetDate(GetUnlockLinkID(item), DateTime.Now, AcquiredDateType.Manual);
                break;

            case ItemActionType.TripleTriadCard when item.AdditionalData.Is<TripleTriadCard>():
                data.CardList.SetDate(GetTrippleTriadID(item), DateTime.Now, AcquiredDateType.Manual);
                break;

            case ItemActionType.FolkloreTome:
                data.FolkloreTomeList.SetDate(GetFolkloreID(item), DateTime.Now, AcquiredDateType.Manual);
                break;

            case ItemActionType.OrchestrionRoll when item.AdditionalData.Is<Orchestrion>():
                data.OrchestrionList.SetDate(GetOrchestrionID(item), DateTime.Now, AcquiredDateType.Manual);
                break;

            case ItemActionType.FramersKit:
                data.FramersList.SetDate(GetFramerKitID(item), DateTime.Now, AcquiredDateType.Manual);
                break;

            case ItemActionType.Ornament:
                data.FashionList.SetDate(GetOrnamentID(item), DateTime.Now, AcquiredDateType.Manual);
                break;

            case ItemActionType.Glasses:
                data.FacewearList.SetDate(GetGlassesID(item), DateTime.Now, AcquiredDateType.Manual);
                break;
        }
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

    void OnLevelChanged(uint classJobId, uint level)
    {
        PluginHandlers.PluginLog.Verbose($"Detected a level change on the job: {classJobId} to level: {level}");

        ClassJob? classJob = Sheets.GetClassJob(classJobId);
        if (classJob == null)
        {
            PluginHandlers.PluginLog.Verbose("Couldn't find the classjob in the sheets???? HOW");
            return;
        }

        sbyte arrayIndex = classJob.Value.ExpArrayIndex;
        if (arrayIndex < 0 || arrayIndex >= currentClassJobLevels.Length)
        {
            PluginHandlers.PluginLog.Verbose($"Array index is out of range: {arrayIndex} on the classJobArray: {currentClassJobLevels.Length}");
            return;
        }

        short currentLevel = currentClassJobLevels[arrayIndex];
        if (currentLevel >= level)
        {
            PluginHandlers.PluginLog.Verbose($"This resulted in no actual change.");
            return;
        }

        HandleClassJobLevels();

        PluginHandlers.PluginLog.Verbose($"The class: {classJobId}, {classJob.Value.Name.ExtractText()} leveled up from: {currentLevel} to {level}. This has been marked.");

        uint preparedClassJobID = classJobId * 10000;
        preparedClassJobID += level;

        // Because I can only have one indexer im storing multiple pieces of data in one number, the 10000 part is the class job, the small number is the level

        UserList.ActiveUser?.Data.ClassLVLList.SetDate(preparedClassJobID, DateTime.Now, AcquiredDateType.Manual);
    }

    void OnDutyCompleted(object? sender, ushort dutyID)
    {
        ushort contentFinderConditionID = GameMain.Instance()->CurrentContentFinderConditionId;
        bool instanceHasAlreadyBeenCompleted = true; 

        ContentFinderCondition ? contentFinderCondition = Sheets.GetContentFinderCondition(contentFinderConditionID);
        string ccName = "[ERROR]";
        if (contentFinderCondition != null)
        {
            ccName = contentFinderCondition.Value.Name.ExtractText();
            instanceHasAlreadyBeenCompleted = InstancedContentCompleted.Contains(contentFinderCondition.Value.Content.RowId);
        }

        PluginHandlers.PluginLog.Verbose($"Detected a duty completed with the ID: {dutyID} with the detected ContentFinderConditionID: {contentFinderConditionID} with the name: {ccName}");
        if (instanceHasAlreadyBeenCompleted)
        {
            PluginHandlers.PluginLog.Verbose("This instance has already been completed however.");
            return;
        }

        UserList.ActiveUser?.Data.DutyList.SetDate(contentFinderConditionID, DateTime.Now, AcquiredDateType.Manual);
    }

    void AchievementUnlockedDetour(UIAlias.Achievement* achievement, uint achievementID)
    {
        AchievementUnlockHook!.Original(achievement, achievementID);

        PluginHandlers.PluginLog.Verbose($"Detected Acquired Achievement with ID: {achievementID}");
        UserList.ActiveUser?.Data.AchievementList.SetDate(achievementID, DateTime.Now, AcquiredDateType.Manual);
    }

    void RaptureAtkModule_UpdateDetour(RaptureAtkModule* module, float deltaTime)
    {
        RaptureAtkModuleUpdateHook!.OriginalDisposeSafe(module, deltaTime);

        try
        {
            if (!module->AgentUpdateFlag.HasFlag(RaptureAtkModule.AgentUpdateFlags.UnlocksUpdate)) return;

            List<Item> unlockedItems = GetNewlyUnlockedItems();

            foreach (Item item in unlockedItems)
            {
                try
                {
                    PluginHandlers.PluginLog.Verbose($"Detected Acquired Item with ID: {item.RowId} and the name: {item.Name.ExtractText()}");
                    StoreItemUnlock(item);
                }
                catch (Exception ex)
                {
                    PluginHandlers.PluginLog.Error(ex, $"Storing item: {item.Name.ExtractText()} failed.");
                }
            }
        }
        catch (Exception ex)
        {
            PluginHandlers.PluginLog.Error(ex, "Error during RaptureAtkModule_UpdateDetour");
        }
    }

    public override void Dispose()
    {
        PluginHandlers.DutyState.DutyCompleted -= OnDutyCompleted;
        PluginHandlers.ClientState.LevelChanged -= OnLevelChanged;

        DirtyListener.UnregisterDirtyUser(Reset);

        RaptureAtkModuleUpdateHook?.Dispose();
        AchievementUnlockHook?.Dispose();
    }
}
