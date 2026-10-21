using SuperMetroid.Verification.Bosses.MotherBrain;
using SuperMetroid.Core.Enemies.FallingSparks;

namespace SuperMetroid.Verification.Enemies.FallingSparks;

/// <summary>
/// The native bank-$86 lists the falling spark ROM oracle (#641) walks: each program's entry point. Only verification
/// reads them; the port runs typed programs.
/// </summary>
internal static class FallingSparkNativeListAddresses
{
    /// <summary>Each typed program's native entry point, in authored order.</summary>
    internal static readonly NativeProgramEntry<FallingSparkProgram>[] Entries =
    [
        new NativeProgramEntry<FallingSparkProgram>(FallingSparkProgram.Falling, 0xf353),
        new NativeProgramEntry<FallingSparkProgram>(FallingSparkProgram.HitFloor, 0xf363),
    ];

    /// <summary>The bank-$86 bytes the lists occupy, $86:F353-F390, which production must never read.</summary>
    internal static readonly MotherBrainEffectNativeListAddressesExtent Extent = new MotherBrainEffectNativeListAddressesExtent(0xf353, 0xf391);
}
