using AcquisitionDate.Database.Interfaces;
using AcquisitionDate.LodestoneData;
using AcquisitionDate.LodestoneRequests.Requests.Abstractions.PageList;
using System;

namespace AcquisitionDate.LodestoneRequests.Requests;

internal class FacewearPageListRequest : ItemPageListRequest
{
    public FacewearPageListRequest(IDatableData data, Action<PageURLListData?> pageUrlCallback) : base(data, pageUrlCallback, "faceaccessory") { }
}
