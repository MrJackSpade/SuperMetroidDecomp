namespace SuperMetroid.Core.Game;

/// <summary>Bank-$91 animation control commands, as interpreted by <c>$90:8324-$84DB</c>.</summary>
/// <remarks>Retail never compiles <c>$F1-$F5</c> or <c>$FA</c>; the interpreter still accepts them.</remarks>
internal enum SamusAnimationCommand : byte
{
    /// <summary><c>$F0</c>: one of six no-op slots. The timer is not reloaded, so the next delay starts a frame later.</summary>
    Hold = 0xF0,

    /// <summary><c>$F6</c>, <c>$90:8346</c>: loop to frame zero unless energy is below 30, then continue into the drained breathing frames.</summary>
    LoopUnlessLowEnergy = 0xF6,

    /// <summary><c>$F7</c>, <c>$90:8360</c>: install the drained falling handler and continue past the command.</summary>
    InstallDrainedFall = 0xF7,

    /// <summary><c>$F8 pp</c>, <c>$90:8370</c>: publish a turn's final pose unless an auto-jump input supersedes it.</summary>
    TurnTransition = 0xF8,

    /// <summary><c>$F9 eeee gg aa GG AA</c>, <c>$90:839A</c>: item- and vertical-motion-selected transitional pose.</summary>
    ItemAirborneTransition = 0xF9,

    /// <summary><c>$FB</c>, <c>$90:841D</c>: continue into the ordinary, Space Jump or Screw Attack wall-jump frames.</summary>
    WallJumpSelect = 0xFB,

    /// <summary><c>$FC eeee gg aa</c>, <c>$90:848B</c>: item-selected transitional pose.</summary>
    ItemTransition = 0xFC,

    /// <summary><c>$FD pp</c>, <c>$90:83A0</c>: publish a transitional pose.</summary>
    Transition = 0xFD,

    /// <summary><c>$FE nn</c>, <c>$90:84C7</c>: move back <c>nn</c> bytes to an earlier frame.</summary>
    RepeatFrom = 0xFE,

    /// <summary><c>$FF</c>, <c>$90:84DB</c>: restart at frame zero.</summary>
    Loop = 0xFF,
}

/// <summary>One run of animation frame delays at its native bank-$91 address, ended by one control command.</summary>
/// <remarks>Command operands are semantic: back distances derive from the repeat target and pose bytes are pose identities.</remarks>
internal sealed class SamusAnimationSegment
{
    private readonly byte[] delays;

    internal SamusAnimationSegment(ushort address, byte[] delays, SamusAnimationCommand command,
        ushort repeatTarget = 0, SamusEquipmentFlags item = 0,
        SamusPoseId unequippedGrounded = 0, SamusPoseId unequippedAirborne = 0,
        SamusPoseId equippedGrounded = 0, SamusPoseId equippedAirborne = 0, bool wordPose = false)
    {
        WordPose = wordPose;
        Address = address;
        this.delays = delays;
        Command = command;
        RepeatTarget = repeatTarget;
        Item = item;
        UnequippedGrounded = unequippedGrounded;
        UnequippedAirborne = unequippedAirborne;
        EquippedGrounded = equippedGrounded;
        EquippedAirborne = equippedAirborne;
    }

    internal ushort Address { get; }
    internal SamusAnimationCommand Command { get; }

    /// <summary>Address of the frame selected by <see cref="SamusAnimationCommand.RepeatFrom"/>.</summary>
    internal ushort RepeatTarget { get; }

    /// <summary>Equipment tested by the item-selected transitions.</summary>
    internal SamusEquipmentFlags Item { get; }

    /// <summary>Pose for an unequipped item at rest, and the only pose of <c>$F8</c>/<c>$FD</c>.</summary>
    internal SamusPoseId UnequippedGrounded { get; }
    internal SamusPoseId UnequippedAirborne { get; }
    internal SamusPoseId EquippedGrounded { get; }
    internal SamusPoseId EquippedAirborne { get; }

