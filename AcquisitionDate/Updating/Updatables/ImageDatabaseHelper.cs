using AcquisitionDate.ImageDatabase.Interfaces;
using AcquisitionDate.Updating.Interfaces;

namespace AcquisitionDate.Updating.Updatables;

internal class ImageDatabaseHelper : IUpdatable
{
    readonly IImageDatabase ImageDatabase;

    public ImageDatabaseHelper(IImageDatabase imageDatabase)
    {
        ImageDatabase = imageDatabase;
    }

    public void Update(float deltaTime)
    {
        ImageDatabase.Update(deltaTime);
    }
}
