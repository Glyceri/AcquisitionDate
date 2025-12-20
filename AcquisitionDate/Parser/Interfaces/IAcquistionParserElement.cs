using HtmlAgilityPack;
using System;

namespace AcquisitionDate.Parser.Interfaces;

internal interface IAcquistionParserElement<T>
{
    void Parse(HtmlNode rootNode, Action<T> onSuccess, Action<Exception> onFailure);
}
