using SuperMetroid.Core.Frontend.Ending.Artwork;
namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>SNES background map and scanout settings used across the ending explosion's display phases.</summary>
internal static class EndingExplosionDisplayDefinitions
{
    /// <summary>Native visible scanout begins at physical line one. With BG VOFS=0, the first displayed BG row is source row one, while OBJ remains screen-relative.</summary>
    public const ushort FirstVisibleBackgroundRow = 1;
    /// <summary>Func117's BG1SC=$70 map before F2FA changes it.</summary>
    public const EndingObjectFragmentId InitialMap = EndingObjectFragmentId.Segment70;
    /// <summary>F2FA's BG1SC=$74 map.</summary>
    public const EndingObjectFragmentId FinaleMainMap = EndingObjectFragmentId.Segment74;
    /// <summary>F2FA's BG2SC=$78 map, used on both screens.</summary>
    public const EndingObjectFragmentId FinaleSubMap = EndingObjectFragmentId.Segment78;
    /// <summary>F2B7's BG2SC=$7C map.</summary>
    public const EndingObjectFragmentId BurstSubMap = EndingObjectFragmentId.Segment7C;
}
