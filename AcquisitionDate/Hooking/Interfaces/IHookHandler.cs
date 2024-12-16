using AcquisitionDate.Hooking.Hooks.Interfaces;
using System;

namespace AcquisitionDate.Hooking.Interfaces;

internal interface IHookHandler : IDisposable
{
    IUnlocksHook UnlocksHook { get; }
}
