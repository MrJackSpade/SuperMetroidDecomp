using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Samus;

namespace SuperMetroid.Core.World;

/// <summary>
/// The room FX as room objects use it (#1395). Room objects write the shared liquid values Samus reads; the model keeps Samus's
/// copy of whether the water's surface affects her, so enabling water physics writes both in the same operation.
/// </summary>
internal sealed class RoomObjectFx(RoomFxOverlayState fx, Func<SamusState?> samus) : IRoomObjectFx
{
    /// <inheritdoc/>
    public FixedCoordinate? BaseY => fx.BaseY;

    /// <inheritdoc/>
    public int? TargetSurface => fx.TargetSurface;

    /// <inheritdoc/>
    public void RemoveLiquidTarget() => fx.RemoveLiquidTarget();

    /// <inheritdoc/>
    public void SetLiquidMotion(int? liquidLevel = null, int? targetLevel = null,
        SignedEightEight? levelSpeed = null, int? moveDelay = null) =>
        fx.SetLiquidMotion(liquidLevel, targetLevel, levelSpeed, moveDelay);

    /// <inheritdoc/>
    public void EnableWaterPhysics()
    {
        fx.EnableWaterPhysics();
        (samus() ?? throw new InvalidOperationException("Room water physics has no live Samus owner."))
            .LiquidPhysics.ConfigureFor(fx);
    }
}
