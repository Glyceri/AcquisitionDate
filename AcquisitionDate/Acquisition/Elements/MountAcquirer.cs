﻿using AcquisitionDate.Acquisition.Elements.Bases;
using AcquisitionDate.LodestoneData;
using AcquisitionDate.LodestoneNetworking.Interfaces;
using AcquisitionDate.LodestoneRequests.Interfaces;
using AcquisitionDate.LodestoneRequests.Requests;
using AcquisitionDate.Services.Interfaces;

namespace AcquisitionDate.Acquisition.Elements;

internal class MountAcquirer : AcquirerItem
{
    public MountAcquirer(ISheets sheets, ILodestoneNetworker networker) : base(sheets, networker) { }

    protected override ILodestoneRequest PageDataRequest(string page) => new MountDataRequest(Sheets, page, OnItemData, UpCounterAndActivate, OnFailure);

    protected override ILodestoneRequest PageCountRequest() => new MountPageListRequest(_currentUser, OnPageURLList);

    void OnItemData(ItemData itemData)
    {
        _currentUser.MountList.SetDate(itemData.ItemID, itemData.AchievedDate, Database.Enums.AcquiredDateType.Lodestone);
    }

    protected override void OnDispose() { }
}
