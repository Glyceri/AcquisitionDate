using AcquisitionDate.LodestoneNetworking.Interfaces;
using AcquisitionDate.Updating.Interfaces;

namespace AcquisitionDate.Updating.Updatables;

internal class LodestoneNetworkHelper : IUpdatable
{
    readonly ILodestoneNetworker LodestoneNetworker;

    public LodestoneNetworkHelper(ILodestoneNetworker networker)
    {
        LodestoneNetworker = networker;
    }

    public void Update(float deltaTime)
    {
        LodestoneNetworker.Update(deltaTime);
    }
}
