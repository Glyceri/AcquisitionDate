using AcquisitionDate.Acquisition.Elements.Bases;
using AcquisitionDate.LodestoneData;
using AcquisitionDate.LodestoneNetworking.Interfaces;
using AcquisitionDate.LodestoneRequests.Interfaces;
using AcquisitionDate.LodestoneRequests.Requests;
using AcquisitionDate.Services.Interfaces;

namespace AcquisitionDate.Acquisition.Elements;

internal class MinionAcquirer : AcquirerItem
{
    // TEMPORARY
    public static MinionAcquirer Instance { get; private set; }

    public MinionAcquirer(ISheets sheets, ILodestoneNetworker networker) : base(sheets, networker)
    {
        Instance = this;
    }

    protected override ILodestoneRequest PageDataRequest(string page) => new MinionDataRequest(Sheets, page, OnItemData, UpCounterAndActivate, OnFailure);

    protected override ILodestoneRequest PageCountRequest() => new MinionPageListRequest(_currentUser, OnPageURLList);

    void OnItemData(ItemData itemData)
    {
        _currentUser.MinionList.SetDate(itemData.ItemID, itemData.AchievedDate, Database.Enums.AcquiredDateType.Lodestone);
    }

    protected override void OnDispose() { }
}
