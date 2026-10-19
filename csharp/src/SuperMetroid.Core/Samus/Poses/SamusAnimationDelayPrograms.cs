namespace SuperMetroid.Core.Samus.Poses;

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
    /// <summary>
    /// Contiguous segments from the start of the pose delay streams, laid out from preceding encoded lengths
    /// so individual start offsets do not need a separate stored table.
    /// </summary>
    internal static readonly SamusAnimationSegment[] segments = Layout(
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

    /// <summary>Builds one segment at its laid-out stream offset; <c>earlier</c> holds the preceding segments.</summary>
    private delegate SamusAnimationSegment Placement(int offset, IReadOnlyList<SamusAnimationSegment> earlier);

    /// <summary>Places each segment at the prior segment's end, from stream offset zero, and resolves earlier repeat targets.</summary>
    private static SamusAnimationSegment[] Layout(Placement[] placements)
    {
        var laidOut = new List<SamusAnimationSegment>(placements.Length);
        int offset = 0;
        foreach (Placement place in placements)
        {
            SamusAnimationSegment segment = place(offset, laidOut);
            laidOut.Add(segment);
            offset += segment.Length;
        }
        return [.. laidOut];
    }

    /// <summary>Expands a uniform authored pose-delay run without changing its byte cadence.</summary>
    private static byte[] Repeat(byte delay, int count)
    {
        byte[] run = new byte[count];
        Array.Fill(run, delay);
        return run;
    }

    /// <summary>Ends a duration run with the frame-zero loop command.</summary>
    private static Placement Loop(byte[] delays) =>
        (offset, earlier) =>
            new(offset, delays, SamusAnimationCommand.Loop, new SamusAnimationTransition());

    /// <summary>Ends a duration run with the no-op control byte that leaves the timer unchanged.</summary>
    private static Placement Hold(byte[] delays) =>
        (offset, earlier) =>
            new(offset, delays, SamusAnimationCommand.Hold, new SamusAnimationTransition());

    /// <summary>Loops breathing animation unless Samus's energy enters the drained phase.</summary>
    private static Placement LoopUnlessLowEnergy(byte[] delays) =>
        (offset, earlier) =>
            new(offset, delays, SamusAnimationCommand.LoopUnlessLowEnergy, new SamusAnimationTransition());

    /// <summary>Installs the drained-fall animation handler after the duration run.</summary>
    private static Placement InstallDrainedFall(byte[] delays) =>
        (offset, earlier) =>
            new(offset, delays, SamusAnimationCommand.InstallDrainedFall, new SamusAnimationTransition());

    /// <summary>Selects ordinary, Space Jump, or Screw Attack wall-jump frames.</summary>
    private static Placement WallJumpSelect(byte[] delays) =>
        (offset, earlier) =>
            new(offset, delays, SamusAnimationCommand.WallJumpSelect, new SamusAnimationTransition());

    /// <summary>Repeat from <paramref name="frame"/> of this segment.</summary>
    private static Placement RepeatFrom(byte[] delays, int frame) =>
        (offset, earlier) =>
            new(offset, delays, SamusAnimationCommand.RepeatFrom, new SamusAnimationTransition { RepeatOffset = offset + frame });

    /// <summary>Repeat from <paramref name="frame"/> of the shared loop body <paramref name="segmentsBack"/> segments earlier.</summary>
    private static Placement RepeatFrom(byte[] delays, int segmentsBack, int frame) =>
        (offset, earlier) =>
            new(offset, delays, SamusAnimationCommand.RepeatFrom, new SamusAnimationTransition { RepeatOffset = earlier[^segmentsBack].Offset + frame });

    /// <summary>Publishes a turn's final pose unless auto-jump supersedes it.</summary>
    private static Placement TurnTransition(byte[] delays, SamusPoseId pose) =>
        (offset, earlier) =>
            new(offset, delays, SamusAnimationCommand.TurnTransition, new SamusAnimationTransition { UnequippedGrounded = pose });

    /// <summary>Publishes a one-byte transitional pose after its preceding durations.</summary>
    private static Placement Transition(byte[] delays, SamusPoseId pose) =>
        (offset, earlier) =>
            new(offset, delays, SamusAnimationCommand.Transition, new SamusAnimationTransition { UnequippedGrounded = pose });

    /// <summary>A <c>$FD</c> transition whose pose the source wrote as a word.</summary>
    private static Placement TransitionWord(byte[] delays, SamusPoseId pose) =>
        (offset, earlier) =>
            new(offset, delays, SamusAnimationCommand.Transition, new SamusAnimationTransition { UnequippedGrounded = pose, WordPose = true });

    /// <summary>Selects a grounded transitional pose according to the tested equipment bit.</summary>
    private static Placement ItemTransition(byte[] delays,
        SamusEquipmentFlags item, SamusPoseId unequipped, SamusPoseId equipped) =>
        (offset, earlier) =>
            new(offset, delays, SamusAnimationCommand.ItemTransition, new SamusAnimationTransition { Item = item, UnequippedGrounded = unequipped, EquippedGrounded = equipped });

    /// <summary>Selects among grounded and airborne poses with and without the tested item.</summary>
    private static Placement ItemAirborneTransition(byte[] delays,
        SamusEquipmentFlags item, SamusPoseId unequippedGrounded, SamusPoseId unequippedAirborne,
        SamusPoseId equippedGrounded, SamusPoseId equippedAirborne) =>
        (offset, earlier) =>
            new(offset, delays, SamusAnimationCommand.ItemAirborneTransition, new SamusAnimationTransition { Item = item, UnequippedGrounded = unequippedGrounded, UnequippedAirborne = unequippedAirborne, EquippedGrounded = equippedGrounded, EquippedAirborne = equippedAirborne });

    /// <summary>Encodes the native byte at a bank-$91 address inside the programs.</summary>
    internal static byte ByteAt(int bankAddress)
    {
        int streamOffset = bankAddress - SnesAddressMath.OffsetInBank(SamusAnimationDelayDefinitions.DelayStreamsAddress);
        int low = 0;
        int high = segments.Length - 1;
        while (low < high)
        {
            int middle = (low + high + 1) >> 1;
            if (segments[middle].Offset <= streamOffset) low = middle;
            else high = middle - 1;
        }
        SamusAnimationSegment segment = segments[low];
        return segment.ByteAt(streamOffset - segment.Offset);
    }
}
