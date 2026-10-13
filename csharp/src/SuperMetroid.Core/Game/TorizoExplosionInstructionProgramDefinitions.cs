namespace SuperMetroid.Core.Game;

/// <summary>
/// Compiled control for Bomb Torizo's low-health and death-explosion programs at
/// $86:A3CB-$A455. Their fifteen interleaved spritemap operands use extracted
/// presentation art.
/// </summary>
internal abstract class TorizoExplosionInstructionProgramDefinitions
{
    /// <summary><c>InstList_EnemyProjectile_BombTorizoLowHealthExplosion_0</c> at $86:A3CB.</summary>
    internal const ushort LowHealthInitial = 0xa3cb;
    /// <summary><c>InstList_EnemyProjectile_TorizoDeathExplosion_0</c> at $86:A3FA.</summary>
    internal const ushort DeathInitial = 0xa3fa;
    /// <summary><c>InstList_EnemyProjectile_TorizoDeathExplosion_2</c> at $86:A431.</summary>
    internal const ushort DeathSmokeSetup = 0xa431;

    /// <summary>Small and large explosion pose holds. Reviewed under #1165 as authored animation cadence: the interpreter loads each value into the instruction timer and no simulation quantity derives it.</summary>
    private static readonly ushort[] SmallExplosionHolds = [2, 2, 3, 3, 2];
    /// <summary>Authored large-death pose holds; random spread and repetition remain separate effect choices (#1165).</summary>
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

    /// <summary>The three authored explosion lists sharing one calculated instruction layout.</summary>
    private enum ExplosionPhase
    {
        /// <summary>Short, tight scatter used during Bomb Torizo's low-health reaction.</summary>
        LowHealth,
        /// <summary>Large random scatter during the death explosion.</summary>
        LargeDeath,
        /// <summary>Alternate death branch that uses smoke frames and vertical bias.</summary>
        DeathSmoke,
    }

    /// <summary>Compiled control words across low-health, large-death, and smoke lists.</summary>
    public static int MechanicsWordCount => 53;
    /// <summary>Sprite-frame operands kept separate from the compiled explosion control.</summary>
    public static int PresentationWordCount => 15;
    /// <summary>Finds a native control address/value pair by its ordinal across the three lists.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new ArgumentOutOfRangeException(nameof(index));
        return BuildLayout(index, false).Selected;
    }
    /// <summary>Locates an explosion frame operand by ordinal without importing its editable spritemap value.</summary>
    public static ushort PresentationWordAddress(int index)
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

    /// <summary>Emits one phase's reset, random spread, sound, timed poses, loop, and deletion in native order.</summary>
    private static void BuildExplosion(ref Layout layout, ExplosionPhase phase)
    {
        bool small = phase == ExplosionPhase.LowHealth;
        bool smoke = phase == ExplosionPhase.DeathSmoke;
        layout.Address = small ? LowHealthInitial : smoke ? DeathSmokeSetup : DeathInitial;
        if (!smoke)
        {
            layout.Word((ushort)EnemyProjectileInstruction.ClearPreInstruction);
            layout.Command((ushort)EnemyProjectileInstruction.Properties_OrY, ExplosionPropertyMask);
            if (!small) layout.Command((ushort)EnemyProjectileInstruction.GotoY_Probability_1_4, DeathSmokeSetup);
        }
        layout.Command((ushort)EnemyProjectileInstruction.TimerInY, small ? (ushort)3 : (ushort)2);
        ushort loop = layout.Address;
        layout.Word((ushort)EnemyProjectileInstruction.Torizo_ResetPosition);
        layout.Word((ushort)EnemyProjectileInstruction.MoveRandomlyWithinXRadius_YRadius);
        layout.Word(small ? SmallSpreadMask : DeathHorizontalSpreadMask);
        layout.Word(small ? SmallSpreadMask : smoke ? SmokeVerticalSpread : LargeVerticalSpread);
        layout.Word((ushort)EnemyProjectileInstruction.QueueSoundInY_Lib2_Max6);
        layout.Address++; // The sound argument is a byte, outside this word catalog.
        int poses = small ? SmallExplosionHolds.Length : smoke ? 4 : LargeExplosionHolds.Length;
        for (int pose = 0; pose < poses; pose++)
            layout.Pose(small ? SmallExplosionHolds[pose] : smoke ? (ushort)8 : LargeExplosionHolds[pose]);
        layout.Command((ushort)EnemyProjectileInstruction.DecrementTimer_GotoYIfNonZero, loop);
        layout.Word((ushort)EnemyProjectileInstruction.Delete);
    }

    /// <summary>Counts native words while selecting either a mechanics entry or a visual operand address.</summary>
    private struct Layout(int target, bool presentation)
    {
        /// <summary>Ordinal distance remaining before the requested kind of word is reached.</summary>
        private int remaining = target;
        /// <summary>Current bank-$86 address, including byte-sized sound operands and skipped visual words.</summary>
        internal ushort Address;
        /// <summary>Selected address/value pair; presentation entries carry a placeholder value.</summary>
        internal InstructionMechanicsWord Selected;
        /// <summary>Emits one control word and captures it when its ordinal is requested.</summary>
        internal void Word(ushort value)
        {
            if (!presentation && remaining-- == 0) Selected = new(Address, value);
            Address += 2;
        }
        /// <summary>Emits a two-word instruction and its native operand.</summary>
        internal void Command(ushort instruction, ushort operand) { Word(instruction); Word(operand); }
        /// <summary>Emits a duration followed by one separately owned spritemap operand.</summary>
        internal void Pose(ushort duration)
        {
            Word(duration);
            if (presentation && remaining-- == 0) Selected = new(Address, 0);
            Address += 2;
        }
    }
    /// <summary>Admits only Bomb Torizo's low-health and death explosion projectile kinds.</summary>
    internal static bool Owns(RoomEnemyProjectileKind kind) => kind is
        RoomEnemyProjectileKind.BombTorizoLowHealthExplosion or
        RoomEnemyProjectileKind.BombTorizoDeathExplosion;

    /// <summary>Looks up a compiled control word by bank-$86 address and rejects visual or unknown addresses.</summary>
    internal static ushort ReadMechanicsWord(ushort address)
    {
        int low = 0;
        int high = MechanicsWordCount - 1;
        while (low <= high)
        {
            int middle = low + ((high - low) >> 1);
            InstructionMechanicsWord candidate = MechanicsWord(middle);
            if (candidate.Address == address) return candidate.Value;
            if (candidate.Address < address) low = middle + 1;
            else high = middle - 1;
        }

        throw new InvalidDataException(
            $"Torizo explosion mechanics pointer $86:{address:X4} is not compiled.");
    }
}
