using AcquisitionDate.Core.Handlers;
using AcquisitionDate.Database.Interfaces;
using AcquisitionDate.HtmlParser;
using AcquisitionDate.LodestoneData;
using AcquisitionDate.LodestoneRequests.Requests.Abstractions;
using AcquisitionDate.Services.Interfaces;
using Dalamud.Utility;
using HtmlAgilityPack;
using Lumina.Excel.Sheets;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace AcquisitionDate.LodestoneRequests.Requests;

internal class QuestDataRequest : CharacterRequest
{
    readonly System.Action? SuccessCallback;
    readonly Action<Exception>? FailureCallback;
    readonly Action<QuestData> ContinuousSuccessCallback;
    readonly int Page;

    readonly ISheets Sheets;

    readonly CancellationTokenSource tokenSource = new CancellationTokenSource();

    public QuestDataRequest(ISheets sheets, IDatableData data, int page, Action<QuestData> continuousSuccessCallback, System.Action successCallback = null, Action<Exception> failureCallback = null) : base(data)
    {
        Sheets = sheets;

        SuccessCallback = successCallback;
        FailureCallback = failureCallback;
        Page = page;
        ContinuousSuccessCallback = continuousSuccessCallback;
    }

    public override void HandleFailure(Exception exception) => FailureCallback?.Invoke(exception);

    public override void HandleSuccess(HtmlDocument document)
    {
        SuccessCallback?.Invoke();

        HtmlNode rootNode = document.DocumentNode;

        HtmlNode? listNode = HtmlParserHelper.GetNode(rootNode, "ldst__achievement");
        if (listNode == null) return;

        List<HtmlNode> nodes = HtmlParserHelper.GetNodes(listNode, "entry__quest");
        foreach (HtmlNode node in nodes)
        {
            HtmlNode? entryQuestName = HtmlParserHelper.GetNode(node, "entry__quest__name");
            if (entryQuestName == null) continue;
            if (entryQuestName.ChildNodes.Count < 2) continue;

            DateTime? time = HtmlParserHelper.GetAcquiredTime(entryQuestName);
            if (time == null) continue;

            string value = entryQuestName.ChildNodes[1].GetDirectInnerText().Trim();
            string decoded = HttpUtility.HtmlDecode(value);

            if (decoded.IsNullOrWhitespace()) continue;

            Task.Run(async () => await HandleQuestSearch(decoded, time.Value), tokenSource.Token);
        }
    }

    async Task HandleQuestSearch(string decoded, DateTime time)
    {
        await Task.Yield();

        List<(int size, Quest validQuest)> questsThatFit = new List<(int size, Quest validQuest)>();

        foreach (Quest quest in Sheets.AllQuests)
        {
            string qName = quest.Name.ExtractText();
            if (qName.IsNullOrWhitespace()) continue;
            if (!decoded.Contains(qName, StringComparison.InvariantCultureIgnoreCase)) continue;

            questsThatFit.Add((qName.Length, quest));
        }

        if (questsThatFit.Count == 0) return;

        questsThatFit.Sort((quest1, quest2) => quest1.size.CompareTo(quest2.size));

        uint rowID = questsThatFit[questsThatFit.Count - 1].validQuest.RowId;

        await PluginHandlers.Framework.Run(() => ContinuousSuccessCallback?.Invoke(new QuestData(rowID, time)), tokenSource.Token);
    }

    public override string GetURL() =>  base.GetURL() + $"quest/?page={Page}#anchor_quest";
}
