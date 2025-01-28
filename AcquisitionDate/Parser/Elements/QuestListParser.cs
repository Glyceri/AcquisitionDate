using AcquisitionDate.HtmlParser;
using AcquisitionDate.Parser.Interfaces;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;

namespace AcquisitionDate.Parser.Elements;

internal class QuestListParser : IAcquistionParserElement<List<HtmlNode>>
{
    public void Parse(HtmlNode rootNode, Action<List<HtmlNode>> onSuccess, Action<Exception> onFailure)
    {
        HtmlNode? listNode = HtmlParserHelper.GetNode(rootNode, "ldst__achievement");
        if (listNode == null)
        {
            onFailure?.Invoke(new Exception("ldst__achievement was NOT found"));
            return;
        }

        List<HtmlNode> nodes = HtmlParserHelper.GetNodes(listNode, "entry__quest");
        if (nodes.Count == 0)
        {
            onFailure?.Invoke(new Exception("No nodes found under the tag 'entry__quest'"));
            return;
        }

        onSuccess?.Invoke(nodes);
    }
}
