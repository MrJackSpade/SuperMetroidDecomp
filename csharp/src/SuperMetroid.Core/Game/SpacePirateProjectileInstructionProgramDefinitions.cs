namespace SuperMetroid.Core.Game;

/// <summary>One compiled Space Pirate projectile mechanics word at its bank-$86 address.</summary>
internal readonly record struct SpacePirateProjectileInstructionMechanicsWord(
    ushort Address,
    ushort Value);

/// <summary>
/// Compiled control for the shared Pirate/Mother Brain laser and Ninja Pirate claw
/// programs. Interleaved spritemap selectors address separately installed artwork.
/// </summary>
internal static class SpacePirateProjectileInstructionProgramDefinitions
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

    /// <summary>Unresolved laser startup dwell at86:9F41/9F45/9F49 and right-facing equivalents; remains required under #1165.</summary>
    private const ushort LaserStartupFrames = 2;

    internal static int MechanicsWordCount => 58;
    internal static int PresentationWordCount => 42;

    /// <summary>
    /// Each laser has three two-tick startup poses, an immediate movement callback,
    /// ten one-tick poses and a two-pose loop. Each claw installs its movement callback
    /// then loops eight one-tick poses. Left/right variants share these widths.
    /// </summary>
    internal static SpacePirateProjectileInstructionMechanicsWord MechanicsWord(int index)
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

    internal static ushort PresentationWordAddress(int index)
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

    internal static bool Owns(RoomEnemyProjectileKind kind) => kind is
        RoomEnemyProjectileKind.PirateMotherBrainLaser or
        RoomEnemyProjectileKind.PirateClaw;

    internal static ushort ReadMechanicsWord(ushort address)
    {
        int low = 0;
        int high = MechanicsWordCount - 1;
        while (low <= high)
        {
            int middle = low + ((high - low) >> 1);
            SpacePirateProjectileInstructionMechanicsWord candidate = MechanicsWord(middle);
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

    internal static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != EnemyProjectileCodePointers.BankBase)
            return false;
        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < MechanicsWordCount; index++)
        {
            ushort wordAddress = MechanicsWord(index).Address;
            if (bankAddress == wordAddress ||
                bankAddress == unchecked((ushort)(wordAddress + 1)))
            {
                return true;
            }
        }
        return false;
    }
}
