using HtmlAgilityPack;
using System;

namespace AcquisitionDate.LodestoneRequests.Interfaces;

internal interface ILodestoneRequest
{
    void HandleSuccess(HtmlDocument document);
    void HandleFailure(Exception exception);
    string GetURL();
}
