namespace SuperMetroid.Core.Game;

/// <summary>One compiled mechanics-owned word at its native bank-$B2 address.</summary>
internal readonly record struct NinjaSpacePirateInstructionMechanicsWord(
    ushort Address,
    ushort Value);

/// <summary>
/// Compiled engine-control words for every production ninja Space Pirate body program.
/// Interleaved extended-spritemap pointers remain live cartridge presentation data.
/// </summary>
internal static class NinjaSpacePirateInstructionProgramDefinitions
{
    /// <summary><c>InstList_PirateNinja_ProjectileClawAttack_Left</c> at $B2:F15C.</summary>
    internal const ushort ClawAttackLeft = 0xf15c;
    /// <summary><c>InstList_PirateNinja_SpinJumpLeft_0</c> at $B2:F1C4.</summary>
    internal const ushort SpinJumpLeft = 0xf1c4;
    /// <summary><c>InstList_PirateNinja_Active_FacingLeft_0</c> at $B2:F22E.</summary>
    internal const ushort ActiveFacingLeft = 0xf22e;
    /// <summary><c>InstList_PirateNinja_Flinch_FacingLeft</c> at $B2:F270.</summary>
    internal const ushort FlinchFacingLeft = 0xf270;
    /// <summary><c>InstList_PirateNinja_DivekickLeft_Jump_0</c> at $B2:F27C.</summary>
    internal const ushort DivekickLeftJump = 0xf27c;
    /// <summary><c>InstList_PirateNinja_DivekickLeft_Divekick</c> at $B2:F2A0.</summary>
    internal const ushort DivekickLeftDive = 0xf2a0;
    /// <summary><c>InstList_PirateNinja_DivekickLeft_WalkToLeftPost_0</c> at $B2:F2B2.</summary>
    internal const ushort WalkToLeftPost = 0xf2b2;
    /// <summary><c>InstList_PirateNinja_Initial_FacingLeft_0</c> at $B2:F2DA.</summary>
    internal const ushort InitialFacingLeft = 0xf2da;
    /// <summary><c>InstList_PirateNinja_Land_FacingLeft_0</c> at $B2:F2F8.</summary>
    internal const ushort LandFacingLeft = 0xf2f8;
    /// <summary><c>InstList_PirateNinja_StandingKick_FacingLeft</c> at $B2:F32E.</summary>
    internal const ushort KickFacingLeft = 0xf32e;
    /// <summary><c>InstList_PirateNinja_ProjectileClawAttack_Right</c> at $B2:F34A.</summary>
    internal const ushort ClawAttackRight = 0xf34a;
    /// <summary><c>InstList_PirateNinja_SpinJumpRight_0</c> at $B2:F3B2.</summary>
    internal const ushort SpinJumpRight = 0xf3b2;
    /// <summary><c>InstList_PirateNinja_Active_FacingRight_0</c> at $B2:F420.</summary>
    internal const ushort ActiveFacingRight = 0xf420;
    /// <summary><c>InstList_PirateNinja_Flinch_FacingRight</c> at $B2:F462.</summary>
    internal const ushort FlinchFacingRight = 0xf462;
    /// <summary><c>InstList_PirateNinja_DivekickRight_Jump_0</c> at $B2:F46E.</summary>
    internal const ushort DivekickRightJump = 0xf46e;
    /// <summary><c>InstList_PirateNinja_DivekickRight_Divekick</c> at $B2:F492.</summary>
    internal const ushort DivekickRightDive = 0xf492;
    /// <summary><c>InstList_PirateNinja_DivekickRight_WalkToRightPost_0</c> at $B2:F4A4.</summary>
    internal const ushort WalkToRightPost = 0xf4a4;
    /// <summary><c>InstList_PirateNinja_Initial_FacingRight_0</c> at $B2:F4CC.</summary>
    internal const ushort InitialFacingRight = 0xf4cc;
    /// <summary><c>InstList_PirateNinja_Land_FacingRight_0</c> at $B2:F4EA.</summary>
    internal const ushort LandFacingRight = 0xf4ea;
    /// <summary><c>InstList_PirateNinja_StandingKick_FacingRight</c> at $B2:F51A.</summary>
    internal const ushort KickFacingRight = 0xf51a;

