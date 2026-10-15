using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>
/// Compiled control skeleton for Hyper Beam palette-FX program <c>$8D:D900</c>.
/// The ten eight-color payloads remain presentation data; this catalog owns the fixed
/// entry command, timing, record terminators, loop command, and valid pointer domain.
/// </summary>
internal static class HyperBeamPaletteFxColorDefinitions
{

    /// <summary>Bank-local instruction pointer of the first timed color record.</summary>
    public const ushort FirstFramePointer = 0xd904;

    /// <summary>Bytes occupied by one timer, eight colors, and the done command.</summary>
    public const ushort FrameByteCount = 20;

    /// <summary>The first color word of <paramref name="frame"/>, after the frame's leading command word.</summary>
    internal static ushort FrameColorsPointer(int frame) => (ushort)(FirstFramePointer + frame * FrameByteCount + sizeof(ushort));
}
