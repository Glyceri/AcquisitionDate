using AcquisitionDate.Database.Interfaces;
using AcquisitionDate.LodestoneData;
using AcquisitionDate.LodestoneRequests.Requests.Abstractions.PageList;
using System;

namespace AcquisitionDate.LodestoneRequests.Requests;

internal class MountPageListRequest : ItemPageListRequest
{
    public MountPageListRequest(ItemPageListParser itemPageListParser, IDatableData data, Action<PageURLListData?> pageUrlCallback) : base(itemPageListParser, data, pageUrlCallback, "mount") { }
}
