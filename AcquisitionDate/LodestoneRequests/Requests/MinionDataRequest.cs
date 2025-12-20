using AcquisitionDate.LodestoneData;
using AcquisitionDate.LodestoneNetworking.Enums;
using AcquisitionDate.LodestoneRequests.Requests.Abstractions.PageList;
using AcquisitionDate.Parser.Elements;
using AcquisitionDate.Services.Interfaces;
using System;

namespace AcquisitionDate.LodestoneRequests.Requests;

internal class MinionDataRequest : ItemPageDataRequest
{
    public MinionDataRequest(ItemPageDataParser itemPageDataParser, ISheets sheets, string baseURL, LodestoneRegion pageRegion, Action<ItemData> onItemData, Action? successCallback = null, Action<Exception>? failureCallback = null) : base(itemPageDataParser, sheets, baseURL, "minion", pageRegion, onItemData, successCallback, failureCallback) { }

    protected override uint? GetIDFromString(string itemName) => (uint)(Sheets.GetCompanionByName(itemName)?.ID ?? 0);
}
