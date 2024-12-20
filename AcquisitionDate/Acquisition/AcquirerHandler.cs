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

    public AcquirerHandler(IAcquisitionServices services, ILodestoneNetworker networker)
    {
        Services = services;
        LodestoneNetworker = networker;

        Setup();
    }

    void Setup()
    {
        AddAcquirer(new MinionAcquirer(Services.Sheets, LodestoneNetworker));    
        AddAcquirer(new AchievementAcquirer(LodestoneNetworker));
        AddAcquirer(new QuestAcquirer(Services, LodestoneNetworker));
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
