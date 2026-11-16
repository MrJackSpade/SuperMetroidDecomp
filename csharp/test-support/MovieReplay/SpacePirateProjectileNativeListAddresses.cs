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
    internal static readonly NativeProgramEntry<SpacePirateProjectileSegment>[] Entries =
    [
        new NativeProgramEntry<SpacePirateProjectileSegment>(SpacePirateProjectileSegment.LaserLeft, SpacePirateProjectileInstructionProgramDefinitions.LaserLeft),
        new NativeProgramEntry<SpacePirateProjectileSegment>(SpacePirateProjectileSegment.LaserLeftLoop, SpacePirateProjectileInstructionProgramToolingDefinitions.LaserLeftLoop),
        new NativeProgramEntry<SpacePirateProjectileSegment>(SpacePirateProjectileSegment.LaserRight, SpacePirateProjectileInstructionProgramToolingDefinitions.LaserRight),
        new NativeProgramEntry<SpacePirateProjectileSegment>(SpacePirateProjectileSegment.LaserRightLoop, SpacePirateProjectileInstructionProgramToolingDefinitions.LaserRightLoop),
        new NativeProgramEntry<SpacePirateProjectileSegment>(SpacePirateProjectileSegment.ClawLeft, SpacePirateProjectileInstructionProgramToolingDefinitions.ClawLeft),
        new NativeProgramEntry<SpacePirateProjectileSegment>(SpacePirateProjectileSegment.ClawLeftLoop, SpacePirateProjectileInstructionProgramToolingDefinitions.ClawLeftLoop),
        new NativeProgramEntry<SpacePirateProjectileSegment>(SpacePirateProjectileSegment.ClawRight, SpacePirateProjectileInstructionProgramToolingDefinitions.ClawRight),
        new NativeProgramEntry<SpacePirateProjectileSegment>(SpacePirateProjectileSegment.ClawRightLoop, SpacePirateProjectileInstructionProgramToolingDefinitions.ClawRightLoop),
    ];

    /// <summary>The family's typed programs beside their native lists.</summary>
    internal static NativeEnemyProjectileFamily Family { get; } = NativeEnemyProjectileFamily.Of(Entries, SpacePirateProjectileAnimations.Segments, SpacePirateProjectileAnimations.StartingAt,
        frame => EnemyProjectileProgramFrameSources.Operand(frame), SpacePirateProjectileCueRules.Rules);
}
