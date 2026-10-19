namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>Immutable mechanics for the wide foreground part of the Zebes explosion.</summary>
/// <remarks>
/// Issue #842 / #625: the pinned NTSC J/U v1.0 ROM starts definition
/// <c>$8D:E1C8</c> at <c>$8D:CB3C</c>: <c>SetColorIndex($0002)</c>, sixteen
/// 34-byte records of <c>duration, colors[15], Wait</c>, then <c>Delete</c>
/// at <c>$CD60</c>. Durations are 4 for frames 0..2, 60 for frame 3, and 6
/// for frames 4..15: 144 frames total. All 35 control words match. For each
/// frame <c>f</c> (0..15), exactly the first <c>min(f + 1, 15)</c> colors are
/// nonzero; the rest are black, matching all 240 ROM positions (135 nonzero,
/// 105 black). Frames 0..5 fill that prefix uniformly with, respectively,
/// <c>7C00,7CA0,7DE0,7DE0,7E80,7F20</c>. Later frames have authored accents:
/// frame 6 starts <c>7FFD,7FE9</c>, frame 14 ends <c>6B40</c>, and frame 15
/// ends <c>7FF7</c>. All 240 presentation words belong to a separate color entry;
/// this cadence conversion grants no color exception. ROM SHA-256:
/// <c>12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72</c>.
/// </remarks>
public static class ZebesExplosionForegroundPaletteFxColorDefinitions
{

    /// <summary>The one-shot foreground explosion contains sixteen timed records.</summary>
    public const int FrameCount = 16;

    /// <summary>Each record writes fifteen live BGR555 colors.</summary>
    public const int ColorsPerFrame = 15;

    /// <summary>The family's records, consecutive from the first its program shows, and the colours each writes.</summary>
    public static PaletteFxColorRows Rows { get; } = new(93, FrameCount, ColorsPerFrame);
}
