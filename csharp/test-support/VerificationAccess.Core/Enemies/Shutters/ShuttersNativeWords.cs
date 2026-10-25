using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Core.Enemies.Shutters;

/// <summary>
/// Verification's native view of the Shutters identities (#1412): the cartridge routine words naming each member. Core's
/// values are arbitrary; a native snapshot or ROM oracle translates through these tables.
/// </summary>
internal static class ShuttersNativeWords
{
    /// <summary>The routine word naming each <see cref="GrowingShutterFunction"/>.</summary>
    internal static readonly NativeWordIdentities<GrowingShutterFunction> GrowingShutterFunctions = new(new Dictionary<GrowingShutterFunction, ushort>
    {
        [GrowingShutterFunction.WaitToGrowUpForTimer] = 0xeabd,
        [GrowingShutterFunction.WaitToGrowUpForProximity] = 0xead1,
        [GrowingShutterFunction.WaitToGrowDownForProximity] = 0xeae7,
        [GrowingShutterFunction.WaitToGrowDownForTimer] = 0xeafd,
        [GrowingShutterFunction.GrowDown] = 0xeb11,
        [GrowingShutterFunction.GrowUp] = 0xec13,
    });

    /// <summary>The routine word naming each <see cref="HorizontalShutterFunction"/>.</summary>
    internal static readonly NativeWordIdentities<HorizontalShutterFunction> HorizontalShutterFunctions = new(new Dictionary<HorizontalShutterFunction, ushort>
    {
        [HorizontalShutterFunction.Initial] = 0xf224,
        [HorizontalShutterFunction.WaitForTimer] = 0xf230,
        [HorizontalShutterFunction.WaitForHorizontalProximity] = 0xf243,
        [HorizontalShutterFunction.Activate] = 0xf254,
        [HorizontalShutterFunction.InitialNoOp] = 0xf25b,
        [HorizontalShutterFunction.MovingLeft] = 0xf272,
        [HorizontalShutterFunction.MovingRight] = 0xf2e4,
        [HorizontalShutterFunction.StoppedAfterMovingLeft] = 0xf38c,
        [HorizontalShutterFunction.StoppedAfterMovingRight] = 0xf3b0,
        [HorizontalShutterFunction.PermanentNoOp] = 0xf3d4,
    });

    /// <summary>The routine word naming each <see cref="VerticalShutterFunction"/>.</summary>
    internal static readonly NativeWordIdentities<VerticalShutterFunction> VerticalShutterFunctions = new(new Dictionary<VerticalShutterFunction, ushort>
    {
        [VerticalShutterFunction.Initial] = 0xef09,
        [VerticalShutterFunction.WaitForTimer] = 0xef15,
        [VerticalShutterFunction.WaitForHorizontalProximity] = 0xef28,
        [VerticalShutterFunction.Activate] = 0xef39,
        [VerticalShutterFunction.InitialNoOp] = 0xef40,
        [VerticalShutterFunction.MovingUp] = 0xef68,
        [VerticalShutterFunction.MovingDown] = 0xefd4,
        [VerticalShutterFunction.StoppedAfterMovingUp] = 0xf040,
        [VerticalShutterFunction.StoppedAfterMovingDown] = 0xf072,
        [VerticalShutterFunction.PermanentNoOp] = 0xf099,
    });
}
