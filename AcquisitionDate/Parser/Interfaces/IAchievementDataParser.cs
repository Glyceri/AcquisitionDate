using AcquisitionDate.LodestoneNetworking.Enums;

namespace AcquisitionDate.Parser.Interfaces;

internal interface IAchievementDataParser<T> : IAcquistionParserElement<T>
{
    void SetPageLanguage(LodestoneRegion lodestoneRegion);
}
