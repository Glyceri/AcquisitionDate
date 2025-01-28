using AcquisitionDate.Core.Handlers;
using AcquisitionDate.Database.Interfaces;
using AcquisitionDate.LodestoneData;
using System;

namespace AcquisitionDate.LodestoneRequests.Requests.Abstractions.PageCount;

internal abstract class PageCountRequest : CharacterRequest
{
    readonly Action<PageCountData?> PageCountCallback;

    public PageCountRequest(IDatableData data, Action<PageCountData?> pageCountCallback) : base(data)
    {
        PageCountCallback = pageCountCallback;
    }

    public sealed override void HandleFailure(Exception exception)
    {
        PluginHandlers.PluginLog.Error(exception, "An Error Occurred during a Page Count Request.");
        PageCountCallback?.Invoke(null);
    }

    protected void CallbackOutcome(int count) => PageCountCallback?.Invoke(new PageCountData(count));
}
