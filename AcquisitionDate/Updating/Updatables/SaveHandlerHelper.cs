using AcquisitionDate.Serializiation;
using AcquisitionDate.Updating.Interfaces;

namespace AcquisitionDate.Updating.Updatables;

internal class SaveHandlerHelper : IUpdatable
{
    readonly SaveHandler SaveHandler;

    public SaveHandlerHelper(SaveHandler saveHandler)
    {
        SaveHandler = saveHandler;
    }

    public void Update(float deltaTime)
    {
       SaveHandler.Update(deltaTime);
    }
}
