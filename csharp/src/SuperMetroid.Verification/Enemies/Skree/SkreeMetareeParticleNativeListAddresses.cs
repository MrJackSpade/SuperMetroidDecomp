using SuperMetroid.Verification.Bosses.MotherBrain;
using SuperMetroid.Core.Enemies.Skree;

namespace SuperMetroid.Verification.Enemies.Skree;

/// <summary>
/// The native bank-$86 lists the Skree/Metaree particle ROM oracle (#641) walks: each program's entry point. Only verification
/// reads them; the port runs typed programs.
/// </summary>
internal static class SkreeMetareeParticleNativeListAddresses
{
    /// <summary>Each typed program's native entry point, in authored order.</summary>
    internal static readonly NativeProgramEntry<SkreeMetareeParticleProgram>[] Entries =
    [
        new NativeProgramEntry<SkreeMetareeParticleProgram>(SkreeMetareeParticleProgram.Skree, 0x8abd),
        new NativeProgramEntry<SkreeMetareeParticleProgram>(SkreeMetareeParticleProgram.Metaree, 0x8ac5),
    ];

    /// <summary>The bank-$86 bytes the lists occupy, $86:8ABD-8ACC, which production must never read.</summary>
    internal static readonly MotherBrainEffectNativeListAddressesExtent Extent = new MotherBrainEffectNativeListAddressesExtent(0x8abd, 0x8acd);

    /// <summary>The native entry of <paramref name="program"/>.</summary>
    internal static ushort Entry(SkreeMetareeParticleProgram program) => NativeEnemyProjectileProgramOracle.EntryOf(Entries, program);
}
