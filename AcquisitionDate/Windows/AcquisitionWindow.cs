using Dalamud.Interface.Windowing;
using ImGuiNET;
using AcquisitionDate.Windows.Interfaces;
using System.Numerics;
using Dalamud.Interface.Utility;
using AcquistionDate.PetNicknames.Windowing.Components.Header;

namespace AcquisitionDate.Windows;

internal abstract class AcquisitionWindow : Window, IAcquisitionWindow
{
    protected abstract Vector2 MinSize { get; }
    protected abstract Vector2 MaxSize { get; }
    protected abstract Vector2 DefaultSize { get; }

    protected abstract bool HasHeaderBar { get; }

    protected readonly WindowHandler WindowHandler;
    protected readonly Configuration Configuration;

    public AcquisitionWindow(WindowHandler windowHandler, Configuration configuration, string name, ImGuiWindowFlags flags = ImGuiWindowFlags.NoCollapse, bool forceMainWindow = false) : base(name, flags, forceMainWindow) 
    {
        WindowHandler = windowHandler;
        Configuration = configuration;

        SizeCondition = ImGuiCond.FirstUseEver;
        Size = DefaultSize;

        SizeConstraints = new WindowSizeConstraints()
        {
            MinimumSize = MinSize,
            MaximumSize = MaxSize,
        };
    }

    readonly Vector2 windowPadding = new(8, 8);
    readonly Vector2 framePadding = new(4, 3);
    readonly Vector2 itemInnerSpacing = new(4, 4);
    readonly Vector2 itemSpacing = new(4, 4);

    public sealed override void PreDraw()
    {
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, windowPadding * ImGuiHelpers.GlobalScale);
        ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, framePadding * ImGuiHelpers.GlobalScale);
        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, itemSpacing * ImGuiHelpers.GlobalScale);
        ImGui.PushStyleVar(ImGuiStyleVar.ItemInnerSpacing, itemInnerSpacing * ImGuiHelpers.GlobalScale);

        OnEarlyDraw();
    }

    public sealed override void PostDraw()
    {
        OnLateDraw();
        ImGui.PopStyleVar(4);
    }

    public sealed override void Draw()
    {
        if (HasHeaderBar) HeaderBar.Draw(WindowHandler, Configuration, this);
        OnDraw();
    }

    protected virtual void OnEarlyDraw() { }
    protected virtual void OnDraw() { }
    protected virtual void OnLateDraw() { }
    protected virtual void OnDispose() { }

    public void Close() => IsOpen = false;
    public void Open() => IsOpen = true;

    public void Dispose() => OnDispose();
}
