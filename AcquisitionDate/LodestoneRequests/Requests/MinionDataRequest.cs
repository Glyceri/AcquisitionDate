using AcquisitionDate.LodestoneData;
using AcquisitionDate.LodestoneRequests.Requests.Abstractions.PageList;
using AcquisitionDate.Services.Interfaces;
using System;

namespace AcquisitionDate.LodestoneRequests.Requests;

internal class MinionDataRequest : ItemPageDataRequest
{
    public MinionDataRequest(ISheets sheets, string baseURL, Action<ItemData> onItemData, Action? successCallback = null, Action<Exception>? failureCallback = null) : base(sheets, baseURL, "minion", onItemData, successCallback, failureCallback) { }

    protected override uint? GetIDFromString(string itemName) => (uint)(Sheets.GetCompanionByName(itemName)?.Model ?? 0);
}
