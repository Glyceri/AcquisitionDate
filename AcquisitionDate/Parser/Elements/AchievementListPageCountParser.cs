using AcquisitionDate.HtmlParser;
using AcquisitionDate.Parser.Interfaces;
using HtmlAgilityPack;
using System;

namespace AcquisitionDate.Parser.Elements;

internal class AchievementListPageCountParser : IAcquistionParserElement<int>
{
    readonly ListPageCountParser PageCountParser;

    public AchievementListPageCountParser(ListPageCountParser pageCountParser)
    {
        PageCountParser = pageCountParser;
    }

    public void Parse(HtmlNode rootNode, Action<int> onSuccess, Action<Exception> onFailure)
    {
        HtmlNode? listNode = HtmlParserHelper.GetNode(rootNode, "ldst__achievement");
        if (listNode == null)
        {
            onFailure?.Invoke(new Exception("ldst__achievement was NOT found!"));
            return;
        }

        HtmlNode? btnPager = HtmlParserHelper.GetNode(listNode, "btn__pager");
        if (btnPager == null)
        {
            onFailure?.Invoke(new Exception("btn__pager was NOT found!"));
            return;
        }

        HtmlNode? btnPagerCurrent = HtmlParserHelper.GetNode(btnPager, "btn__pager__current");
        if (btnPagerCurrent == null)
        {
            onFailure?.Invoke(new Exception("btn__pager__current was NOT found!"));
            return;
        }

        PageCountParser?.Parse(btnPagerCurrent, onSuccess, onFailure);
    }
}
