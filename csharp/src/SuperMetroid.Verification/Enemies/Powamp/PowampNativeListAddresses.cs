using SuperMetroid.Verification.Bosses.MotherBrain;
using SuperMetroid.Core.Enemies.Powamp;

namespace SuperMetroid.Verification.Enemies.Powamp;

/// <summary>
/// The native bank-$A8 lists the Powamp ROM oracle (#641) walks: each program's entry point.
/// Only verification reads them; the port runs typed programs.
/// </summary>
internal static class PowampNativeListAddresses
{
    /// <summary>The lists' bank.</summary>
    internal const byte Bank = 0xa8;

    /// <summary>Each typed program's native entry point, in authored order.</summary>
    internal static readonly NativeProgramEntry<PowampProgram>[] Entries =
    [
        new NativeProgramEntry<PowampProgram>(PowampProgram.BodyFast, 0xc163),
        new NativeProgramEntry<PowampProgram>(PowampProgram.BodySlow, 0xc173),
        new NativeProgramEntry<PowampProgram>(PowampProgram.BalloonInflate0, 0xc183),
        new NativeProgramEntry<PowampProgram>(PowampProgram.BalloonInflate1, 0xc187),
        new NativeProgramEntry<PowampProgram>(PowampProgram.BalloonStartSinking, 0xc191),
        new NativeProgramEntry<PowampProgram>(PowampProgram.BalloonDeflated, 0xc199),
    ];

}
