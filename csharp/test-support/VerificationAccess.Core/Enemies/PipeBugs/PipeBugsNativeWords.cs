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
        [NorfairPipeBugAction.WaitForStagger] = 0x8e5a,
    });

    /// <summary>
    /// The routine word naming each way a departed Norfair member flies (#2165): one <see cref="NorfairPipeBugAction.Fly"/>
    /// carrying its direction, the cartridge giving each direction a routine of its own.
    /// </summary>
    internal static readonly NativeWordIdentities<HorizontalDirection> NorfairPipeBugFlights = new(new Dictionary<HorizontalDirection, ushort>
    {
        [HorizontalDirection.Left] = 0x8e14,
        [HorizontalDirection.Right] = 0x8e35,
    });

    /// <summary>The routine word naming each <see cref="YellowPipeBugAction"/>.</summary>
    internal static readonly NativeWordIdentities<YellowPipeBugAction> YellowPipeBugActions = new(new Dictionary<YellowPipeBugAction, ushort>
    {
        [YellowPipeBugAction.WaitForSamus] = 0x8fb5,
        [YellowPipeBugAction.EmergenceDelay] = 0x8ff5,
    });

    /// <summary>
    /// The routine word naming each yellow flight or arc and the way it goes (#2165): one action carrying its direction, the
    /// cartridge giving each direction a routine of its own.
    /// </summary>
    internal static readonly NativeWordIdentities<(YellowPipeBugAction Action, HorizontalDirection Direction)> YellowPipeBugFlights =
        new(new Dictionary<(YellowPipeBugAction Action, HorizontalDirection Direction), ushort>
        {
            [(YellowPipeBugAction.Fly, HorizontalDirection.Left)] = 0x9028,
            [(YellowPipeBugAction.Fly, HorizontalDirection.Right)] = 0x90bd,
            [(YellowPipeBugAction.Arc, HorizontalDirection.Left)] = 0x915a,
            [(YellowPipeBugAction.Arc, HorizontalDirection.Right)] = 0x91d8,
        });

    /// <summary>
    /// The routine word naming each Norfair formation role's stagger after rising (#2071): the leader's holds its height,
    /// and each other member's moves it by its role's offset, the cartridge giving every offset a routine of its own.
    /// </summary>
    internal static readonly NativeWordIdentities<NorfairPipeBugRole> FormationStaggers = new(new Dictionary<NorfairPipeBugRole, ushort>
    {
        [PipeBugDefinitions.LeaderRole] = 0x8cff,
        [PipeBugDefinitions.UpperNearRole] = 0x8d0c,
        [PipeBugDefinitions.UpperFarRole] = 0x8d4e,
        [PipeBugDefinitions.LowerNearRole] = 0x8d90,
        [PipeBugDefinitions.LowerFarRole] = 0x8dd2,
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
    internal static NativeWordIdentities<PipeBugSegment> BrinstarAnimations(BrinstarPipeBugKind kind) =>
        kind == BrinstarPipeBugKind.Strong ? ZebboAnimations : ZebAnimations;
}
