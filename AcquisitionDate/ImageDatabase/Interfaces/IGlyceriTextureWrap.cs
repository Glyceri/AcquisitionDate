using AcquisitionDate.ImageDatabase.Structs;
using Dalamud.Interface.Textures.TextureWraps;
using System;

namespace AcquisitionDate.ImageDatabase.Interfaces;

internal interface IGlyceriTextureWrap : IDisposable
{
    IDalamudTextureWrap? TextureWrap { get; set; }
    TextureRequestUser User { get; }

    bool IsOld { get; }

    void Update();
    void Refresh();
}

