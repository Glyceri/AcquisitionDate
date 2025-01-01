using AcquisitionDate.AcquisitionDate.Commands.Commands;
using AcquisitionDate.Windows;

namespace AcquisitionDate.Commands.Commands;

internal class AcquisitionMainSubCommand : AcquisitionMainCommand
{
    public override string CommandCode => "/acqd";

    public AcquisitionMainSubCommand(WindowHandler windowHandler) : base(windowHandler) { }
}
