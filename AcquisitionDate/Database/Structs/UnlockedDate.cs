using AcquisitionDate.Database.Enums;

namespace AcquisitionDate.Database.Structs;

internal readonly struct UnlockedDate
{
    private readonly Configuration        Configuration;
    private readonly AcquisitionDateTime? ManualDate;
    private readonly AcquisitionDateTime? LodestoneDate;

    public UnlockedDate(Configuration configuration, AcquisitionDateTime? manualTime, AcquisitionDateTime? lodestoneTime)
    {
        Configuration   = configuration;
        ManualDate      = manualTime;
        LodestoneDate   = lodestoneTime;
    }

    public AcquisitionDateTime? GetDateTime() 
        => ManualDate ?? LodestoneDate;

    public AcquisitionDateTime? GetSpecificDateTime(AcquiredDateType dateType)
    {
        switch (dateType)
        {
            case AcquiredDateType.Lodestone: return LodestoneDate;
            case AcquiredDateType.Manual:    return ManualDate;
        }
        
        return null;
    }
}
