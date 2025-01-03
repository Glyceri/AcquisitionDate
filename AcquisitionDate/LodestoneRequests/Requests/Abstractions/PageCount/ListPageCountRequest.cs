using AcquisitionDate.Database.Interfaces;
using AcquisitionDate.LodestoneData;
using Dalamud.Utility;
using HtmlAgilityPack;
using System;
using System.Text.RegularExpressions;

namespace AcquisitionDate.LodestoneRequests.Requests.Abstractions.PageCount;

internal abstract class ListPageCountRequest : PageCountRequest
{
    protected ListPageCountRequest(IDatableData data, Action<PageCountData?> pageCountCallback) : base(data, pageCountCallback) { }

    Regex regex = new Regex(@"(?:von|\/|of|ページ)\s*(\d+)");

    protected int? GetPageCount(HtmlNode pageCountNode)
    {
        string pageText = pageCountNode.InnerText;
        if (pageText.IsNullOrWhitespace()) return null;

        Match match = regex.Match(pageText);
        if (!match.Success) return null;
        if (match.Groups.Count < 2) return null;

        string pageCount = match.Groups[1].Value;
        if (pageCount.IsNullOrWhitespace()) return null;

        if (!int.TryParse(pageCount, out int pageCountValue)) return null;

        return pageCountValue;
    }
}
