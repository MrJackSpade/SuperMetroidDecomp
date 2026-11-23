using SuperMetroid.Core.Fx.ColourEffects;

namespace SuperMetroid.AssetExtraction.Fx.PaletteFx;

/// <summary>The records of the post-credits Super Metroid icon glare and the colours each writes, for the import.</summary>
public static partial class PostCreditsIconGlarePaletteFxColorDefinitions
{

    /// <summary>Each record writes sixteen live colours.</summary>
    internal const int ColorsPerFrame = 16;

    /// <summary>The family's records, consecutive from the first its program shows, and the colours each writes.</summary>
    public static PaletteFxColorFrames ColorFrames { get; } = PaletteFxColorFrames.Of(ColourEffectId.PostCreditsSuperMetroidIcon, ColorsPerFrame);
}
