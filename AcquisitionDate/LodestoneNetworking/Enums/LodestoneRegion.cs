using System.ComponentModel;

namespace AcquisitionDate.LodestoneNetworking.Enums;

/// <summary>
/// The [Description] for each LodestoneRegion holds how it writes the date on the Lodestone
/// What an absolute shitty and garbage way of handling this...
/// </summary>

internal enum LodestoneRegion
{
    [Description("dd/MM/yyyy")]
    Europe,
    [Description("MM/dd/yyyy")]
    America,
    [Description("yyyy/MM/dd")]
    Japan,
    [Description("dd.MM.yyyy")]
    Germany,
    [Description("dd.MM.yyyy")]
    France
}
