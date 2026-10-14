namespace SuperMetroid.Core.Game;

/// <summary>One run of animation frame delays at its native bank-$91 address, ended by one control command.</summary>
/// <remarks>Command operands are semantic: back distances derive from the repeat target and pose bytes are pose identities.</remarks>
internal sealed class SamusAnimationSegment
{
    /// <summary>Authored frame durations preceding this segment's terminal command.</summary>
    private readonly byte[] delays;

    /// <summary>Captures native placement and semantic operands for one contiguous delay run.</summary>
    internal SamusAnimationSegment(ushort address, byte[] delays, SamusAnimationCommand command, SamusAnimationTransition transition)
    {
        WordPose = transition.WordPose;
        Address = address;
        this.delays = delays;
        Command = command;
        RepeatTarget = transition.RepeatTarget;
        Item = transition.Item;
        UnequippedGrounded = transition.UnequippedGrounded;
        UnequippedAirborne = transition.UnequippedAirborne;
        EquippedGrounded = transition.EquippedGrounded;
        EquippedAirborne = transition.EquippedAirborne;
    }

    /// <summary>Bank-$91 address of this run's first duration byte.</summary>
    internal ushort Address { get; }
    /// <summary>Terminal control opcode following the authored durations.</summary>
    internal SamusAnimationCommand Command { get; }

    /// <summary>Address of the frame selected by <see cref="SamusAnimationCommand.RepeatFrom"/>.</summary>
    internal ushort RepeatTarget { get; }

    /// <summary>Equipment tested by the item-selected transitions.</summary>
    internal SamusEquipmentFlags Item { get; }

    /// <summary>Pose for an unequipped item at rest, and the only pose of <c>$F8</c>/<c>$FD</c>.</summary>
    internal SamusPoseId UnequippedGrounded { get; }
    /// <summary>Airborne pose selected when the tested equipment is absent.</summary>
    internal SamusPoseId UnequippedAirborne { get; }
    /// <summary>Grounded pose selected when the tested equipment is present.</summary>
    internal SamusPoseId EquippedGrounded { get; }
    /// <summary>Airborne pose selected when the tested equipment is present.</summary>
    internal SamusPoseId EquippedAirborne { get; }

    /// <summary>
    /// The source writes this <c>$FD</c> pose as a little-endian word. The interpreter reads
    /// only its low byte, so the zero high byte is never executed as a frame.
    /// </summary>
    internal bool WordPose { get; }

    /// <summary>Bank-$91 address immediately after this run's duration bytes.</summary>
    internal ushort CommandAddress => (ushort)(Address + delays.Length);

    /// <summary>Number of encoded duration, opcode, and operand bytes in this segment.</summary>
    internal int Length => delays.Length + 1 + Command switch
    {
        SamusAnimationCommand.Transition => WordPose ? 2 : 1,
        SamusAnimationCommand.TurnTransition or SamusAnimationCommand.RepeatFrom => 1,
        SamusAnimationCommand.ItemTransition => 4,
        SamusAnimationCommand.ItemAirborneTransition => 6,
        SamusAnimationCommand.Hold or SamusAnimationCommand.LoopUnlessLowEnergy or
            SamusAnimationCommand.InstallDrainedFall or SamusAnimationCommand.WallJumpSelect or
            SamusAnimationCommand.Loop => 0,
        _ => throw new InvalidOperationException($"Undefined SamusAnimationCommand {Command}."),
    };

    /// <summary>Encodes the native byte at <paramref name="offset"/> from the segment start.</summary>
    internal byte ByteAt(int offset)
    {
        if ((uint)offset >= Length) throw new ArgumentOutOfRangeException(nameof(offset));
        if (offset < delays.Length) return delays[offset];
        int operand = offset - delays.Length;
        if (operand == 0) return (byte)Command;
        return Command switch
        {
            SamusAnimationCommand.RepeatFrom => (byte)(CommandAddress - RepeatTarget),
            SamusAnimationCommand.TurnTransition or SamusAnimationCommand.Transition =>
                operand == 1 ? (byte)UnequippedGrounded : (byte)0,
            SamusAnimationCommand.Hold or SamusAnimationCommand.LoopUnlessLowEnergy or
                SamusAnimationCommand.InstallDrainedFall or SamusAnimationCommand.ItemAirborneTransition or
                SamusAnimationCommand.WallJumpSelect or SamusAnimationCommand.ItemTransition or
                SamusAnimationCommand.Loop => operand switch
            {
                1 => unchecked((byte)Item),
                2 => SnesLittleEndian.HighByte((ushort)Item),
                3 => (byte)UnequippedGrounded,
                // $FC's equipped pose immediately follows its unequipped pose.
                4 => (byte)(Command == SamusAnimationCommand.ItemTransition ? EquippedGrounded : UnequippedAirborne),
                5 => (byte)EquippedGrounded,
                _ => (byte)EquippedAirborne,
            },
            _ => throw new InvalidOperationException($"Undefined SamusAnimationCommand {Command}."),
        };
    }
}
