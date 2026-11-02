using AcquisitionDate.Commands.Interfaces;
using AcquisitionDate.Windows;

namespace AcquisitionDate.Commands.Commands.Base;

internal abstract class Command : ICommand
{
    public abstract string Description { get; }
    public abstract bool   ShowInHelp  { get; }
    public abstract string CommandCode { get; }
    
    protected readonly WindowHandler WindowHandler;

    protected Command(WindowHandler windowHandler)
    {
        WindowHandler = windowHandler;

        PluginHandlers.CommandManager.AddHandler(CommandCode, new Dalamud.Game.Command.CommandInfo(OnCommand)
        {
            HelpMessage = Description,
            ShowInHelp  = ShowInHelp,
        });
    }

    public void Dispose()
    {
        PluginHandlers.CommandManager.RemoveHandler(CommandCode);
    }
    
    public abstract void OnCommand(string command, string args);
}
