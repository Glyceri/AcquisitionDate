using AcquisitionDate.Core.Handlers;
using AcquisitionDate.Database.Enums;
using AcquisitionDate.Database.Interfaces;
using AcquisitionDate.DatableUsers.Interfaces;
using AcquisitionDate.Services.Enums;
using AcquisitionDate.Services.Interfaces;
using Dalamud.Hooking;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.UI;
using Lumina.Excel.Sheets;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace AcquisitionDate.Hooking.Hooks.UnlockHooks;

internal unsafe class ItemUnlockHook : UnlockHook
{
    readonly List<uint> UnlockedItems = new List<uint>();

    readonly Hook<RaptureAtkModuleUpdateDelegate>? RaptureAtkModuleUpdateHook;

    delegate void RaptureAtkModuleUpdateDelegate(RaptureAtkModule* ram, float deltaTime);

    public ItemUnlockHook(IUserList userList, ISheets sheets) : base(userList, sheets)
    {
        RaptureAtkModuleUpdateHook = PluginHandlers.Hooking.HookFromAddress<RaptureAtkModuleUpdateDelegate>((nint)RaptureAtkModule.StaticVirtualTablePointer->Update, RaptureAtkModule_UpdateDetour);
    }

    public override void Init()
    {
        RaptureAtkModuleUpdateHook?.Enable();
    }

    public override void Reset()
    {
        UnlockedItems.Clear();

        foreach (Item item in Sheets.AllItems)
        {
            if (!IsUnlocked(item, out bool isUnlocked)) continue;
            if (!isUnlocked) continue;

            UnlockedItems.Add(item.RowId);
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
                itemIsUnlocked = UIState.Instance()->IsCompanionUnlocked(GetCompanionID(item));
                return true;

            case ItemActionType.BuddyEquip:
                itemIsUnlocked = UIState.Instance()->Buddy.CompanionInfo.IsBuddyEquipUnlocked(GetBuddyEquipID(item));
                return true;

            case ItemActionType.Mount:
                itemIsUnlocked = PlayerState.Instance()->IsMountUnlocked(GetMountID(item));
                return true;

            case ItemActionType.SecretRecipeBook:
                itemIsUnlocked = PlayerState.Instance()->IsSecretRecipeBookUnlocked(GetSecretRecipeID(item));
                return true;

            case ItemActionType.UnlockLink:
                itemIsUnlocked = UIState.Instance()->IsUnlockLinkUnlocked(GetUnlockLinkID(item));
                return true;

            case ItemActionType.TripleTriadCard when item.AdditionalData.Is<TripleTriadCard>():
                itemIsUnlocked = UIState.Instance()->IsTripleTriadCardUnlocked((ushort)GetTrippleTriadID(item));
                return true;

            case ItemActionType.FolkloreTome:
                itemIsUnlocked = PlayerState.Instance()->IsFolkloreBookUnlocked(GetFolkloreID(item));
                return true;

            case ItemActionType.OrchestrionRoll when item.AdditionalData.Is<Orchestrion>():
                itemIsUnlocked = PlayerState.Instance()->IsOrchestrionRollUnlocked(GetOrchestrionID(item));
                return true;

            case ItemActionType.FramersKit:
                itemIsUnlocked = PlayerState.Instance()->IsFramersKitUnlocked(GetFramerKitID(item));
                return true;

            case ItemActionType.Ornament:
                itemIsUnlocked = PlayerState.Instance()->IsOrnamentUnlocked(GetOrnamentID(item));
                return true;

            case ItemActionType.Glasses:
                itemIsUnlocked = PlayerState.Instance()->IsGlassesUnlocked((ushort)GetGlassesID(item));
                return true;
        }

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

    void RaptureAtkModule_UpdateDetour(RaptureAtkModule* module, float deltaTime)
    {
        if (UserList.ActiveUser != null)
        {
            try
            {
                if (module->AgentUpdateFlag.HasFlag(RaptureAtkModule.AgentUpdateFlags.UnlocksUpdate) || 
                    module->AgentUpdateFlag.HasFlag(RaptureAtkModule.AgentUpdateFlags.InventoryUpdate))
                {
                    PluginHandlers.PluginLog.Verbose($"Unlocks Update Flag got set High: {module->AgentUpdateFlag}");
                    List<Item> unlockedItems = GetNewlyUnlockedItems();

                    foreach (Item item in unlockedItems)
                    {
                        PluginHandlers.PluginLog.Verbose($"Detected Acquired Item with ID: {item.RowId} and the name: {item.Name.ExtractText()}");
                        StoreItemUnlock(item);
                    }
                }
            }
            catch (Exception ex)
            {
                PluginHandlers.PluginLog.Error(ex, "Error during RaptureAtkModule_UpdateDetour");
            }
        }

        try
        {
            RaptureAtkModuleUpdateHook!.OriginalDisposeSafe(module, deltaTime);
        }
        catch (Exception e)
        {
            PluginHandlers.PluginLog.Error(e, "Failed ATKModuleUpdate");
        }
    }

    public override void Dispose()
    {
        RaptureAtkModuleUpdateHook?.Dispose();
    }

    public override void Update(float deltaTime) { } // No update needed for this one
}