    /// <summary>
    /// The source writes this <c>$FD</c> pose as a little-endian word. The interpreter reads
    /// only its low byte, so the zero high byte is never executed as a frame.
    /// </summary>
    internal bool WordPose { get; }

    internal ushort CommandAddress => (ushort)(Address + delays.Length);

    internal int Length => delays.Length + 1 + Command switch
    {
        SamusAnimationCommand.Transition => WordPose ? 2 : 1,
        SamusAnimationCommand.TurnTransition or SamusAnimationCommand.RepeatFrom => 1,
        SamusAnimationCommand.ItemTransition => 4,
        SamusAnimationCommand.ItemAirborneTransition => 6,
        _ => 0,
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
            _ => operand switch
            {
                1 => unchecked((byte)Item),
                2 => (byte)((ushort)Item >> 8),
                3 => (byte)UnequippedGrounded,
                // $FC's equipped pose immediately follows its unequipped pose.
                4 => (byte)(Command == SamusAnimationCommand.ItemTransition ? EquippedGrounded : UnequippedAirborne),
                5 => (byte)EquippedGrounded,
                _ => (byte)EquippedAirborne,
            },
        };
    }
}

/// <summary>The bank-$91 Samus animation programs at <c>$91:B20A-$B5D0</c>, in native address order.</summary>
/// <remarks>
/// Independently reviewed for #1165 against every native byte and the <c>$90:8324-$84DB</c>
/// interpreter. All 160 segments are contiguous and each ends in exactly one command; every
/// one of the 127 pose pointers selects a segment start. Command opcodes, operand layout,
/// back distances, item masks and transitional poses are semantic definitions here. The
/// six aimed jump transitions <c>$55-$5A</c> write their <c>$FD</c> pose as a word
/// (pinned bank_91 <c>db $01, $FD,$15, $00</c>); that unread zero is not a frame.
/// The frame delays are retained as authored animation cadence: they are the
/// chosen timing of each drawn pose sequence (turn, landing, breathing, spin, appearance)
/// and do not follow from any physical gameplay quantity. Fitting or reciting them in
/// arithmetic would re-encode the same choreography. Long uniform runs are expressed as repeats.
/// </remarks>
internal static class SamusAnimationDelayPrograms
{
    // Native segments are contiguous from $91:B20A: each starts where the previous one ends,
    // so start addresses are laid out from the stream start rather than stored.
    private static readonly SamusAnimationSegment[] segments = Layout(
        (ushort)(SamusAnimationDelayDefinitions.DelayStreamsAddress & ushort.MaxValue),
    [
        Loop([2, 3, 2, 3, 2, 3, 2, 3, 2, 3]),
        Loop([..Repeat(4, 6), 3, 4, 4, 3]),
        Loop([10]),
        RepeatFrom([2, 16], frame: 1),
        Loop([..Repeat(16, 6)]),
        TurnTransition([5, 2], SamusPoseId.FacingRightNormalPose),
        TurnTransition([5, 2], SamusPoseId.FacingLeftNormalPose),
        TurnTransition([3, 5, 2], SamusPoseId.FacingRightNormalPose),
        TurnTransition([3, 5, 2], SamusPoseId.FacingLeftNormalPose),
        TurnTransition([5, 2], SamusPoseId.StandingAimUpRightPose),
        TurnTransition([5, 2], SamusPoseId.StandingAimUpLeftPose),
        TurnTransition([5, 2], SamusPoseId.StandingAimDiagonalUpRightPose),
        TurnTransition([5, 2], SamusPoseId.StandingAimDiagonalUpLeftPose),
        TurnTransition([5, 2], SamusPoseId.StandingAimDiagonalDownRightPose),
        TurnTransition([5, 2], SamusPoseId.StandingAimDiagonalDownLeftPose),
        InstallDrainedFall([2, 2, 2, 16]),
        RepeatFrom([1], frame: 0),
        RepeatFrom([16, 16, 16, 16], frame: 0),
        Transition([3], SamusPoseId.FacingRightNormalPose),
        InstallDrainedFall([2, 2, 16]),
        RepeatFrom([1], frame: 0),
        RepeatFrom([8, 16, 16, 16, 16], frame: 1),
        Transition([16, 16, 16], SamusPoseId.FacingLeftNormalPose),
        RepeatFrom([16, 16, 16, 16, 16], segmentsBack: 2, frame: 3),
        RepeatFrom([16], segmentsBack: 3, frame: 3),
        RepeatFrom([16], frame: 0),
        Loop([16, 16, 16, 16]),
        Transition([3], SamusPoseId.FacingRightNormalPose),
        Loop([16, 16, 16, 16]),
        Transition([3], SamusPoseId.FacingLeftNormalPose),
        LoopUnlessLowEnergy([10, 10, 10, 10]),
        RepeatFrom([8, 8, 8, 8], frame: 0),
        LoopUnlessLowEnergy([10, 10, 10, 10]),
        RepeatFrom([8, 8, 8, 8], frame: 0),
        Loop([15, 15, 15, 15, 15]),
        Loop([16]),
        Loop([16]),
        RepeatFrom([2, 16], frame: 1),
        RepeatFrom([2, 16], frame: 1),
        RepeatFrom([2, 16], frame: 1),
        RepeatFrom([..Repeat(8, 66)], frame: 65),
        Transition([1], SamusPoseId.NeutralJumpRightPose),
        Transition([1], SamusPoseId.NeutralJumpLeftPose),
        TransitionWord([1], SamusPoseId.NormalJumpAimUpRightPose),
        TransitionWord([1], SamusPoseId.NormalJumpAimUpLeftPose),
        TransitionWord([1], SamusPoseId.NormalJumpAimDiagonalUpRightPose),
        TransitionWord([1], SamusPoseId.NormalJumpAimDiagonalUpLeftPose),
        TransitionWord([1], SamusPoseId.NormalJumpAimDiagonalDownRightPose),
        TransitionWord([1], SamusPoseId.NormalJumpAimDiagonalDownLeftPose),
        RepeatFrom([3, 4, 4, 4, 4, 80], frame: 5),
        RepeatFrom([8, ..Repeat(2, 9)], frame: 9),
        RepeatFrom([2, 16], frame: 1),
        RepeatFrom([2, 16], frame: 1),
        RepeatFrom([2, 3], frame: 1),
        RepeatFrom([2, 16], frame: 1),
        RepeatFrom([8, 6, 6], frame: 2),
        RepeatFrom([8, 16], frame: 1),
        RepeatFrom([8, 6, 6], frame: 2),
        RepeatFrom([8, 16], frame: 1),
        RepeatFrom([2, 16, 16], frame: 2),
        Hold([2]),
        RepeatFrom([16], frame: 0),
        RepeatFrom([2, 16], frame: 1),
        RepeatFrom([2, 16], frame: 1),
        Loop([6, 6, 6, 8]),
        Loop([8, 8]),
        Loop([10]),
        Loop([..Repeat(3, 8)]),
        RepeatFrom([3], segmentsBack: 1, frame: 0),
        RepeatFrom([4, 3, 2, 3, 2, 3, 2, 3, 2], frame: 1),
        Loop([8]),
        RepeatFrom([4, ..Repeat(1, 8)], frame: 1),
        Loop([8]),
        RepeatFrom([4, ..Repeat(1, 24)], frame: 1),
        Loop([8]),
        TurnTransition([2, 2, 2], SamusPoseId.FacingLeftNormalPose),
        TurnTransition([2, 2, 2], SamusPoseId.FacingRightNormalPose),
        TurnTransition([2, 2, 2], SamusPoseId.NormalJumpForwardLeftPose),
        TurnTransition([2, 2, 2], SamusPoseId.NormalJumpForwardRightPose),
        TurnTransition([2, 2, 2], SamusPoseId.CrouchingLeftPose),
        TurnTransition([2, 2, 2], SamusPoseId.CrouchingRightPose),
        TurnTransition([2, 2, 2], SamusPoseId.FallingLeftPose),
        TurnTransition([2, 2, 2], SamusPoseId.FallingRightPose),
        TurnTransition([2, 2, 2], SamusPoseId.StandingAimUpLeftPose),
        TurnTransition([2, 2, 2], SamusPoseId.StandingAimUpRightPose),
        TurnTransition([2, 2, 2], SamusPoseId.StandingAimDiagonalDownLeftPose),
        TurnTransition([2, 2, 2], SamusPoseId.StandingAimDiagonalDownRightPose),
        TurnTransition([2, 2, 2], SamusPoseId.NormalJumpAimUpLeftPose),
        TurnTransition([2, 2, 2], SamusPoseId.NormalJumpAimUpRightPose),
        TurnTransition([2, 2, 2], SamusPoseId.NormalJumpAimDownLeftPose),
        TurnTransition([2, 2, 2], SamusPoseId.NormalJumpAimDownRightPose),
        TurnTransition([2, 2, 2], SamusPoseId.FallingAimUpLeftPose),
        TurnTransition([2, 2, 2], SamusPoseId.FallingAimUpRightPose),
        TurnTransition([2, 2, 2], SamusPoseId.FallingAimDownLeftPose),
        TurnTransition([2, 2, 2], SamusPoseId.FallingAimDownRightPose),
        TurnTransition([2, 2, 2], SamusPoseId.CrouchingAimUpLeftPose),
        TurnTransition([2, 2, 2], SamusPoseId.CrouchingAimUpRightPose),
        TurnTransition([2, 2, 2], SamusPoseId.CrouchingAimDiagonalDownLeftPose),
        TurnTransition([2, 2, 2], SamusPoseId.CrouchingAimDiagonalDownRightPose),
        TurnTransition([2, 2, 2], SamusPoseId.StandingAimDiagonalUpLeftPose),
        TurnTransition([2, 2, 2], SamusPoseId.StandingAimDiagonalUpRightPose),
        TurnTransition([2, 2, 2], SamusPoseId.NormalJumpAimDiagonalUpLeftPose),
        TurnTransition([2, 2, 2], SamusPoseId.NormalJumpAimDiagonalUpRightPose),
        TurnTransition([2, 2, 2], SamusPoseId.FallingAimDiagonalUpLeftPose),
        TurnTransition([2, 2, 2], SamusPoseId.FallingAimDiagonalUpRightPose),
        TurnTransition([2, 2, 2], SamusPoseId.CrouchingAimDiagonalUpLeftPose),
        TurnTransition([2, 2, 2], SamusPoseId.CrouchingAimDiagonalUpRightPose),
        TurnTransition([2, 2, 2], SamusPoseId.SpinJumpLeftPose),
        TurnTransition([2, 2, 2], SamusPoseId.SpinJumpRightPose),
        TurnTransition([2, 2, 2], SamusPoseId.SpinJumpLeftPose),
        TurnTransition([2, 2, 2], SamusPoseId.SpinJumpRightPose),
        TurnTransition([2, 2, 2], SamusPoseId.SpinJumpLeftPose),
        TurnTransition([2, 2, 2], SamusPoseId.SpinJumpRightPose),
        Transition([2, 2, 2], SamusPoseId.DraygonGrabbedNeutralLeftPose),
        RepeatFrom([4, 3], frame: 1),
        RepeatFrom([4, 3], frame: 1),
        RepeatFrom([3, ..Repeat(2, 8)], frame: 1),
        WallJumpSelect([5, 5]),
        RepeatFrom([3, 2, 3, 2, 3, 2, 3, 2], frame: 0),
        RepeatFrom([2, 1, 2, 1, 2, 1, 2, 1], frame: 0),
        RepeatFrom([..Repeat(1, 24)], frame: 0),
        Transition([3], SamusPoseId.CrouchingRightPose),
        Transition([3], SamusPoseId.CrouchingLeftPose),
        ItemAirborneTransition([3, 3], SamusEquipmentFlags.SpringBall, SamusPoseId.MorphBallGroundRightPose, SamusPoseId.MorphBallFallingRightPose, SamusPoseId.SpringBallGroundRightPose, SamusPoseId.SpringBallFallingRightPose),
        ItemAirborneTransition([3, 3], SamusEquipmentFlags.SpringBall, SamusPoseId.MorphBallGroundLeftPose, SamusPoseId.MorphBallFallingLeftPose, SamusPoseId.SpringBallGroundLeftPose, SamusPoseId.SpringBallFallingLeftPose),
        Transition([0], SamusPoseId.UnusedPose20),
        Transition([0], SamusPoseId.UnusedPose42),
        Transition([3], SamusPoseId.FacingRightNormalPose),
        Transition([3], SamusPoseId.FacingLeftNormalPose),
        Transition([3, 3], SamusPoseId.CrouchingRightPose),
        Transition([3, 3], SamusPoseId.CrouchingLeftPose),
        ItemTransition([0], SamusEquipmentFlags.SpringBall, SamusPoseId.MorphBallGroundRightPose, SamusPoseId.SpringBallGroundRightPose),
        ItemTransition([0], SamusEquipmentFlags.SpringBall, SamusPoseId.MorphBallGroundLeftPose, SamusPoseId.SpringBallGroundLeftPose),
        ItemAirborneTransition([3, 3, 3], SamusEquipmentFlags.SpringBall, SamusPoseId.MorphBallGroundRightPose, SamusPoseId.MorphBallFallingRightPose, SamusPoseId.SpringBallGroundRightPose, SamusPoseId.SpringBallFallingRightPose),
        ItemAirborneTransition([3, 3, 3], SamusEquipmentFlags.SpringBall, SamusPoseId.UnusedPoseDf, SamusPoseId.UnusedPoseDf, SamusPoseId.UnusedPoseDf, SamusPoseId.UnusedPoseDf),
        Transition([3, 3, 3], SamusPoseId.FacingRightNormalPose),
        Transition([3, 3, 3], SamusPoseId.DraygonGrabbedNeutralLeftPose),
        Transition([3], SamusPoseId.CrouchingAimUpRightPose),
        Transition([3], SamusPoseId.CrouchingAimUpLeftPose),
        Transition([3], SamusPoseId.CrouchingAimDiagonalUpRightPose),
        Transition([3], SamusPoseId.CrouchingAimDiagonalUpLeftPose),
        Transition([3], SamusPoseId.CrouchingAimDiagonalDownRightPose),
        Transition([3], SamusPoseId.CrouchingAimDiagonalDownLeftPose),
        Transition([3], SamusPoseId.StandingAimUpRightPose),
        Transition([3], SamusPoseId.StandingAimUpLeftPose),
        Transition([3], SamusPoseId.StandingAimDiagonalUpRightPose),
        Transition([3], SamusPoseId.StandingAimDiagonalUpLeftPose),
        Transition([3], SamusPoseId.StandingAimDiagonalDownRightPose),
        Transition([3], SamusPoseId.StandingAimDiagonalDownLeftPose),
        Loop([..Repeat(6, 6)]),
        Loop([8]),
        RepeatFrom([3, 3, 1, 1], frame: 2),
        RepeatFrom([12, 12, 12, 12], frame: 0),
        Transition([3, 3, 3], SamusPoseId.FacingRightNormalPose),
        RepeatFrom([3, 3, 1, 1], frame: 2),
        RepeatFrom([12, 12, 12, 12], frame: 0),
        Transition([3, 3, 3], SamusPoseId.FacingLeftNormalPose),
        RepeatFrom([..Repeat(2, 6)], frame: 5),
        Loop([8]),
        RepeatFrom([..Repeat(3, 89), 51, 2, 2, 2, 48], frame: 93),
    ]);

