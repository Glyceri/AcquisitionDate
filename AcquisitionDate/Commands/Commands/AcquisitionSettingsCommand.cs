using AcquisitionDate.Commands.Commands.Base;
using AcquisitionDate.Windows;
using AcquisitionDate.Windows.Windows;

namespace AcquisitionDate.Commands.Commands;

internal class AcquisitionSettingsCommand : Command
{
    public AcquisitionSettingsCommand(WindowHandler windowHandler) 
        : base(windowHandler) 
        { }

    public override string CommandCode
        => "/acquisitionsettings";
    
    public override string Description
        => "Opens the Acquisition Date Settings Window";
    
    public override bool ShowInHelp
        => true;

    public override void OnCommand(string command, string args) 
        => WindowHandler.OpenWindow<AcquisitionConfigWindow>();
}
