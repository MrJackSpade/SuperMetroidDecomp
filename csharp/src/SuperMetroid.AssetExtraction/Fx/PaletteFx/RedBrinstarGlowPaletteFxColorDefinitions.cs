using SuperMetroid.Core.Fx.ColourEffects;

namespace SuperMetroid.AssetExtraction.Fx.PaletteFx;

/// <summary>The records of Red Brinstar's background glow and the colours each writes, for the import.</summary>
public static partial class RedBrinstarGlowPaletteFxColorDefinitions
{

    /// <summary>Eight BGR555 colors are presentation-owned by each timed record.</summary>
    internal const int ColorsPerFrame = 8;

    /// <summary>The family's records, consecutive from the first its program shows, and the colours each writes.</summary>
    public static PaletteFxColorFrames ColorFrames { get; } = PaletteFxColorFrames.Of(ColourEffectId.BrinstarRedBackgroundGlow, ColorsPerFrame);
}
