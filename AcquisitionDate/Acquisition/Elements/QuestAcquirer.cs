using AcquisitionDate.Acquisition.Elements.Bases;
using AcquisitionDate.LodestoneData;
using AcquisitionDate.LodestoneNetworking.Interfaces;
using AcquisitionDate.LodestoneRequests.Interfaces;
using AcquisitionDate.LodestoneRequests.Requests;
using AcquisitionDate.Parser.Interfaces;

namespace AcquisitionDate.Acquisition.Elements;

internal class QuestAcquirer : AcquirerCounter
{
    public QuestAcquirer(ILodestoneNetworker networker, IAcquisitionParser acquistionParser) : base(networker, acquistionParser) { }

    protected override ILodestoneRequest PageCountRequest() => new QuestPageCountRequest
    (
        AcquisitionParser.AchievementListPageCountParser, 
        _currentUser, 
        OnPageCountData
    );

    protected override ILodestoneRequest DataRequest(int page) => new QuestDataRequest
    (
        AcquisitionParser.QuestListParser, 
        AcquisitionParser.QuestDataParser, 
        _currentUser, 
        page, 
        OnQuestData, 
        OnPageComplete, 
        OnFailure
    );

    void OnQuestData(QuestData data)
    {
        _currentUser.QuestList.SetDate(data.QuestID, data.AchievedDate, Database.Enums.AcquiredDateType.Lodestone);
    }

    protected override void OnDispose() { }
}
