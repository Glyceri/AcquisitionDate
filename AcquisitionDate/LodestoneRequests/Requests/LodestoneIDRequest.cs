using AcquisitionDate.Core.Handlers;
using AcquisitionDate.Database.Interfaces;
using AcquisitionDate.HtmlParser;
using AcquisitionDate.Services.Interfaces;
using HtmlAgilityPack;
using System;
using System.Web;

namespace AcquisitionDate.LodestoneRequests.Requests;

internal class LodestoneIDRequest : LodestoneRequest
{
    readonly IDatableData Data;
    readonly ISheets Sheets;

    public LodestoneIDRequest(IDatableData datableData, ISheets sheets)
    {
        Data = datableData;
        Sheets = sheets;
    }    

    public override void HandleFailure(Exception exception)
    {
        // Maybe Don't do this... idk yet
        Data.HasSearchedLodestoneID = true;
    }

    public override void HandleSuccess(HtmlDocument document)
    {
        HtmlNode rootNode = document.DocumentNode;

        HtmlNode? listNode = HtmlParserHelper.GetNode(rootNode, "ldst__window");
        if (listNode == null) return;

        HtmlNode? entryNode = HtmlParserHelper.GetNode(listNode, "entry");
        if (entryNode == null) return;

        HtmlNode? nameNode = HtmlParserHelper.GetNode(entryNode, "entry__name");
        if (nameNode == null) return;

        string userName = nameNode.InnerText;

        HtmlNode? worldNode = HtmlParserHelper.GetNode(entryNode, "entry__world");
        if (worldNode == null) return;

        string[] splitArr = worldNode.InnerText.Split(' ');
        if (splitArr.Length != 2) return;

        string worldName = splitArr[0];

        uint? worldID = Sheets.GetWorldID(worldName);
        if (worldID == null) return;

        HtmlNode? linkNode = HtmlParserHelper.GetNode(entryNode, "entry__link");
        if (linkNode == null) return;

        string IDvalue = linkNode.GetAttributeValue("href", string.Empty);

        uint? lodestoneID = HtmlParserHelper.GetValueFromDashedLink(IDvalue);
        if (lodestoneID == null) return;

        if (!Data.Name.Equals(userName, StringComparison.InvariantCultureIgnoreCase) || Data.Homeworld != (ushort)worldID.Value) return;

        Data.SetLodestoneID(lodestoneID.Value);
    }

    public override string GetURL()
    {
        PluginHandlers.PluginLog.Verbose($"/lodestone/character/?q={HttpUtility.UrlEncode(Data.Name.Replace(" ", "+"))}&worldname={Data.HomeworldName}");
        return $"/lodestone/character/?q={HttpUtility.UrlEncode(Data.Name.Replace(" ", "+"))}&worldname={Data.HomeworldName}";
    }
}
