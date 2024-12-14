using System;

namespace AcquisitionDate.Windows.Interfaces;

internal interface IAcquisitionWindow : IDisposable
{
    void Open();
    void Close();
    void Toggle();
}
