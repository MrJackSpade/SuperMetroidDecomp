namespace SuperMetroid.Core.Game;

/// <summary>
/// Compiled control for Golden Torizo's eye-beam programs at $86:B3CD-$B428. Their
/// seventeen interleaved spritemap operands are extracted presentation data; the packed
/// floor-impact sound ID remains live cartridge audio data.
/// </summary>
internal abstract class GoldenTorizoEyeBeamInstructionProgramDefinitions
{
    /// <summary><c>InstList_EnemyProjectile_GoldenTorizoEyeBeam_HitWall</c> at $86:B3CD.</summary>
    internal const ushort WallImpact = 0xb3cd;
    /// <summary><c>InstList_EnemyProjectile_GoldenTorizoEyeBeam_HitFloor_0</c> at $86:B3E5.</summary>
    internal const ushort FloorImpact = 0xb3e5;
    /// <summary><c>InstList_EnemyProjectile_GoldenTorizoEyeBeam_HitFloor_1</c> at $86:B3E7.</summary>
    internal const ushort FloorImpactLoop = 0xb3e7;
    /// <summary><c>InstList_EnemyProjectile_GoldenTorizoEyeBeam_Normal</c> at $86:B410.</summary>
    internal const ushort Normal = 0xb410;

    /// <summary>Wall, landing, flight and initial explosion holds. Reviewed under #1165 as authored animation cadence: the interpreter loads each value into the instruction timer and no simulation quantity derives it.</summary>
    private const ushort WallHold = 4, LandingHold = 8, FlightHold = 1, ExplosionInitialHold = 4;
    /// <summary>$86:B3FC clears projectile property bit13, enabling Samus damage.</summary>
    private const ushort EnableSamusDamageMask = unchecked((ushort)~(1 << 13));

    /// <summary>Number of non-presentation operands addressable in the compiled eye-beam instruction streams.</summary>
    public static int MechanicsWordCount => 28;

    /// <summary>Number of extracted pose operands whose addresses identify eye-beam presentation frames.</summary>
    public static int PresentationWordCount => 17;

    /// <summary>Returns the address and value of one mechanics operand by its ordinal in the compiled stream.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new IndexOutOfRangeException();
        return Select(index, visual: false);
    }
    /// <summary>Returns the bank offset of one extracted pose operand by its presentation ordinal.</summary>
    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount) throw new IndexOutOfRangeException();
        return Select(index, visual: true).Address;
    }

    /// <summary>
    /// $86:B3CD-B427: wall impact has five four-byte poses, floor impact waits for
    /// explosion admission then slows its six poses by one tick per phase, and
    /// flight loops five poses. Floor sound uses a one-byte operand between words.
    /// </summary>
    private static InstructionMechanicsWord Select(int index, bool visual)
    {
        var layout = new Layout(index, visual);
        layout.Word(EnemyProjectileCodePointers.Instruction_EnemyProjectile_ClearPreInstruction);
        for (int pose = 0; pose < 5; pose++) layout.Pose(WallHold);
        layout.Word(EnemyProjectileCodePointers.Instruction_EnemyProjectile_Delete);
        layout.Word(EnemyProjectileCodePointers.Instruction_EnemyProjectile_ClearPreInstruction);
        layout.Pose(LandingHold);
        layout.Word(EnemyProjectileCodePointers.Instruction_EnemyProjectile_GotoYIfEyeBeamExplosionsDisabled);
        layout.Word(FloorImpactLoop);
        layout.Word(EnemyProjectileCodePointers.Instruction_EnemyProjectile_QueueSoundInY_Lib3_Max6);
        layout.SkipByte();
        for (int phase = 0; phase < 6; phase++)
        {
            if (phase == 2)
            {
                layout.Word(EnemyProjectileCodePointers.Instruction_EnemyProjectile_Properties_AndY);
                layout.Word(EnableSamusDamageMask);
            }
            layout.Pose((ushort)(ExplosionInitialHold + phase));
        }
        layout.Word(EnemyProjectileCodePointers.Instruction_EnemyProjectile_Delete);
        for (int pose = 0; pose < 5; pose++) layout.Pose(FlightHold);
        layout.Word(EnemyProjectileCodePointers.Instruction_EnemyProjectile_GotoY);
        layout.Word(Normal);
        return layout.Result;
    }

    /// <summary>Walks the interleaved instruction bytes and records the requested mechanics word or pose address.</summary>
    /// <param name="requested">Ordinal of the mechanics or visual operand to capture during the walk.</param>
    /// <param name="visual">Selects pose-address capture instead of mechanics-word capture.</param>
    private ref struct Layout(int requested, bool visual)
    {
        /// <summary>Current bank-$86 byte offset while traversing the two impact programs and flight loop.</summary>
        private ushort cursor = WallImpact;

        /// <summary>Counts visited mechanics operands in <c>mechanics</c> and visited pose-duration operands in <c>presentation</c>.</summary>
        private int mechanics, presentation;

        /// <summary>Captured address/value pair, or the default value when the requested ordinal is not encountered.</summary>
        internal InstructionMechanicsWord Result { get; private set; }

        /// <summary>Advances past the inline one-byte operand between instruction words.</summary>
        internal void SkipByte() => cursor++;

        /// <summary>Counts a two-byte operand and captures its address and value when it matches the requested mechanics ordinal.</summary>
        internal void Word(ushort value)
        {
            if (!visual && mechanics == requested) Result = new(cursor, value);
            mechanics++; cursor += sizeof(ushort);
        }
        /// <summary>Counts a pose-duration word and captures its address when visual selection is requested.</summary>
        internal void Pose(ushort duration)
        {
            Word(duration);
            if (visual && presentation == requested) Result = new(cursor, 0);
            presentation++; cursor += sizeof(ushort);
        }
    }
    /// <summary>Looks up a compiled mechanics value by bank offset and rejects addresses not present in the program.</summary>
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
            $"Golden Torizo eye-beam mechanics pointer $86:{address:X4} is not compiled.");
    }
}
