using AcquisitionDate.Database.Interfaces;
using System;

namespace AcquisitionDate.ImageDatabase.Interfaces;

internal interface IImageReader : IDisposable
{
    bool ReadImage(IDatableData entry, IImageDatabase imageDatabase, Action<Exception> onFailure);
    string GetFilePath(IDatableData entry);
    string GetFilePath(string name, ushort homeworld);
}