    // Independent phase dwell, launch magnitudes and palette choices remain
    // REQUIRED under Words. Composition removes repeated addresses/control data,
    // not those separate authored scalar obligations.
    private const ushort AttackWindupHold = 5, ClawReleaseHold = 2, SpinPreparationHold = 8,
        SpinHold = 1, ActiveHold = 10, FlinchHold = 16, JumpPreparationHold = 8,
        JumpFlashHold = 4, DiveHold = 1, WalkHold = 5, InitialLongHold = 32,
        InitialShortHold = 10, LandingHold = 4, LandingDeepHold = 8,
        ReadyHold = 10, KickMoveHold = 4, KickExtendedHold = 32;
    /// <summary>$B2:F17C/F1AC and mirrored F36A/F39A: two independent horizontal claw-spawn magnitudes.</summary>
    private const int FirstClawX = 32, SecondClawX = 16;
    /// <summary>$B2:F17E/F1AE: first/second claw release is eight pixels above/below the enemy origin.</summary>
    private const int ClawY = 8;
    /// <summary>$B2:F28C/F294: normal/flash sprite palette selectors, occupying attribute bits9-11.</summary>
    private const ushort NormalPalette = 1 << 9, FlashPalette = 7 << 9;
    /// <summary>$B2:F182 and facing counterparts queue library-two attack sound $66.</summary>
    private const ushort AttackSound = 0x66;
    /// <summary>$B2:F208/F3F6 queue library-two spin sound $3F.</summary>
    private const ushort SpinSound = 0x3f;

