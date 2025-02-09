using AcquisitionDate.Core.Handlers;
using AcquisitionDate.Database.Interfaces;
using AcquisitionDate.DatableUsers.Interfaces;
using AcquisitionDate.ImageDatabase.Interfaces;
using AcquisitionDate.Windows.Components.Image;
using System.Numerics;

namespace AcquisitionDate.Windows.Windows;

internal unsafe class AcquisitionListWindow : AcquisitionWindow
{
    protected override Vector2 MinSize { get; } = new Vector2(400, 250);
    protected override Vector2 MaxSize { get; } = new Vector2(1600, 1500);
    protected override Vector2 DefaultSize { get; } = new Vector2(800, 500);

    protected override bool HasHeaderBar { get; } = true;

    readonly IUserList UserList;
    readonly IDatabase Database;
    readonly IImageDatabase ImageDatabase;

    public AcquisitionListWindow(WindowHandler windowHandler, Configuration configuration, IUserList userList, IDatabase database, IImageDatabase imageDatabase) : base(windowHandler, configuration, "Acquisition List")
    {
        Open();

        UserList = userList;
        Database = database;
        ImageDatabase = imageDatabase;
    }

    protected override void OnDraw()
    {
        foreach (IDatableData data in Database.GetEntries())
        {
            //PluginHandlers.PluginLog.Debug($"Tries to draw Entry: {data.Name}@{data.HomeworldName}");
            PlayerImage.Draw(data, ImageDatabase);
        }
    }

    protected override void OnDispose()
    {

    }
}
