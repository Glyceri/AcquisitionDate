using AcquisitionDate.Acquisition.Elements;
using AcquisitionDate.Acquisition.Interfaces;
using AcquisitionDate.LodestoneNetworking.Interfaces;
using AcquisitionDate.Parser.Interfaces;
using AcquisitionDate.Services.Interfaces;
using System.Collections.Generic;

namespace AcquisitionDate.Acquisition;

internal class AcquirerHandler : IAcquirerHandler
{
    private readonly ILodestoneNetworker  LodestoneNetworker;
    private readonly IAcquisitionServices Services;
    private readonly IAcquisitionParser   Parser;
    
    public IAcquirer MinionAcquirer      { get; private set; } = null!;
    public IAcquirer MountAcquirer       { get; private set; } = null!;
    public IAcquirer FacewearAcquirer    { get; private set; } = null!;
    public IAcquirer AchievementAcquirer { get; private set; } = null!;
    public IAcquirer QuestAcquirer       { get; private set; } = null!;

    private List<IAcquirer> _acquirers = [];
    
    public AcquirerHandler(IAcquisitionServices services, ILodestoneNetworker networker, IAcquisitionParser acquistionParser)
    {
        Services           = services;
        LodestoneNetworker = networker;
        Parser             = acquistionParser;

        Setup();
    }
    
    public void Dispose()
    {
        int acquirerCount = _acquirers.Count;

        for (int i = 0; i < acquirerCount; i++)
        {
            _acquirers[i].Dispose();
        }

        _acquirers.Clear();
    }

    private void Setup()
    {
        AddAcquirer(MinionAcquirer      = new MinionAcquirer(Services.Sheets, LodestoneNetworker, Parser));
        AddAcquirer(MountAcquirer       = new MountAcquirer(Services.Sheets, LodestoneNetworker, Parser));
        AddAcquirer(FacewearAcquirer    = new FacewearAcquirer(Services.Sheets, LodestoneNetworker, Parser));
        AddAcquirer(AchievementAcquirer = new AchievementAcquirer(LodestoneNetworker, Parser));
        AddAcquirer(QuestAcquirer       = new QuestAcquirer(LodestoneNetworker, Parser));
    }

    private void AddAcquirer(IAcquirer acquirer)
    {
        if (_acquirers.Contains(acquirer))
        {
            return;
        }

        _acquirers.Add(acquirer);
    }
}