    internal static int MechanicsWordCount => 308;
    internal static int PresentationWordCount => 140;
    internal static NinjaSpacePirateInstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new IndexOutOfRangeException();
        return Select(index, visual: false);
    }
    internal static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount) throw new IndexOutOfRangeException();
        return Select(index, visual: true).Address;
    }

    /// <summary>
    /// $B2:F15C-F535: twenty facing-paired action programs. Timed poses occupy
    /// four bytes; instructions and operands occupy two each. Explicit starts
    /// skip the native unused walking/pose programs and unreachable sleep words.
    /// </summary>
    private static NinjaSpacePirateInstructionMechanicsWord Select(int index, bool visual)
    {
        var layout = new Layout(index, visual);
        for (int facing = 0; facing < 2; facing++)
        {
            bool right = facing != 0;
            ushort active = right ? ActiveFacingRight : ActiveFacingLeft;
            layout.Begin(right ? ClawAttackRight : ClawAttackLeft);
            layout.Function(NinjaSpacePirateFunction.NoOperation);
            for (int attack = 0; attack < 2; attack++)
                layout.Claw(right, second: attack != 0);
            layout.Goto(active);

            layout.Begin(right ? SpinJumpRight : SpinJumpLeft);
            layout.Function(NinjaSpacePirateFunction.NoOperation);
            layout.Claw(right, second: false);
            layout.Word(SpacePirateInstructionCodes.Instruction_PirateNinja_ResetSpeed);
            layout.Function(NinjaSpacePirateFunction.NoOperation);
            layout.Pose(SpinPreparationHold);
            layout.Function(right ? NinjaSpacePirateFunction.SpinJumpRightRising : NinjaSpacePirateFunction.SpinJumpLeftRising);
            ushort spinLoop = layout.Cursor;
            layout.Sound(SpinSound);
            layout.Poses(8, SpinHold);
            layout.Goto(spinLoop);

            layout.Begin(active);
            layout.Function(NinjaSpacePirateFunction.Active);
            ushort activeLoop = layout.Cursor;
            layout.Poses(4, ActiveHold);
            layout.Function(NinjaSpacePirateFunction.NoOperation);
            layout.Word(SpacePirateInstructionCodes.Instruction_PirateNinja_SetFunction0FAC_Active);
            layout.Goto(activeLoop);

            layout.Begin(right ? FlinchFacingRight : FlinchFacingLeft);
            layout.Function(NinjaSpacePirateFunction.NoOperation);
            layout.Pose(FlinchHold);
            layout.Goto(active);

            layout.Begin(right ? DivekickRightJump : DivekickLeftJump);
            layout.Function(NinjaSpacePirateFunction.NoOperation);
            layout.Pose(JumpPreparationHold);
            layout.Word(right ? SpacePirateInstructionCodes.Instruction_PirateNinja_SetRightDivekickJumpInitialYSpeed
                : SpacePirateInstructionCodes.Instruction_PirateNinja_SetLeftDivekickJumpInitialYSpeed);
            layout.Function(right ? NinjaSpacePirateFunction.DivekickRightJump : NinjaSpacePirateFunction.DivekickLeftJump);
            ushort flashLoop = layout.Cursor;
            layout.Palette(NormalPalette); layout.Pose(JumpFlashHold);
            layout.Palette(FlashPalette); layout.Pose(JumpFlashHold);
            layout.Goto(flashLoop);

            layout.Begin(right ? DivekickRightDive : DivekickLeftDive);
            layout.Palette(FlashPalette);
            layout.Function(right ? NinjaSpacePirateFunction.DivekickRightDive : NinjaSpacePirateFunction.DivekickLeftDive);
            layout.Sound(AttackSound);
            layout.Pose(DiveHold);
            layout.Word(CommonEnemyInstructionCodes.Sleep);

            layout.Begin(right ? WalkToRightPost : WalkToLeftPost);
            layout.Function(right ? NinjaSpacePirateFunction.DivekickRightWalkToPost : NinjaSpacePirateFunction.DivekickLeftWalkToPost);
            ushort walkLoop = layout.Cursor;
            layout.Poses(8, WalkHold);
            layout.Goto(walkLoop);

            layout.Begin(right ? InitialFacingRight : InitialFacingLeft);
            layout.Function(NinjaSpacePirateFunction.Initial);
            ushort initialLoop = layout.Cursor;
            for (int pose = 0; pose < 5; pose++)
                layout.Pose((pose & 1) == 0 ? InitialLongHold : InitialShortHold);
            layout.Goto(initialLoop);

            layout.Begin(right ? LandFacingRight : LandFacingLeft);
            layout.Palette(NormalPalette);
            layout.Function(NinjaSpacePirateFunction.NoOperation);
            for (int pose = 0; pose < 4; pose++) layout.Pose(pose == 1 ? LandingDeepHold : LandingHold);
            layout.Function(NinjaSpacePirateFunction.ReadyToDivekick);
            ushort readyLoop = layout.Cursor;
            layout.Poses(4, ReadyHold);
            layout.Goto(readyLoop);

            layout.Begin(right ? KickFacingRight : KickFacingLeft);
            layout.Function(NinjaSpacePirateFunction.NoOperation);
            layout.Pose(KickMoveHold);
            layout.Sound(AttackSound);
            layout.Pose(KickMoveHold);
            layout.Pose(KickExtendedHold);
            layout.Pose(KickMoveHold);
            layout.Goto(active);
        }
        return layout.Result;
    }

    private ref struct Layout(int requested, bool visual)
    {
        internal ushort Cursor { get; private set; }
        private int mechanics, presentation;
        internal NinjaSpacePirateInstructionMechanicsWord Result { get; private set; }
        internal void Begin(ushort address) => Cursor = address;
        internal void Word(ushort value)
        {
            if (!visual && mechanics == requested) Result = new(Cursor, value);
            mechanics++; Cursor += sizeof(ushort);
        }
        internal void Pose(ushort duration)
        {
            Word(duration);
            if (visual && presentation == requested) Result = new(Cursor, 0);
            presentation++; Cursor += sizeof(ushort);
        }
        internal void Poses(int count, ushort duration)
        {
            for (int pose = 0; pose < count; pose++) Pose(duration);
        }
        internal void Function(NinjaSpacePirateFunction function)
        {
            Word(SpacePirateInstructionCodes.Instruction_PirateWall_FunctionInY); Word((ushort)function);
        }
        internal void Palette(ushort palette)
        {
            Word(SpacePirateInstructionCodes.Instruction_PirateNinja_PaletteIndexInY); Word(palette);
        }
        internal void Sound(ushort sound)
        {
            Word(SpacePirateInstructionCodes.Instruction_PirateNinja_QueueSoundInY_Lib2_Max6); Word(sound);
        }
        internal void Goto(ushort address) { Word(CommonEnemyInstructionCodes.Goto); Word(address); }
        internal void Claw(bool right, bool second)
        {
            Poses(5, AttackWindupHold); Pose(ClawReleaseHold);
            Word(SpacePirateInstructionCodes.Instruction_PirateNinja_SpawnClawProjWithThrowDirSpawnOffset);
            Word((ushort)(right ? 1 : 0));
            Word(unchecked((ushort)((right ? 1 : -1) * (second ? SecondClawX : FirstClawX))));
            Word(unchecked((ushort)(second ? ClawY : -ClawY)));
            // The right-hand second throw publishes its first release pose before
            // the sound command; every other release plays sound before all poses.
            if (right && second) Pose(ClawReleaseHold);
            Sound(AttackSound);
            Poses(right && second ? 2 : 3, ClawReleaseHold);
        }
    }
    internal static ushort ReadMechanicsWord(ushort address)
    {
        int low = 0;
        int high = MechanicsWordCount - 1;
        while (low <= high)
        {
            int middle = low + ((high - low) >> 1);
            NinjaSpacePirateInstructionMechanicsWord candidate = MechanicsWord(middle);
            if (candidate.Address == address) return candidate.Value;
            if (candidate.Address < address) low = middle + 1;
            else high = middle - 1;
        }
        throw new InvalidDataException(
            $"Ninja Space Pirate instruction mechanics pointer $B2:{address:X4} is not compiled.");
    }

    internal static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xb20000) return false;
        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < MechanicsWordCount; index++)
        {
            ushort wordAddress = MechanicsWord(index).Address;
            if (bankAddress == wordAddress || bankAddress == unchecked((ushort)(wordAddress + 1)))
                return true;
        }
        return false;
    }
}
