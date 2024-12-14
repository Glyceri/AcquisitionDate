using AcquisitionDate.Database.Interfaces;

namespace AcquisitionDate.LodestoneRequests.Requests.Abstractions;

internal abstract class CharacterRequest : LodestoneRequest
{
    protected readonly ulong? LodestoneCharacterID;

    public CharacterRequest(IDatableData data)
    {
        if (!data.IsReady)
        {
            ShouldError = true;
            return;
        }

        LodestoneCharacterID = data.LodestoneID;
    }

    public override string GetURL() => $"/lodestone/character/{LodestoneCharacterID}/";
}
