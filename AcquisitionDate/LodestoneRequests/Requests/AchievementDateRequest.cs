using AcquisitionDate.Core.Handlers;
using AcquisitionDate.Database.Interfaces;
using AcquisitionDate.HtmlParser;
using AcquisitionDate.LodestoneData;
using AcquisitionDate.LodestoneRequests.Requests.Abstractions;
using Dalamud.Utility;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AcquisitionDate.LodestoneRequests.Requests;

internal class AchievementDateRequest : CharacterRequest
{
    readonly Action? SuccessCallback;
    readonly Action<Exception>? FailureCallback;
    readonly Action<AchievementData> ContinuousSuccessCallback;
    readonly int Page;

    public AchievementDateRequest(IDatableData data, int page, Action<AchievementData> continuousSuccessCallback, Action successCallback = null, Action<Exception> failureCallback = null) : base(data)
    {
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
        
        List<HtmlNode> nodes = HtmlParserHelper.GetNodes(listNode, "entry");
        foreach (HtmlNode node in nodes)
        {
            HtmlNode? entryAchievement = HtmlParserHelper.GetNode(node, "entry__achievement");
            if (entryAchievement == null) continue;
            
            string value = entryAchievement.GetAttributeValue("href", string.Empty);

            uint? achievementID = HtmlParserHelper.GetValueFromDashedLink(value);
            if (achievementID == null) continue;

            HtmlNode? listentry = HtmlParserHelper.GetNode(entryAchievement, "entry__achievement--list");
            if (listentry == null) continue;
            

            HtmlNode? timeNode = HtmlParserHelper.GetNode(entryAchievement, "entry__activity__time");
            if (timeNode == null) continue;
            if (timeNode.ChildNodes.Count <= 1) continue;
            
            if (value == null) continue;

            string num = value.Split('/', StringSplitOptions.RemoveEmptyEntries).Last();
            if (num.IsNullOrWhitespace()) continue;

            DateTime? acquiredTime = HtmlParserHelper.GetAcquiredTime(timeNode);
            if (acquiredTime == null) continue;

            PluginHandlers.Framework.Run(() => ContinuousSuccessCallback?.Invoke(new AchievementData(achievementID.Value, acquiredTime.Value)));
        }
    }

    public override string GetURL() =>  base.GetURL() + $"achievement/?page={Page}#anchor_achievement";
}
