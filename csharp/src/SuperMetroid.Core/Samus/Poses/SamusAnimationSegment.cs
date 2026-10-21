using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Samus.Poses;

/// <summary>One run of animation frame delays at its offset in the pose delay streams, ended by one control command.</summary>
/// <remarks>Command operands are semantic: back distances derive from the repeat target and pose bytes are pose identities.</remarks>
internal sealed class SamusAnimationSegment
{
    /// <summary>Authored frame durations preceding this segment's terminal command.</summary>
    private readonly byte[] delays;

    /// <summary>Captures the stream placement and semantic operands for one contiguous delay run.</summary>
    internal SamusAnimationSegment(int offset, byte[] delays, SamusAnimationCommand command, SamusAnimationTransition transition)
    {
        WordPose = transition.WordPose;
        Offset = Ensure.AtLeastZero(offset);
        this.delays = delays;
        Command = command;
        RepeatOffset = transition.RepeatOffset;
        Item = transition.Item;
        UnequippedGrounded = transition.UnequippedGrounded;
        UnequippedAirborne = transition.UnequippedAirborne;
        EquippedGrounded = transition.EquippedGrounded;
        EquippedAirborne = transition.EquippedAirborne;
    }

    /// <summary>Byte offset of this run's first duration byte from the start of the pose delay streams.</summary>
    internal int Offset { get; }
    /// <summary>Terminal control opcode following the authored durations.</summary>
    internal SamusAnimationCommand Command { get; }

    /// <summary>Stream offset of the frame selected by <see cref="SamusAnimationCommand.RepeatFrom"/>.</summary>
    internal int RepeatOffset { get; }

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

    /// <summary>Stream offset immediately after this run's duration bytes, where its control command sits.</summary>
    internal int CommandOffset => Offset + delays.Length;

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
            SamusAnimationCommand.RepeatFrom => (byte)(CommandOffset - RepeatOffset),
            SamusAnimationCommand.TurnTransition or SamusAnimationCommand.Transition =>
                operand == 1 ? (byte)UnequippedGrounded : (byte)0,
            SamusAnimationCommand.Hold or SamusAnimationCommand.LoopUnlessLowEnergy or
                SamusAnimationCommand.InstallDrainedFall or SamusAnimationCommand.ItemAirborneTransition or
                SamusAnimationCommand.WallJumpSelect or SamusAnimationCommand.ItemTransition or
                SamusAnimationCommand.Loop => operand switch
            {
                1 => unchecked((byte)Item.ToNativeWord()),
                2 => SnesLittleEndian.HighByte(Item.ToNativeWord()),
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
