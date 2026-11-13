using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Core.Enemies.Shutters;

/// <summary>
/// Verification's native view of the Shutters identities (#1412): the cartridge routine words naming each member. Core's
/// values are arbitrary; a native snapshot or ROM oracle translates through these tables.
/// </summary>
internal static class ShuttersNativeWords
{
    /// <summary>The routine word naming each <see cref="GrowingShutterAction"/>.</summary>
    internal static readonly NativeWordIdentities<GrowingShutterAction> GrowingShutterActions = new(new Dictionary<GrowingShutterAction, ushort>
    {
        [GrowingShutterAction.WaitToGrowUpForTimer] = 0xeabd,
        [GrowingShutterAction.WaitToGrowUpForProximity] = 0xead1,
        [GrowingShutterAction.WaitToGrowDownForProximity] = 0xeae7,
        [GrowingShutterAction.WaitToGrowDownForTimer] = 0xeafd,
        [GrowingShutterAction.GrowDown] = 0xeb11,
        [GrowingShutterAction.GrowUp] = 0xec13,
    });

    /// <summary>The routine word naming each <see cref="HorizontalShutterAction"/>.</summary>
    internal static readonly NativeWordIdentities<HorizontalShutterAction> HorizontalShutterActions = new(new Dictionary<HorizontalShutterAction, ushort>
    {
        [HorizontalShutterAction.Initial] = 0xf224,
        [HorizontalShutterAction.WaitForTimer] = 0xf230,
        [HorizontalShutterAction.WaitForHorizontalProximity] = 0xf243,
        [HorizontalShutterAction.Activate] = 0xf254,
        [HorizontalShutterAction.InitialNoOp] = 0xf25b,
        [HorizontalShutterAction.MovingLeft] = 0xf272,
        [HorizontalShutterAction.MovingRight] = 0xf2e4,
        [HorizontalShutterAction.StoppedAfterMovingLeft] = 0xf38c,
        [HorizontalShutterAction.StoppedAfterMovingRight] = 0xf3b0,
        [HorizontalShutterAction.PermanentNoOp] = 0xf3d4,
    });

    /// <summary>The routine word naming each <see cref="VerticalShutterAction"/>.</summary>
    internal static readonly NativeWordIdentities<VerticalShutterAction> VerticalShutterActions = new(new Dictionary<VerticalShutterAction, ushort>
    {
        [VerticalShutterAction.Initial] = 0xef09,
        [VerticalShutterAction.WaitForTimer] = 0xef15,
        [VerticalShutterAction.WaitForHorizontalProximity] = 0xef28,
        [VerticalShutterAction.Activate] = 0xef39,
        [VerticalShutterAction.InitialNoOp] = 0xef40,
        [VerticalShutterAction.MovingUp] = 0xef68,
        [VerticalShutterAction.MovingDown] = 0xefd4,
        [VerticalShutterAction.StoppedAfterMovingUp] = 0xf040,
        [VerticalShutterAction.StoppedAfterMovingDown] = 0xf072,
        [VerticalShutterAction.PermanentNoOp] = 0xf099,
    });
}
