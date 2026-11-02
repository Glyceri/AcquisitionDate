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
    
    private readonly string             BaseURL;
    private readonly string             BaseName;
    private readonly Action?            SuccessCallback;
    private readonly Action<Exception>? FailureCallback;
    private readonly Action<ItemData>   OnItemData;
    private readonly ItemPageDataParser ItemDataParser;
    private readonly LodestoneRegion    PageRegion;

    public ItemPageDataRequest(ItemPageDataParser itemPageDataParser, ISheets sheets, string baseURL, string baseName, LodestoneRegion pageRegion, Action<ItemData> onItemData, Action? successCallback = null, Action<Exception>? failureCallback = null)
    {
        ItemDataParser  = itemPageDataParser;
        Sheets          = sheets;
        BaseURL         = baseURL;
        BaseName        = baseName;
        PageRegion      = pageRegion;
        OnItemData      = onItemData;
        SuccessCallback = successCallback;
        FailureCallback = failureCallback;
    }

    public override string GetURL() 
        => BaseURL;
    
    public sealed override void HandleFailure(Exception exception) 
        => FailureCallback?.Invoke(exception);

    public override void HandleSuccess(HtmlDocument document)
    {
        SuccessCallback?.Invoke();

        HtmlNode rootNode = document.DocumentNode;

        ItemDataParser.SetListIconName(BaseName);
        ItemDataParser.SetGetIDFunc(GetIdFromString);
        ItemDataParser.SetPageLanguage(PageRegion);
        ItemDataParser.Parse(rootNode, OnSuccess, PrintFailure);
    }

    private void PrintFailure(Exception e)
        => PluginHandlers.PluginLog.Error(e, "Error in ItemPageDataRequest");
    
    private void OnSuccess(ItemData item) 
        => PluginHandlers.Framework.Run(() => OnItemData?.Invoke(item));
    
    protected abstract uint? GetIdFromString(string itemName);
}
