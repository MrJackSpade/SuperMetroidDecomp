namespace SuperMetroid.Core.Game;

/// <summary>
/// Compiled control for the shared Pirate/Mother Brain laser and Ninja Pirate claw
/// programs. Interleaved spritemap selectors address separately installed artwork.
/// </summary>
internal abstract class SpacePirateProjectileInstructionProgramDefinitions
{
    /// <summary><c>InstList_EnemyProjectile_Pirate_MotherBrain_Laser_Left_0</c> at $86:9F41.</summary>
    internal const ushort LaserLeft = 0x9f41;

    /// <summary><c>InstList_EnemyProjectile_Pirate_MotherBrain_Laser_Left_1</c> at $86:9F71.</summary>
    internal const ushort LaserLeftLoop = 0x9f71;

    /// <summary><c>InstList_EnemyProjectile_Pirate_MotherBrain_Laser_Right_0</c> at $86:9F7D.</summary>
    internal const ushort LaserRight = 0x9f7d;

    /// <summary><c>InstList_EnemyProjectile_Pirate_MotherBrain_Laser_Right_1</c> at $86:9FAD.</summary>
    internal const ushort LaserRightLoop = 0x9fad;

    /// <summary><c>InstList_EnemyProjectile_PirateClaw_Left_0</c> at $86:9FB9.</summary>
    internal const ushort ClawLeft = 0x9fb9;

    /// <summary><c>InstList_EnemyProjectile_PirateClaw_Left_1</c> at $86:9FBD.</summary>
    internal const ushort ClawLeftLoop = 0x9fbd;

    /// <summary><c>InstList_EnemyProjectile_PirateClaw_Right_0</c> at $86:9FE1.</summary>
    internal const ushort ClawRight = 0x9fe1;

    /// <summary><c>InstList_EnemyProjectile_PirateClaw_Right_1</c> at $86:9FE5.</summary>
    internal const ushort ClawRightLoop = 0x9fe5;

    /// <summary>Laser startup dwell at $86:9F41/9F45/9F49 and right-facing equivalents. Reviewed under #1165 as authored animation cadence: the interpreter loads each value into the instruction timer and no simulation quantity derives it.</summary>
    private const ushort LaserStartupFrames = 2;

    /// <summary>Number of mechanics words in the four compiled laser and claw instruction lists.</summary>
    public static int MechanicsWordCount => 58;

    /// <summary>Number of spritemap operands across the left- and right-facing laser and claw lists.</summary>
    public static int PresentationWordCount => 42;

    /// <summary>
    /// Each laser has three two-tick startup poses, an immediate movement callback,
    /// ten one-tick poses and a two-pose loop. Each claw installs its movement callback
    /// then loops eight one-tick poses. Left/right variants share these widths.
    /// </summary>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new ArgumentOutOfRangeException(nameof(index));
        if (index < 34)
        {
            bool right = index >= 17;
            int word = index % 17;
            ushort start = right ? LaserRight : LaserLeft;
            ushort loop = right ? LaserRightLoop : LaserLeftLoop;
            if (word < 3) return new((ushort)(start + word * 4), LaserStartupFrames);
            if (word < 5)
                return new((ushort)(start + 12 + (word - 3) * 2),
                    word == 3 ? EnemyProjectileCodePointers.Instruction_PreInstructionInY_ExecuteY
                        : right ? EnemyProjectileCodePointers.PreInst_EnemyProjectile_Pirate_MotherBrain_Laser_Right
                            : EnemyProjectileCodePointers.PreInstruction_EnemyProjectile_Pirate_MotherBrain_Laser_Left);
            if (word < 15) return new((ushort)(start + 16 + (word - 5) * 4), 1);
            return new((ushort)(loop + 8 + (word - 15) * 2),
                word == 15 ? EnemyProjectileCodePointers.Instruction_EnemyProjectile_GotoY : loop);
        }
        else
        {
            bool right = index >= 46;
            int word = (index - 34) % 12;
            ushort start = right ? ClawRight : ClawLeft;
            ushort loop = right ? ClawRightLoop : ClawLeftLoop;
            if (word < 2)
                return new((ushort)(start + word * 2),
                    word == 0 ? EnemyProjectileCodePointers.Instruction_EnemyProjectile_PreInstructionInY
                        : right ? EnemyProjectileCodePointers.PreInstruction_EnemyProjectile_PirateClaw_Right
                            : EnemyProjectileCodePointers.PreInstruction_EnemyProjectile_PirateClaw_Left);
            if (word < 10) return new((ushort)(loop + (word - 2) * 4), 1);
            return new((ushort)(loop + 32 + (word - 10) * 2),
                word == 10 ? EnemyProjectileCodePointers.Instruction_EnemyProjectile_GotoY : loop);
        }
    }

    /// <summary>Maps a presentation operand ordinal to its address in the appropriate laser or claw instruction list.</summary>
    /// <param name="index">Zero-based ordinal across both laser lists followed by both claw lists.</param>
    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount) throw new ArgumentOutOfRangeException(nameof(index));
        if (index < 26)
        {
            ushort start = index < 13 ? LaserLeft : LaserRight;
            int frame = index % 13;
            return (ushort)(start + frame * 4 + (frame < 3 ? 2 : 6));
        }
        ushort loop = index < 34 ? ClawLeftLoop : ClawRightLoop;
        return (ushort)(loop + (index - 26) % 8 * 4 + 2);
    }

    /// <summary>Determines whether these compiled programs cover the Mother Brain Pirate laser or Ninja Pirate claw.</summary>
    /// <param name="kind">Projectile kind to compare with the laser and claw program owners.</param>
    internal static bool Owns(RoomEnemyProjectileKind kind) => kind is
        RoomEnemyProjectileKind.PirateMotherBrainLaser or
        RoomEnemyProjectileKind.PirateClaw;

    /// <summary>Looks up the compiled mechanics value associated with a bank-relative instruction address.</summary>
    /// <param name="address">Address of a mechanics word in one of the compiled projectile instruction lists.</param>
    /// <returns>The mechanics value stored at the requested instruction address.</returns>
    /// <exception cref="InvalidDataException">The address is not represented by a compiled mechanics word.</exception>
    internal static ushort ReadMechanicsWord(ushort address)
    {
        int low = 0;
        int high = MechanicsWordCount - 1;
        while (low <= high)
        {
            int middle = low + ((high - low) >> 1);
            InstructionMechanicsWord candidate = MechanicsWord(middle);
            if (candidate.Address == address)
                return candidate.Value;
            if (candidate.Address < address)
                low = middle + 1;
            else
                high = middle - 1;
        }

        throw new InvalidDataException(
            $"Space Pirate projectile mechanics pointer $86:{address:X4} is not compiled.");
    }
}
