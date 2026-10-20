namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>Immutable mechanics for the post-credits Super Metroid icon glare.</summary>
/// <remarks>
/// Issue #851 / #625: pinned NTSC J/U v1.0 ROM definition <c>$8D:E200</c>
/// enters at <c>$8D:DF94</c>: <c>SetColorIndex($01E0)</c>, fourteen
/// 36-byte records of <c>1, colors[16], Wait</c>, then <c>Delete</c> at
/// <c>$E190</c> after 14 frames. All 31 control words match. Let
/// <c>base[c]</c> be the final record's sixteen authored BGR555 colors.
/// For record <c>f</c> (0..13), set <c>t = f + 1</c> for 0..6 and
/// <c>t = 13 - f</c> for 7..13. For each color column <c>c</c> and
/// five-bit channel <c>q</c> of <c>base[c]</c>, the output channel is
/// <c>floor(((7 - t)*q + 31*t)/7)</c>. Frame 6 is white, frames 7..12
/// mirror frames 5..0, and frame 13 restores the base. This rule matches
/// all 224 ROM colors exactly. LogoGlarePaletteColorDefinitions evaluates matching
/// presentation colors from the final supplied row; independent edits remain explicit.
/// The presentation compiler owns the base colors and overrides. ROM SHA-256:
/// <c>12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72</c>.
/// </remarks>
public static class PostCreditsIconGlarePaletteFxColorDefinitions
{

    /// <summary>The one-shot glare contains fourteen timed records.</summary>
    public static int FrameCount => Rows.FrameCount;

    /// <summary>Each record writes sixteen live BGR555 colors.</summary>
    public const int ColorsPerFrame = 16;

    /// <summary>The family's records, consecutive from the first its program shows, and the colours each writes.</summary>
    public static PaletteFxColorRows Rows { get; } = PaletteFxColorRows.Of(PaletteFxProgram.PostCreditsSuperMetroidIconGlare, ColorsPerFrame);

    /// <summary>Colour <paramref name="color"/> of record <paramref name="frame"/>.</summary>
    public static PaletteFxColorKey Color(int frame, int color) => Rows.Color(frame, color);
}
