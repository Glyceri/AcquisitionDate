namespace AcquisitionDate.LodestoneRequests.Requests.Abstractions;

internal abstract class CharacterRequest : LodestoneRequest
{
    protected readonly int LodestoneCharacterID;

    public CharacterRequest(int lodestoneCharacterID)
    {
        LodestoneCharacterID = lodestoneCharacterID;
    }

    public override string GetURL() => $"/lodestone/character/{LodestoneCharacterID}/";
}
