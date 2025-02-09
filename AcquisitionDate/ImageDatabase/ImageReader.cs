using AcquisitionDate.Database.Interfaces;
using AcquisitionDate.ImageDatabase.Interfaces;
using Dalamud.Interface.Textures.TextureWraps;
using Dalamud.Interface.Textures;
using System;
using System.Threading.Tasks;
using System.IO;
using AcquisitionDate.Core.Handlers;
using System.Threading;

namespace AcquisitionDate.ImageDatabase;

internal class ImageReader : IImageReader
{
    readonly CancellationTokenSource CancellationTokenSource;

    public ImageReader()
    {
        CancellationTokenSource = new CancellationTokenSource();
    }

    public bool ReadImage(IDatableData entry, IImageDatabase imageDatabase, Action<Exception> onFailure)
    {
        string filePath = GetFilePath(entry);

        try
        {
            if (File.Exists(filePath))
            {
                Task.Run(async () => await CreateImage(entry, imageDatabase, onFailure), CancellationTokenSource.Token).ConfigureAwait(false);
                return true;
            }
        }
        catch (Exception e)
        {
            onFailure?.Invoke(e);
        }

        return false;
    }

    async Task CreateImage(IDatableData entry, IImageDatabase imageDatabase, Action<Exception> onFailure)
    {
        try
        {
            ISharedImmediateTexture texture = PluginHandlers.TextureProvider.GetFromFile(GetFilePath(entry));
            if (texture == null)
            {
                onFailure.Invoke(new Exception("Texture couldnt load"));
                return;
            }
            IDalamudTextureWrap textureWrap = await texture.RentAsync();
            if (textureWrap == null)
            {
                onFailure.Invoke(new Exception("Wrap null"));
                return;
            }

            IGlyceriTextureWrap? gTextureWrap = imageDatabase.RegisterWrap(entry, textureWrap);
            if (gTextureWrap == null)
            {
                onFailure.Invoke(new Exception("IGlyceriTextureWrap null"));
                return;
            }
        }
        catch (Exception e)
        {
            onFailure.Invoke(e);
            return;
        }
    }

    public string GetFilePath(IDatableData entry) => GetFilePath(entry.Name, entry.Homeworld);
    public string GetFilePath(string name, ushort homeworld) => Path.Combine(Path.GetTempPath(), $"AcquisitionnDate_{name}_{homeworld}.jpg");

    public void Dispose()
    {
        CancellationTokenSource.Cancel();
        CancellationTokenSource.Dispose();
    }
}
