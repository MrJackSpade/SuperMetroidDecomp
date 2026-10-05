namespace SuperMetroid.Core.Game;

internal readonly record struct TorizoExplosionInstructionMechanicsWord(
    ushort Address,
    ushort Value);

/// <summary>
/// Compiled control for Bomb Torizo's low-health and death-explosion programs at
/// $86:A3CB-$A455. Their fifteen interleaved spritemap operands use extracted
/// presentation art.
/// </summary>
internal static class TorizoExplosionInstructionProgramDefinitions
{
    /// <summary><c>InstList_EnemyProjectile_BombTorizoLowHealthExplosion_0</c> at $86:A3CB.</summary>
    internal const ushort LowHealthInitial = 0xa3cb;
    /// <summary><c>InstList_EnemyProjectile_BombTorizoLowHealthExplosion_1</c> at $86:A3D5.</summary>
    internal const ushort LowHealthLoop = 0xa3d5;
    /// <summary><c>InstList_EnemyProjectile_TorizoDeathExplosion_0</c> at $86:A3FA.</summary>
    internal const ushort DeathInitial = 0xa3fa;
    /// <summary><c>InstList_EnemyProjectile_TorizoDeathExplosion_1</c> at $86:A408.</summary>
    internal const ushort DeathExplosionLoop = 0xa408;
    /// <summary><c>InstList_EnemyProjectile_TorizoDeathExplosion_2</c> at $86:A431.</summary>
    internal const ushort DeathSmokeSetup = 0xa431;
    /// <summary><c>InstList_EnemyProjectile_TorizoDeathExplosion_3</c> at $86:A435.</summary>
    internal const ushort DeathSmokeLoop = 0xa435;

    // Independent timing, random spread and repetition choices remain pending
    // under issue1165. Program geometry does not exempt these operand payloads.
    private static readonly ushort[] SmallExplosionHolds = [2, 2, 3, 3, 2];
    private static readonly ushort[] LargeExplosionHolds = [4, 6, 5, 5, 5, 6];
    /// <summary>$86:A3CF/A3FE: common property mask applied by both explosion initializers.</summary>
    private const ushort ExplosionPropertyMask = 0x3000;
    /// <summary>$86:A3D9/A3DB: low-health random spread masks.</summary>
    private const ushort SmallSpreadMask = 0x000f;
    /// <summary>$86:A40C/A439: death explosion/smoke horizontal random spread mask.</summary>
    private const ushort DeathHorizontalSpreadMask = 0x001f;
    /// <summary>$86:A40E: large explosion packed vertical bias and random mask.</summary>
    private const ushort LargeVerticalSpread = 0x103f;
    /// <summary>$86:A43B: smoke packed vertical bias and random mask.</summary>
    private const ushort SmokeVerticalSpread = 0x043f;

    private enum ExplosionPhase { LowHealth, LargeDeath, DeathSmoke }

