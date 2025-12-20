using AcquisitionDate.Database.Interfaces;
using AcquisitionDate.ImageDatabase.Interfaces;
using AcquisitionDate.LodestoneNetworking.Interfaces;
using AcquisitionDate.LodestoneRequests.Requests.Abstractions;
using AcquisitionDate.Parser.Elements;

namespace AcquisitionDate.LodestoneRequests.Requests;

internal class LodestoneProfilePictureRequest : PictureRequest
{
    readonly IImageDatabase ImageDatabase;

    public LodestoneProfilePictureRequest(IDatableData data, IImageReader imageReader, IImageDatabase imageDatabase, LodestoneIDParser lodestoneIDParser, INetworkClient networkClient) : base(imageReader, lodestoneIDParser, data, networkClient)
    {
        ImageDatabase = imageDatabase;

        if (!data.IsReady)
        {
            ShouldError = true;
            return;
        }
    }

    protected override void OnImageRead(string atFile)
    {
        ImageReader.ReadImage(Data, ImageDatabase, HandleFailure);
    }
}
