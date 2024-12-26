using AcquisitionDate.AcquisitionDate.Commands.Interfaces;
using AcquisitionDate.Core.Handlers;
using AcquisitionDate.Windows;

namespace AcquisitionDate.AcquisitionDate.Commands.Commands.Base;

internal abstract class Command : ICommand
{
    public abstract string Description { get; }
    public abstract bool ShowInHelp { get; }
    public abstract string CommandCode { get; }

    public abstract void OnCommand(string command, string args);

    protected readonly WindowHandler WindowHandler;

    public Command(WindowHandler windowHandler)
    {
        WindowHandler = windowHandler;

        PluginHandlers.CommandManager.AddHandler(CommandCode, new Dalamud.Game.Command.CommandInfo(OnCommand)
        {
            HelpMessage = Description,
            ShowInHelp = ShowInHelp,
        });
    }

    public void Dispose()
    {
        PluginHandlers.CommandManager.RemoveHandler(CommandCode);
    }
}
