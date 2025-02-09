using AcquisitionDate.HtmlParser;
using AcquisitionDate.Parser.Interfaces;
using AcquisitionDate.Parser.Structs;
using AcquisitionDate.Services.Interfaces;
using HtmlAgilityPack;
using System;
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

        HtmlNode? entryNode = HtmlParserHelper.GetNode(listNode, "entry");
        if (entryNode == null)
        {
            onFailure?.Invoke(new Exception("entry is NOT found!"));
            return;
        }

        string? imageURL = null;

        HtmlNode? cNode = HtmlParserHelper.GetNode(listNode, "entry__chara__face");
        if (cNode == null)
        {
            onFailure?.Invoke(new Exception("cNode is NOT found!"));
            return;
        }

        if (cNode.ChildNodes.Count != 0)
        {
            HtmlNode img = cNode.ChildNodes[0];
            if (img != null)
            { 
                imageURL = img.GetAttributeValue("src", string.Empty);
            }
        }

        if (imageURL == null)
        {
            onFailure?.Invoke(new Exception("imageURL is NOT found!"));
            return;
        }

        HtmlNode? nameNode = HtmlParserHelper.GetNode(entryNode, "entry__name");
        if (nameNode == null)
        {
            onFailure?.Invoke(new Exception("entry__name is NOT found!"));
            return;
        }

        string userName = HttpUtility.HtmlDecode(nameNode.InnerText);

        HtmlNode? worldNode = HtmlParserHelper.GetNode(entryNode, "entry__world");
        if (worldNode == null)
        {
            onFailure?.Invoke(new Exception("entry__world is NOT found!"));
            return;
        }

        string[] splitArr = worldNode.InnerText.Split(' ');
        if (splitArr.Length != 2)
        {
            onFailure?.Invoke(new Exception("splitArr length is NOT 2"));
            return;
        }

        string worldName = splitArr[0];

        uint? worldID = Sheets.GetWorldID(worldName);
        if (worldID == null)
        {
            onFailure?.Invoke(new Exception("worldID was NOT found in the sheets!"));
            return;
        }

        HtmlNode? linkNode = HtmlParserHelper.GetNode(entryNode, "entry__link");
        if (linkNode == null)
        {
            onFailure?.Invoke(new Exception("entry__link is NOT found!"));
            return;
        }

        string IDvalue = linkNode.GetAttributeValue("href", string.Empty);

        uint? lodestoneID = HtmlParserHelper.GetValueFromDashedLink(IDvalue);
        if (lodestoneID == null)
        {
            onFailure?.Invoke(new Exception("lodestoneID is NOT found!"));
            return;
        }

        onSuccess?.Invoke(new LodestoneParseData(userName, worldID.Value, lodestoneID.Value, imageURL));
    }
}
