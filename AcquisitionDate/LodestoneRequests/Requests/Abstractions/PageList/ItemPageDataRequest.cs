using AcquisitionDate.Core.Handlers;
using AcquisitionDate.LodestoneData;
using AcquisitionDate.LodestoneNetworking.Enums;
using AcquisitionDate.Parser.Elements;
using AcquisitionDate.Services.Interfaces;
using HtmlAgilityPack;
using System;

namespace AcquisitionDate.LodestoneRequests.Requests.Abstractions.PageList;

internal abstract class ItemPageDataRequest : LodestoneRequest
{
    protected readonly ISheets Sheets;
    readonly string BaseURL;
    readonly string BaseName;
    readonly Action? SuccessCallback;
    readonly Action<Exception>? FailureCallback;
    readonly Action<ItemData> OnItemData;
    readonly ItemPageDataParser ItemDataParser;
    readonly LodestoneRegion PageRegion;

    public ItemPageDataRequest(ItemPageDataParser itemPageDataParser, ISheets sheets, string baseURL, string baseName, LodestoneRegion pageRegion, Action<ItemData> onItemData, Action? successCallback = null, Action<Exception>? failureCallback = null)
    {
        ItemDataParser = itemPageDataParser;
        Sheets = sheets;
        BaseURL = baseURL;
        BaseName = baseName;
        PageRegion = pageRegion;
        OnItemData = onItemData;
        SuccessCallback = successCallback;
        FailureCallback = failureCallback;
    }

    public sealed override void HandleFailure(Exception exception) => FailureCallback?.Invoke(exception);

    public override void HandleSuccess(HtmlDocument document)
    {
        SuccessCallback?.Invoke();

        HtmlNode rootNode = document.DocumentNode;

        ItemDataParser.SetListIconName(BaseName);
        ItemDataParser.SetGetIDFunc(GetIDFromString);
        ItemDataParser.SetPageLanguage(PageRegion);
        ItemDataParser.Parse(rootNode, OnSuccess, PrintFailure);
    }

    void PrintFailure(Exception e) => PluginHandlers.PluginLog.Error(e, "Error in ItemPageDataRequest");
    void OnSuccess(ItemData item) => PluginHandlers.Framework.Run(() => OnItemData?.Invoke(item));

    protected abstract uint? GetIDFromString(string itemName);

    public override string GetURL() => BaseURL;
}
