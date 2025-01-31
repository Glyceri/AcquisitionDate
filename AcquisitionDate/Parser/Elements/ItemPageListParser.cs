using AcquisitionDate.HtmlParser;
using AcquisitionDate.Parser.Interfaces;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;

internal class ItemPageListParser : IItemPageListParser<string[]>
{
    string _listIconName = string.Empty;

    public void Parse(HtmlNode rootNode, Action<string[]> onSuccess, Action<Exception> onFailure)
    {
        List<HtmlNode> nodes = HtmlParserHelper.GetNodes(rootNode, $"{_listIconName}__list_icon");

        if (nodes.Count == 0)
        {
            onFailure?.Invoke(new Exception("No nodes found"));
            return;
        }

        List<string> hrefs = new List<string>();

        foreach (HtmlNode node in nodes)
        {
            string outcome = node.GetAttributeValue("data-tooltip_href", string.Empty);
            hrefs.Add(outcome);
        }

        if (hrefs.Count == 0)
        {
            onFailure?.Invoke(new Exception("No hrefs found"));
            return;
        }

        onSuccess?.Invoke(hrefs.ToArray());
    }

    public void SetListIconName(string listIconName)
    {
        _listIconName = listIconName;
    }
}