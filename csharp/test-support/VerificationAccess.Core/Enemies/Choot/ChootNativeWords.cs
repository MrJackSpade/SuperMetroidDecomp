using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Core.Enemies.Choot;

/// <summary>
/// Verification's native view of the Choot identities (#1412): the cartridge routine words naming each member. Core's
/// values are arbitrary; a native snapshot or ROM oracle translates through these tables.
/// </summary>
internal static class ChootNativeWords
{
    /// <summary>The routine word naming each <see cref="ChootAction"/>.</summary>
    internal static readonly NativeWordIdentities<ChootAction> ChootActions = new(new Dictionary<ChootAction, ushort>
    {
        [ChootAction.WaitingForSamus] = 0xe035,
        [ChootAction.PreparingJump] = 0xe04f,
        [ChootAction.Jumping] = 0xe06a,
        [ChootAction.Falling] = 0xe0cd,
    });

    /// <summary>The routine word naming each <see cref="ChootFallingPath"/>.</summary>
    internal static readonly NativeWordIdentities<ChootFallingPath> ChootFallingPaths = new(new Dictionary<ChootFallingPath, ushort>
    {
        [ChootFallingPath.Normal] = 0xd84c,
        [ChootFallingPath.Wide] = 0xd976,
        [ChootFallingPath.VeryWide] = 0xdaa0,
        [ChootFallingPath.Slow] = 0xdbca,
        [ChootFallingPath.VerySlow] = 0xdd44,
    });
}
