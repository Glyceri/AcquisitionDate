using AcquisitionDate.Database.Interfaces;
using AcquisitionDate.LodestoneData;
using HtmlAgilityPack;
using System;

namespace AcquisitionDate.LodestoneRequests.Requests.Abstractions.PageList;

internal abstract class ItemPageListRequest : PageListRequest
{
    readonly string ListIconName;
    readonly ItemPageListParser ItemPageListParser;

    protected ItemPageListRequest(ItemPageListParser itemPageListParser, IDatableData data, Action<PageURLListData?> pageUrlCallback, string listIconName) : base(data, pageUrlCallback)
    {
        ItemPageListParser = itemPageListParser;
        ListIconName = listIconName;
    }

    public override void HandleSuccess(HtmlDocument document)
    {
        HtmlNode rootNode = document.DocumentNode;

        ItemPageListParser.SetListIconName(ListIconName);
        ItemPageListParser.Parse(rootNode, CallbackOutcome, HandleFailure);
    }

    public override string GetURL() => base.GetURL() + $"{ListIconName}/";
}
