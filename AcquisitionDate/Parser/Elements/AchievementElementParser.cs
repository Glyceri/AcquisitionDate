using AcquisitionDate.HtmlParser;
using AcquisitionDate.LodestoneData;
using AcquisitionDate.Parser.Interfaces;
using Dalamud.Utility;
using HtmlAgilityPack;
using System;
using System.Linq;

namespace AcquisitionDate.Parser.Elements;

internal class AchievementElementParser : IAcquistionParserElement<AchievementData>
{
    public void Parse(HtmlNode rootNode, Action<AchievementData> onSuccess, Action<Exception> onFailure)
    {
        HtmlNode? entryAchievement = HtmlParserHelper.GetNode(rootNode, "entry__achievement");
        if (entryAchievement == null)
        {
            onFailure?.Invoke(new Exception("entry__achievement was NOT found!"));
            return;
        }

        string value = entryAchievement.GetAttributeValue("href", string.Empty);
        if (value.IsNullOrWhitespace())
        {
            onFailure?.Invoke(new Exception("href value was NOT found or is empty!"));
            return;
        }

        uint? achievementID = HtmlParserHelper.GetValueFromDashedLink(value);
        if (achievementID == null)
        {
            onFailure?.Invoke(new Exception("achievementID was NOT found!"));
            return;
        }

        HtmlNode? listentry = HtmlParserHelper.GetNode(entryAchievement, "entry__achievement--list");
        if (listentry == null)
        {
            onFailure?.Invoke(new Exception("entry__achievement--list was NOT found!"));
            return;
        }

        HtmlNode? timeNode = HtmlParserHelper.GetNode(entryAchievement, "entry__activity__time");
        if (timeNode == null)
        {
            onFailure?.Invoke(new Exception("entry__activity__time was NOT found!"));
            return;
        }
        if (timeNode.ChildNodes.Count <= 1)
        {
            onFailure?.Invoke(new Exception("The time node does NOT contain enough elements to parse a date"));
            return;
        }

        string num = value.Split('/', StringSplitOptions.RemoveEmptyEntries).Last();
        if (num.IsNullOrWhitespace())
        {
            onFailure?.Invoke(new Exception("The parsed number was NOT found!"));
            return;
        }

        DateTime? acquiredTime = HtmlParserHelper.GetAcquiredTime(timeNode);
        if (acquiredTime == null)
        {
            onFailure?.Invoke(new Exception("Could NOT acquire a date for the given timeNode"));
            return;
        }

        onSuccess?.Invoke(new AchievementData(achievementID.Value, acquiredTime.Value));
    }
}
