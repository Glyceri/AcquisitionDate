using AcquisitionDate.LodestoneRequests.Interfaces;
using HtmlAgilityPack;
using System;

namespace AcquisitionDate.LodestoneRequests;

internal abstract class LodestoneRequest : ILodestoneRequest
{
    public abstract void HandleSuccess(HtmlDocument document);
    public abstract void HandleFailure(Exception exception);
    public abstract string GetURL();
}
