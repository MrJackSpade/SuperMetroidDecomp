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
        (SpacePirateProjectileProgram.LaserLeftLoop, SpacePirateProjectileInstructionProgramDefinitions.LaserLeftLoop),
        (SpacePirateProjectileProgram.LaserRight, SpacePirateProjectileInstructionProgramDefinitions.LaserRight),
        (SpacePirateProjectileProgram.LaserRightLoop, SpacePirateProjectileInstructionProgramDefinitions.LaserRightLoop),
        (SpacePirateProjectileProgram.ClawLeft, SpacePirateProjectileInstructionProgramDefinitions.ClawLeft),
        (SpacePirateProjectileProgram.ClawLeftLoop, SpacePirateProjectileInstructionProgramDefinitions.ClawLeftLoop),
        (SpacePirateProjectileProgram.ClawRight, SpacePirateProjectileInstructionProgramDefinitions.ClawRight),
        (SpacePirateProjectileProgram.ClawRightLoop, SpacePirateProjectileInstructionProgramDefinitions.ClawRightLoop),
    ];
}
