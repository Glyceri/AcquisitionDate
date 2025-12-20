using AcquisitionDate.Core.Handlers;
using AcquisitionDate.Database.Interfaces;
using AcquisitionDate.LodestoneData;
using AcquisitionDate.LodestoneNetworking.Enums;
using AcquisitionDate.LodestoneRequests.Requests.Abstractions;
using AcquisitionDate.Parser.Elements;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;

namespace AcquisitionDate.LodestoneRequests.Requests;

internal class AchievementDateRequest : CharacterRequest
{
    readonly AchievementListParser AchievementListParser;
    readonly AchievementElementParser AchievementElementParser;

    readonly Action? SuccessCallback;
    readonly Action<Exception>? FailureCallback;
    readonly Action<AchievementData> ContinuousSuccessCallback;
    readonly int Page;
    readonly LodestoneRegion PageLanguage;

    public AchievementDateRequest(AchievementListParser achievementListParser, AchievementElementParser achievementElementParser, IDatableData data, int page, LodestoneRegion pageLanguage, Action<AchievementData> continuousSuccessCallback, Action? successCallback = null, Action<Exception>? failureCallback = null) : base(data)
    {
        AchievementListParser = achievementListParser;
        AchievementElementParser = achievementElementParser;

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

        AchievementListParser.Parse(rootNode, HandleList, PrintFailure);
    }

    void HandleList(List<HtmlNode> nodes)
    {
        foreach (HtmlNode node in nodes)
        {
            AchievementElementParser.SetPageLanguage(PageLanguage);
            AchievementElementParser.Parse(node, OnAchievementData, PrintFailure);
        }
    }

    void OnAchievementData(AchievementData achievementData) => PluginHandlers.Framework.Run(() => ContinuousSuccessCallback?.Invoke(achievementData));

    void PrintFailure(Exception exception) => PluginHandlers.PluginLog.Error(exception, "Error in achievement data request");

    public override string GetURL() =>  base.GetURL() + $"achievement/?page={Page}#anchor_achievement";
}
