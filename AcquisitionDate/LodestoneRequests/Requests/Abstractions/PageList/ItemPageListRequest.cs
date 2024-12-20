using AcquisitionDate.Database.Interfaces;
using AcquisitionDate.HtmlParser;
using AcquisitionDate.LodestoneData;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;

namespace AcquisitionDate.LodestoneRequests.Requests.Abstractions.PageList;

internal abstract class ItemPageListRequest : PageListRequest
{
    readonly string ListIconName;

    protected sealed override string[] Outcome { get; set; } = [];

    protected ItemPageListRequest(IDatableData data, Action<PageURLListData?> pageUrlCallback, string listIconName) : base(data, pageUrlCallback)
    {
        ListIconName = listIconName;
    }

    protected override bool OnSuccess(HtmlDocument document)
    {
        HtmlNode rootNode = document.DocumentNode;
        List<HtmlNode> nodes = HtmlParserHelper.GetNodes(rootNode, $"{ListIconName}__list_icon");

        List<string> hrefs = new List<string>();

        foreach (HtmlNode node in nodes)
        {
            string outcome = node.GetAttributeValue("data-tooltip_href", string.Empty);
            hrefs.Add(outcome); 
        }

        Outcome = hrefs.ToArray();

        return true;
    }

    public override string GetURL() => base.GetURL() + $"{ListIconName}/";
}
