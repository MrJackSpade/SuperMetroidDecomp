using SuperMetroid.Core.Frontend.Ending.Artwork;
namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>Which explosion artwork each background plane of the ending's Zebes explosion shows, phase by phase.</summary>
internal static class EndingExplosionDisplayDefinitions
{
    /// <summary>Native visible scanout begins at physical line one. With BG VOFS=0, the first displayed BG row is source row one, while OBJ remains screen-relative.</summary>
    public const int FirstVisibleBackgroundRow = 1;
    /// <summary>The explosion the first background plane shows from Func117 until the finale replaces it.</summary>
    public const EndingObjectFragmentId InitialMap = EndingObjectFragmentId.Segment70;
    /// <summary>The explosion the first background plane shows during the finale (F2FA).</summary>
    public const EndingObjectFragmentId FinaleMainMap = EndingObjectFragmentId.Segment74;
    /// <summary>The explosion the second background plane shows during the finale (F2FA), drawn and blended on both screens.</summary>
    public const EndingObjectFragmentId FinaleSubMap = EndingObjectFragmentId.Segment78;
    /// <summary>The burst the second background plane shows and adds over the first (F2B7).</summary>
    public const EndingObjectFragmentId BurstSubMap = EndingObjectFragmentId.Segment7C;
}
