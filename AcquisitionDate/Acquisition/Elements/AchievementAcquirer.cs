using AcquisitionDate.Acquisition.Elements.Bases;
using AcquisitionDate.Database.Enums;
using AcquisitionDate.LodestoneData;
using AcquisitionDate.LodestoneNetworking.Interfaces;
using AcquisitionDate.LodestoneRequests.Interfaces;
using AcquisitionDate.LodestoneRequests.Requests;
using AcquisitionDate.Parser.Interfaces;

namespace AcquisitionDate.Acquisition.Elements;

internal class AchievementAcquirer : AcquirerCounter
{
    public AchievementAcquirer(ILodestoneNetworker networker, IAcquisitionParser acquistionParser)
        : base(networker, acquistionParser) { }

    protected override ILodestoneRequest PageCountRequest() => new AchievementPageCountRequest
    (
        AcquisitionParser.AchievementListPageCountParser, 
        _currentUser, 
        OnPageCountData
    );

    protected override ILodestoneRequest DataRequest(int page) => new AchievementDateRequest
    (
        AcquisitionParser.AchievementListParser, 
        AcquisitionParser.AchievementElementParser, 
        _currentUser, 
        page, 
        pageRegion,
        OnAchievementData, 
        OnPageComplete, 
        OnFailure
    );
    
    private void OnAchievementData(AchievementData data) 
    {
        PluginHandlers.PluginLog.Debug("Just got achievement data: " + data.AchievementID);
        
        // This is the chocobo quest, a failsafe because chocobo mount is peepeepoopoo
        if (data.AchievementID == 66698)
        {
            _currentUser?.GetDateList(AcquirableDateType.Mount).SetDate(1, data.AchievedDate, Database.Enums.AcquiredDateType.Lodestone);
        }

        _currentUser?.GetDateList(AcquirableDateType.Achievement).SetDate(data.AchievementID, data.AchievedDate, Database.Enums.AcquiredDateType.Lodestone);
    }

    protected override void OnDispose() { }
}
