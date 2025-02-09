using AcquisitionDate.LodestoneRequests.Interfaces;
using HtmlAgilityPack;
using System;
using System.Net.Http;
using System.Threading;

namespace AcquisitionDate.LodestoneRequests;

internal abstract class LodestoneRequest : ILodestoneRequest
{
    // If the tickcount is higher than 1 OnTick gets called with the given tick
    public int TickCount { get; protected set; } = 1;

    protected CancellationToken CancellationToken;
    public abstract void HandleSuccess(HtmlDocument document);
    public abstract void HandleFailure(Exception exception);
    public abstract string GetURL();

    Action? tickSuccessCallback;

    public virtual void OnTick(int tick, Action successCallback) 
    {
        tickSuccessCallback = successCallback;
    }

    protected void SuccessCallbackTick()
    {
        tickSuccessCallback?.Invoke();
    }

    public void SetCancellationToken(CancellationToken token)
    {
        CancellationToken = token;
    }

    public bool ShouldError { get; protected set; } = false;
}
