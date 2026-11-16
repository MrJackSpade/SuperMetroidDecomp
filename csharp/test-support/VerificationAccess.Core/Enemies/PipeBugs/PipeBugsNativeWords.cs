using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Core.Enemies.PipeBugs;

/// <summary>
/// Verification's native view of the PipeBugs identities (#1412): the cartridge routine words naming each member. Core's
/// values are arbitrary; a native snapshot or ROM oracle translates through these tables.
/// </summary>
internal static class PipeBugsNativeWords
{
    /// <summary>The routine word naming each <see cref="BrinstarPipeBugAction"/>.</summary>
    internal static readonly NativeWordIdentities<BrinstarPipeBugAction> BrinstarPipeBugActions = new(new Dictionary<BrinstarPipeBugAction, ushort>
    {
        [BrinstarPipeBugAction.WaitUntilOnScreen] = 0x8880,
        [BrinstarPipeBugAction.WaitForSamus] = 0x8890,
        [BrinstarPipeBugAction.Emerge] = 0x88e3,
        [BrinstarPipeBugAction.FlyHorizontally] = 0x891c,
        [BrinstarPipeBugAction.RespawnDelay] = 0x897e,
    });

    /// <summary>The routine word naming each <see cref="NorfairPipeBugAction"/>.</summary>
    internal static readonly NativeWordIdentities<NorfairPipeBugAction> NorfairPipeBugActions = new(new Dictionary<NorfairPipeBugAction, ushort>
    {
        [NorfairPipeBugAction.WaitForFormation] = 0x8bcd,
        [NorfairPipeBugAction.WaitForSamus] = 0x8bff,
        [NorfairPipeBugAction.Rise] = 0x8ca6,
        [NorfairPipeBugAction.LeaderStagger] = 0x8cff,
        [NorfairPipeBugAction.UpperNearStagger] = 0x8d0c,
        [NorfairPipeBugAction.UpperFarStagger] = 0x8d4e,
        [NorfairPipeBugAction.LowerNearStagger] = 0x8d90,
        [NorfairPipeBugAction.LowerFarStagger] = 0x8dd2,
        [NorfairPipeBugAction.FlyLeft] = 0x8e14,
        [NorfairPipeBugAction.FlyRight] = 0x8e35,
        [NorfairPipeBugAction.WaitForStagger] = 0x8e5a,
    });

    /// <summary>The routine word naming each <see cref="YellowPipeBugAction"/>.</summary>
    internal static readonly NativeWordIdentities<YellowPipeBugAction> YellowPipeBugActions = new(new Dictionary<YellowPipeBugAction, ushort>
    {
        [YellowPipeBugAction.WaitForSamus] = 0x8fb5,
        [YellowPipeBugAction.EmergenceDelay] = 0x8ff5,
        [YellowPipeBugAction.FlyLeft] = 0x9028,
        [YellowPipeBugAction.FlyRight] = 0x90bd,
        [YellowPipeBugAction.ArcLeft] = 0x915a,
        [YellowPipeBugAction.ArcRight] = 0x91d8,
    });

    /// <summary>
    /// The Brinstar animation-table index naming each Zeb animation: bit zero is the shooting animation and bit one the
    /// right facing.
    /// </summary>
    private static readonly NativeWordIdentities<PipeBugSegment> ZebAnimations = new(new Dictionary<PipeBugSegment, ushort>
    {
        [PipeBugSegment.ZebRisingLeft] = 0,
        [PipeBugSegment.ZebShootingLeft] = 1,
        [PipeBugSegment.ZebRisingRight] = 2,
        [PipeBugSegment.ZebShootingRight] = 3,
    });

    /// <summary>The same index naming each Zebbo animation, read through the strong bug's table.</summary>
    private static readonly NativeWordIdentities<PipeBugSegment> ZebboAnimations = new(new Dictionary<PipeBugSegment, ushort>
    {
        [PipeBugSegment.ZebboRisingLeft] = 0,
        [PipeBugSegment.ZebboShootingLeft] = 1,
        [PipeBugSegment.ZebboRisingRight] = 2,
        [PipeBugSegment.ZebboShootingRight] = 3,
    });

    /// <summary>The animation-table indexes of a strong Zebbo's or an ordinary Zeb's animations.</summary>
    internal static NativeWordIdentities<PipeBugSegment> BrinstarAnimations(bool strong) => strong ? ZebboAnimations : ZebAnimations;
}
