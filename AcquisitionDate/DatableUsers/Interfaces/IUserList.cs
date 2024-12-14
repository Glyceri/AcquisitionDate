using FFXIVClientStructs.FFXIV.Client.Game.Character;

namespace AcquisitionDate.DatableUsers.Interfaces;

internal unsafe interface IUserList
{
    IDatableUser? ActiveUser { get; }

    void Create(BattleChara* bChara);
    void Delete(BattleChara* bChara);
}
