using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Bosses.Torizo;

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
    /// <summary>Nonvisual opcode and duration words across the wall, floor, and flight lists.</summary>
    public static int MechanicsWordCount => 28;
    /// <summary>Spritemap operands supplied by the editable eye-beam presentation.</summary>
    public static int PresentationWordCount => 17;
    /// <summary>Calculates an indexed native opcode or duration word while skipping visual operands.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new IndexOutOfRangeException();
        return Select(index, visual: false);
    }
    /// <summary>Locates an indexed spritemap operand interleaved with the compiled instructions.</summary>
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
        layout.Word((ushort)EnemyProjectileInstruction.ClearPreInstruction);
        for (int pose = 0; pose < 5; pose++) layout.Pose(WallHold);
        layout.Word((ushort)EnemyProjectileInstruction.Delete);
        layout.Word((ushort)EnemyProjectileInstruction.ClearPreInstruction);
        layout.Pose(LandingHold);
        layout.Word((ushort)EnemyProjectileInstruction.GotoYIfEyeBeamExplosionsDisabled);
        layout.Word(FloorImpactLoop);
        layout.Word((ushort)EnemyProjectileInstruction.QueueSoundInY_Lib3_Max6);
        layout.SkipByte();
        for (int phase = 0; phase < 6; phase++)
        {
            if (phase == 2)
            {
                layout.Word((ushort)EnemyProjectileInstruction.Properties_AndY);
                layout.Word(EnableSamusDamageMask);
            }
            layout.Pose((ushort)(ExplosionInitialHold + phase));
        }
        layout.Word((ushort)EnemyProjectileInstruction.Delete);
        for (int pose = 0; pose < 5; pose++) layout.Pose(FlightHold);
        layout.Word((ushort)EnemyProjectileInstruction.GotoY);
        layout.Word(Normal);
        return layout.Result;
    }

    /// <summary>Walks the three native lists while tracking separate mechanics and presentation indices.</summary>
    private ref struct Layout(int requested, bool visual)
    {
        /// <summary>Bank-$86 cursor, beginning with the wall-impact instruction list.</summary>
        private ushort cursor = WallImpact;
        /// <summary>Independent counters for compiled mechanics and extracted spritemap operands.</summary>
        private int mechanics, presentation;
        /// <summary>The requested word or operand address selected during layout traversal.</summary>
        internal InstructionMechanicsWord Result { get; private set; }
        /// <summary>Accounts for the one-byte packed floor sound between instruction words.</summary>
        internal void SkipByte() => cursor++;
        /// <summary>Advances an opcode or duration word, capturing it for a mechanics lookup.</summary>
        internal void Word(ushort value)
        {
            if (!visual && mechanics == requested) Result = new(cursor, value);
            mechanics++; cursor += sizeof(ushort);
        }
        /// <summary>Places a timed pose and captures its following spritemap address for a visual lookup.</summary>
        internal void Pose(ushort duration)
        {
            Word(duration);
            if (visual && presentation == requested) Result = new(cursor, 0);
            presentation++; cursor += sizeof(ushort);
        }
    }
    /// <summary>Finds a compiled native mechanics value by address, rejecting visual and unsupported words.</summary>
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
