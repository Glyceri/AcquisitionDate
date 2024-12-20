using AcquisitionDate.LodestoneData;
using AcquisitionDate.LodestoneNetworking.Interfaces;
using AcquisitionDate.LodestoneRequests.Interfaces;
using AcquisitionDate.Services.Interfaces;

namespace AcquisitionDate.Acquisition.Elements.Bases;

internal abstract class AcquirerItem : AcquirerCounter
{
    protected readonly ISheets Sheets;

    string[] urls = [];

    protected AcquirerItem(ISheets sheets, ILodestoneNetworker networker) : base(networker)
    {
        Sheets = sheets;
    }

    protected sealed override ILodestoneRequest DataRequest(int page) => PageDataRequest(urls[page]);
    protected abstract ILodestoneRequest PageDataRequest(string page);

    protected override void ResetCounter()
    {
        base.ResetCounter();
        urls = [];
    }

    protected void OnPageURLList(PageURLListData? data)
    {
        SetPercentage(10);
        if (data == null)
        {
            Failed();
            return;
        }

        maxCounter = data.Value.URLs.Length;
        urls = data.Value.URLs;

        UpCounterAndActivate();
    }

    protected override bool ExtraCheck() => internalCounter < 0 || internalCounter >= urls.Length;
}
