namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>Immutable mechanics for the Zebes explosion finale.</summary>
/// <remarks>
/// Issue #843 / #625: pinned NTSC J/U v1.0 ROM definition <c>$8D:E1CC</c>
/// enters at <c>$8D:CD62</c>: <c>SetColorIndex($0002)</c>, 45 records of
/// <c>duration, colors[15], Wait</c>, then <c>Delete</c> at <c>$D360</c>.
/// The first 15 durations are 2; the remaining 30 are 9 (300 frames total).
/// All 93 control words match. Let the 15-color gradient <c>G</c> be
/// <c>[7FFF,77BD,6F7B,6318,5AD6,5294,4A52,4210,35AD,2D6B,2529,
/// 1CE7,14A5,0842,0000]</c>. For color column <c>c</c> (0..14), frames
/// <c>f</c> 0..14 use <c>G[max(0, c - (14 - f))]</c>; frame 15 uses
/// <c>G[c]</c>. Frames 16..29 right-rotate <c>G</c> by <c>k = f - 15</c>:
/// <c>G[(c - k + 15) % 15]</c>. In frames 23..29, columns 0 through
/// <c>f - 23</c> instead use the preceding <c>G</c> entry, an authored
/// 28-position exception to plain rotation. Frames 30..44 restore white
/// using <c>G[max(0, c - (f - 30))]</c>. This rule matches all 675 ROM
/// colors exactly. They remain live presentation data supplied by the
/// presentation compiler; this catalog supplies only the controls. ROM SHA-256:
/// <c>12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72</c>.
/// </remarks>
public static class ZebesExplosionFinalePaletteFxColorDefinitions
{

    /// <summary>Each record writes fifteen live BGR555 colors.</summary>
    public const int ColorsPerFrame = 15;

    /// <summary>The family's records, consecutive from the first its program shows, and the colours each writes.</summary>
    public static PaletteFxColorRows Rows { get; } = PaletteFxColorRows.Of(PaletteFxProgram.ZebesExplosionFinale, ColorsPerFrame);
}
