using AcquisitionDate.Database.Interfaces;
using AcquisitionDate.LodestoneData;
using AcquisitionDate.LodestoneRequests.Requests.Abstractions.PageList;
using System;

namespace AcquisitionDate.LodestoneRequests.Requests;

internal class MinionPageListRequest : ItemPageListRequest
{
    public MinionPageListRequest(IDatableData data, Action<PageURLListData?> pageUrlCallback) : base(data, pageUrlCallback, "minion") { }  
}
