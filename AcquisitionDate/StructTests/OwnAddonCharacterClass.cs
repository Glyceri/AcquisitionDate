using System.Runtime.InteropServices;


namespace AcquisitionDate.StructTests;

[StructLayout(LayoutKind.Explicit, Size = 0x8A8)]
public unsafe struct OwnAddonCharacterClass
{
    [FieldOffset(0x8A0)] public int HoverIndex;
}
