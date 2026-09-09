namespace SuperMetroid.Core.Frontend;

internal static class EndingExplosionDisplayDefinitions
{
    /// <summary>Func117's BG1SC=$70 map before F2FA changes it.</summary>
    public const ushort InitialMap = 0x7000;
    /// <summary>F2FA's BG1SC=$74 map.</summary>
    public const ushort FinaleMainMap = 0x7400;
    /// <summary>F2FA's BG2SC=$78 map, used on both screens.</summary>
    public const ushort FinaleSubMap = 0x7800;
    /// <summary>F2B7's BG2SC=$7C map.</summary>
    public const ushort BurstSubMap = 0x7c00;
    /// <summary>Func117's BG12NBA=$44 gives both four-bit backgrounds character base $4000.</summary>
    public const ushort Characters = 0x4000;
}
