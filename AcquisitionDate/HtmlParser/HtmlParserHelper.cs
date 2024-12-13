using Dalamud.Utility;
using HtmlAgilityPack;
using System.Text.RegularExpressions;
using System;
using System.Collections.Generic;

namespace AcquisitionDate.HtmlParser;

internal static class HtmlParserHelper
{
    const string PATTERN = @"ldst_strftime\((\d+),";

    public static DateTime? GetAcquiredTime(HtmlNode node)
    {
        string? innerHTML = node.InnerHtml;
        if (innerHTML.IsNullOrWhitespace()) return null;

        Match match = Regex.Match(innerHTML, PATTERN);
        if (!match.Success) return null;

        int groupCount = match.Groups.Count;
        if (groupCount <= 1) return null;

        string timeString = match.Groups[1].Value;
        if (!long.TryParse(timeString, out long timeLong)) return null;

        return DateTimeOffset.FromUnixTimeSeconds(timeLong).LocalDateTime;
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
}
