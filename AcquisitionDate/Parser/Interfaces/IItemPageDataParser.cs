using AcquisitionDate.LodestoneNetworking.Enums;
using System;

namespace AcquisitionDate.Parser.Interfaces;

internal interface IItemPageDataParser<T> : IItemPageListParser<T>, IAchievementDataParser<T>
{
    void SetGetIDFunc(Func<string, uint?> getIDFromString);
}
