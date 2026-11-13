using SuperMetroid.Core.Fx.PaletteFx;

namespace SuperMetroid.AssetExtraction.Fx.PaletteFx;

/// <summary>Immutable mechanics for the gunship emerging from the Zebes explosion.</summary>
/// <remarks>
/// Issue #845 / #625: the gunship shows sixteen records of sixteen colours, each held for
/// 24 frames, and ends after 384 frames. Every one of the 256 colours is installed presentation data, kept as
/// installed. This catalog supplies controls to the palette-FX runtime.
/// </remarks>
public static class ZebesExplosionGunshipPaletteFxColorDefinitions
{

    /// <summary>Each record writes sixteen live colours.</summary>
    public const int ColorsPerFrame = 16;

    /// <summary>The family's records, consecutive from the first its program shows, and the colours each writes.</summary>
    public static PaletteFxColorFrames ColorFrames { get; } = PaletteFxColorFrames.Of(PaletteFxSegment.GunshipEmergingFromZebesExplosion, ColorsPerFrame);
}
