using SuperMetroid.Core.Enemies.Nuclear;

namespace SuperMetroid.Verification.Enemies.Nuclear;

/// <summary>
/// The native bank-$A6 lists the Nuclear Waffle ROM oracle (#641) walks: each program's entry point.
/// Only verification reads them; the port runs typed programs.
/// </summary>
internal static class NuclearWaffleNativeListAddresses
{
    /// <summary>The lists' bank.</summary>
    internal const byte Bank = 0xa6;

    /// <summary>Each typed program's native entry point, in authored order.</summary>
    internal static readonly (NuclearWaffleProgram Program, ushort Entry)[] Entries =
    [
        (NuclearWaffleProgram.BodyLoop, 0x9490),
    ];

}
