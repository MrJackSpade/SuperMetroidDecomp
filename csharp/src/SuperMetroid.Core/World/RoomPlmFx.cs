using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Samus;

namespace SuperMetroid.Core.World;

/// <summary>
/// The room FX as room PLMs use it (#1395). Bank $84 writes the shared liquid words Samus reads; the port keeps Samus's
/// copy of the liquid options, so enabling water physics writes both in the same operation.
/// </summary>
internal sealed class RoomPlmFx(RoomLayer3FxState fx, Func<SamusState?> samus) : IRoomPlmFx
{
    /// <inheritdoc/>
    public SnesFixedPosition BaseY => fx.BaseY;

    /// <inheritdoc/>
    public Position16<RoomPixels> TargetSurface => fx.TargetSurface;

    /// <inheritdoc/>
    public void ApplyCartridgeMotionWrites(ushort? baseYPosition = null, ushort? targetYPosition = null,
        ushort? packedYVelocity = null, ushort? timer = null) =>
        fx.ApplyCartridgeMotionWrites(baseYPosition, targetYPosition, packedYVelocity, timer);

    /// <inheritdoc/>
    public void EnableWaterPhysics()
    {
        fx.EnableWaterPhysics();
        (samus() ?? throw new InvalidOperationException("Room water physics has no live Samus owner."))
            .LiquidPhysics.ConfigureFor(fx);
    }
}
