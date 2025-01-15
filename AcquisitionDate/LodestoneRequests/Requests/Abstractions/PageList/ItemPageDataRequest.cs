using AcquisitionDate.Core.Handlers;
using AcquisitionDate.HtmlParser;
using AcquisitionDate.LodestoneData;
using AcquisitionDate.Services.Interfaces;
using HtmlAgilityPack;
using System;
using System.Web;

namespace AcquisitionDate.LodestoneRequests.Requests.Abstractions.PageList;

internal abstract class ItemPageDataRequest : LodestoneRequest
{
    protected readonly ISheets Sheets;
    readonly string BaseURL;
    readonly string BaseName;
    readonly Action? SuccessCallback;
    readonly Action<Exception>? FailureCallback;
    readonly Action<ItemData> OnItemData;

    public ItemPageDataRequest(ISheets sheets, string baseURL, string baseName, Action<ItemData> onItemData, Action? successCallback = null, Action<Exception>? failureCallback = null)
    {
        Sheets = sheets;
        BaseURL = baseURL;
        BaseName = baseName;
        OnItemData = onItemData;
        SuccessCallback = successCallback;
        FailureCallback = failureCallback;
    }

    public sealed override void HandleFailure(Exception exception) => FailureCallback?.Invoke(exception);

    public override void HandleSuccess(HtmlDocument document)
    {
        SuccessCallback?.Invoke();

        HtmlNode rootNode = document.DocumentNode;

        HtmlNode? labelNode = HtmlParserHelper.GetNode(rootNode, $"{BaseName}__header__label");
        if (labelNode == null) return;

        string itemName = HttpUtility.HtmlDecode(labelNode.InnerText);

        uint? itemID = GetIDFromString(itemName);
        if (itemID == null) return;

        HtmlNode? timeNode = HtmlParserHelper.GetNode(rootNode, $"{BaseName}__header__data");
        if (timeNode == null) return;
        if (timeNode.ChildNodes.Count == 0) return;

        DateTime? acquiredTime = HtmlParserHelper.GetAcquiredTime(timeNode);
        if (acquiredTime == null) return;

        PluginHandlers.Framework.Run(() => OnItemData?.Invoke(new ItemData(itemID.Value, acquiredTime.Value)));
    }

    protected abstract uint? GetIDFromString(string itemName);

    public override string GetURL() => BaseURL;
}
