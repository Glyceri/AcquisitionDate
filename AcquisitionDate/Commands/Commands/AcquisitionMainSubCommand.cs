using AcquisitionDate.Windows;

namespace AcquisitionDate.Commands.Commands;

internal class AcquisitionMainSubCommand : AcquisitionMainCommand
{
    public AcquisitionMainSubCommand(WindowHandler windowHandler) 
        : base(windowHandler) 
        { }
    
    public override string CommandCode 
        => "/acqdate";
}
