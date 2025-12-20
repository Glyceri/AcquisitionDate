using HtmlAgilityPack;
using System;
using System.Threading;

namespace AcquisitionDate.LodestoneRequests.Interfaces;

internal interface ILodestoneRequest
{
    int TickCount { get; }

    void HandleSuccess(HtmlDocument document);
    void HandleFailure(Exception exception);
    string GetURL();

    void SetCancellationToken(CancellationToken token);

    void OnTick(int tick, Action successCallback);
}