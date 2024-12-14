using AcquisitionDate.Core.Handlers;
using AcquisitionDate.HtmlParser;
using AcquisitionDate.LodestoneData;
using AcquisitionDate.LodestoneRequests.Requests.Abstractions;
using Dalamud.Utility;
using HtmlAgilityPack;
using Lumina.Excel;
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

    readonly ExcelSheet<Quest>[] quests;

    readonly Regex regex = new Regex(@"^(?<questGroup>.*?)(\s*(?:""(?<questName>.*?)""|「(?<questName>.*?)」|„(?<questName>.*?)“|\((?<questName>.*?)\))\s*)$");

    public QuestDataRequest(int lodestoneCharacterID, int page, Action<QuestData> continuousSuccessCallback, System.Action successCallback = null, Action<Exception> failureCallback = null) : base(lodestoneCharacterID)
    {
        SuccessCallback = successCallback;
        FailureCallback = failureCallback;
        Page = page;
        ContinuousSuccessCallback = continuousSuccessCallback;

        quests = [
            PluginHandlers.DataManager.GetExcelSheet<Quest>(Dalamud.Game.ClientLanguage.English),
            PluginHandlers.DataManager.GetExcelSheet<Quest>(Dalamud.Game.ClientLanguage.Japanese),
            PluginHandlers.DataManager.GetExcelSheet<Quest>(Dalamud.Game.ClientLanguage.German),
            PluginHandlers.DataManager.GetExcelSheet<Quest>(Dalamud.Game.ClientLanguage.French),
            ];
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

            uint? rowID = null;

            for (int i = 0; i < quests.Length; i++)
            {
                ExcelSheet<Quest> sheet = quests[i];

                if (rowID != null) break;

                foreach (Quest quest in sheet)
                {
                    if (!QuestValid(quest, match)) continue;

                    rowID = quest.RowId;
                    break;
                }
            }

            if (rowID == null) continue;

            ContinuousSuccessCallback?.Invoke(new QuestData(rowID.Value, time.Value));
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
