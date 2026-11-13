using SuperMetroid.Core.Fx.PaletteFx;

namespace SuperMetroid.AssetExtraction.Fx.PaletteFx;

/// <summary>Immutable mechanics for the post-credits Super Metroid icon glare.</summary>
/// <remarks>
/// Issue #851 / #625: the glare shows fourteen records of sixteen colours, each for one
/// frame, and ends after 14 frames. Every one of the 224 colours is installed presentation data, kept as installed.
/// </remarks>
public static class PostCreditsIconGlarePaletteFxColorDefinitions
{

    /// <summary>Each record writes sixteen live colours.</summary>
    public const int ColorsPerFrame = 16;

    /// <summary>The family's records, consecutive from the first its program shows, and the colours each writes.</summary>
    public static PaletteFxColorFrames ColorFrames { get; } = PaletteFxColorFrames.Of(PaletteFxSegment.PostCreditsSuperMetroidIconGlare, ColorsPerFrame);
}
