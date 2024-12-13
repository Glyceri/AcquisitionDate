using AcquisitionDate.Core.Handlers;
using Dalamud.Configuration;
using System;

namespace AcquisitionDate;

[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 0;

    public void Save() => PluginHandlers.PluginInterface.SavePluginConfig(this);
}
