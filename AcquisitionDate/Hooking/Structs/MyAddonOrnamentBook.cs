using System.Runtime.InteropServices;

namespace AcquisitionDate.Hooking.Structs;

[StructLayout(LayoutKind.Explicit)]
internal struct MyAddonOrnamentBook
{
    [FieldOffset(1056)]
    public byte SelectedFashionIndex;
}