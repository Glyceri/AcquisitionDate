using AcquisitionDate.Database.Interfaces;
using AcquisitionDate.LodestoneData;
using AcquisitionDate.LodestoneRequests.Requests.Abstractions.PageList;
using System;

namespace AcquisitionDate.LodestoneRequests.Requests;

internal class MinionPageListRequest : ItemPageListRequest
{
    public MinionPageListRequest(ItemPageListParser itemPageListParser, IDatableData data, Action<PageURLListData?> pageUrlCallback) : base(itemPageListParser, data, pageUrlCallback, "minion") { }  
}