    /// <summary>Builds one segment at its laid-out address; <c>earlier</c> holds the preceding segments.</summary>
    private delegate SamusAnimationSegment Placement(ushort address, IReadOnlyList<SamusAnimationSegment> earlier);

    private static SamusAnimationSegment[] Layout(ushort start, Placement[] placements)
    {
        var laidOut = new List<SamusAnimationSegment>(placements.Length);
        ushort address = start;
        foreach (Placement place in placements)
        {
            SamusAnimationSegment segment = place(address, laidOut);
            laidOut.Add(segment);
            address = (ushort)(address + segment.Length);
        }
        return [.. laidOut];
    }

    private static byte[] Repeat(byte delay, int count)
    {
        byte[] run = new byte[count];
        Array.Fill(run, delay);
        return run;
    }

    private static Placement Loop(byte[] delays) =>
        (address, earlier) =>
            new(address, delays, SamusAnimationCommand.Loop);

    private static Placement Hold(byte[] delays) =>
        (address, earlier) =>
            new(address, delays, SamusAnimationCommand.Hold);

    private static Placement LoopUnlessLowEnergy(byte[] delays) =>
        (address, earlier) =>
            new(address, delays, SamusAnimationCommand.LoopUnlessLowEnergy);

    private static Placement InstallDrainedFall(byte[] delays) =>
        (address, earlier) =>
            new(address, delays, SamusAnimationCommand.InstallDrainedFall);

