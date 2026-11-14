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

    /// <summary>The routine word naming each <see cref="VerticalShutterAction"/>.</summary>
    /// <remarks>
    /// <see cref="VerticalShutterAction.Still"/> has two: the reaction-triggered shutter's initial wait and the permanent
    /// stop at an end do the same nothing (#1818).
    /// </remarks>
    internal static readonly NativeWordIdentities<VerticalShutterAction> VerticalShutterActions = new(new Dictionary<ushort, VerticalShutterAction>
    {
        [0xef09] = VerticalShutterAction.Initial,
        [0xef15] = VerticalShutterAction.WaitForTimer,
        [0xef28] = VerticalShutterAction.WaitForHorizontalProximity,
        [0xef39] = VerticalShutterAction.Activate,
        [0xef40] = VerticalShutterAction.Still,
        [0xef68] = VerticalShutterAction.MovingUp,
        [0xefd4] = VerticalShutterAction.MovingDown,
        [0xf040] = VerticalShutterAction.StoppedAfterMovingUp,
        [0xf072] = VerticalShutterAction.StoppedAfterMovingDown,
        [0xf099] = VerticalShutterAction.Still,
    });
}
