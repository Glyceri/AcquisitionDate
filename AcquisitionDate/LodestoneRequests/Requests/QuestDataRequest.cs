using AcquisitionDate.Core.Handlers;
using AcquisitionDate.Database.Interfaces;
using AcquisitionDate.LodestoneData;
using AcquisitionDate.LodestoneNetworking.Enums;
using AcquisitionDate.LodestoneRequests.Requests.Abstractions;
using AcquisitionDate.Parser.Elements;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Threading;

namespace AcquisitionDate.LodestoneRequests.Requests;

internal class QuestDataRequest : CharacterRequest
{
    readonly Action? SuccessCallback;
    readonly Action<Exception>? FailureCallback;
    readonly Action<QuestData> ContinuousSuccessCallback;
    readonly int Page;
    readonly LodestoneRegion PageLanguage;

    readonly CancellationTokenSource tokenSource = new CancellationTokenSource();

    readonly QuestListParser QuestListParser;
    readonly QuestDataParser QuestDataParser;

    public QuestDataRequest(QuestListParser questListParser, QuestDataParser questDataParser, IDatableData data, int page, LodestoneRegion pageLanguage, Action<QuestData> continuousSuccessCallback, System.Action? successCallback = null, Action<Exception>? failureCallback = null) : base(data)
    {
        QuestListParser = questListParser;
        QuestDataParser = questDataParser;

        SuccessCallback = successCallback;
        FailureCallback = failureCallback;
        Page = page;
        PageLanguage = pageLanguage;

        ContinuousSuccessCallback = continuousSuccessCallback;
    }

    public override void HandleFailure(Exception exception) => FailureCallback?.Invoke(exception);

    public override void HandleSuccess(HtmlDocument document)
    {
        SuccessCallback?.Invoke();

        HtmlNode rootNode = document.DocumentNode;

        QuestListParser.Parse(rootNode, OnListAcquire, PrintFailure);
    }

    void OnListAcquire(List<HtmlNode> questNodes)
    {
        foreach (HtmlNode node in questNodes)
        {
            QuestDataParser.SetPageLanguage(PageLanguage);
            QuestDataParser.Parse(node, ContinuousSuccessCallback, PrintFailure);
        }
    }

    void PrintFailure(Exception exception) => PluginHandlers.PluginLog.Error(exception, "Failure in QuestDataRequest");   

    public override string GetURL() =>  base.GetURL() + $"quest/?page={Page}#anchor_quest";
}
