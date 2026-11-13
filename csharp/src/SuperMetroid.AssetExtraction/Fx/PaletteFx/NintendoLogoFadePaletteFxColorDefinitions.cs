using SuperMetroid.Core.Fx.PaletteFx;

namespace SuperMetroid.AssetExtraction.Fx.PaletteFx;

/// <summary>Immutable mechanics shared by the Nintendo boot-logo and copyright fades.</summary>
/// <remarks>
/// Issue #850 / #625: the boot-logo definition
/// sets colour index 306 and falls through to
/// the shared body. The copyright definition sets index
/// 402 and branches to the same body.
/// Eight records of <c>3, colors[2], Wait</c> end in <c>Delete</c>
/// after 24 frames; all 23 control words match. For record
/// <c>f</c> (0..7), let <c>L = 4*f + 3</c>. The first BGR555 color is
/// <c>L</c> in each of red, green and blue. The second has red zero, blue
/// <c>L</c>, and green <c>floor((3*L + 4)/8)</c>. This rule matches all
/// sixteen shared original colors exactly. They remain live presentation data
/// supplied by the presentation compiler.
/// </remarks>
public static class NintendoLogoFadePaletteFxColorDefinitions
{

    /// <summary>Each record writes two live BGR555 colors.</summary>
    public const int ColorsPerFrame = 2;

    /// <summary>The family's records, consecutive from the first its program shows, and the colours each writes.</summary>
    public static PaletteFxColorFrames ColorFrames { get; } = PaletteFxColorFrames.Of(PaletteFxSegment.CommonFadeInLogo, ColorsPerFrame);
}
