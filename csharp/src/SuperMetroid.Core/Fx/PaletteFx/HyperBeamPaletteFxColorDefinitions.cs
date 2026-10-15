using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>
/// Compiled control skeleton for Hyper Beam palette-FX program <c>$8D:D900</c>.
/// The ten eight-color payloads remain presentation data; this catalog owns the fixed
/// entry command, timing, record terminators, loop command, and valid pointer domain.
/// </summary>
internal static class HyperBeamPaletteFxColorDefinitions
{

    /// <summary>Bank-local instruction pointer installed by the object definition.</summary>
    public const ushort InitialInstructionPointer = 0xd900;

    /// <summary>Bank-local instruction pointer of the first timed color record.</summary>
    public const ushort FirstFramePointer = 0xd904;

    /// <summary>Bank-local instruction pointer of the terminal loop command.</summary>
    public const ushort LoopInstructionPointer = 0xd9cc;

    /// <summary>Number of authored timed color records.</summary>
    public const int FrameCount = 10;

    /// <summary>Bytes occupied by one timer, eight colors, and the done command.</summary>
    public const ushort FrameByteCount = 20;

    /// <summary>Fixed duration loaded by every authored color record.</summary>
    public const ushort FrameDuration = 2;

    /// <summary>The two control commands outside the timed records; both resume at the first record.</summary>
    private enum ControlPointer : ushort
    {
        /// <summary>The entry command at <c>$8D:D900</c>.</summary>
        Entry = InitialInstructionPointer,
        /// <summary>The terminal loop command at <c>$8D:D9CC</c>.</summary>
        Loop = LoopInstructionPointer,
    }

    /// <summary>The first color word of <paramref name="frame"/>, after the frame's leading command word.</summary>
    internal static ushort FrameColorsPointer(int frame) => (ushort)(FirstFramePointer + frame * FrameByteCount + sizeof(ushort));
}
