using SuperMetroid.Verification.Bosses.MotherBrain;
using SuperMetroid.Core.Enemies.FuneNamihe;

namespace SuperMetroid.Verification.Enemies.FuneNamihe;

/// <summary>
/// The native bank-$86 lists the Fune/Namihe fireball ROM oracle (#641) walks: each program's entry point. Only verification
/// reads them; the port runs typed programs.
/// </summary>
internal static class FuneNamiheFireballNativeListAddresses
{
    /// <summary>Each typed program's native entry point, in authored order.</summary>
    internal static readonly NativeProgramEntry<FuneNamiheFireballProgram>[] Entries =
    [
        new NativeProgramEntry<FuneNamiheFireballProgram>(FuneNamiheFireballProgram.Left, 0xde96),
        new NativeProgramEntry<FuneNamiheFireballProgram>(FuneNamiheFireballProgram.Right, 0xdea6),
    ];

    /// <summary>The bank-$86 bytes the lists occupy, $86:DE96-DEB5, which production must never read.</summary>
    internal static readonly MotherBrainEffectNativeListAddressesExtent Extent = new MotherBrainEffectNativeListAddressesExtent(0xde96, 0xdeb6);
}
