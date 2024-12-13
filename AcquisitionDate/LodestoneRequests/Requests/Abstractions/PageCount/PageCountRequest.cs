using AcquisitionDate.Core.Handlers;
using AcquisitionDate.LodestoneData;
using HtmlAgilityPack;
using System;

namespace AcquisitionDate.LodestoneRequests.Requests.Abstractions.PageCount;

internal abstract class PageCountRequest : CharacterRequest
{
    readonly Action<PageCountData?> PageCountCallback;

    protected abstract int Outcome { get; set; }

    public PageCountRequest(int lodestoneCharacterID, Action<PageCountData?> pageCountCallback) : base(lodestoneCharacterID)
    {
        PageCountCallback = pageCountCallback;
    }

    public sealed override void HandleFailure(Exception exception)
    {
        PluginHandlers.PluginLog.Error(exception, "An Error Occurred during a Page Count Request.");
        PageCountCallback?.Invoke(null);
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

    void CallbackOutcome() => PageCountCallback?.Invoke(new PageCountData(Outcome));
}