    private static Placement WallJumpSelect(byte[] delays) =>
        (address, earlier) =>
            new(address, delays, SamusAnimationCommand.WallJumpSelect);

    /// <summary>Repeat from <paramref name="frame"/> of this segment.</summary>
    private static Placement RepeatFrom(byte[] delays, int frame) =>
        (address, earlier) =>
            new(address, delays, SamusAnimationCommand.RepeatFrom, repeatTarget: (ushort)(address + frame));

    /// <summary>Repeat from <paramref name="frame"/> of the shared loop body <paramref name="segmentsBack"/> segments earlier.</summary>
    private static Placement RepeatFrom(byte[] delays, int segmentsBack, int frame) =>
        (address, earlier) =>
            new(address, delays, SamusAnimationCommand.RepeatFrom, repeatTarget: (ushort)(earlier[^segmentsBack].Address + frame));

    private static Placement TurnTransition(byte[] delays, SamusPoseId pose) =>
        (address, earlier) =>
            new(address, delays, SamusAnimationCommand.TurnTransition, unequippedGrounded: pose);

    private static Placement Transition(byte[] delays, SamusPoseId pose) =>
        (address, earlier) =>
            new(address, delays, SamusAnimationCommand.Transition, unequippedGrounded: pose);

    /// <summary>A <c>$FD</c> transition whose pose the source wrote as a word.</summary>
    private static Placement TransitionWord(byte[] delays, SamusPoseId pose) =>
        (address, earlier) =>
            new(address, delays, SamusAnimationCommand.Transition, unequippedGrounded: pose, wordPose: true);

