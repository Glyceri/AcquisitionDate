using System.Collections.Generic;

namespace AcquisitionDate.Parser.Interfaces;

internal interface IItemPageListParser<T> : IAcquistionParserElement<T>
{
    void SetListIconName(string listIconName);
}
