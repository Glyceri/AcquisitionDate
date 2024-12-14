using AcquisitionDate.LodestoneNetworking.Enums;

namespace AcquisitionDate.LodestoneNetworking.LodestoneStringBuilder;

internal static class LodestoneStringBuiler
{
    static string GetBaseString(LodestoneRegion lodestoneRegion)
    {
        string prefix = string.Empty;

        prefix = lodestoneRegion switch
        {
            LodestoneRegion.Europe => "eu",
            LodestoneRegion.America => "na",
            LodestoneRegion.Japan => "jp",
            LodestoneRegion.Germany => "de",
            LodestoneRegion.France => "fr",
            _ => "eu"
        };

        return $"https://{prefix}.finalfantasyxiv.com/";
    }

    static string GetBaseLodestoneString(LodestoneRegion lodestoneRegion)
    {
        string baseString = GetBaseString(lodestoneRegion);

        return $"{baseString}lodestone/";
    }
}
