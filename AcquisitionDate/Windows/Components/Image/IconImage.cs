using Dalamud.Interface.Textures.TextureWraps;
using Dalamud.Bindings.ImGui;
using System.Numerics;

namespace AcquisitionDate.Windows.Components.Image;

internal static class IconImage
{
    public static void Draw(IDalamudTextureWrap textureWrap, Vector2 size)
    {
        ImGui.Image(textureWrap.Handle, size);
    }
}
