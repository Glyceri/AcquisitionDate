using AcquisitionDate.Core.Handlers;
using AcquisitionDate.Database.Enums;
using AcquisitionDate.Database.Interfaces;
using AcquisitionDate.DatableUsers.Interfaces;
using AcquisitionDate.Services.Interfaces;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using System;
using System.Linq;
using System.Numerics;

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
        _spearFishStore = PlayerState.Instance()->CaughtSpearfishBitmask.ToArray();
    }

    public uint? GetCaughtFishIndices(Span<byte> oldStore, Span<byte> newStore)
    {
        // Calculate the minimum length of both bitmasks
        int maxLength = Math.Min(oldStore.Length, newStore.Length);

        // Iterate through each byte in the bitmask arrays
        for (int byteIndex = 0; byteIndex < maxLength; byteIndex++)
        {
            // Get the difference between new and old byte (new byte with old byte masked out)
            byte difference = (byte)(newStore[byteIndex] & ~oldStore[byteIndex]);

            // If there is any difference, find the fish index
            if (difference != 0)
            {
                // Use a fast bit scan to find the first bit set in 'difference'
                int bitIndex = BitOperations.TrailingZeroCount(difference);

                // Return the global fish index by combining byte and bit indices
                return (uint)(byteIndex * 8 + bitIndex);
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
            data.GetDate(AcquirableDateType.Fishing).SetDate(fishOutcome.Value, DateTime.Now, AcquiredDateType.Manual);
            PluginHandlers.PluginLog.Information($"Found new fish caught with ID: {fishOutcome.Value}");
        }

        uint? spfishOutcome = CheckFishies(ref _spearFishStore, PlayerState.Instance()->CaughtSpearfishBitmask);
        if (spfishOutcome != null)
        {
            spfishOutcome += SpearFishIdOffset;
            data.GetDate(AcquirableDateType.Fishing).SetDate(spfishOutcome.Value, DateTime.Now, AcquiredDateType.Manual);
            PluginHandlers.PluginLog.Information($"Found new spearfish caught with ID: {spfishOutcome.Value}");
        }
    }

    public override void Init() { }     // Init is unused
    public override void Dispose() { }  // Dispose is unsued
}
