using AcquisitionDate.Core.Handlers;
using AcquisitionDate.Database.Interfaces;
using AcquisitionDate.LodestoneData;
using System;

namespace AcquisitionDate.LodestoneRequests.Requests.Abstractions.PageList;

internal abstract class PageListRequest : CharacterRequest
{
    readonly Action<PageURLListData?> PageURLCountback;

    public PageListRequest(IDatableData data, Action<PageURLListData?> pageUrlCallback) : base(data)
    {
        PageURLCountback = pageUrlCallback;
    }

    public sealed override void HandleFailure(Exception exception)
    {
        PluginHandlers.PluginLog.Error(exception, "An Error Occurred during a Page Url List Request.");
        PageURLCountback?.Invoke(null);
    }

    protected void CallbackOutcome(string[] urls) => PageURLCountback?.Invoke(new PageURLListData(urls));
}
