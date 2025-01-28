using AcquisitionDate.Parser.Interfaces;
using Dalamud.Utility;
using HtmlAgilityPack;
using System;
using System.Text.RegularExpressions;
using System.Web;

namespace AcquisitionDate.Parser.Elements;

internal class ListPageCountParser : IAcquistionParserElement<int>
{
    static readonly Regex pageCountRegex = new Regex(@"(?:von|\/|of|ページ)\s*(\d+)", RegexOptions.Compiled);

    public void Parse(HtmlNode rootNode, Action<int> onSuccess, Action<Exception> onFailure)
    {
        string innerText = rootNode.InnerText;

        string pageText = HttpUtility.HtmlDecode(innerText);
        if (pageText.IsNullOrWhitespace())
        {
            onFailure.Invoke(new Exception("innerText is empty"));
            return;
        }

        Match match = pageCountRegex.Match(pageText);
        if (!match.Success)
        {
            onFailure.Invoke(new Exception("Page count regex had no match!"));
            return;
        }
        if (match.Groups.Count < 2)
        {
            onFailure.Invoke(new Exception("Not enough regex groups found!"));
            return;
        }

        string pageCount = match.Groups[1].Value;
        if (pageCount.IsNullOrWhitespace())
        {
            onFailure.Invoke(new Exception("pageCount is null or whitespace!"));
            return;
        }

        if (!int.TryParse(pageCount, out int pageCountValue))
        {
            onFailure.Invoke(new Exception("pageCount failed to parse to int"));
            return;
        }

        onSuccess?.Invoke(pageCountValue);
    }
}
