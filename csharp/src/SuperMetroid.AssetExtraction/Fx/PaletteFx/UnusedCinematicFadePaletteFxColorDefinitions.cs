using SuperMetroid.Core.Fx.PaletteFx;

namespace SuperMetroid.AssetExtraction.Fx.PaletteFx;

/// <summary>Immutable mechanics for the game's unused cinematic fade.</summary>
/// <remarks>
/// Issue #849 / #625: the unused fade's definition
/// runs <c>SetColorIndex(160)</c>, eleven 36-byte
/// records of <c>2, colors[16], Wait</c>, then <c>Delete</c> at
/// its end after 22 frames. All 25 control words match. One exact
/// equivalent factorization starts from white and uses this possible
/// 16-color BGR555 anchor <c>Q</c>:
/// <c>[0000,1529,00C8,0023,0000,0508,00C7,0085,0064,1120,04C0,
/// 0460,1484,0C21,0421,2529]</c>. For frame <c>f</c> (0..10), column
/// <c>c</c>, and each five-bit channel <c>q</c> of <c>Q[c]</c>, the
/// output channel is <c>floor((31*(15 - f) + q*f)/15)</c>. Anchors selected
/// from frames 0..8 predict frames 9 and 10 and reproduce all 176 original
/// words. Nine anchor components admit more than one value over this
/// truncated fade, so this is a bounded data relationship, not a claim that
/// the runtime interpolates. All 176 colors remain live presentation
/// data supplied by the presentation compiler.
/// </remarks>
public static class UnusedCinematicFadePaletteFxColorDefinitions
{

    /// <summary>Each record writes sixteen live BGR555 colors.</summary>
    public const int ColorsPerFrame = 16;

    /// <summary>The family's records, consecutive from the first its program shows, and the colours each writes.</summary>
    public static PaletteFxColorFrames ColorFrames { get; } = PaletteFxColorFrames.Of(PaletteFxSegment.UnusedCinematicFade, ColorsPerFrame);
}
