using AcquisitionDate.Core.Handlers;
using AcquisitionDate.Database.Interfaces;
using AcquisitionDate.LodestoneNetworking.Enums;
using AcquisitionDate.LodestoneRequests.Requests.Abstractions;
using AcquisitionDate.Parser.Elements;
using HtmlAgilityPack;
using System;

namespace AcquisitionDate.LodestoneRequests.Requests;

internal class LodestoneLanguageRequest : CharacterRequest
{
    readonly LodestonePageLanguageParser LodestoneLanguageParser;

    readonly Action<LodestoneRegion> OnLodestoneRegion;

    public LodestoneLanguageRequest(LodestonePageLanguageParser lodestoneLanguageParser, IDatableData data, Action<LodestoneRegion> onLodestoneRegion) : base(data)
    {
        LodestoneLanguageParser = lodestoneLanguageParser;

        OnLodestoneRegion = onLodestoneRegion;
    }

    public sealed override void HandleFailure(Exception exception)
    {
        PrintFailure(exception);
        OnSuccess(LodestoneRegion.Europe);
    }

    public override void HandleSuccess(HtmlDocument document)
    {
        HtmlNode rootNode = document.DocumentNode;

        LodestoneLanguageParser.Parse(rootNode, OnSuccess, HandleFailure);
    }

    void PrintFailure(Exception e) => PluginHandlers.PluginLog.Error(e, "Error in ItemPageDataRequest");
    void OnSuccess(LodestoneRegion region) => PluginHandlers.Framework.Run(() => OnLodestoneRegion?.Invoke(region));
}
