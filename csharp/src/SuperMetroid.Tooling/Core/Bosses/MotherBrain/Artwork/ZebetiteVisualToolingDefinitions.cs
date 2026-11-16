using SuperMetroid.AssetExtraction.Enemies;

namespace SuperMetroid.Core.Bosses.MotherBrain.Artwork;

/// <summary>Development-tool views of the Zebetites' health pictures (#2250); never linked by player hosts.</summary>
internal static class ZebetiteVisualToolingDefinitions
{
    /// <summary>Zebetite ordinary OAM bank $A6.</summary>
    internal const byte Bank = 0xa6;
    /// <summary>Ten barrier health-tier compositions at $A6:FE08..FEB0.</summary>
    internal const int FrameCount = 10;
    /// <summary>The spritemaps the health pictures show: the single barrier's, then a split pair's, most health first.</summary>
    internal static ushort[] Spritemaps() => [.. ZebetiteHealthPictureDefinitions.ByForm.Values
        .SelectMany(pictures => pictures).Select(picture => EnemyFrameCartridgeLookupTooling.Word(picture.Picture))];
}
