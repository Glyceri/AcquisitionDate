using Dalamud.Utility;
using HtmlAgilityPack;
using System.Text.RegularExpressions;
using System;
using System.Collections.Generic;
using System.Linq;
using AcquisitionDate.LodestoneNetworking.Enums;
using AcquisitionDate.Core.Handlers;
using System.Globalization;
using System.ComponentModel;

namespace AcquisitionDate.HtmlParser;

internal static class HtmlParserHelper
{
    const string LDST_STRFTIME_PATTERN = @"ldst_strftime\((\d+),";
    const string DATE_PATTERN = @"(\d{4}/\d{2}/\d{2}|\d{2}/\d{2}/\d{4}|\d{1,2}\.\d{1,2}\.\d{4})";

    public static DateTime? GetAcquiredTime(HtmlNode node, LodestoneRegion lodestoneRegion = LodestoneRegion.America)
    {
        string? innerHTML = node.InnerHtml;
        if (innerHTML.IsNullOrWhitespace()) return GetAlternativeAcquiredTime(node, lodestoneRegion);

        Match match = Regex.Match(innerHTML, LDST_STRFTIME_PATTERN);
        if (!match.Success) return GetAlternativeAcquiredTime(node, lodestoneRegion);

        int groupCount = match.Groups.Count;
        if (groupCount <= 1) return null;

        string timeString = match.Groups[1].Value;
        if (!long.TryParse(timeString, out long timeLong)) return null;

        return DateTimeOffset.FromUnixTimeSeconds(timeLong).LocalDateTime;
    }

    static DateTime? GetAlternativeAcquiredTime(HtmlNode node, LodestoneRegion lodestoneRegion)
    {
        string? innerText = node.InnerText;
        if (innerText.IsNullOrWhitespace()) return null;

        Match match = Regex.Match(innerText, DATE_PATTERN);
        if (!match.Success) return null;

        string dateText = match.Value;

        DateTime? parsedTime = ParseTime(dateText, lodestoneRegion);

        return parsedTime;
    }

    static DateTime? ParseTime(string dateText, LodestoneRegion lodestoneRegion)
    {
        DescriptionAttribute? description = lodestoneRegion.GetAttribute<DescriptionAttribute>();
        if (description == null) return null;

        string dateFormatText = description.Description.Trim();
        if (dateFormatText.IsNullOrWhitespace()) return null;

        PluginHandlers.PluginLog.Verbose($"Date Text: {dateText}, Date Format Text: {dateFormatText}");

        if (DateTime.TryParseExact(dateText, description.Description, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date))
        {
            return date;
        }

        return null;
    }

    public static HtmlNode? GetNode(HtmlNode baseNode, string nodeName)
    {
        foreach (HtmlNode childNode in baseNode.ChildNodes)
        {
            HtmlNode gottenNode = GetNode(childNode, nodeName)!;
            if (gottenNode != null) return gottenNode;
            if (!childNode.HasClass(nodeName)) continue;
            return childNode;
        }

        return null;
    }

    public static List<HtmlNode> GetNodes(HtmlNode baseNode, string nodeName)
    {
        List<HtmlNode> nodes = new List<HtmlNode>();

        foreach (HtmlNode childNode in baseNode.ChildNodes)
        {
            List<HtmlNode> newNodes = GetNodes(childNode, nodeName)!;
            nodes.AddRange(newNodes);
            if (!childNode.HasClass(nodeName)) continue;
            nodes.Add(childNode);
        }

        return nodes;
    }

    public static uint? GetValueFromDashedLink(string value)
    {
        string num = value.Split('/', StringSplitOptions.RemoveEmptyEntries).Last();
        if (num.IsNullOrWhitespace()) return null;

        if (!uint.TryParse(num, out uint ID)) return null;

        return ID;
    }
}
