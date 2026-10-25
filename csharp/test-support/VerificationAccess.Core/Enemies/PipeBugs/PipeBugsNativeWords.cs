using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Core.Enemies.PipeBugs;

/// <summary>
/// Verification's native view of the PipeBugs identities (#1412): the cartridge routine words naming each member. Core's
/// values are arbitrary; a native snapshot or ROM oracle translates through these tables.
/// </summary>
internal static class PipeBugsNativeWords
{
    /// <summary>The routine word naming each <see cref="PipeBugEnemyFunction"/>.</summary>
    internal static readonly NativeWordIdentities<PipeBugEnemyFunction> PipeBugEnemyFunctions = new(new Dictionary<PipeBugEnemyFunction, ushort>
    {
        [PipeBugEnemyFunction.BrinstarWaitUntilOnScreen] = 0x8880,
        [PipeBugEnemyFunction.BrinstarWaitForSamus] = 0x8890,
        [PipeBugEnemyFunction.BrinstarEmerge] = 0x88e3,
        [PipeBugEnemyFunction.BrinstarFlyHorizontally] = 0x891c,
        [PipeBugEnemyFunction.BrinstarRespawnDelay] = 0x897e,
        [PipeBugEnemyFunction.NorfairWaitForFormation] = 0x8bcd,
        [PipeBugEnemyFunction.NorfairWaitForSamus] = 0x8bff,
        [PipeBugEnemyFunction.NorfairRise] = 0x8ca6,
        [PipeBugEnemyFunction.NorfairLeaderStagger] = 0x8cff,
        [PipeBugEnemyFunction.NorfairUpperNearStagger] = 0x8d0c,
        [PipeBugEnemyFunction.NorfairUpperFarStagger] = 0x8d4e,
        [PipeBugEnemyFunction.NorfairLowerNearStagger] = 0x8d90,
        [PipeBugEnemyFunction.NorfairLowerFarStagger] = 0x8dd2,
        [PipeBugEnemyFunction.NorfairFlyLeft] = 0x8e14,
        [PipeBugEnemyFunction.NorfairFlyRight] = 0x8e35,
        [PipeBugEnemyFunction.NorfairWaitForStagger] = 0x8e5a,
        [PipeBugEnemyFunction.YellowWaitForSamus] = 0x8fb5,
        [PipeBugEnemyFunction.YellowEmergenceDelay] = 0x8ff5,
        [PipeBugEnemyFunction.YellowFlyLeft] = 0x9028,
        [PipeBugEnemyFunction.YellowFlyRight] = 0x90bd,
        [PipeBugEnemyFunction.YellowArcLeft] = 0x915a,
        [PipeBugEnemyFunction.YellowArcRight] = 0x91d8,
    });
}
