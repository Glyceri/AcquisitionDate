using System;

namespace AcquisitionDate.Acquisition.Interfaces;

internal interface IAcquirerHandler : IDisposable
{
    IAcquirer MinionAcquirer { get; }
    IAcquirer MountAcquirer { get; }
    IAcquirer FacewearAcquirer { get; }
    IAcquirer AchievementAcquirer { get; }
    IAcquirer QuestAcquirer { get; }
}
