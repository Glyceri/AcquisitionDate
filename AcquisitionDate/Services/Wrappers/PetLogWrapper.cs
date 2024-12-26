using Dalamud.Plugin.Services;
using AcquisitionDate.AcquisitionDate.Services.ServiceWrappers.Interfaces;
using System;

namespace AcquisitionDate.AcquisitionDate.Services.ServiceWrappers;

internal class PetLogWrapper : IPetLog
{
    public static IPetLog? Instance { get; private set; }

    readonly IPluginLog PluginLog;

    public PetLogWrapper(IPluginLog pluginLog) 
    {
        PluginLog = pluginLog;
        Instance = this;
    }

    public void Log(object? message)
    {
        if (message == null) return;
        PluginLog.Debug($"{message}");
    }

    public void LogError(Exception e, object? message)
    {
        if (message == null) return;
        PluginLog.Error($"{e} : {message}");
    }

    public void LogException(Exception e)
    {
        PluginLog.Error($"{e}");
    }

    public void LogFatal(object? message)
    {
        if (message == null) return;
        PluginLog.Fatal($"{message}");
    }

    public void LogInfo(object? message)
    {
        if (message == null) return;
        PluginLog.Info($"{message}");
    }

    public void LogVerbose(object? message)
    {
        if (message == null) return;
        PluginLog.Verbose($"{message}");
    }

    public void LogWarning(object? message)
    {
        if (message == null) return;
        PluginLog.Warning($"{message}");
    }
}
