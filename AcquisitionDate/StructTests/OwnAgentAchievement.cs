using System.Runtime.InteropServices;

namespace AcquisitionDate.StructTests;

[StructLayout(LayoutKind.Explicit, Size = 75584)]
internal unsafe struct OwnAgentAchievement
{
    [FieldOffset(80)]
    public uint SelectedSubmenu;

    [FieldOffset(104)]
    public uint SelectedAchievementListing;

    [FieldOffset(108)]
    public uint SelectedRightClickedAchievement;
}
