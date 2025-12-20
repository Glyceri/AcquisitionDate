using AcquisitionDate.Core.Handlers;
using AcquisitionDate.Database.Interfaces;
using AcquisitionDate.ImageDatabase.Interfaces;
using AcquisitionDate.ImageDatabase.Structs;
using AcquisitionDate.ImageDatabase.Texture;
using AcquisitionDate.LodestoneNetworking.Interfaces;
using AcquisitionDate.LodestoneRequests.Requests;
using AcquisitionDate.Parser.Elements;
using Dalamud.Interface.Textures.TextureWraps;
using System;
using System.Collections.Generic;
using System.IO;

namespace AcquisitionDate.ImageDatabase;

internal class ImageDatabase : IImageDatabase
{
    public bool IsDirty { get; private set; }

    readonly List<IGlyceriTextureWrap> _imageDatabase = new List<IGlyceriTextureWrap>();
    readonly List<TextureRequestUser> _downloadingUsers = new List<TextureRequestUser>();

    readonly IDalamudTextureWrap SearchTexture;

    readonly ILodestoneNetworker LodestoneNetworker;
    readonly IImageReader ImageReader;
    readonly LodestoneIDParser IDParser;
    readonly INetworkClient NetworkClient;

    public ImageDatabase(ILodestoneNetworker lodestoneNetworker, IImageReader imageReader, LodestoneIDParser parser, INetworkClient networkClient)
    {
        SearchTexture = PluginHandlers.TextureProvider.GetFromGameIcon(66310).RentAsync().Result;

        ImageReader = imageReader;
        LodestoneNetworker = lodestoneNetworker;
        IDParser = parser;
        NetworkClient = networkClient;
    }

    public IDalamudTextureWrap? GetWrapFor(IDatableData? databaseEntry)
    {
        if (databaseEntry == null) return SearchTexture;

        lock (_imageDatabase)
        {
            TextureRequestUser user = new TextureRequestUser(databaseEntry.Name, databaseEntry.Homeworld);

            int length = _imageDatabase.Count;
            for (int i = 0; i < length; i++)
            {
                IGlyceriTextureWrap wrap = _imageDatabase[i];
                if (wrap.User.Equals(user))
                {
                    wrap.Refresh();
                    return wrap.TextureWrap ?? SearchTexture;
                }
            }

            _imageDatabase.Add(new GlyceriTextureWrap(user));
        }

        if (!ImageReader.ReadImage(databaseEntry, this, (exception) => PluginHandlers.PluginLog.Error(exception, "error in image read")))
        {
            LodestoneNetworker.AddElementToQueue(new LodestoneProfilePictureRequest(databaseEntry, ImageReader, this, IDParser, NetworkClient));
        }

        return SearchTexture;
    }

    public bool IsBeingDownloaded(IDatableData? databaseEntry)
    {
        IDalamudTextureWrap? wrap = GetWrapFor(databaseEntry);
        if (wrap == null) return false;

        bool wrapIsSearch = SearchTexture == wrap;

        return wrapIsSearch;
    }

    public IGlyceriTextureWrap? RegisterWrap(IDatableData data, IDalamudTextureWrap textureWrap)
    {
        lock (_imageDatabase)
        {
            TextureRequestUser user = new TextureRequestUser(data.Name, data.Homeworld);

            int length = _imageDatabase.Count;
            for (int i = 0; i < length; i++)
            {
                IGlyceriTextureWrap wrap = _imageDatabase[i];
                if (!wrap.User.Equals(user)) continue;

                wrap.TextureWrap = textureWrap;
                return wrap;
            }
        }

        return null;
    }

    public void Redownload(IDatableData entry)
    {
        if (IsBeingDownloaded(entry)) return;

        lock (_imageDatabase)
        {
            TextureRequestUser user = new TextureRequestUser(entry.Name, entry.Homeworld);

            int length = _imageDatabase.Count;
            for (int i = 0; i < length; i++)
            {
                IGlyceriTextureWrap wrap = _imageDatabase[i];
                if (!wrap.User.Equals(user)) continue;

                wrap?.Dispose();
                _imageDatabase.RemoveAt(i);
                break;
            }
        }

        string filePath = ImageReader.GetFilePath(entry);

        try
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            _ = GetWrapFor(entry);
        }
        catch (Exception ex)
        {
            PluginHandlers.PluginLog.Error(ex, "Failure in delete");
        }
    }


    public void Update(float deltaTime)
    {
        int length = _imageDatabase.Count;
        for (int i = length - 1; i >= 0; i--)
        {
            IGlyceriTextureWrap wrap = _imageDatabase[i];

            wrap.Update();

            if (!wrap.IsOld) continue;

            wrap.Dispose();
            _imageDatabase.RemoveAt(i);
        }
    }

    public void Dispose()
    {
        SearchTexture?.Dispose();

        lock (_imageDatabase)
        {
            foreach (IGlyceriTextureWrap? wrap in _imageDatabase)
            {
                wrap?.Dispose();
            }

            _imageDatabase.Clear();
        }
    }
}
