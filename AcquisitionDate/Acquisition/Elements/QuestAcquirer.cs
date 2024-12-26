using AcquisitionDate.Acquisition.Elements.Bases;
using AcquisitionDate.LodestoneData;
using AcquisitionDate.LodestoneNetworking.Interfaces;
using AcquisitionDate.LodestoneRequests.Interfaces;
using AcquisitionDate.LodestoneRequests.Requests;
using AcquisitionDate.Services.Interfaces;

namespace AcquisitionDate.Acquisition.Elements;

internal class QuestAcquirer : AcquirerCounter
{
    readonly ISheets Sheets;

    public QuestAcquirer(ISheets sheets, ILodestoneNetworker networker) : base(networker)
    {
        Sheets = sheets;
    }

    protected override ILodestoneRequest PageCountRequest() => new QuestPageCountRequest(_currentUser, OnPageCountData);

    protected override ILodestoneRequest DataRequest(int page) => new QuestDataRequest(Sheets, _currentUser, page, OnQuestData, OnPageComplete, OnFailure);

    void OnQuestData(QuestData data)
    {
        _currentUser.QuestList.SetDate(data.QuestID, data.AchievedDate, Database.Enums.AcquiredDateType.Lodestone);
    }

    protected override void OnDispose() { }
}
