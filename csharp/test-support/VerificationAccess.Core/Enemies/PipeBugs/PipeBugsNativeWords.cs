using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Core.Enemies.PipeBugs;

/// <summary>
/// Verification's native view of the PipeBugs identities (#1412): the cartridge routine words naming each member. Core's
/// values are arbitrary; a native snapshot or ROM oracle translates through these tables.
/// </summary>
internal static class PipeBugsNativeWords
{
    /// <summary>The routine word naming each <see cref="PipeBugAction"/>.</summary>
    internal static readonly NativeWordIdentities<PipeBugAction> PipeBugActions = new(new Dictionary<PipeBugAction, ushort>
    {
        [PipeBugAction.BrinstarWaitUntilOnScreen] = 0x8880,
        [PipeBugAction.BrinstarWaitForSamus] = 0x8890,
        [PipeBugAction.BrinstarEmerge] = 0x88e3,
        [PipeBugAction.BrinstarFlyHorizontally] = 0x891c,
        [PipeBugAction.BrinstarRespawnDelay] = 0x897e,
        [PipeBugAction.NorfairWaitForFormation] = 0x8bcd,
        [PipeBugAction.NorfairWaitForSamus] = 0x8bff,
        [PipeBugAction.NorfairRise] = 0x8ca6,
        [PipeBugAction.NorfairLeaderStagger] = 0x8cff,
        [PipeBugAction.NorfairUpperNearStagger] = 0x8d0c,
        [PipeBugAction.NorfairUpperFarStagger] = 0x8d4e,
        [PipeBugAction.NorfairLowerNearStagger] = 0x8d90,
        [PipeBugAction.NorfairLowerFarStagger] = 0x8dd2,
        [PipeBugAction.NorfairFlyLeft] = 0x8e14,
        [PipeBugAction.NorfairFlyRight] = 0x8e35,
        [PipeBugAction.NorfairWaitForStagger] = 0x8e5a,
        [PipeBugAction.YellowWaitForSamus] = 0x8fb5,
        [PipeBugAction.YellowEmergenceDelay] = 0x8ff5,
        [PipeBugAction.YellowFlyLeft] = 0x9028,
        [PipeBugAction.YellowFlyRight] = 0x90bd,
        [PipeBugAction.YellowArcLeft] = 0x915a,
        [PipeBugAction.YellowArcRight] = 0x91d8,
    });

    /// <summary>
    /// The Brinstar animation-table index naming each <see cref="PipeBugAnimationSelector"/>: bit zero is the shooting
    /// animation and bit one the right facing.
    /// </summary>
    internal static readonly NativeWordIdentities<PipeBugAnimationSelector> BrinstarAnimations = new(new Dictionary<PipeBugAnimationSelector, ushort>
    {
        [PipeBugAnimationSelector.RisingFacingLeft] = 0,
        [PipeBugAnimationSelector.ShootingFacingLeft] = 1,
        [PipeBugAnimationSelector.RisingFacingRight] = 2,
        [PipeBugAnimationSelector.ShootingFacingRight] = 3,
    });
}
