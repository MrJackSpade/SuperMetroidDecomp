using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Enemies.SpacePirates;

namespace SuperMetroid.TestSupport.MovieReplay;

/// <summary>
/// The native bank-$86 lists the laser and claw ROM oracle (#641) walks: each program's entry point. Only verification
/// reads them; the port runs typed programs.
/// </summary>
internal static class SpacePirateProjectileNativeListAddresses
{
    /// <summary>Each typed laser program's native entry point, in authored order.</summary>
    internal static readonly NativeProgramEntry<SpacePirateLaserSegment>[] LaserEntries =
    [
        new NativeProgramEntry<SpacePirateLaserSegment>(SpacePirateLaserSegment.LaserLeft, SpacePirateProjectileInstructionProgramDefinitions.LaserLeft),
        new NativeProgramEntry<SpacePirateLaserSegment>(SpacePirateLaserSegment.LaserLeftFlicker, 0x9f71),
        new NativeProgramEntry<SpacePirateLaserSegment>(SpacePirateLaserSegment.LaserRight, SpacePirateProjectileInstructionProgramToolingDefinitions.LaserRight),
        new NativeProgramEntry<SpacePirateLaserSegment>(SpacePirateLaserSegment.LaserRightFlicker, 0x9fad),
    ];

    /// <summary>Each typed claw program's native entry point, in authored order.</summary>
    internal static readonly NativeProgramEntry<SpacePirateClawSegment>[] ClawEntries =
    [
        new NativeProgramEntry<SpacePirateClawSegment>(SpacePirateClawSegment.ClawLeft, SpacePirateProjectileInstructionProgramToolingDefinitions.ClawLeft),
        new NativeProgramEntry<SpacePirateClawSegment>(SpacePirateClawSegment.ClawLeftSpin, 0x9fbd),
        new NativeProgramEntry<SpacePirateClawSegment>(SpacePirateClawSegment.ClawRight, SpacePirateProjectileInstructionProgramToolingDefinitions.ClawRight),
        new NativeProgramEntry<SpacePirateClawSegment>(SpacePirateClawSegment.ClawRightSpin, 0x9fe5),
    ];

    /// <summary>The laser family's typed programs beside their native lists.</summary>
    internal static NativeEnemyProjectileFamily LaserFamily { get; } = NativeEnemyProjectileFamily.Of(LaserEntries, SpacePirateLaserAnimations.Segments,
        SpacePirateLaserAnimations.StartingAt, EnemyProjectileProgramFrameOperands.KeyOf, SpacePirateProjectileCueRules.LaserRules,
        onlyRunOnInto: new HashSet<SpacePirateLaserSegment> { SpacePirateLaserSegment.LaserLeftFlicker, SpacePirateLaserSegment.LaserRightFlicker });

    /// <summary>The claw family's typed programs beside their native lists.</summary>
    internal static NativeEnemyProjectileFamily ClawFamily { get; } = NativeEnemyProjectileFamily.Of(ClawEntries, SpacePirateClawAnimations.Segments,
        SpacePirateClawAnimations.StartingAt, EnemyProjectileProgramFrameOperands.KeyOf, SpacePirateProjectileCueRules.ClawRules,
        onlyRunOnInto: new HashSet<SpacePirateClawSegment> { SpacePirateClawSegment.ClawLeftSpin, SpacePirateClawSegment.ClawRightSpin });

    /// <summary>The native pointer each laser or claw animation stands at, null for an animation of neither family.</summary>
    internal static Func<EnemyProjectileAnimation, ushort?> NativePointers(IImportCartridgeSource cartridge)
    {
        Func<EnemyProjectileAnimation, ushort?> laser = LaserFamily.NativePointers(cartridge);
        Func<EnemyProjectileAnimation, ushort?> claw = ClawFamily.NativePointers(cartridge);
        return animation => laser(animation) ?? claw(animation);
    }
}
