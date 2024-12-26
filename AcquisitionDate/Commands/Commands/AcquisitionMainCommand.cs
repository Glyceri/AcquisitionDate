using AcquisitionDate.AcquisitionDate.Commands.Commands.Base;
using AcquisitionDate.Windows;
using AcquisitionDate.Windows.Windows;

namespace AcquisitionDate.AcquisitionDate.Commands.Commands;

internal class AcquisitionMainCommand : Command
{
    public AcquisitionMainCommand(WindowHandler windowHandler) : base(windowHandler) { }

    public override string CommandCode { get; } = "/acquisitiondate";
    public override string Description { get; } = "Opens the Acquisition Date window.";
    public override bool ShowInHelp { get; } = true;

    public override void OnCommand(string command, string args)
    {
        WindowHandler.OpenWindow<AcquiryWindow>();
    }
}
