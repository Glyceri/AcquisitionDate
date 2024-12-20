using AcquisitionDate.LodestoneData;
using AcquisitionDate.LodestoneRequests.Requests.Abstractions.PageList;
using AcquisitionDate.Services.Interfaces;
using System;

namespace AcquisitionDate.LodestoneRequests.Requests;

internal class FacewearDataRequest : ItemPageDataRequest
{
    public FacewearDataRequest(ISheets sheets, string baseURL, Action<ItemData> onItemData, Action? successCallback = null, Action<Exception>? failureCallback = null) : base(sheets, baseURL, "faceaccessory", onItemData, successCallback, failureCallback) { }

    protected override uint? GetIDFromString(string itemName) => (uint)(Sheets.GetGlassesByName(itemName)?.Model ?? 0);
}
