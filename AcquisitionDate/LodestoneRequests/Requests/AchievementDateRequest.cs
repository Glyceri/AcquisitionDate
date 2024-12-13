using AcquisitionDate.HtmlParser;
using AcquisitionDate.LodestoneData;
using AcquisitionDate.LodestoneRequests.Requests.Abstractions;
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

    public AchievementDateRequest(int lodestoneCharacterID, int page, Action<AchievementData> continuousSuccessCallback, Action successCallback = null, Action<Exception> failureCallback = null) : base(lodestoneCharacterID)
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

            HtmlNode? listentry = HtmlParserHelper.GetNode(entryAchievement, "entry__achievement--list");
            if (listentry == null) continue;
            

            HtmlNode? timeNode = HtmlParserHelper.GetNode(entryAchievement, "entry__activity__time");
            if (timeNode == null) continue;
            if (timeNode.ChildNodes.Count <= 1) continue;
            
            if (value == null) continue;

            string num = value.Split('/', StringSplitOptions.RemoveEmptyEntries).Last();
            uint achievementID = uint.Parse(num);

            DateTime? acquiredTime = HtmlParserHelper.GetAcquiredTime(timeNode);
            if (acquiredTime == null) continue;

            ContinuousSuccessCallback?.Invoke(new AchievementData(achievementID, acquiredTime.Value));
        }
    }

    public override string GetURL() =>  base.GetURL() + $"achievement/?page={Page}#anchor_achievement";
}
