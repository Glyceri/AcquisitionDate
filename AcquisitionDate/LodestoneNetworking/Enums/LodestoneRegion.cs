using System.ComponentModel;

namespace AcquisitionDate.LodestoneNetworking.Enums;

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
