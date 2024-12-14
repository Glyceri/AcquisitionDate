using AcquisitionDate.Database.Interfaces;
using AcquisitionDate.HtmlParser;
using AcquisitionDate.LodestoneData;
using HtmlAgilityPack;
using System;

namespace AcquisitionDate.LodestoneRequests.Requests.Abstractions.PageCount;

internal abstract class AchievementTypePageCountRequest : ListPageCountRequest
{
    public AchievementTypePageCountRequest(IDatableData data, Action<PageCountData?> pageCountCallback) : base(data, pageCountCallback) { }

    protected override int Outcome { get; set; } = 0;

    protected override bool OnSuccess(HtmlDocument document)
    {
        HtmlNode rootNode = document.DocumentNode;

        HtmlNode? listNode = HtmlParserHelper.GetNode(rootNode, "ldst__achievement");
        if (listNode == null) return false;

        HtmlNode? btnPager = HtmlParserHelper.GetNode(listNode, "btn__pager");
        if (btnPager == null) return false;

        HtmlNode? btnPagerCurrent = HtmlParserHelper.GetNode(btnPager, "btn__pager__current");
        if (btnPagerCurrent == null) return false;

        int? pageCount = GetPageCount(btnPagerCurrent);
        if (pageCount == null) return false;

        Outcome = pageCount.Value;
        return true;
    }
}
