using System.Runtime.InteropServices;

namespace AcquisitionDate.StructTests;

[StructLayout(LayoutKind.Explicit)]
internal struct OwnAgentOrchestrionInn
{
    [FieldOffset(0x774)] public uint rollID;
}
