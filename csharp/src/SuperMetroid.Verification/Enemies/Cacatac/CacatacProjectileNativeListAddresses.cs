using SuperMetroid.Verification.Bosses.MotherBrain;
using SuperMetroid.Core.Enemies.Cacatac;

namespace SuperMetroid.Verification.Enemies.Cacatac;

/// <summary>
/// The native bank-$86 lists the Cacatac spike ROM oracle (#641) walks: each program's entry point. Only verification
/// reads them; the port runs typed programs.
/// </summary>
internal static class CacatacProjectileNativeListAddresses
{
    /// <summary>Each typed program's native entry point, in authored order.</summary>
    internal static readonly NativeProgramEntry<CacatacProjectileProgram>[] Entries =
    [
        new NativeProgramEntry<CacatacProjectileProgram>(CacatacProjectileProgram.LeftFacingUp, 0xd92e),
        new NativeProgramEntry<CacatacProjectileProgram>(CacatacProjectileProgram.UpLeft, 0xd934),
        new NativeProgramEntry<CacatacProjectileProgram>(CacatacProjectileProgram.Up, 0xd93a),
        new NativeProgramEntry<CacatacProjectileProgram>(CacatacProjectileProgram.UpRight, 0xd940),
        new NativeProgramEntry<CacatacProjectileProgram>(CacatacProjectileProgram.RightFacingUp, 0xd946),
        new NativeProgramEntry<CacatacProjectileProgram>(CacatacProjectileProgram.LeftFacingDown, 0xd94c),
        new NativeProgramEntry<CacatacProjectileProgram>(CacatacProjectileProgram.DownLeft, 0xd952),
        new NativeProgramEntry<CacatacProjectileProgram>(CacatacProjectileProgram.Down, 0xd958),
        new NativeProgramEntry<CacatacProjectileProgram>(CacatacProjectileProgram.DownRight, 0xd95e),
        new NativeProgramEntry<CacatacProjectileProgram>(CacatacProjectileProgram.RightFacingDown, 0xd964),
    ];

    /// <summary>The bank-$86 bytes from the first list to the end of the last, $86:D92E-D969, which production must never read.</summary>
    internal static readonly MotherBrainEffectNativeListAddressesExtent Extent = new MotherBrainEffectNativeListAddressesExtent(0xd92e, 0xd96a);

    /// <summary>The native entry of <paramref name="program"/>.</summary>
    internal static ushort Entry(CacatacProjectileProgram program) => NativeEnemyProjectileProgramOracle.EntryOf(Entries, program);
}
