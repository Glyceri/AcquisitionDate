using AcquisitionDate.AcquisitionDate.Commands.Commands.Base;
using AcquisitionDate.Windows;
using AcquisitionDate.Windows.Windows;

namespace AcquisitionDate.AcquisitionDate.Commands.Commands;

internal class DevCommand : Command
{
    readonly Configuration Configuration;

    public DevCommand(Configuration configuration, WindowHandler windowHandler) : base(windowHandler) 
    { 
        Configuration = configuration;
    }

    public override string CommandCode { get; } = "/acquisitiondev";
    public override string Description { get; } = "Opens the Acquisition Date Dev Window";
    public override bool ShowInHelp { get; } = false;

    public override void OnCommand(string command, string args)
    {
        if (Configuration.debugModeActive)
        {
            WindowHandler.OpenWindow<AcquisitionDebugWindow>();
        }
    }
}
