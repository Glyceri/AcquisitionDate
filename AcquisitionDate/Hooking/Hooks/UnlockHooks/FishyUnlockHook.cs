using AcquisitionDate.Core.Handlers;
using AcquisitionDate.Database.Enums;
using AcquisitionDate.Database.Interfaces;
using AcquisitionDate.DatableUsers.Interfaces;
using AcquisitionDate.Services.Interfaces;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using System;
using System.Linq;

namespace AcquisitionDate.Hooking.Hooks.UnlockHooks;

internal unsafe class FishyUnlockHook : UnlockHook
{
    public const uint SpearFishIdOffset = 20000;

    byte[] _fishStore       = [];
    byte[] _spearFishStore  = [];

    public FishyUnlockHook(IUserList userList, ISheets sheets) : base(userList, sheets) { }

    public override void Reset() 
    {
        _fishStore      = PlayerState.Instance()->CaughtFishBitmask.ToArray();
        _spearFishStore = new Span<byte>((void*)((nint)PlayerState.Instance() + 0x4B1), 39).ToArray();
    } 
    
    public uint? GetCaughtFishIndices(Span<byte> oldStore, Span<byte> newStore)
    {
        // Ensure both bitmasks are available
        Span<byte> oldBitmask = oldStore;
        Span<byte> newBitmask = newStore;

        // Iterate through each byte in the bitmask arrays
        int maxLength = Math.Min(oldBitmask.Length, newBitmask.Length);
        for (int byteIndex = 0; byteIndex < maxLength; byteIndex++)
        {
            byte oldByte = oldBitmask[byteIndex];
            byte newByte = newBitmask[byteIndex];

            // Compare corresponding bits in the old and new byte arrays
            byte difference = (byte)(newByte & ~oldByte); // This isolates the bits set in newByte but not in oldByte

            // Iterate through each bit in the difference byte
            for (int bitIndex = 0; bitIndex < 8; bitIndex++)
            {
                if ((difference & (1 << bitIndex)) != 0)
                {
                    // Calculate the global fish index
                    int fishIndex = byteIndex * 8 + bitIndex;
                    return (uint)fishIndex;
                }
            }
        }

        return null;
    }

    uint? CheckFishies(ref byte[] store, Span<byte> bitmask)
    {
        Span<byte> span = bitmask;

        bool fishyEquals = new Span<byte>(store, 0, store.Length).SequenceEqual(span);
        if (fishyEquals) return null;

        uint? outcome = GetCaughtFishIndices(store, span);

        store = span.ToArray();

        return outcome;
    }

    public override void Update(float deltaTime) 
    {
        IDatableUser? localUser = UserList.ActiveUser;
        if (localUser == null) return;

        IDatableData data = localUser.Data;

        uint? fishOutcome = CheckFishies(ref _fishStore, PlayerState.Instance()->CaughtFishBitmask);
        if (fishOutcome != null)
        {
            data.FishingList.SetDate(fishOutcome.Value, DateTime.Now, AcquiredDateType.Manual);
            PluginHandlers.PluginLog.Verbose($"Found new fish caught with ID: {fishOutcome.Value}");
        }

        // new Span<byte>((void*)((nint)PlayerState.Instance() + 0x4B1), 39)
        // That should be PlayerState.Instance()->CaughtSpearFishBitmask, but it is still inaccurate in CS
        uint? spfishOutcome = CheckFishies(ref _spearFishStore, new Span<byte>((void*)((nint)PlayerState.Instance() + 0x4B1), 39));
        if (spfishOutcome != null)
        {
            spfishOutcome += SpearFishIdOffset;
            data.FishingList.SetDate(spfishOutcome.Value, DateTime.Now, AcquiredDateType.Manual);
            PluginHandlers.PluginLog.Verbose($"Found new spearfish caught with ID: {spfishOutcome.Value}");
        }
    }

    public override void Init() { }     // Init is unused
    public override void Dispose() { }  // Dispose is unsued
}
