using AcquisitionDate.LodestoneData;
using AcquisitionDate.LodestoneRequests.Requests.Abstractions.PageCount;
using System;

namespace AcquisitionDate.LodestoneRequests.Requests;

internal class QuestPageCountRequest : AchievementTypePageCountRequest
{
    public QuestPageCountRequest(int lodestoneCharacterID, Action<PageCountData?> pageCountCallback) : base(lodestoneCharacterID, pageCountCallback) { }

    public override string GetURL() => base.GetURL() + "quest/?page=1#anchor_quest";
}
