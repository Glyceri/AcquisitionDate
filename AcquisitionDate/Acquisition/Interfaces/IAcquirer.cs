using AcquisitionDate.Database.Interfaces;
using System;

namespace AcquisitionDate.Acquisition.Interfaces;

internal interface IAcquirer : IDisposable
{
    bool HasSucceeded { get; }
    bool IsAcquiring { get; }
    byte CompletionRate { get; }
    string? AcquisitionError { get; }

    void Acquire(IDatableData user);
    void Cancel();
}