    internal static int MechanicsWordCount => 53;
    internal static int PresentationWordCount => 15;
    internal static TorizoExplosionInstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new ArgumentOutOfRangeException(nameof(index));
        return BuildLayout(index, false).Selected;
    }
    internal static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount) throw new ArgumentOutOfRangeException(nameof(index));
        return BuildLayout(index, true).Selected.Address;
    }

    /// <summary>
    /// $86:A3CB-A455: low-health, large-death and smoke lists share position reset,
    /// random spread, a sound command with one packed operand byte, timed poses,
    /// counted back-edge and deletion. The death initializer may branch to smoke.
    /// </summary>
    private static Layout BuildLayout(int index, bool presentation)
    {
        var layout = new Layout(index, presentation);
        BuildExplosion(ref layout, ExplosionPhase.LowHealth);
        BuildExplosion(ref layout, ExplosionPhase.LargeDeath);
        BuildExplosion(ref layout, ExplosionPhase.DeathSmoke);
        return layout;
    }

    private static void BuildExplosion(ref Layout layout, ExplosionPhase phase)
    {
        bool small = phase == ExplosionPhase.LowHealth;
        bool smoke = phase == ExplosionPhase.DeathSmoke;
        layout.Address = small ? LowHealthInitial : smoke ? DeathSmokeSetup : DeathInitial;
        if (!smoke)
        {
            layout.Word(EnemyProjectileCodePointers.Instruction_EnemyProjectile_ClearPreInstruction);
            layout.Command(EnemyProjectileCodePointers.Instruction_EnemyProjectile_Properties_OrY, ExplosionPropertyMask);
            if (!small) layout.Command(EnemyProjectileCodePointers.Instruction_EnemyProjectile_GotoY_Probability_1_4, DeathSmokeSetup);
        }
        layout.Command(EnemyProjectileCodePointers.Instruction_EnemyProjectile_TimerInY, small ? (ushort)3 : (ushort)2);
        ushort loop = layout.Address;
        layout.Word(EnemyProjectileCodePointers.Instruction_EnemyProjectile_Torizo_ResetPosition);
        layout.Word(EnemyProjectileCodePointers.Instruction_MoveRandomlyWithinXRadius_YRadius);
        layout.Word(small ? SmallSpreadMask : DeathHorizontalSpreadMask);
        layout.Word(small ? SmallSpreadMask : smoke ? SmokeVerticalSpread : LargeVerticalSpread);
        layout.Word(EnemyProjectileCodePointers.Instruction_EnemyProjectile_QueueSoundInY_Lib2_Max6);
        layout.Address++; // The sound argument is a byte, outside this word catalog.
        int poses = small ? SmallExplosionHolds.Length : smoke ? 4 : LargeExplosionHolds.Length;
        for (int pose = 0; pose < poses; pose++)
            layout.Pose(small ? SmallExplosionHolds[pose] : smoke ? (ushort)8 : LargeExplosionHolds[pose]);
        layout.Command(EnemyProjectileCodePointers.Instruction_EnemyProjectile_DecrementTimer_GotoYIfNonZero, loop);
        layout.Word(EnemyProjectileCodePointers.Instruction_EnemyProjectile_Delete);
    }

    private struct Layout(int target, bool presentation)
    {
        private int remaining = target;
        internal ushort Address;
        internal TorizoExplosionInstructionMechanicsWord Selected;
        internal void Word(ushort value)
        {
            if (!presentation && remaining-- == 0) Selected = new(Address, value);
            Address += 2;
        }
        internal void Command(ushort instruction, ushort operand) { Word(instruction); Word(operand); }
        internal void Pose(ushort duration)
        {
            Word(duration);
            if (presentation && remaining-- == 0) Selected = new(Address, 0);
            Address += 2;
        }
    }
    internal static bool Owns(RoomEnemyProjectileKind kind) => kind is
        RoomEnemyProjectileKind.BombTorizoLowHealthExplosion or
        RoomEnemyProjectileKind.BombTorizoDeathExplosion;

    internal static ushort ReadMechanicsWord(ushort address)
    {
        int low = 0;
        int high = MechanicsWordCount - 1;
        while (low <= high)
        {
            int middle = low + ((high - low) >> 1);
            TorizoExplosionInstructionMechanicsWord candidate = MechanicsWord(middle);
            if (candidate.Address == address) return candidate.Value;
            if (candidate.Address < address) low = middle + 1;
            else high = middle - 1;
        }

        throw new InvalidDataException(
            $"Torizo explosion mechanics pointer $86:{address:X4} is not compiled.");
    }

    internal static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != EnemyProjectileCodePointers.BankBase) return false;
        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < MechanicsWordCount; index++)
        {
            var word = MechanicsWord(index);
            if (bankAddress == word.Address || bankAddress == unchecked((ushort)(word.Address + 1)))
                return true;
        }
        return false;
    }
}
