using System;

namespace AcquisitionDate.Database.Structs;

internal readonly struct AcquisitionDateTime
{
    private readonly Configuration Configuration;
    public  readonly DateTime      DateTime;
    public  readonly bool          IsLowest;

    public AcquisitionDateTime(Configuration configuration, DateTime dateTime, bool isLowest)
    {
        Configuration = configuration;
        DateTime      = dateTime;
        IsLowest      = isLowest;
    }
    
    public string GetTimeString()
        => DateTime.ToString(Configuration.DateParseString());
}
