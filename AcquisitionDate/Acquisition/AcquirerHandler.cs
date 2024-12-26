using AcquisitionDate.Acquisition.Elements;
using AcquisitionDate.Acquisition.Interfaces;
using AcquisitionDate.LodestoneNetworking.Interfaces;
using AcquisitionDate.Services.Interfaces;
using System.Collections.Generic;

namespace AcquisitionDate.Acquisition;

internal class AcquirerHandler : IAcquirerHandler
{
    readonly ILodestoneNetworker LodestoneNetworker;
    readonly IAcquisitionServices Services;

    List<IAcquirer> acquirers = new List<IAcquirer>();

    public IAcquirer MinionAcquirer { get; private set; } = null!;
    public IAcquirer MountAcquirer { get; private set; } = null!;
    public IAcquirer FacewearAcquirer { get; private set; } = null!;
    public IAcquirer AchievementAcquirer { get; private set; } = null!;
    public IAcquirer QuestAcquirer { get; private set; } = null!;

    public AcquirerHandler(IAcquisitionServices services, ILodestoneNetworker networker)
    {
        Services = services;
        LodestoneNetworker = networker;

        Setup();
    }

    void Setup()
    {
        AddAcquirer(MinionAcquirer = new MinionAcquirer(Services.Sheets, LodestoneNetworker));
        AddAcquirer(MountAcquirer = new MountAcquirer(Services.Sheets, LodestoneNetworker));
        AddAcquirer(FacewearAcquirer = new FacewearAcquirer(Services.Sheets, LodestoneNetworker));
        AddAcquirer(AchievementAcquirer = new AchievementAcquirer(LodestoneNetworker));
        AddAcquirer(QuestAcquirer = new QuestAcquirer(Services.Sheets, LodestoneNetworker));
    }

    void AddAcquirer(IAcquirer acquirer)
    {
        if (acquirer == null) return;
        if (acquirers.Contains(acquirer)) return;

        acquirers.Add(acquirer);
    }

    public void Dispose()
    {
        int acquirerCount = acquirers.Count;

        for (int i = 0; i < acquirerCount; i++)
        {
            acquirers[i].Dispose();
        }

        acquirers.Clear();
    }
}
