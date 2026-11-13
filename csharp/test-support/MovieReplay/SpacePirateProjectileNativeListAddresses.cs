using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core.Enemies.SpacePirates.Artwork;
using SuperMetroid.Core.Enemies.SpacePirates;

namespace SuperMetroid.TestSupport.MovieReplay;

/// <summary>
/// The native bank-$86 lists the laser and claw ROM oracle (#641) walks: each program's entry point. Only verification
/// reads them; the port runs typed programs.
/// </summary>
internal static class SpacePirateProjectileNativeListAddresses
{
    /// <summary>Each typed program's native entry point, in authored order.</summary>
    internal static readonly NativeProgramEntry<SpacePirateProjectileProgram>[] Entries =
    [
        new NativeProgramEntry<SpacePirateProjectileProgram>(SpacePirateProjectileProgram.LaserLeft, SpacePirateProjectileInstructionProgramDefinitions.LaserLeft),
        new NativeProgramEntry<SpacePirateProjectileProgram>(SpacePirateProjectileProgram.LaserLeftLoop, SpacePirateProjectileInstructionProgramToolingDefinitions.LaserLeftLoop),
        new NativeProgramEntry<SpacePirateProjectileProgram>(SpacePirateProjectileProgram.LaserRight, SpacePirateProjectileInstructionProgramToolingDefinitions.LaserRight),
        new NativeProgramEntry<SpacePirateProjectileProgram>(SpacePirateProjectileProgram.LaserRightLoop, SpacePirateProjectileInstructionProgramToolingDefinitions.LaserRightLoop),
        new NativeProgramEntry<SpacePirateProjectileProgram>(SpacePirateProjectileProgram.ClawLeft, SpacePirateProjectileInstructionProgramToolingDefinitions.ClawLeft),
        new NativeProgramEntry<SpacePirateProjectileProgram>(SpacePirateProjectileProgram.ClawLeftLoop, SpacePirateProjectileInstructionProgramToolingDefinitions.ClawLeftLoop),
        new NativeProgramEntry<SpacePirateProjectileProgram>(SpacePirateProjectileProgram.ClawRight, SpacePirateProjectileInstructionProgramToolingDefinitions.ClawRight),
        new NativeProgramEntry<SpacePirateProjectileProgram>(SpacePirateProjectileProgram.ClawRightLoop, SpacePirateProjectileInstructionProgramToolingDefinitions.ClawRightLoop),
    ];

    /// <summary>The family's typed programs beside their native lists.</summary>
    internal static NativeEnemyProjectileFamily Family { get; } = NativeEnemyProjectileFamily.Of(Entries, SpacePirateProjectileAnimations.Segments, SpacePirateProjectileAnimations.StartingAt,
        frame => EnemyProjectileProgramFrameSources.Operand(frame), SpacePirateProjectileCueRules.Rules);
}