    private static Placement ItemTransition(byte[] delays,
        SamusEquipmentFlags item, SamusPoseId unequipped, SamusPoseId equipped) =>
        (address, earlier) =>
            new(address, delays, SamusAnimationCommand.ItemTransition, item: item,
            unequippedGrounded: unequipped, equippedGrounded: equipped);

    private static Placement ItemAirborneTransition(byte[] delays,
        SamusEquipmentFlags item, SamusPoseId unequippedGrounded, SamusPoseId unequippedAirborne,
        SamusPoseId equippedGrounded, SamusPoseId equippedAirborne) =>
        (address, earlier) =>
            new(address, delays, SamusAnimationCommand.ItemAirborneTransition, item: item,
            unequippedGrounded: unequippedGrounded, unequippedAirborne: unequippedAirborne,
            equippedGrounded: equippedGrounded, equippedAirborne: equippedAirborne);

    /// <summary>Encodes the native byte at a bank-$91 address inside the programs.</summary>
    internal static byte ByteAt(int bankAddress)
    {
        int low = 0;
        int high = segments.Length - 1;
        while (low < high)
        {
            int middle = (low + high + 1) >> 1;
            if (segments[middle].Address <= bankAddress) low = middle;
            else high = middle - 1;
        }
        SamusAnimationSegment segment = segments[low];
        return segment.ByteAt(bankAddress - segment.Address);
    }
}
