using AcquisitionDate.DatableUsers.Interfaces;
using AcquisitionDate.LodestoneNetworking.Interfaces;
using AcquisitionDate.LodestoneRequests.Requests;
using AcquisitionDate.Parser.Interfaces;
using AcquisitionDate.Updating.Interfaces;

namespace AcquisitionDate.Updating.Updatables;

internal class LodestoneIDHelper : IUpdatable
{
    const float lodestoneIDCooldown = 30;

    readonly ILodestoneNetworker LodestoneNetworker;
    readonly IUserList UserList;
    readonly IAcquisitionParser AcquisitionParser;

    float searchTimer = 0;

    public LodestoneIDHelper(IAcquisitionParser parser, ILodestoneNetworker networker, IUserList userList)
    {
        AcquisitionParser = parser;
        LodestoneNetworker = networker;
        UserList = userList;
    }

    public void Update(float deltaTime)
    {
        searchTimer -= deltaTime;
        if (searchTimer < 0) searchTimer = 0;
        if (searchTimer > 0) return;

        if (UserList.ActiveUser == null) return;
        if (UserList.ActiveUser.Data.HasSearchedLodestoneID) return;

        searchTimer = lodestoneIDCooldown;

        LodestoneNetworker.AddElementToQueue
        (
            new LodestoneIDRequest
            (
                AcquisitionParser.LodestoneIDParser,
                UserList.ActiveUser.Data
            )
        );
    }
}
