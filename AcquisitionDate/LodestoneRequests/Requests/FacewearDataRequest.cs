using AcquisitionDate.LodestoneData;
using AcquisitionDate.LodestoneRequests.Requests.Abstractions.PageList;
using AcquisitionDate.Parser.Elements;
using AcquisitionDate.Services.Interfaces;
using System;

namespace AcquisitionDate.LodestoneRequests.Requests;

internal class FacewearDataRequest : ItemPageDataRequest
{
    public FacewearDataRequest(ItemPageDataParser itemPageDataParser, ISheets sheets, string baseURL, Action<ItemData> onItemData, Action? successCallback = null, Action<Exception>? failureCallback = null) : base(itemPageDataParser, sheets, baseURL, "faceaccessory", onItemData, successCallback, failureCallback) { }

    protected override uint? GetIDFromString(string itemName) => (uint)(Sheets.GetGlassesByName(itemName)?.Model ?? 0);
}
