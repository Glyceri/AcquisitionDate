using AcquisitionDate.Core.Handlers;
using AcquisitionDate.Database.Interfaces;
using AcquisitionDate.Serializiation;
using Dalamud.Configuration;
using System;

namespace AcquisitionDate;

[Serializable]
internal class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 0;

    public SerializableUser[]? SerializableUsers { get; set; } = null;

    IDatabase? Database;
    bool hasSetUp = false;

    public void Initialise(IDatabase database)
    {
        Database = database;
        hasSetUp = true;
    }

    public void Save()
    {
        if (!hasSetUp) return;

        SerializableUsers = Database!.SerializeDatabase();

        PluginHandlers.PluginInterface.SavePluginConfig(this);

    }
}
