using AcquisitionDate.Database.Interfaces;
using AcquisitionDate.LodestoneData;
using AcquisitionDate.LodestoneRequests.Requests.Abstractions.PageCount;
using System;

namespace AcquisitionDate.LodestoneRequests.Requests;

internal class QuestPageCountRequest : AchievementTypePageCountRequest
{
    public QuestPageCountRequest(IDatableData data, Action<PageCountData?> pageCountCallback) : base(data, pageCountCallback) { }

    public override string GetURL() => base.GetURL() + "quest/?page=1#anchor_quest";
}
