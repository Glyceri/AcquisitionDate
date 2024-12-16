namespace AcquisitionDate.DirtySystem.Interfaces;

internal interface IDirtySetter
{
    void NotifyDirtyDatabase();
    void NotifyDirtyUser();
}
