using AcquisitionDate.LodestoneNetworking.Enums;
using System;

namespace AcquisitionDate.Parser.Interfaces;

internal interface IItemPageDataParser<T> : IItemPageListParser<T>
{
    void SetPageLanguage(LodestoneRegion region);
    void SetGetIDFunc(Func<string, uint?> getIDFromString);
}
