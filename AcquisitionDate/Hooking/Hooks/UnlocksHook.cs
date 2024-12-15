using AcquisitionDate.Core.Handlers;
using AcquisitionDate.Database.Enums;
using AcquisitionDate.Database.Interfaces;
using AcquisitionDate.DatableUsers.Interfaces;
using AcquisitionDate.Services.Enums;
using AcquisitionDate.Services.Interfaces;
using Dalamud.Hooking;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.Exd;
using Lumina.Excel.Sheets;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UIAlias = FFXIVClientStructs.FFXIV.Client.Game.UI;

namespace AcquisitionDate.Hooking.Hooks;

internal unsafe class UnlocksHook : HookableElement
{
    readonly List<uint> UnlockedItems = new List<uint>();

    delegate void RaptureAtkModuleUpdateDelegate(RaptureAtkModule* ram, float f1);
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
        RaptureAtkModuleUpdateHook?.Enable();

        HandleUnlockedItems();
    }

    void HandleUnlockedItems()
    {
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

    void RaptureAtkModule_UpdateDetour(RaptureAtkModule* module, float delta)
    {
        try
        {
            if (module->AgentUpdateFlag.HasFlag(RaptureAtkModule.AgentUpdateFlags.UnlocksUpdate))
            {
                List<Item> unlockedItems = GetNewlyUnlockedItems();

                foreach (Item item in unlockedItems)
                {
                    PluginHandlers.PluginLog.Verbose("AcquiredDate detected recent unlock: " + item.Name.ExtractText());
                    try
                    {
                        StoreItemUnlock(item);
                    }
                    catch (Exception ex)
                    {
                        PluginHandlers.PluginLog.Error(ex, $"Storing item: {item.Name.ExtractText()} failed.");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            PluginHandlers.PluginLog.Error(ex, "Error during RaptureAtkModule_UpdateDetour");
        }

        RaptureAtkModuleUpdateHook!.OriginalDisposeSafe(module, delta);
    }

    public override void Dispose()
    {
        RaptureAtkModuleUpdateHook?.Dispose();
    }
}
