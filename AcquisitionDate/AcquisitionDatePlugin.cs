using Dalamud.Plugin;
using AcquisitionDate.Windows;
using AcquisitionDate.Core.Handlers;
using System.Net.Http;
using System.Text.RegularExpressions;
using Lumina.Excel;
using Lumina.Excel.Sheets;
using AcquisitionDate.LodestoneNetworking.Interfaces;
using AcquisitionDate.LodestoneNetworking;
using AcquisitionDate.LodestoneRequests.Requests;
using AcquisitionDate.Updating.Interfaces;
using AcquisitionDate.Updating;
using AcquisitionDate.LodestoneData;
using AcquisitionDate.LodestoneRequests.Interfaces;

namespace AcquisitionDate;

public sealed class AcquisitionDatePlugin : IDalamudPlugin
{
    internal Configuration Config { get; private set; }
    internal WindowHandler WindowHandler { get; private set; }

    readonly HttpClient Client = new HttpClient();

    readonly Regex pageRegex = new Regex(@"Page 1 of ^(?<pagecount>.+)", RegexOptions.Compiled);

    readonly ExcelSheet<Achievement> achievements;
    readonly ExcelSheet<Quest> quests;

    readonly IUpdateHandler UpdateHandler;
    readonly ILodestoneNetworker LodestoneNetworker;

    public AcquisitionDatePlugin(IDalamudPluginInterface pluginInterface)
    {
        PluginHandlers.Start(ref pluginInterface, this);
        Config = PluginHandlers.PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
        WindowHandler = new WindowHandler(pluginInterface);

        LodestoneNetworker = new LodestoneNetworker();
        UpdateHandler = new UpdateHandler(LodestoneNetworker);

        achievements = PluginHandlers.DataManager.GetExcelSheet<Achievement>();
        quests = PluginHandlers.DataManager.GetExcelSheet<Quest>();
        
        /*
        foreach (var quest in quests)
        {
            try
            {
                PluginHandlers.PluginLog.Debug("Quest Place: " + quest.JournalGenre.Value.Name.ExtractText());
            }
            catch { }
            PluginHandlers.PluginLog.Debug("Quest: " + quest.Name.ExtractText());   
        }*/

        ILodestoneRequest request = new AchievementPageCountRequest(30338174, OnOutcome);
        LodestoneNetworker.AddElementToQueue(request);

        //ILodestoneRequest request2 = new QuestPageCountRequest(30338174, OnOutcome2);
        //LodestoneNetworker.AddElementToQueue(request2);
    }

    void OnOutcome(PageCountData? val)
    {
        PluginHandlers.PluginLog.Debug($"OUTCOME: {val?.PageCount}");

        if (val == null) return;

        for (int i = 0; i < val.Value.PageCount; i++)
        {
            AchievementDateRequest dataRequest = new AchievementDateRequest(30338174, i, OnAchievement);

            LodestoneNetworker.AddElementToQueue(dataRequest);
        }
    }

    void OnOutcome2(PageCountData? val)
    {
        PluginHandlers.PluginLog.Debug($"OUTCOME2: {val?.PageCount}");

        if (val == null) return;

        for (int i = 0; i < val.Value.PageCount; i++)
        {
            QuestDataRequest dataRequest = new QuestDataRequest(30338174, i, OnQuest);

            LodestoneNetworker.AddElementToQueue(dataRequest);
        }
    }

    void OnAchievement(AchievementData achiData)
    {
        PluginHandlers.PluginLog.Debug($"Got data for achievement: {achievements.GetRow((uint)achiData.AchievementID).Name}: {achiData.AchievedDate.ToString("dd/MM/yy")}");
    }

    void OnQuest(QuestData quest)
    {
        PluginHandlers.PluginLog.Debug($"Got data for quest: {quests.GetRow(quest.QuestID).Name}: {quest.AchievedDate.ToString("dd/MM/yy")}");

    }

    public void Dispose()
    {
        WindowHandler?.Dispose();
        UpdateHandler?.Dispose();
        LodestoneNetworker?.Dispose();
    }
}
