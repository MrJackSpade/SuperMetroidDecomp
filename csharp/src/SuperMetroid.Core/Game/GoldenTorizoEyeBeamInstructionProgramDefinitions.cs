namespace SuperMetroid.Core.Game;

internal readonly record struct GoldenTorizoEyeBeamInstructionMechanicsWord(
    ushort Address,
    ushort Value);

/// <summary>
/// Compiled control for Golden Torizo's eye-beam programs at $86:B3CD-$B428. Their
/// seventeen interleaved spritemap operands are extracted presentation data; the packed
/// floor-impact sound ID remains live cartridge audio data.
/// </summary>
internal static class GoldenTorizoEyeBeamInstructionProgramDefinitions
{
    /// <summary><c>InitAI_EnemyProjectile_GoldenTorizoEyeBeam</c> at $86:B328.</summary>
    internal const ushort InitializationAi = 0xb328;
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
    internal static int MechanicsWordCount => 28;
    internal static int PresentationWordCount => 17;
    internal static GoldenTorizoEyeBeamInstructionMechanicsWord MechanicsWord(int index)
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
    /// $86:B3CD-B427: wall impact has five four-byte poses, floor impact waits for
    /// explosion admission then slows its six poses by one tick per phase, and
    /// flight loops five poses. Floor sound uses a one-byte operand between words.
    /// </summary>
    private static GoldenTorizoEyeBeamInstructionMechanicsWord Select(int index, bool visual)
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

    private ref struct Layout(int requested, bool visual)
    {
        private ushort cursor = WallImpact;
        private int mechanics, presentation;
        internal GoldenTorizoEyeBeamInstructionMechanicsWord Result { get; private set; }
        internal void SkipByte() => cursor++;
        internal void Word(ushort value)
        {
            if (!visual && mechanics == requested) Result = new(cursor, value);
            mechanics++; cursor += sizeof(ushort);
        }
        internal void Pose(ushort duration)
        {
            Word(duration);
            if (visual && presentation == requested) Result = new(cursor, 0);
            presentation++; cursor += sizeof(ushort);
        }
    }
    internal static ushort ReadMechanicsWord(ushort address)
    {
        int low = 0;
        int high = MechanicsWordCount - 1;
        while (low <= high)
        {
            int middle = low + ((high - low) >> 1);
            GoldenTorizoEyeBeamInstructionMechanicsWord candidate = MechanicsWord(middle);
            if (candidate.Address == address) return candidate.Value;
            if (candidate.Address < address) low = middle + 1;
            else high = middle - 1;
        }

        throw new InvalidDataException(
            $"Golden Torizo eye-beam mechanics pointer $86:{address:X4} is not compiled.");
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
