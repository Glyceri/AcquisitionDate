using AcquisitionDate.Database.Interfaces;
using System;

namespace AcquisitionDate.Acquisition.Interfaces;

internal interface IAcquirer : IDisposable
{
    bool IsAcquiring { get; }
    byte CompletionRate { get; }

    void Acquire(IDatableData user);
    void Cancel();
}
