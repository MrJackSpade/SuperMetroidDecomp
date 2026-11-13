using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core.Enemies.EyeDoor;
using SuperMetroid.Core.Rooms.Doors;

namespace SuperMetroid.TestSupport.MovieReplay;

/// <summary>
/// The native bank-$86 lists the eye-door sweat ROM oracle (#1296) walks: each program's entry point. Only verification reads
/// them; the port runs typed programs.
/// </summary>
internal static class EyeDoorSweatNativeListAddresses
{
    /// <summary>Each typed program's native entry point, in authored order.</summary>
    internal static readonly NativeProgramEntry<EyeDoorSweatProgram>[] Entries =
    [
        new NativeProgramEntry<EyeDoorSweatProgram>(EyeDoorSweatProgram.Falling, 0xb615),
        new NativeProgramEntry<EyeDoorSweatProgram>(EyeDoorSweatProgram.HitFloor, 0xb61d),
    ];

    /// <summary>The bank-$86 bytes the lists occupy, which production must never read.</summary>
    internal static readonly MotherBrainEffectNativeListAddressesExtent Extent = new MotherBrainEffectNativeListAddressesExtent(0xb615, 0xb62d);

    /// <summary>The native entry of <paramref name="program"/>.</summary>
    internal static ushort Entry(EyeDoorSweatProgram program) => NativeEnemyProjectileProgramOracle.EntryOf(Entries, program);

    /// <summary>The family's typed programs beside their native lists.</summary>
    internal static NativeEnemyProjectileFamily Family { get; } = NativeEnemyProjectileFamily.Of(Entries, EyeDoorSweatAnimations.Segments, EyeDoorSweatAnimations.StartingAt, (frame => EnemyProjectileProgramFrameSources.Operand(frame)), NativeEnemyProjectileCommonCues.Rules);
}
