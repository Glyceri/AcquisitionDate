using AcquisitionDate.Acquisition.Elements.Bases;
using AcquisitionDate.LodestoneData;
using AcquisitionDate.LodestoneNetworking.Interfaces;
using AcquisitionDate.LodestoneRequests.Interfaces;
using AcquisitionDate.LodestoneRequests.Requests;
using AcquisitionDate.Parser.Interfaces;
using AcquisitionDate.Services.Interfaces;

namespace AcquisitionDate.Acquisition.Elements;

internal class MountAcquirer : AcquirerItem
{
    public MountAcquirer(ISheets sheets, ILodestoneNetworker networker, IAcquisitionParser acquistionParser) : base(sheets, networker, acquistionParser) { }

    protected override ILodestoneRequest PageDataRequest(string page) => new MountDataRequest
    (
        AcquisitionParser.ItemPageDataParser,
        Sheets,
        page,
        pageRegion,
        OnItemData,
        UpCounterAndActivate,
        OnFailure
    );

    protected override ILodestoneRequest PageCountRequest() => new MountPageListRequest
    (
        AcquisitionParser.ItemPageListParser,
        _currentUser, 
        OnPageURLList
    );

    void OnItemData(ItemData itemData)
    {
        _currentUser.MountList.SetDate(itemData.ItemID, itemData.AchievedDate, Database.Enums.AcquiredDateType.Lodestone);
    }

    protected override void OnDispose() { }
}
