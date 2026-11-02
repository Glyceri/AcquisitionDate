using AcquisitionDate.Database.Enums;
using AcquisitionDate.Database.Interfaces;
using AcquisitionDate.Services.Interfaces;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using System;
using System.Numerics;

namespace AcquisitionDate.Hooking.Hooks.UnlockHooks;

internal unsafe class FishyUnlockHook : UnlockHook
{
    public const uint SpearFishIdOffset = 20000;

    private byte[] _fishStore       = [];
    private byte[] _spearFishStore  = [];
    private byte[] _fishZoneStore   = [];

    public FishyUnlockHook(IDatabase database, ISheets sheets) 
        : base(database, sheets) 
        { }

    public override void Reset() 
    {
        _fishZoneStore  = PlayerState.Instance()->UnlockedFishingSpots.ToArray();
        _fishStore      = PlayerState.Instance()->CaughtFish.ToArray();
        _spearFishStore = PlayerState.Instance()->CaughtSpearfish.ToArray();
    }
    
    private uint? GetCaughtFishIndices(Span<byte> oldStore, Span<byte> newStore)
    {
        // Calculate the minimum length of both bitmasks
        int maxLength = Math.Min(oldStore.Length, newStore.Length);

        // Iterate through each byte in the bitmask arrays
        for (int byteIndex = 0; byteIndex < maxLength; byteIndex++)
        {
            // Get the difference between new and old byte (new byte with old byte masked out)
            byte difference = (byte)(newStore[byteIndex] & ~oldStore[byteIndex]);

            // If there is any difference, find the fish index
            if (difference == 0)
            {
                continue;
            }
            
            // Use a fast bit scan to find the first bit set in 'difference'
            int bitIndex = BitOperations.TrailingZeroCount(difference);

            // Return the global fish index by combining byte and bit indices
            return (uint)(byteIndex * 8 + bitIndex);
        }

        return null;
    }

    private uint? CheckFishies(ref byte[] store, Span<byte> bitmask)
    {
        Span<byte> span = bitmask;

        bool fishyEquals = new Span<byte>(store, 0, store.Length).SequenceEqual(span);
        
        if (fishyEquals)
        {
            return null;
        }

        uint? outcome = GetCaughtFishIndices(store, span);

        store = span.ToArray();

        return outcome;
    }

    public override void Update(float deltaTime) 
    {
        IDatableData? data = Database.GetLocalUser();
        
        if (data == null)
        {
            return;
        }

        HandleFishy(data);
    }
    
    private void HandleFishy(IDatableData data)
    {
        HandleFishyStore(data, ref _fishStore,      PlayerState.Instance()->CaughtFish,           AcquirableDateType.Fishing);
        HandleFishyStore(data, ref _spearFishStore, PlayerState.Instance()->CaughtSpearfish,      AcquirableDateType.Fishing,     SpearFishIdOffset);
        HandleFishyStore(data, ref _fishZoneStore,  PlayerState.Instance()->UnlockedFishingSpots, AcquirableDateType.FishingSpot);
    }
    
    private void HandleFishyStore(IDatableData data, ref byte[] store, Span<byte> bitmask, AcquirableDateType dateType, uint offset = 0)
    {
        uint? outcome = CheckFishies(ref store, bitmask);
        
        if (outcome == null)
        {
            return;
        }
        
        outcome += offset;
        
        data.GetDateList(dateType).SetDate(outcome.Value, DateTime.Now, AcquiredDateType.Manual);
        
        PluginHandlers.PluginLog.Information($"Found new {dateType} with ID: {outcome.Value}");
    }

    public override void Init() // Init is unused
        { }     
    
    public override void Dispose()  // Dispose is unsued
        { } 
}
