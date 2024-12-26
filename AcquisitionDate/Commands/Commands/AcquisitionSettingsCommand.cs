using AcquisitionDate.AcquisitionDate.Commands.Commands.Base;
using AcquisitionDate.Windows;
using AcquisitionDate.Windows.Windows;

namespace AcquisitionDate.AcquisitionDate.Commands.Commands;

internal class AcquisitionSettingsCommand : Command
{
    public AcquisitionSettingsCommand(WindowHandler windowHandler) : base(windowHandler) { }

    public override string CommandCode { get; } = "/acquisitionsettings";
    public override string Description { get; } = "Opens the Acquisition Date Settings Window";
    public override bool ShowInHelp { get; } = true;

    public override void OnCommand(string command, string args)
    {
        WindowHandler.OpenWindow<AcquisitionConfigWindow>();
    }
}
