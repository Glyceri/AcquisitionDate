using AcquisitionDate.Database.Interfaces;
using AcquisitionDate.LodestoneData;
using AcquisitionDate.Parser.Elements;
using HtmlAgilityPack;
using System;

namespace AcquisitionDate.LodestoneRequests.Requests.Abstractions.PageCount;

internal abstract class AchievementTypePageCountRequest : PageCountRequest
{
    readonly AchievementListPageCountParser PageCountParser;

    public AchievementTypePageCountRequest(AchievementListPageCountParser countParser, IDatableData data, Action<PageCountData?> pageCountCallback) : base(data, pageCountCallback) 
    {
        PageCountParser = countParser;
    }

    public override void HandleSuccess(HtmlDocument document)
    {
        HtmlNode rootNode = document.DocumentNode;

        PageCountParser.Parse(rootNode, CallbackOutcome, HandleFailure);
    }
}
