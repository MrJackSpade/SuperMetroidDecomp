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

    // Reviewed under #1165: holds are authored explosion cadence, and the random spread radii and
    // repetition counts are authored scatter choices for the effect; program geometry calculates.
    /// <summary>Small and large explosion pose holds. Reviewed under #1165 as authored animation cadence: the interpreter loads each value into the instruction timer and no simulation quantity derives it.</summary>
    private static readonly ushort[] SmallExplosionHolds = [2, 2, 3, 3, 2];
    /// <summary>Authored hold duration for each successive large-death explosion pose.</summary>
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

    /// <summary>Selects the authored spread and presentation sequence for a Torizo explosion branch.</summary>
    private enum ExplosionPhase
    {
        /// <summary>Small random-spread explosion used by Bomb Torizo's low-health attack.</summary>
        LowHealth,
        /// <summary>Large random-spread explosion used during a Torizo death sequence.</summary>
        LargeDeath,
        /// <summary>Death-smoke branch with its own vertical spread and four repeated poses.</summary>
        DeathSmoke
    }

    /// <summary>Number of translated mechanics words in the three Torizo explosion programs.</summary>
    public static int MechanicsWordCount => 53;
    /// <summary>Number of extracted spritemap operand addresses across the compiled programs.</summary>
    public static int PresentationWordCount => 15;

    /// <summary>Gets the mechanics word at an index in the concatenated program layout.</summary>
    /// <param name="index">Zero-based position among the compiled mechanics words.</param>
    /// <returns>The bank-$86 address and value of the selected opcode, operand, or duration word.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is outside the mechanics word range.</exception>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new ArgumentOutOfRangeException(nameof(index));
        return BuildLayout(index, false).Selected;
    }
    /// <summary>Gets the bank-$86 address of one spritemap operand kept in extracted presentation data.</summary>
    /// <param name="index">Zero-based position among the fifteen presentation operands.</param>
    /// <returns>Address of the word populated by the installed explosion artwork.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is outside the presentation operand range.</exception>
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

    /// <summary>Appends the selected explosion's setup, randomized placement, sound, timed poses, loop, and terminal opcode.</summary>
    /// <param name="layout">Program cursor that records the requested mechanics word or presentation address.</param>
    /// <param name="phase">Low-health, large-death, or death-smoke program variant.</param>
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

    /// <summary>Cursor for walking the combined instruction layout and capturing one selected mechanics or presentation word.</summary>
    /// <param name="target">Zero-based combined-layout position to capture.</param>
    /// <param name="presentation">When true, selects spritemap operand addresses instead of mechanics values.</param>
    private struct Layout(int target, bool presentation)
    {
        /// <summary>Remaining selection index as the layout cursor advances through words and pose operands.</summary>
        private int remaining = target;
        /// <summary>Current bank-$86 word address while building the concatenated instruction programs.</summary>
        internal ushort Address;
        /// <summary>Captured mechanics word or presentation placeholder at the requested layout position.</summary>
        internal InstructionMechanicsWord Selected;

        /// <summary>Appends one word and captures it when this is the selected mechanics position.</summary>
        /// <param name="value">Instruction, operand, duration, or terminal word to append.</param>
        internal void Word(ushort value)
        {
            if (!presentation && remaining-- == 0) Selected = new(Address, value);
            Address += 2;
        }
        /// <summary>Appends an opcode followed by its word-sized operand.</summary>
        /// <param name="instruction">Bank-$86 instruction word.</param>
        /// <param name="operand">Word consumed by the instruction.</param>
        internal void Command(ushort instruction, ushort operand) { Word(instruction); Word(operand); }

        /// <summary>Appends a timed pose duration and, in presentation mode, selects the following spritemap operand address.</summary>
        /// <param name="duration">Number of updates the explosion pose remains active.</param>
        internal void Pose(ushort duration)
        {
            Word(duration);
            if (presentation && remaining-- == 0) Selected = new(Address, 0);
            Address += 2;
        }
    }
    /// <summary>Reports whether this program catalog owns the instruction mechanics for a Torizo explosion projectile.</summary>
    /// <param name="kind">Projectile kind being dispatched.</param>
    /// <returns>True for Bomb Torizo's low-health and shared Torizo death explosions.</returns>
    internal static bool Owns(RoomEnemyProjectileKind kind) => kind is
        RoomEnemyProjectileKind.BombTorizoLowHealthExplosion or
        RoomEnemyProjectileKind.BombTorizoDeathExplosion;

    /// <summary>Finds the compiled mechanics value at a bank-$86 address.</summary>
    /// <param name="address">Instruction address requested by the projectile interpreter.</param>
    /// <returns>The translated word at that address.</returns>
    /// <exception cref="InvalidDataException">The address is not a mechanics word in one of the compiled programs.</exception>
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
