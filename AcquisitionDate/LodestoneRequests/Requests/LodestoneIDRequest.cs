using AcquisitionDate.Core.Handlers;
using AcquisitionDate.Database.Interfaces;
using AcquisitionDate.Parser.Elements;
using AcquisitionDate.Parser.Structs;
using HtmlAgilityPack;
using System;
using System.Web;

namespace AcquisitionDate.LodestoneRequests.Requests;

internal class LodestoneIDRequest : LodestoneRequest
{
    protected readonly IDatableData Data;
    readonly LodestoneIDParser LodestoneIDParser;

    public LodestoneIDRequest(LodestoneIDParser lodestoneIDParser, IDatableData datableData)
    {
        LodestoneIDParser = lodestoneIDParser;
        Data = datableData;
    }    

    public override void HandleFailure(Exception exception)
    {
        PluginHandlers.PluginLog.Error("Error in finding lodestone ID", exception);
    }

    public override void HandleSuccess(HtmlDocument document)
    {
        HtmlNode rootNode = document.DocumentNode;

        LodestoneIDParser.Parse(rootNode, OnData, HandleFailure);
    }

    protected virtual void OnData(LodestoneParseData lodestoneData)
    {
        if (!Data.Name.Equals(lodestoneData.UserName, StringComparison.InvariantCultureIgnoreCase) || Data.Homeworld != (ushort)lodestoneData.Homeworld) return;

        Data.SetLodestoneID(lodestoneData.LodestoneID);
    }

    public override string GetURL()
    {
        PluginHandlers.PluginLog.Verbose($"Searching for user with the url: '/lodestone/character/?q={HttpUtility.UrlEncode(Data.Name.Replace(" ", "+"))}&worldname={Data.HomeworldName}'");
        return $"/lodestone/character/?q={HttpUtility.UrlEncode(Data.Name.Replace(" ", "+"))}&worldname={Data.HomeworldName}";
    }
}
