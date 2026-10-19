namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>Immutable mechanics shared by the Nintendo boot-logo and copyright fades.</summary>
/// <remarks>
/// Issue #850 / #625: pinned NTSC J/U v1.0 ROM definition <c>$8D:E198</c>
/// sets color index <c>$0132</c> at <c>$8D:C7AC</c> and falls through to
/// the shared body at <c>$C7B0</c>. Definition <c>$8D:E19C</c> sets index
/// <c>$0192</c> at <c>$8D:C7F2</c> and branches there from <c>$C7F6</c>.
/// Eight records of <c>3, colors[2], Wait</c> end in <c>Delete</c> at
/// <c>$C7F0</c> after 24 frames; all 23 control words match. For record
/// <c>f</c> (0..7), let <c>L = 4*f + 3</c>. The first BGR555 color is
/// <c>L*$0421</c> (equal red, green, blue). The second has red zero, blue
/// <c>L</c>, and green <c>floor((3*L + 4)/8)</c>. This rule matches all
/// sixteen shared ROM colors exactly. They remain live presentation data
/// supplied by the presentation compiler. ROM SHA-256:
/// <c>12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72</c>.
/// </remarks>
public static class NintendoLogoFadePaletteFxColorDefinitions
{

    /// <summary>The shared one-shot fade contains eight timed records.</summary>
    public const int FrameCount = 8;

    /// <summary>Each record writes two live BGR555 colors.</summary>
    public const int ColorsPerFrame = 2;

    /// <summary>The family's records, consecutive from the first its program shows, and the colours each writes.</summary>
    public static PaletteFxColorRows Rows { get; } = new(8, FrameCount, ColorsPerFrame);
}
