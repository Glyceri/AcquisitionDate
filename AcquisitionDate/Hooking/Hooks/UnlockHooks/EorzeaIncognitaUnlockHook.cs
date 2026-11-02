using AcquisitionDate.Database.Enums;
using AcquisitionDate.Services.Interfaces;
using Dalamud.Hooking;
using Dalamud.Utility.Signatures;
using System;
using AcquisitionDate.Database.Interfaces;

namespace AcquisitionDate.Hooking.Hooks.UnlockHooks;

internal class EorzeaIncognitaUnlockHook : UnlockHook
{
    private delegate void VistaUnlockedDelegate(ushort index, int a2, int a3);

    [Signature("E8 ?? ?? ?? ?? E9 ?? ?? ?? ?? 8B 4C 24 70 E8", DetourName = nameof(OnVistaUnlockedDetour))]
    private readonly Hook<VistaUnlockedDelegate>? VistaUnlockHook;

    public EorzeaIncognitaUnlockHook(IDatabase database, ISheets sheets) 
        : base(database, sheets)
        { }

    public override void Init()
    {
        VistaUnlockHook?.Enable();
    }
    
    public override void Dispose()
    {
        VistaUnlockHook?.Dispose();
    }
    
    private void OnVistaUnlockedDetour(ushort index, int a2, int a3)
    {
        PluginHandlers.PluginLog.Information($"Detected a vista unlocked at index: {index}");

        Database.GetLocalUser()?.GetDateList(AcquirableDateType.Incognita).SetDate(index, DateTime.Now, AcquiredDateType.Manual);

        VistaUnlockHook!.Original(index, a2, a3);
    }
    
    public override void Reset() // No Reset needed
        { }    
    
    public override void Update(float deltaTime) // No Update needed
        { }    
}
