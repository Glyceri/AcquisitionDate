using AcquisitionDate.Database.Interfaces;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using System;

namespace AcquisitionDate.DatableUsers.Interfaces;

internal unsafe interface IDatableUser : IDisposable
{
    BattleChara* Self { get; }

    string Name { get; }
    ushort Homeworld { get; }
    ulong ContentID { get; }
    ulong? LodestoneID { get; }

    bool IsReady { get; }

    IDatableData Data { get; }
}
