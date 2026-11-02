using AcquisitionDate.Commands.Commands.Base;
using AcquisitionDate.Windows;
using AcquisitionDate.Windows.Windows;

namespace AcquisitionDate.Commands.Commands;

internal class DevCommand : Command
{
    public DevCommand(WindowHandler windowHandler) 
        : base(windowHandler) 
        { }

    public override string CommandCode
        => "/acqdev";
    
    public override string Description 
        => "Opens the Acquisition Date Dev Window";
    
    public override bool ShowInHelp 
        => false;

    public override void OnCommand(string command, string args)
        => WindowHandler.OpenWindow<AcquisitionDebugWindow>();
}
