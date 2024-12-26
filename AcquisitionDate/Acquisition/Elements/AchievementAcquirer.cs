using AcquisitionDate.Acquisition.Elements.Bases;
using AcquisitionDate.LodestoneData;
using AcquisitionDate.LodestoneNetworking.Interfaces;
using AcquisitionDate.LodestoneRequests.Interfaces;
using AcquisitionDate.LodestoneRequests.Requests;

namespace AcquisitionDate.Acquisition.Elements;

internal class AchievementAcquirer : AcquirerCounter
{
    public AchievementAcquirer(ILodestoneNetworker networker) : base(networker) { }

    protected override ILodestoneRequest PageCountRequest() => new AchievementPageCountRequest(_currentUser, OnPageCountData);

    protected override ILodestoneRequest DataRequest(int page) => new AchievementDateRequest(_currentUser, page, OnAchievementData, OnPageComplete, OnFailure);
    
    void OnAchievementData(AchievementData data) 
    {
        _currentUser?.AchievementList.SetDate(data.AchievementID, data.AchievedDate, Database.Enums.AcquiredDateType.Lodestone);
    }

    protected override void OnDispose() { }
}
