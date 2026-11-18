using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Core.Enemies.Shutters;

/// <summary>
/// Verification's native view of the Shutters identities (#1412): the cartridge routine words naming each member. Core's
/// values are arbitrary; a native snapshot or ROM oracle translates through these tables.
/// </summary>
internal static class ShuttersNativeWords
{
    /// <summary>
    /// The routine word naming each <see cref="GrowingShutterAction"/> and the way the shutter grows (#2512), the cartridge
    /// giving each way a routine of its own.
    /// </summary>
    internal static readonly NativeWordIdentities<(GrowingShutterAction Action, VerticalDirection Growth)> GrowingShutterActions =
        new(new Dictionary<(GrowingShutterAction Action, VerticalDirection Growth), ushort>
        {
            [(GrowingShutterAction.WaitForTimer, VerticalDirection.Up)] = 0xeabd,
            [(GrowingShutterAction.WaitForProximity, VerticalDirection.Up)] = 0xead1,
            [(GrowingShutterAction.WaitForProximity, VerticalDirection.Down)] = 0xeae7,
            [(GrowingShutterAction.WaitForTimer, VerticalDirection.Down)] = 0xeafd,
            [(GrowingShutterAction.Grow, VerticalDirection.Down)] = 0xeb11,
            [(GrowingShutterAction.Grow, VerticalDirection.Up)] = 0xec13,
        });

    /// <summary>
    /// The routine word of the vertical shutter's initial dispatch, which calls its trigger mode's entry every update
    /// until activation replaces it. Core installs that entry's action at initialization instead (#2075), so a recorded
    /// dispatch decodes to <see cref="ShuttersBehavior.InitialAction"/> of the shutter's trigger.
    /// </summary>
    internal const ushort VerticalShutterTriggerDispatch = 0xef09;

    /// <summary>
    /// The vertical shutter action a recorded routine word names: the trigger dispatch decodes to the action its
    /// <paramref name="triggerMode"/> selects, every other word through <see cref="VerticalShutterActions"/>.
    /// </summary>
    internal static VerticalShutterAction DecodeVerticalShutterAction(ushort word, ShutterTriggerMode triggerMode) =>
        word == VerticalShutterTriggerDispatch
            ? ShuttersBehavior.InitialAction(triggerMode)
            : VerticalShutterActions.Decode(word, "recorded VerticalShutterAction");

    /// <summary>The routine word naming each <see cref="VerticalShutterAction"/>.</summary>
    /// <remarks>
    /// <see cref="VerticalShutterAction.Still"/> has two: the reaction-triggered shutter's initial wait and the permanent
    /// stop at an end do the same nothing (#1814, #1818).
    /// </remarks>
    internal static readonly NativeWordIdentities<VerticalShutterAction> VerticalShutterActions = new(new Dictionary<ushort, VerticalShutterAction>
    {
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
