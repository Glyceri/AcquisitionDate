using System.Runtime.InteropServices;

namespace AcquisitionDate.Hooking.Structs;

[StructLayout(LayoutKind.Explicit)]
struct MyAgentCutsceneReplay
{
    [FieldOffset(56)]
    public byte JournalTypeCount;

    [FieldOffset(57)]
    public byte JournalTab;
    
    [FieldOffset(76)]
    public int ActiveQuest;
}