using AcquisitionDate.Database.Interfaces;
using AcquisitionDate.Updating.Interfaces;
using Dalamud.Interface.Textures.TextureWraps;
using System;

namespace AcquisitionDate.ImageDatabase.Interfaces;

internal interface IImageDatabase : IDisposable, IUpdatable
{
    bool IsDirty { get; }

    void Redownload(IDatableData entry);
    IDalamudTextureWrap? GetWrapFor(IDatableData? databaseEntry);
    bool IsBeingDownloaded(IDatableData? databaseEntry);

    IGlyceriTextureWrap? RegisterWrap(IDatableData data, IDalamudTextureWrap textureWrap);
}
