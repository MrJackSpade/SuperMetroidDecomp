using SuperMetroid.Core.Enemies.Shaktool;

namespace SuperMetroid.Verification.Enemies.Shaktool;

/// <summary>
/// The native bank-$86 lists the Shaktool attack circle ROM oracle (#641) walks: each program's entry point. Only verification
/// reads them; the port runs typed programs.
/// </summary>
internal static class ShaktoolProjectileNativeListAddresses
{
    /// <summary>Each typed program's native entry point, in authored order.</summary>
    internal static readonly (ShaktoolProjectileProgram Program, ushort Entry)[] Entries =
    [
        (ShaktoolProjectileProgram.Front, 0xbd68),
        (ShaktoolProjectileProgram.FrontLoop, 0xbd70),
        (ShaktoolProjectileProgram.Middle, 0xbd78),
        (ShaktoolProjectileProgram.MiddleLoop, 0xbd84),
        (ShaktoolProjectileProgram.Back, 0xbd8c),
        (ShaktoolProjectileProgram.BackLoop, 0xbd94),
    ];

    /// <summary>The bank-$86 bytes from the first list to the end of the last, $86:BD68-BD9B, which production must never read.</summary>
    internal static readonly (ushort First, ushort End) Extent = (0xbd68, 0xbd9c);

    /// <summary>The native entry of <paramref name="program"/>.</summary>
    internal static ushort Entry(ShaktoolProjectileProgram program) => NativeEnemyProjectileProgramOracle.EntryOf(Entries, program);
}
