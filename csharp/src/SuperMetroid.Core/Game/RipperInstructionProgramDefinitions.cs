namespace SuperMetroid.Core.Game;

/// <summary>
/// Compiled engine-control words for the GRipper, Ripper II, and Ripper animation loops.
/// Interleaved spritemap operands are selected by the installed visual catalog.
/// </summary>
internal abstract class RipperInstructionProgramDefinitions
{
    /// <summary><c>InstList_GRipper_MovingLeft</c> at $A2:E19B.</summary>
    internal const ushort GRipperMovingLeft = 0xe19b;
    /// <summary><c>InstList_GRipper_MovingRight</c> at $A2:E1AF.</summary>
    internal const ushort GRipperMovingRight = 0xe1af;
    /// <summary><c>InstList_Ripper2_MovingRight</c> at $A2:E2E0.</summary>
    internal const ushort Ripper2MovingRight = 0xe2e0;
    /// <summary><c>InstList_Ripper2_MovingLeft</c> at $A2:E2F4.</summary>
    internal const ushort Ripper2MovingLeft = 0xe2f4;
    /// <summary><c>InstList_Ripper_MovingRight</c> at $A2:E477.</summary>
    internal const ushort RipperMovingRight = 0xe477;
    /// <summary><c>InstList_Ripper_MovingLeft</c> at $A2:E48B.</summary>
    internal const ushort RipperMovingLeft = 0xe48b;

    /// <summary><c>Spritemap_GRipper_Ripper2_Frozen_FacingLeft</c> at $A2:E43F.</summary>
    internal const ushort FrozenFacingLeftSpritemap = 0xe43f;
    /// <summary><c>Spritemap_GRipper_Ripper2_Frozen_FacingRight</c> at $A2:E44B.</summary>
    internal const ushort FrozenFacingRightSpritemap = 0xe44b;

    /// <summary>$A2:E19B/E1AF/E2E0/E2F4/E477/E48B: selected eight-tick hold of the
    /// neutral pose, repeated between the two alternate wing/accessory poses. Reviewed
    /// visual cadence only; population speed, fixed collision and reversal scheduling are separate.</summary>
    private const ushort NeutralVisualHoldTicks = 8;
    /// <summary>The second/fourth visual records of those six loops hold each alternate
    /// pose for seven ticks. This chosen visual performance does not control movement,
    /// grapple/freeze behavior or the one-tick list reset on direction reversal.</summary>
    private const ushort AlternateVisualHoldTicks = 7;

    /// <summary>Returns the authored display duration for one of the alternating neutral and accessory poses.</summary>
    /// <param name="phase">Timed-record position; even phases select the neutral pose and odd phases select the alternate pose.</param>
    /// <returns>Eight ticks for the neutral pose or seven ticks for the alternate pose.</returns>
    internal static ushort VisualHold(int phase) =>
        (phase & 1) == 0 ? NeutralVisualHoldTicks : AlternateVisualHoldTicks;

    /// <summary>Three family pairs; each direction is four timed records followed by Goto and its target.</summary>
    internal static ushort ProgramStart(int program) => (ushort)((program / 2) switch
    {
        0 => GRipperMovingLeft + 20 * (program & 1),
        1 => Ripper2MovingRight + 20 * (program & 1),
        2 => RipperMovingRight + 20 * (program & 1),
        _ => throw new ArgumentOutOfRangeException(nameof(program)),
    });

    /// <summary>Reads a compiled timed-record or loop-control word from one of the six Ripper-family animation lists.</summary>
    /// <param name="address">Bank-$A2 address of the mechanics word.</param>
    /// <returns>The selected display duration, goto command, or loop target.</returns>
    /// <exception cref="InvalidDataException">The address is not a compiled mechanics word in these lists.</exception>
    internal static ushort ReadMechanicsWord(ushort address)
    {
        if (TryRead(address, out ushort value))
            return value;
        throw new InvalidDataException(
            $"Ripper-family instruction mechanics pointer $A2:{address:X4} is not compiled.");
    }

    /// <summary>Attempts to resolve an address in the six directional Ripper-family loops, excluding their spritemap operands.</summary>
    /// <param name="address">Bank-$A2 address to look up.</param>
    /// <param name="value">Receives the duration or control value when found; receives zero when absent.</param>
    /// <returns><see langword="true"/> when the address is a compiled mechanics word; otherwise, <see langword="false"/>.</returns>
    internal static bool TryRead(ushort address, out ushort value)
    {
        for (int program = 0; program < 6; program++)
        {
            ushort start = ProgramStart(program);
            int offset = address - start;
            if (offset < 0 || offset >= 20)
                continue;
            if (offset < 16 && (offset & 3) == 0)
            {
                value = VisualHold(offset / 4);
                return true;
            }
            if (offset is 16 or 18)
            {
                value = offset == 16 ? CommonEnemyInstructionCodes.Goto : start;
                return true;
            }
        }
        value = 0;
        return false;
    }
}
