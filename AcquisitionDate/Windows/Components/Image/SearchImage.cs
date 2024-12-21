using Dalamud.Interface.Textures;
using Dalamud.Interface.Textures.TextureWraps;
using System.Numerics;
using AcquisitionDate.Core.Handlers;

namespace AcquistionDate.PetNicknames.Windowing.Components.Image;

internal static class SearchImage
{
    static ISharedImmediateTexture? SearchTexture;

    public static IDalamudTextureWrap? SearchTextureWrap => SearchTexture?.GetWrapOrEmpty();

    public static void Constructor()
    {
        SearchTexture = PluginHandlers.TextureProvider.GetFromGameIcon(66310);
    }

    public static void Draw(Vector2 size)
    {
        if (SearchTexture == null) return;

        IconImage.Draw(SearchTexture.GetWrapOrEmpty(), size);
    }
}
