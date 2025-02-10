namespace AcquisitionDate.Database.Structs;

internal readonly struct UnlockedDate
{
    readonly Configuration Configuration;

    readonly AcquisitionDateTime? ManualDate;
    readonly AcquisitionDateTime? LodestoneDate;

    public UnlockedDate(Configuration configuration, AcquisitionDateTime? manualTime, AcquisitionDateTime? lodestoneTime)
    {
        Configuration   = configuration;
        ManualDate      = manualTime;
        LodestoneDate   = lodestoneTime;
    }

    public AcquisitionDateTime? GetDateTime() => ManualDate ?? LodestoneDate;

    public string? GetTimeString() => GetDateTime()?.DateTime.ToString(Configuration.DateParseString());
}
