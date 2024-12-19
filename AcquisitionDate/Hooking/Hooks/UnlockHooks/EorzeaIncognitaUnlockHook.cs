using AcquisitionDate.Core.Handlers;
using AcquisitionDate.Database.Enums;
using AcquisitionDate.DatableUsers.Interfaces;
using AcquisitionDate.Services.Interfaces;
using Dalamud.Hooking;
using Dalamud.Utility.Signatures;
using System;

namespace AcquisitionDate.Hooking.Hooks.UnlockHooks;

internal unsafe class EorzeaIncognitaUnlockHook : UnlockHook
{
    delegate IntPtr VistaUnlockedDelegate(ushort index, int a2, int a3);

    [Signature("E8 ?? ?? ?? ?? E9 ?? ?? ?? ?? 8B 4C 24 70 E8", DetourName = nameof(OnVistaUnlockedDetour))]
    readonly Hook<VistaUnlockedDelegate>? VistaUnlockHook;

    public EorzeaIncognitaUnlockHook(IUserList userList, ISheets sheets) : base(userList, sheets) { }

    public override void Init()
    {
        VistaUnlockHook?.Enable();
    }

    IntPtr OnVistaUnlockedDetour(ushort index, int a2, int a3)
    {
        PluginHandlers.PluginLog.Verbose($"Detected a vista unlocked at index: {index}");

        UserList.ActiveUser?.Data.SightList.SetDate(index, DateTime.Now, AcquiredDateType.Manual);

        return VistaUnlockHook!.Original(index, a2, a3);
    }

    public override void Dispose()
    {
        VistaUnlockHook?.Dispose();
    }

    public override void Reset() { }    // No Reset needed
    public override void Update(float deltaTime) { }    // No Update needed
}
