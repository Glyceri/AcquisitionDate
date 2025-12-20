using AcquisitionDate.Database.Interfaces;
using AcquisitionDate.ImageDatabase.Interfaces;
using AcquisitionDate.LodestoneNetworking.Interfaces;
using AcquisitionDate.Parser.Elements;
using AcquisitionDate.Parser.Structs;
using Dalamud.Utility;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace AcquisitionDate.LodestoneRequests.Requests.Abstractions;

internal abstract class PictureRequest : LodestoneIDRequest
{
    protected readonly IImageReader ImageReader;
    string FilePath = string.Empty;

    readonly INetworkClient NetworkClient;

    string lodestoneURL = string.Empty;

    public PictureRequest(IImageReader imageReader, LodestoneIDParser lodestoneIDParser, IDatableData datableData, INetworkClient networkClient) : base (lodestoneIDParser, datableData)
    {
        ImageReader = imageReader;
        FilePath = ImageReader.GetFilePath(Data);
        NetworkClient = networkClient;

        TickCount = 2;
    }

    protected override void OnData(LodestoneParseData lodestoneData)
    {
        lodestoneURL = lodestoneData.IconURL;
    }

    public override void OnTick(int tick, Action callback)
    {
        base.OnTick(tick, callback);

        _ = NetworkClient.SendRequest(lodestoneURL, OnResponse, HandleFailure);
    }

    void OnResponse(HttpResponseMessage response)
    {
        Task.Run(async () => await ReadTexture(response.Content), CancellationToken).ConfigureAwait(false);
    }

    async Task ReadTexture(HttpContent content)
    {
        // Thank DarkArchon for this code :D
        try
        {
            if (FilePath.IsNullOrWhitespace()) throw new Exception("User not found!");

            FileStream fileStream = File.Create(FilePath);
            await using (fileStream.ConfigureAwait(false))
            {
                int bufferSize = content.Headers.ContentLength > 1024 * 1024 ? 4096 : 1024;
                byte[] buffer = new byte[bufferSize];

                int bytesRead = 0;
                while ((bytesRead = await (await content.ReadAsStreamAsync(CancellationToken).ConfigureAwait(false)).ReadAsync(buffer, CancellationToken).ConfigureAwait(false)) > 0)
                {
                    await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead), CancellationToken).ConfigureAwait(false);
                }
            }

            OnImageRead(FilePath);
            SuccessCallbackTick();
        }
        catch (Exception ex)
        {
            HandleFailure(ex);
        }
    }

    protected abstract void OnImageRead(string atFile);
}
