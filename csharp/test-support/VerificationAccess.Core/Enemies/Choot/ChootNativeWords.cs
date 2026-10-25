using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Core.Enemies.Choot;

/// <summary>
/// Verification's native view of the Choot identities (#1412): the cartridge routine words naming each member. Core's
/// values are arbitrary; a native snapshot or ROM oracle translates through these tables.
/// </summary>
internal static class ChootNativeWords
{
    /// <summary>The routine word naming each <see cref="ChootEnemyFunction"/>.</summary>
    internal static readonly NativeWordIdentities<ChootEnemyFunction> ChootEnemyFunctions = new(new Dictionary<ChootEnemyFunction, ushort>
    {
        [ChootEnemyFunction.WaitingForSamus] = 0xe035,
        [ChootEnemyFunction.PreparingJump] = 0xe04f,
        [ChootEnemyFunction.Jumping] = 0xe06a,
        [ChootEnemyFunction.Falling] = 0xe0cd,
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
