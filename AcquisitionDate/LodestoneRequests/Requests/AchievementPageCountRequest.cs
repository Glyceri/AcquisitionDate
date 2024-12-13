using AcquisitionDate.LodestoneData;
using AcquisitionDate.LodestoneRequests.Requests.Abstractions.PageCount;
using System;

namespace AcquisitionDate.LodestoneRequests.Requests;

internal class AchievementPageCountRequest : AchievementTypePageCountRequest
{
    public AchievementPageCountRequest(int lodestoneCharacterID, Action<PageCountData?> pageCountCallback) : base(lodestoneCharacterID, pageCountCallback) { }

    public override string GetURL() => base.GetURL() + "achievement/?page=1#anchor_achievement";
}
