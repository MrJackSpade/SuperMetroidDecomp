using SuperMetroid.Core.Enemies.SpacePirates;

namespace SuperMetroid.Verification.Enemies.SpacePirates;

/// <summary>
/// The native bank-$86 lists the laser and claw ROM oracle (#641) walks: each program's entry point. Only verification
/// reads them; the port runs typed programs.
/// </summary>
internal static class SpacePirateProjectileNativeListAddresses
{
    /// <summary>Each typed program's native entry point, in authored order.</summary>
    internal static readonly (SpacePirateProjectileProgram Program, ushort Entry)[] Entries =
    [
        (SpacePirateProjectileProgram.LaserLeft, SpacePirateProjectileInstructionProgramDefinitions.LaserLeft),
        (SpacePirateProjectileProgram.LaserLeftLoop, SpacePirateProjectileInstructionProgramToolingDefinitions.LaserLeftLoop),
        (SpacePirateProjectileProgram.LaserRight, SpacePirateProjectileInstructionProgramToolingDefinitions.LaserRight),
        (SpacePirateProjectileProgram.LaserRightLoop, SpacePirateProjectileInstructionProgramToolingDefinitions.LaserRightLoop),
        (SpacePirateProjectileProgram.ClawLeft, SpacePirateProjectileInstructionProgramToolingDefinitions.ClawLeft),
        (SpacePirateProjectileProgram.ClawLeftLoop, SpacePirateProjectileInstructionProgramToolingDefinitions.ClawLeftLoop),
        (SpacePirateProjectileProgram.ClawRight, SpacePirateProjectileInstructionProgramToolingDefinitions.ClawRight),
        (SpacePirateProjectileProgram.ClawRightLoop, SpacePirateProjectileInstructionProgramToolingDefinitions.ClawRightLoop),
    ];
}
