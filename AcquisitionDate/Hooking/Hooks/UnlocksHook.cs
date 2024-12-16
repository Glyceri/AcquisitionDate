using AcquisitionDate.Core.Handlers;
using AcquisitionDate.Database.Enums;
using AcquisitionDate.Database.Interfaces;
using AcquisitionDate.DatableUsers.Interfaces;
using AcquisitionDate.Services.Enums;
using AcquisitionDate.Services.Interfaces;
using Dalamud.Hooking;
using Dalamud.Utility.Signatures;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.InstanceContent;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.Exd;
using Lumina.Excel.Sheets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UIAlias = FFXIVClientStructs.FFXIV.Client.Game.UI;

namespace AcquisitionDate.Hooking.Hooks;

internal unsafe class UnlocksHook : HookableElement
{
    readonly List<uint> InstancedContentCompleted = new List<uint>();
    readonly List<uint> UnlockedItems = new List<uint>();

    delegate void OnAchievementUnlockDelegate(UIAlias.Achievement* achievement, uint achievementID);
    delegate void RaptureAtkModuleUpdateDelegate(RaptureAtkModule* ram, float deltaTime);

    [Signature("81 FA ?? ?? ?? ?? 0F 87 ?? ?? ?? ?? 53", DetourName = nameof(AchievementUnlockedDetour))]
    readonly Hook<OnAchievementUnlockDelegate>? AchievementUnlockHook;
    readonly Hook<RaptureAtkModuleUpdateDelegate>? RaptureAtkModuleUpdateHook;

    readonly ISheets Sheets;
    readonly IUserList UserList;

    public UnlocksHook(ISheets sheets, IUserList userList)
    {
        Sheets = sheets;
        UserList = userList;

        RaptureAtkModuleUpdateHook = PluginHandlers.Hooking.HookFromFunctionPointerVariable<RaptureAtkModuleUpdateDelegate>(new nint(&RaptureAtkModule.StaticVirtualTablePointer->Update), RaptureAtkModule_UpdateDetour);
    }

    public override void Init()
    {
        Reset();

        RaptureAtkModuleUpdateHook?.Enable();
        AchievementUnlockHook?.Enable();

        PluginHandlers.DutyState.DutyCompleted += OnDutyCompleted;
    }

    public void Reset()
    {
        HandleUnlockedItems();
        HandleUnlockedInstances();
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
        RaptureAtkModuleUpdateHook?.Dispose();
        AchievementUnlockHook?.Dispose();
        PluginHandlers.DutyState.DutyCompleted -= OnDutyCompleted;
    }
}
