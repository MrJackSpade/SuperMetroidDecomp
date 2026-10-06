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
    internal ReadOnlySpan<byte> Delays => delays;
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
    internal static ReadOnlySpan<SamusAnimationSegment> Segments => segments;

    private static readonly SamusAnimationSegment[] segments =
    [
        Loop(0xB20A, [2, 3, 2, 3, 2, 3, 2, 3, 2, 3]),
        Loop(0xB215, [..Repeat(4, 6), 3, 4, 4, 3]),
        Loop(0xB220, [10]),
        RepeatFrom(0xB222, [2, 16], frame: 1),
        Loop(0xB226, [..Repeat(16, 6)]),
        TurnTransition(0xB22D, [5, 2], SamusPoseId.FacingRightNormalPose),
        TurnTransition(0xB231, [5, 2], SamusPoseId.FacingLeftNormalPose),
        TurnTransition(0xB235, [3, 5, 2], SamusPoseId.FacingRightNormalPose),
        TurnTransition(0xB23A, [3, 5, 2], SamusPoseId.FacingLeftNormalPose),
        TurnTransition(0xB23F, [5, 2], SamusPoseId.StandingAimUpRightPose),
        TurnTransition(0xB243, [5, 2], SamusPoseId.StandingAimUpLeftPose),
        TurnTransition(0xB247, [5, 2], SamusPoseId.StandingAimDiagonalUpRightPose),
        TurnTransition(0xB24B, [5, 2], SamusPoseId.StandingAimDiagonalUpLeftPose),
        TurnTransition(0xB24F, [5, 2], SamusPoseId.StandingAimDiagonalDownRightPose),
        TurnTransition(0xB253, [5, 2], SamusPoseId.StandingAimDiagonalDownLeftPose),
        InstallDrainedFall(0xB257, [2, 2, 2, 16]),
        RepeatFrom(0xB25C, [1], frame: 0),
        RepeatFrom(0xB25F, [16, 16, 16, 16], frame: 0),
        Transition(0xB265, [3], SamusPoseId.FacingRightNormalPose),
        InstallDrainedFall(0xB268, [2, 2, 16]),
        RepeatFrom(0xB26C, [1], frame: 0),
        RepeatFrom(0xB26F, [8, 16, 16, 16, 16], frame: 1),
        Transition(0xB276, [16, 16, 16], SamusPoseId.FacingLeftNormalPose),
        RepeatFrom(0xB27B, [16, 16, 16, 16, 16], segment: 0xB26F, frame: 3),
        RepeatFrom(0xB282, [16], segment: 0xB26F, frame: 3),
        RepeatFrom(0xB285, [16], frame: 0),
        Loop(0xB288, [16, 16, 16, 16]),
        Transition(0xB28D, [3], SamusPoseId.FacingRightNormalPose),
        Loop(0xB290, [16, 16, 16, 16]),
        Transition(0xB295, [3], SamusPoseId.FacingLeftNormalPose),
        LoopUnlessLowEnergy(0xB298, [10, 10, 10, 10]),
        RepeatFrom(0xB29D, [8, 8, 8, 8], frame: 0),
        LoopUnlessLowEnergy(0xB2A3, [10, 10, 10, 10]),
        RepeatFrom(0xB2A8, [8, 8, 8, 8], frame: 0),
        Loop(0xB2AE, [15, 15, 15, 15, 15]),
        Loop(0xB2B4, [16]),
        Loop(0xB2B6, [16]),
        RepeatFrom(0xB2B8, [2, 16], frame: 1),
        RepeatFrom(0xB2BC, [2, 16], frame: 1),
        RepeatFrom(0xB2C0, [2, 16], frame: 1),
        RepeatFrom(0xB2C4, [..Repeat(8, 66)], frame: 65),
        Transition(0xB308, [1], SamusPoseId.NeutralJumpRightPose),
        Transition(0xB30B, [1], SamusPoseId.NeutralJumpLeftPose),
        TransitionWord(0xB30E, [1], SamusPoseId.NormalJumpAimUpRightPose),
        TransitionWord(0xB312, [1], SamusPoseId.NormalJumpAimUpLeftPose),
        TransitionWord(0xB316, [1], SamusPoseId.NormalJumpAimDiagonalUpRightPose),
        TransitionWord(0xB31A, [1], SamusPoseId.NormalJumpAimDiagonalUpLeftPose),
        TransitionWord(0xB31E, [1], SamusPoseId.NormalJumpAimDiagonalDownRightPose),
        TransitionWord(0xB322, [1], SamusPoseId.NormalJumpAimDiagonalDownLeftPose),
        RepeatFrom(0xB326, [3, 4, 4, 4, 4, 80], frame: 5),
        RepeatFrom(0xB32E, [8, ..Repeat(2, 9)], frame: 9),
        RepeatFrom(0xB33A, [2, 16], frame: 1),
        RepeatFrom(0xB33E, [2, 16], frame: 1),
        RepeatFrom(0xB342, [2, 3], frame: 1),
        RepeatFrom(0xB346, [2, 16], frame: 1),
        RepeatFrom(0xB34A, [8, 6, 6], frame: 2),
        RepeatFrom(0xB34F, [8, 16], frame: 1),
        RepeatFrom(0xB353, [8, 6, 6], frame: 2),
        RepeatFrom(0xB358, [8, 16], frame: 1),
        RepeatFrom(0xB35C, [2, 16, 16], frame: 2),
        Hold(0xB361, [2]),
        RepeatFrom(0xB363, [16], frame: 0),
        RepeatFrom(0xB366, [2, 16], frame: 1),
        RepeatFrom(0xB36A, [2, 16], frame: 1),
        Loop(0xB36E, [6, 6, 6, 8]),
        Loop(0xB373, [8, 8]),
        Loop(0xB376, [10]),
        Loop(0xB378, [..Repeat(3, 8)]),
        RepeatFrom(0xB381, [3], segment: 0xB378, frame: 0),
        RepeatFrom(0xB384, [4, 3, 2, 3, 2, 3, 2, 3, 2], frame: 1),
        Loop(0xB38F, [8]),
        RepeatFrom(0xB391, [4, ..Repeat(1, 8)], frame: 1),
        Loop(0xB39C, [8]),
        RepeatFrom(0xB39E, [4, ..Repeat(1, 24)], frame: 1),
        Loop(0xB3B9, [8]),
        TurnTransition(0xB3BB, [2, 2, 2], SamusPoseId.FacingLeftNormalPose),
        TurnTransition(0xB3C0, [2, 2, 2], SamusPoseId.FacingRightNormalPose),
        TurnTransition(0xB3C5, [2, 2, 2], SamusPoseId.NormalJumpForwardLeftPose),
        TurnTransition(0xB3CA, [2, 2, 2], SamusPoseId.NormalJumpForwardRightPose),
        TurnTransition(0xB3CF, [2, 2, 2], SamusPoseId.CrouchingLeftPose),
        TurnTransition(0xB3D4, [2, 2, 2], SamusPoseId.CrouchingRightPose),
        TurnTransition(0xB3D9, [2, 2, 2], SamusPoseId.FallingLeftPose),
        TurnTransition(0xB3DE, [2, 2, 2], SamusPoseId.FallingRightPose),
        TurnTransition(0xB3E3, [2, 2, 2], SamusPoseId.StandingAimUpLeftPose),
        TurnTransition(0xB3E8, [2, 2, 2], SamusPoseId.StandingAimUpRightPose),
        TurnTransition(0xB3ED, [2, 2, 2], SamusPoseId.StandingAimDiagonalDownLeftPose),
        TurnTransition(0xB3F2, [2, 2, 2], SamusPoseId.StandingAimDiagonalDownRightPose),
        TurnTransition(0xB3F7, [2, 2, 2], SamusPoseId.NormalJumpAimUpLeftPose),
        TurnTransition(0xB3FC, [2, 2, 2], SamusPoseId.NormalJumpAimUpRightPose),
        TurnTransition(0xB401, [2, 2, 2], SamusPoseId.NormalJumpAimDownLeftPose),
        TurnTransition(0xB406, [2, 2, 2], SamusPoseId.NormalJumpAimDownRightPose),
        TurnTransition(0xB40B, [2, 2, 2], SamusPoseId.FallingAimUpLeftPose),
        TurnTransition(0xB410, [2, 2, 2], SamusPoseId.FallingAimUpRightPose),
        TurnTransition(0xB415, [2, 2, 2], SamusPoseId.FallingAimDownLeftPose),
        TurnTransition(0xB41A, [2, 2, 2], SamusPoseId.FallingAimDownRightPose),
        TurnTransition(0xB41F, [2, 2, 2], SamusPoseId.CrouchingAimUpLeftPose),
        TurnTransition(0xB424, [2, 2, 2], SamusPoseId.CrouchingAimUpRightPose),
        TurnTransition(0xB429, [2, 2, 2], SamusPoseId.CrouchingAimDiagonalDownLeftPose),
        TurnTransition(0xB42E, [2, 2, 2], SamusPoseId.CrouchingAimDiagonalDownRightPose),
        TurnTransition(0xB433, [2, 2, 2], SamusPoseId.StandingAimDiagonalUpLeftPose),
        TurnTransition(0xB438, [2, 2, 2], SamusPoseId.StandingAimDiagonalUpRightPose),
        TurnTransition(0xB43D, [2, 2, 2], SamusPoseId.NormalJumpAimDiagonalUpLeftPose),
        TurnTransition(0xB442, [2, 2, 2], SamusPoseId.NormalJumpAimDiagonalUpRightPose),
        TurnTransition(0xB447, [2, 2, 2], SamusPoseId.FallingAimDiagonalUpLeftPose),
        TurnTransition(0xB44C, [2, 2, 2], SamusPoseId.FallingAimDiagonalUpRightPose),
        TurnTransition(0xB451, [2, 2, 2], SamusPoseId.CrouchingAimDiagonalUpLeftPose),
        TurnTransition(0xB456, [2, 2, 2], SamusPoseId.CrouchingAimDiagonalUpRightPose),
        TurnTransition(0xB45B, [2, 2, 2], SamusPoseId.SpinJumpLeftPose),
        TurnTransition(0xB460, [2, 2, 2], SamusPoseId.SpinJumpRightPose),
        TurnTransition(0xB465, [2, 2, 2], SamusPoseId.SpinJumpLeftPose),
        TurnTransition(0xB46A, [2, 2, 2], SamusPoseId.SpinJumpRightPose),
        TurnTransition(0xB46F, [2, 2, 2], SamusPoseId.SpinJumpLeftPose),
        TurnTransition(0xB474, [2, 2, 2], SamusPoseId.SpinJumpRightPose),
        Transition(0xB479, [2, 2, 2], SamusPoseId.DraygonGrabbedNeutralLeftPose),
        RepeatFrom(0xB47E, [4, 3], frame: 1),
        RepeatFrom(0xB482, [4, 3], frame: 1),
        RepeatFrom(0xB486, [3, ..Repeat(2, 8)], frame: 1),
        WallJumpSelect(0xB491, [5, 5]),
        RepeatFrom(0xB494, [3, 2, 3, 2, 3, 2, 3, 2], frame: 0),
        RepeatFrom(0xB49E, [2, 1, 2, 1, 2, 1, 2, 1], frame: 0),
        RepeatFrom(0xB4A8, [..Repeat(1, 24)], frame: 0),
        Transition(0xB4C2, [3], SamusPoseId.CrouchingRightPose),
        Transition(0xB4C5, [3], SamusPoseId.CrouchingLeftPose),
        ItemAirborneTransition(0xB4C8, [3, 3], SamusEquipmentFlags.SpringBall, SamusPoseId.MorphBallGroundRightPose, SamusPoseId.MorphBallFallingRightPose, SamusPoseId.SpringBallGroundRightPose, SamusPoseId.SpringBallFallingRightPose),
        ItemAirborneTransition(0xB4D1, [3, 3], SamusEquipmentFlags.SpringBall, SamusPoseId.MorphBallGroundLeftPose, SamusPoseId.MorphBallFallingLeftPose, SamusPoseId.SpringBallGroundLeftPose, SamusPoseId.SpringBallFallingLeftPose),
        Transition(0xB4DA, [0], SamusPoseId.UnusedPose20),
        Transition(0xB4DD, [0], SamusPoseId.UnusedPose42),
        Transition(0xB4E0, [3], SamusPoseId.FacingRightNormalPose),
        Transition(0xB4E3, [3], SamusPoseId.FacingLeftNormalPose),
        Transition(0xB4E6, [3, 3], SamusPoseId.CrouchingRightPose),
        Transition(0xB4EA, [3, 3], SamusPoseId.CrouchingLeftPose),
        ItemTransition(0xB4EE, [0], SamusEquipmentFlags.SpringBall, SamusPoseId.MorphBallGroundRightPose, SamusPoseId.SpringBallGroundRightPose),
        ItemTransition(0xB4F4, [0], SamusEquipmentFlags.SpringBall, SamusPoseId.MorphBallGroundLeftPose, SamusPoseId.SpringBallGroundLeftPose),
        ItemAirborneTransition(0xB4FA, [3, 3, 3], SamusEquipmentFlags.SpringBall, SamusPoseId.MorphBallGroundRightPose, SamusPoseId.MorphBallFallingRightPose, SamusPoseId.SpringBallGroundRightPose, SamusPoseId.SpringBallFallingRightPose),
        ItemAirborneTransition(0xB504, [3, 3, 3], SamusEquipmentFlags.SpringBall, SamusPoseId.UnusedPoseDf, SamusPoseId.UnusedPoseDf, SamusPoseId.UnusedPoseDf, SamusPoseId.UnusedPoseDf),
        Transition(0xB50E, [3, 3, 3], SamusPoseId.FacingRightNormalPose),
        Transition(0xB513, [3, 3, 3], SamusPoseId.DraygonGrabbedNeutralLeftPose),
        Transition(0xB518, [3], SamusPoseId.CrouchingAimUpRightPose),
        Transition(0xB51B, [3], SamusPoseId.CrouchingAimUpLeftPose),
        Transition(0xB51E, [3], SamusPoseId.CrouchingAimDiagonalUpRightPose),
        Transition(0xB521, [3], SamusPoseId.CrouchingAimDiagonalUpLeftPose),
        Transition(0xB524, [3], SamusPoseId.CrouchingAimDiagonalDownRightPose),
        Transition(0xB527, [3], SamusPoseId.CrouchingAimDiagonalDownLeftPose),
        Transition(0xB52A, [3], SamusPoseId.StandingAimUpRightPose),
        Transition(0xB52D, [3], SamusPoseId.StandingAimUpLeftPose),
        Transition(0xB530, [3], SamusPoseId.StandingAimDiagonalUpRightPose),
        Transition(0xB533, [3], SamusPoseId.StandingAimDiagonalUpLeftPose),
        Transition(0xB536, [3], SamusPoseId.StandingAimDiagonalDownRightPose),
        Transition(0xB539, [3], SamusPoseId.StandingAimDiagonalDownLeftPose),
        Loop(0xB53C, [..Repeat(6, 6)]),
        Loop(0xB543, [8]),
        RepeatFrom(0xB545, [3, 3, 1, 1], frame: 2),
        RepeatFrom(0xB54B, [12, 12, 12, 12], frame: 0),
        Transition(0xB551, [3, 3, 3], SamusPoseId.FacingRightNormalPose),
        RepeatFrom(0xB556, [3, 3, 1, 1], frame: 2),
        RepeatFrom(0xB55C, [12, 12, 12, 12], frame: 0),
        Transition(0xB562, [3, 3, 3], SamusPoseId.FacingLeftNormalPose),
        RepeatFrom(0xB567, [..Repeat(2, 6)], frame: 5),
        Loop(0xB56F, [8]),
        RepeatFrom(0xB571, [..Repeat(3, 89), 51, 2, 2, 2, 48], frame: 93),
    ];

    private static byte[] Repeat(byte delay, int count)
    {
        byte[] run = new byte[count];
        Array.Fill(run, delay);
        return run;
    }

    private static SamusAnimationSegment Loop(ushort address, byte[] delays) =>
        new(address, delays, SamusAnimationCommand.Loop);

    private static SamusAnimationSegment Hold(ushort address, byte[] delays) =>
        new(address, delays, SamusAnimationCommand.Hold);

    private static SamusAnimationSegment LoopUnlessLowEnergy(ushort address, byte[] delays) =>
        new(address, delays, SamusAnimationCommand.LoopUnlessLowEnergy);

    private static SamusAnimationSegment InstallDrainedFall(ushort address, byte[] delays) =>
        new(address, delays, SamusAnimationCommand.InstallDrainedFall);

    private static SamusAnimationSegment WallJumpSelect(ushort address, byte[] delays) =>
        new(address, delays, SamusAnimationCommand.WallJumpSelect);

    /// <summary>Repeat from <paramref name="frame"/> of this segment.</summary>
    private static SamusAnimationSegment RepeatFrom(ushort address, byte[] delays, int frame) =>
        new(address, delays, SamusAnimationCommand.RepeatFrom, repeatTarget: (ushort)(address + frame));

    /// <summary>Repeat from <paramref name="frame"/> of the shared loop body at <paramref name="segment"/>.</summary>
    private static SamusAnimationSegment RepeatFrom(ushort address, byte[] delays, ushort segment, int frame) =>
        new(address, delays, SamusAnimationCommand.RepeatFrom, repeatTarget: (ushort)(segment + frame));

    private static SamusAnimationSegment TurnTransition(ushort address, byte[] delays, SamusPoseId pose) =>
        new(address, delays, SamusAnimationCommand.TurnTransition, unequippedGrounded: pose);

    private static SamusAnimationSegment Transition(ushort address, byte[] delays, SamusPoseId pose) =>
        new(address, delays, SamusAnimationCommand.Transition, unequippedGrounded: pose);

    /// <summary>A <c>$FD</c> transition whose pose the source wrote as a word.</summary>
    private static SamusAnimationSegment TransitionWord(ushort address, byte[] delays, SamusPoseId pose) =>
        new(address, delays, SamusAnimationCommand.Transition, unequippedGrounded: pose, wordPose: true);

    private static SamusAnimationSegment ItemTransition(ushort address, byte[] delays,
        SamusEquipmentFlags item, SamusPoseId unequipped, SamusPoseId equipped) =>
        new(address, delays, SamusAnimationCommand.ItemTransition, item: item,
            unequippedGrounded: unequipped, equippedGrounded: equipped);

    private static SamusAnimationSegment ItemAirborneTransition(ushort address, byte[] delays,
        SamusEquipmentFlags item, SamusPoseId unequippedGrounded, SamusPoseId unequippedAirborne,
        SamusPoseId equippedGrounded, SamusPoseId equippedAirborne) =>
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
