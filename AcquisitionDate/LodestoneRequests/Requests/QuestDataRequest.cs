using AcquisitionDate.Database.Interfaces;
using AcquisitionDate.HtmlParser;
using AcquisitionDate.LodestoneData;
using AcquisitionDate.LodestoneRequests.Requests.Abstractions;
using AcquisitionDate.Services.Interfaces;
using HtmlAgilityPack;
using Lumina.Excel.Sheets;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Web;

namespace AcquisitionDate.LodestoneRequests.Requests;

internal class QuestDataRequest : CharacterRequest
{
    readonly System.Action? SuccessCallback;
    readonly Action<Exception>? FailureCallback;
    readonly Action<QuestData> ContinuousSuccessCallback;
    readonly int Page;

    readonly Regex regex = new Regex(@"^(?<questGroup>.*?)(\s*(?:""(?<questName>.*?)""|「(?<questName>.*?)」|„(?<questName>.*?)“|\((?<questName>.*?)\))\s*)$");

    readonly ISheets Sheets;

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

            Match match = regex.Match(decoded);
            if (!match.Success) continue;

            string questGroup = match.Groups["questGroup"].Value;
            string questName = match.Groups["questName"].Value;

            Quest? quest = Sheets.GetQuest(questName, questGroup);
            if (quest == null) continue;

            uint rowID = quest.Value.RowId;

            ContinuousSuccessCallback?.Invoke(new QuestData(rowID, time.Value));
        }
    }

    bool QuestValid(Quest quest, Match match)
    {
        try
        {
            string journalGenre = quest.JournalGenre.Value.Name.ExtractText();
            string questName = quest.Name.ExtractText();

            if (journalGenre != match.Groups["questGroup"].Value) return false;
            if (questName != match.Groups["questName"].Value) return false;

            return true;

        } catch { }

        return false;
    }

    public override string GetURL() =>  base.GetURL() + $"quest/?page={Page}#anchor_quest";
}
