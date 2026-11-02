using AcquisitionDate.Database.Interfaces;
using AcquisitionDate.LodestoneNetworking.Interfaces;
using AcquisitionDate.LodestoneRequests.Requests;
using AcquisitionDate.Parser.Interfaces;
using AcquisitionDate.Updating.Interfaces;

namespace AcquisitionDate.Updating.Updatables;

internal class LodestoneIDHelper : IUpdatable
{
    const float lodestoneIDCooldown = 30;

    readonly ILodestoneNetworker LodestoneNetworker;
    readonly IDatabase Database;
    readonly IAcquisitionParser AcquisitionParser;

    float searchTimer = 0;

    public LodestoneIDHelper(IAcquisitionParser parser, ILodestoneNetworker networker, IDatabase database)
    {
        AcquisitionParser = parser;
        LodestoneNetworker = networker;
        Database = database;
    }

    public void Update(float deltaTime)
    {
        searchTimer -= deltaTime;
        if (searchTimer < 0) searchTimer = 0;
        if (searchTimer > 0) return;

        IDatableData? data = Database.GetLocalUser();
        
        if (data == null) return;
        if (data.HasSearchedLodestoneId) return;

        searchTimer = lodestoneIDCooldown;

        LodestoneNetworker.AddElementToQueue
        (
            new LodestoneIDRequest
            (
                AcquisitionParser.LodestoneIDParser,
                data
            )
        );
    }
}
