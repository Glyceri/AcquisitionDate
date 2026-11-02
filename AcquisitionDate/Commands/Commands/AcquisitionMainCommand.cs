using AcquisitionDate.Commands.Commands.Base;
using AcquisitionDate.Windows;
using AcquisitionDate.Windows.Windows;

namespace AcquisitionDate.Commands.Commands;

internal class AcquisitionMainCommand : Command
{
    public AcquisitionMainCommand(WindowHandler windowHandler) 
        : base(windowHandler) 
        { }

    public override string CommandCode 
        => "/acquisitiondate";
    
    public override string Description
        => "Opens the Acquisition Date window.";
    
    public override bool ShowInHelp 
        => true;

    public override void OnCommand(string command, string args) 
        => WindowHandler.OpenWindow<AcquiryWindow>();
}
