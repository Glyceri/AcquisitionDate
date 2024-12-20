using AcquisitionDate.Core.Handlers;
using AcquisitionDate.Database.Interfaces;
using AcquisitionDate.LodestoneData;
using HtmlAgilityPack;
using System;

namespace AcquisitionDate.LodestoneRequests.Requests.Abstractions.PageList;

internal abstract class PageListRequest : CharacterRequest
{
    readonly Action<PageURLListData?> PageURLCountback;

    protected abstract string[] Outcome { get; set; }

    public PageListRequest(IDatableData data, Action<PageURLListData?> pageUrlCallback) : base(data)
    {
        PageURLCountback = pageUrlCallback;
    }

    public sealed override void HandleFailure(Exception exception)
    {
        PluginHandlers.PluginLog.Error(exception, "An Error Occurred during a Page Url List Request.");
        PageURLCountback?.Invoke(null);
    }

    public sealed override void HandleSuccess(HtmlDocument document)
    {
        var successfullParse = OnSuccess(document);

        if (!successfullParse)
        {
            HandleFailure(new Exception("Failed to parse document"));
            return;
        }

        CallbackOutcome();
    }

    protected abstract bool OnSuccess(HtmlDocument document);

    void CallbackOutcome() => PageURLCountback?.Invoke(new PageURLListData(Outcome));
}
