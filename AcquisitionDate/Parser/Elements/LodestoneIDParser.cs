using AcquisitionDate.HtmlParser;
using AcquisitionDate.Parser.Interfaces;
using AcquisitionDate.Parser.Structs;
using AcquisitionDate.Services.Interfaces;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Web;

namespace AcquisitionDate.Parser.Elements;

internal class LodestoneIDParser : IAcquistionParserElement<LodestoneParseData>
{
    readonly ISheets Sheets;

    public LodestoneIDParser(ISheets sheets)
    {
        Sheets = sheets;
    }

    public void Parse(HtmlNode rootNode, Action<LodestoneParseData> onSuccess, Action<Exception> onFailure)
    {
        HtmlNode? listNode = HtmlParserHelper.GetNode(rootNode, "ldst__window");
        if (listNode == null)
        {
            onFailure?.Invoke(new Exception("ldst__window is NOT found!"));
            return;
        }

        List<HtmlNode> entryNodes = HtmlParserHelper.GetNodes(listNode, "entry");
        if (entryNodes.Count == 0)
        {
            onFailure?.Invoke(new Exception("entry is NOT found!"));
            return;
        }

        foreach (HtmlNode entryNode in entryNodes)
        {
            string? imageURL = null;

            HtmlNode? cNode = HtmlParserHelper.GetNode(entryNode, "entry__chara__face");
            if (cNode == null) continue;

            if (cNode.ChildNodes.Count != 0)
            {
                HtmlNode img = cNode.ChildNodes[0];
                if (img != null)
                {
                    imageURL = img.GetAttributeValue("src", string.Empty);
                }
            }

            if (imageURL == null) continue;

            HtmlNode? nameNode = HtmlParserHelper.GetNode(entryNode, "entry__name");
            if (nameNode == null) continue;

            string userName = HttpUtility.HtmlDecode(nameNode.InnerText);

            HtmlNode? worldNode = HtmlParserHelper.GetNode(entryNode, "entry__world");
            if (worldNode == null) continue;

            string[] splitArr = worldNode.InnerText.Split(' ');
            if (splitArr.Length != 2) continue;

            string worldName = splitArr[0];

            uint? worldID = Sheets.GetWorldID(worldName);
            if (worldID == null) continue;

            HtmlNode? linkNode = HtmlParserHelper.GetNode(entryNode, "entry__link");
            if (linkNode == null) continue;

            string IDvalue = linkNode.GetAttributeValue("href", string.Empty);

            uint? lodestoneID = HtmlParserHelper.GetValueFromDashedLink(IDvalue);
            if (lodestoneID == null) continue;

            onSuccess?.Invoke(new LodestoneParseData(userName, worldID.Value, lodestoneID.Value, imageURL));
        }
    }
}
