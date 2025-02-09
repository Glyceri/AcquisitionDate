using AcquisitionDate.Core.Handlers;
using AcquisitionDate.HtmlParser;
using AcquisitionDate.LodestoneData;
using AcquisitionDate.LodestoneNetworking.Enums;
using AcquisitionDate.Parser.Interfaces;
using AcquisitionDate.Services.Interfaces;
using Dalamud.Utility;
using HtmlAgilityPack;
using Lumina.Excel.Sheets;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace AcquisitionDate.Parser.Elements;

internal class QuestDataParser : IAchievementDataParser<QuestData>, IDisposable
{
    readonly CancellationTokenSource CancellationTokenSource = new CancellationTokenSource();
    readonly ISheets Sheets;

    LodestoneRegion pageLanguage;

    public QuestDataParser(ISheets sheets)
    {
        Sheets = sheets;
    }

    public void Parse(HtmlNode rootNode, Action<QuestData> onSuccess, Action<Exception> onFailure)
    {
        HtmlNode? entryQuestName = HtmlParserHelper.GetNode(rootNode, "entry__quest__name");
        if (entryQuestName == null)
        {
            onFailure?.Invoke(new Exception("entry__quest__name was NOT found!"));
            return;
        }
        if (entryQuestName.ChildNodes.Count < 2)
        {
            onFailure?.Invoke(new Exception("entry__quest__name has less than 2 valid elements!"));
            return;
        }

        DateTime? time = HtmlParserHelper.GetAcquiredTime(entryQuestName, pageLanguage);
        if (time == null)
        {
            onFailure?.Invoke(new Exception("unable to acquire dateTime from entryQuestName!"));
            return;
        }

        string value = entryQuestName.ChildNodes[1].GetDirectInnerText().Trim();
        string decoded = HttpUtility.HtmlDecode(value);

        if (decoded.IsNullOrWhitespace())
        {
            onFailure?.Invoke(new Exception("Decoded html text is null or whitespace!"));
            return;
        }

        Task.Run(async () => await HandleQuestSearch(decoded, time.Value, onSuccess, onFailure), CancellationTokenSource.Token).ConfigureAwait(false);
    }

    async Task HandleQuestSearch(string decoded, DateTime time, Action<QuestData> onSuccess, Action<Exception> onFailure)
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

        if (questsThatFit.Count == 0)
        {
            onFailure?.Invoke(new Exception("No quest found that fits"));
            return;
        }
        else if (questsThatFit.Count == 1)
        {
            uint rowID = questsThatFit[0].validQuest.RowId;

            await PluginHandlers.Framework.Run(() => onSuccess?.Invoke(new QuestData(rowID, time)), CancellationTokenSource.Token);
        }
        else
        {
            questsThatFit.Sort((quest1, quest2) => quest1.size.CompareTo(quest2.size));

            Quest finalQuest = questsThatFit[questsThatFit.Count - 1].validQuest;
            string finalQuestName = finalQuest.Name.ExtractText();

            List<uint> allFinalRowIDS = new List<uint>();

            for (int i = 0; i < questsThatFit.Count; i++)
            {
                if (questsThatFit[i].validQuest.Name.ExtractText() != finalQuestName) continue;

                allFinalRowIDS.Add(questsThatFit[i].validQuest.RowId);
            }

            foreach (uint rowID in allFinalRowIDS)
            {
                await PluginHandlers.Framework.Run(() => onSuccess?.Invoke(new QuestData(rowID, time)), CancellationTokenSource.Token);
            }
        }
    }

    public void Dispose()
    {
        try
        {
            CancellationTokenSource?.Cancel();
        }
        catch { }
        try
        {
            CancellationTokenSource?.Dispose();
        }
        catch { }
    }

    public void SetPageLanguage(LodestoneRegion lodestoneRegion)
    {
        pageLanguage = lodestoneRegion;
    }
}
