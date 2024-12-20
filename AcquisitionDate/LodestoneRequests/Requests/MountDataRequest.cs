using AcquisitionDate.LodestoneData;
using AcquisitionDate.LodestoneRequests.Requests.Abstractions.PageList;
using AcquisitionDate.Services.Interfaces;
using System;

namespace AcquisitionDate.LodestoneRequests.Requests;

internal class MountDataRequest : ItemPageDataRequest
{
    public MountDataRequest(ISheets sheets, string baseURL, Action<ItemData> onItemData, Action? successCallback = null, Action<Exception>? failureCallback = null) : base(sheets, baseURL, "mount", onItemData, successCallback, failureCallback) { }

    protected override uint? GetIDFromString(string itemName) => Sheets.GetMountByName(itemName)?.RowId ?? 0;
}
