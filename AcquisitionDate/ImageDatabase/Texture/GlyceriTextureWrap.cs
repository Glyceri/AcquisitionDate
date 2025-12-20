using AcquisitionDate.ImageDatabase.Interfaces;
using AcquisitionDate.ImageDatabase.Structs;
using Dalamud.Interface.Textures.TextureWraps;
using System;

namespace AcquisitionDate.ImageDatabase.Texture;

internal class GlyceriTextureWrap : IGlyceriTextureWrap
{
    // After 5 seconds a texture is OLD
    const float TimeToOld = 5.0f;

    IDalamudTextureWrap? textureWrap = null;

    public IDalamudTextureWrap? TextureWrap
    {
        get => textureWrap;
        set
        {
            textureWrap?.Dispose();
            textureWrap = value;
        }
    }
    public bool IsOld { get; private set; } = false;
    public TextureRequestUser User { get; }

    DateTime timeSinceLastAccess = DateTime.Now;

    public GlyceriTextureWrap(TextureRequestUser user)
    {
        User = user;
    }

    public GlyceriTextureWrap(IDalamudTextureWrap? textureWrap, TextureRequestUser user)
    {
        this.textureWrap = textureWrap;
        User = user;
    }

    public void Dispose()
    {
        TextureWrap?.Dispose();
        TextureWrap = null;
    }

    public void Update()
    {
        TimeSpan timeSpan = DateTime.Now - timeSinceLastAccess;
        if (timeSpan.TotalSeconds < TimeToOld) return;

        IsOld = true;
    }

    public void Refresh()
    {
        timeSinceLastAccess = DateTime.Now;
    }
}

