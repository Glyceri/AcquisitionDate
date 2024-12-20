using System.Runtime.InteropServices;

namespace AcquisitionDate.StructTests;

[StructLayout(LayoutKind.Explicit, Size = 424)]
internal unsafe struct OwnAgentFishGuide
{
    [FieldOffset(80)] public int SelectedFishID;
}
