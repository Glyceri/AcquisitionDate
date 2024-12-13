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

namespace AcquisitionDate;

public sealed class AcquisitionDatePlugin : IDalamudPlugin
{
    internal Configuration Config { get; private set; }
    internal WindowHandler WindowHandler { get; private set; }

    readonly HttpClient Client = new HttpClient();

    readonly Regex pageRegex = new Regex(@"Page 1 of ^(?<pagecount>.+)", RegexOptions.Compiled);

    readonly ExcelSheet<Achievement> achievements;

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

        AchievementPageCountRequest request = new AchievementPageCountRequest(30338174, OnOutcome);

        LodestoneNetworker.AddElementToQueue(request);
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

    void OnAchievement(AchievementData achiData)
    {
        PluginHandlers.PluginLog.Debug($"Got data for achievement: {achievements.GetRow((uint)achiData.AchievementID).Name}: {achiData.AchievedDate.ToString("dd/MM/yy")}");
    }

    public void Dispose()
    {
        WindowHandler?.Dispose();
        UpdateHandler?.Dispose();
        LodestoneNetworker?.Dispose();
    }
}
