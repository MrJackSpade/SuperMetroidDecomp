using SuperMetroid.Core.Hardware;
using SuperMetroid.Tooling;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Enemies.SpacePirates.Artwork;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>Development-tool members of <see cref="SpacePirateProjectileInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(SpacePirateProjectileInstructionProgramDefinitions))]
internal abstract class SpacePirateProjectileInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Laser startup dwell at $86:9F41/9F45/9F49 and the right-facing equivalents: each muzzle pose shows two updates.</summary>
    private const ushort LaserStartupFrames = 2;

    /// <summary>Counts laser and claw timers, callbacks, and loop controls for both facings.</summary>
    public static int MechanicsWordCount => 58;

    /// <summary>Counts visual frame operands across the two lasers and two claw programs.</summary>
    public static int PresentationWordCount => SpacePirateProjectileFrameDefinitions.PoseCount;

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
            ushort start = right ? SpacePirateProjectileInstructionProgramDefinitions.LaserRight : SpacePirateProjectileInstructionProgramDefinitions.LaserLeft;
            ushort loop = right ? SpacePirateProjectileInstructionProgramDefinitions.LaserRightLoop : SpacePirateProjectileInstructionProgramDefinitions.LaserLeftLoop;
            if (word < 3) return new((ushort)(start + word * 4), LaserStartupFrames);
            if (word < 5)
                return new((ushort)(start + 12 + (word - 3) * 2),
                    word == 3 ? (ushort)EnemyProjectileInstruction.PreInstructionInY_ExecuteY
                        : (ushort)(right
                            ? EnemyProjectilePreInstruction.Pirate_MotherBrain_Laser_Right
                            : EnemyProjectilePreInstruction.Pirate_MotherBrain_Laser_Left));
            if (word < 15) return new((ushort)(start + 16 + (word - 5) * 4), 1);
            return new((ushort)(loop + 8 + (word - 15) * 2),
                word == 15 ? (ushort)EnemyProjectileInstruction.GotoY : loop);
        }
        else
        {
            bool right = index >= 46;
            int word = (index - 34) % 12;
            ushort start = right ? SpacePirateProjectileInstructionProgramDefinitions.ClawRight : SpacePirateProjectileInstructionProgramDefinitions.ClawLeft;
            ushort loop = right ? SpacePirateProjectileInstructionProgramDefinitions.ClawRightLoop : SpacePirateProjectileInstructionProgramDefinitions.ClawLeftLoop;
            if (word < 2)
                return new((ushort)(start + word * 2),
                    word == 0 ? (ushort)EnemyProjectileInstruction.PreInstructionInY
                        : (ushort)(right
                            ? EnemyProjectilePreInstruction.PirateClaw_Right
                            : EnemyProjectilePreInstruction.PirateClaw_Left));
            if (word < 10) return new((ushort)(loop + (word - 2) * 4), 1);
            return new((ushort)(loop + 32 + (word - 10) * 2),
                word == 10 ? (ushort)EnemyProjectileInstruction.GotoY : loop);
        }
    }

    /// <summary>Locates a laser or claw projectile drawing operand: the installed artwork key of its pose.</summary>
    public static ushort PresentationWordAddress(int index) => (uint)index < (uint)PresentationWordCount
        ? SpacePirateProjectileFrameDefinitions.PresentationKeys[new(index)]
        : throw new ArgumentOutOfRangeException(nameof(index));

    /// <summary>Matches either byte of a compiled pirate projectile word in bank $86.</summary>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((SnesAddressMath.BankBase(address)) != EnemyProjectileCodePointers.BankBase)
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
