using SuperMetroid.Core.Frontend.Ending.Artwork;
namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>Which explosion artwork each background plane of the ending's Zebes explosion shows, phase by phase.</summary>
internal static class EndingExplosionDisplayDefinitions
{
    /// <summary>The pixel rows of each explosion map's picture that lie above the screen's top.</summary>
    public const int MapRowsAboveScreen = 1;
    /// <summary>The explosion the first background plane shows from the explosion's start until the finale replaces it.</summary>
    public const EndingObjectFragmentId InitialMap = EndingObjectFragmentId.WideExplosion;
    /// <summary>The explosion the first background plane shows during the finale.</summary>
    public const EndingObjectFragmentId FinaleMainMap = EndingObjectFragmentId.ConcentricExplosion;
    /// <summary>The explosion the second background plane shows during the finale, drawn and blended on both screens.</summary>
    public const EndingObjectFragmentId FinaleSubMap = EndingObjectFragmentId.Eclipse;
    /// <summary>The burst the second background plane shows and adds over the first.</summary>
    public const EndingObjectFragmentId BurstSubMap = EndingObjectFragmentId.Blank;
}
