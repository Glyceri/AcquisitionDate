using AcquistionDate.PetNicknames.Windowing.Components.Image;
using AcquistionDate.PetNicknames.Windowing.Components.Image.UldHelpers;

namespace AcquistionDate.PetNicknames.Windowing.Components;

internal static class ComponentLibrary
{
    public static void Initialise()
    {
        SearchImage.Constructor();
        RaceIconHelper.Constructor();
    }

    public static void Dispose()
    {
        RaceIconHelper.Dispose();
    }
}
