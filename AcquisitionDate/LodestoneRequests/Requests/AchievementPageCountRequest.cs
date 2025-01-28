using AcquisitionDate.Database.Interfaces;
using AcquisitionDate.LodestoneData;
using AcquisitionDate.LodestoneRequests.Requests.Abstractions.PageCount;
using AcquisitionDate.Parser.Elements;
using System;

namespace AcquisitionDate.LodestoneRequests.Requests;

internal class AchievementPageCountRequest : AchievementTypePageCountRequest
{
    public AchievementPageCountRequest(AchievementListPageCountParser countParser, IDatableData data, Action<PageCountData?> pageCountCallback) : base(countParser, data, pageCountCallback) { }

    public override string GetURL() => base.GetURL() + "achievement/?page=1#anchor_achievement";
}
