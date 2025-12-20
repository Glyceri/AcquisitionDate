using AcquisitionDate.HtmlParser;
using AcquisitionDate.LodestoneData;
using AcquisitionDate.LodestoneNetworking.Enums;
using AcquisitionDate.Parser.Interfaces;
using HtmlAgilityPack;
using System;
using System.Web;

namespace AcquisitionDate.Parser.Elements;

internal class ItemPageDataParser : IItemPageDataParser<ItemData>
{
    string _listIconName = string.Empty;
    Func<string, uint?>? _getIDFromString = null;
    LodestoneRegion _region = LodestoneRegion.Europe;

    public void Parse(HtmlNode rootNode, Action<ItemData> onSuccess, Action<Exception> onFailure)
    {
        HtmlNode? labelNode = HtmlParserHelper.GetNode(rootNode, $"{_listIconName}__header__label");
        if (labelNode == null)
        {
            onFailure?.Invoke(new Exception($"{_listIconName}__header__label was NOT found!"));
            return;
        }

        string itemName = HttpUtility.HtmlDecode(labelNode.InnerText);

        uint? itemID = _getIDFromString?.Invoke(itemName);
        if (itemID == null)
        {
            onFailure?.Invoke(new Exception($"itemID was NOT found!"));
            return;
        }

        HtmlNode? timeNode = HtmlParserHelper.GetNode(rootNode, $"{_listIconName}__header__data");
        if (timeNode == null)
        {
            onFailure?.Invoke(new Exception($"timeNode was NOT found!"));
            return;
        }

        DateTime? acquiredTime = HtmlParserHelper.GetAcquiredTime(timeNode, _region);
        if (acquiredTime == null)
        {
            onFailure?.Invoke(new Exception($"acquiredTime was NOT found!"));
            return;
        }

        onSuccess?.Invoke(new ItemData(itemID.Value, acquiredTime.Value));
    }

    public void SetGetIDFunc(Func<string, uint?> getIDFromString)
    {
        _getIDFromString = getIDFromString;
    }

    public void SetListIconName(string listIconName)
    {
        _listIconName = listIconName;
    }

    public void SetPageLanguage(LodestoneRegion region)
    {
        _region = region;
    }
}
