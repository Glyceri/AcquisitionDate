using AcquisitionDate.Database.Enums;
using AcquisitionDate.Database.Interfaces;
using AcquisitionDate.Services.Interfaces;

namespace AcquisitionDate.Windows.Windows.ListTabs;

internal abstract class DataTab
{
    protected readonly ISheets Sheets;

    public DataTab(ISheets sheets)
    {
        Sheets = sheets;
    }

    public abstract AcquirableDateType MyType { get; }

    public abstract void Draw(IDatableList dataList, string searchComment);
}
