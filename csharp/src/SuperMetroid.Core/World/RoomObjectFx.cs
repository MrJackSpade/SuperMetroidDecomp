using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Samus;

namespace SuperMetroid.Core.World;

/// <summary>
/// The room FX as room objects use it (#1395). Room objects move the liquid whose surface Samus reads; the model keeps Samus's
/// copy of whether the water's surface affects her, so enabling water physics writes both in the same operation.
/// </summary>
internal sealed class RoomObjectFx(RoomFxOverlayState fx, Func<SamusState?> samus) : IRoomObjectFx
{
    /// <inheritdoc/>
    public FixedCoordinate? BaseY => fx.BaseY;

    /// <inheritdoc/>
    public int? TargetSurface => fx.TargetSurface;

    /// <inheritdoc/>
    public void SetLiquidLevel(int line) => fx.SetLiquidLevel(line);

    /// <inheritdoc/>
    public void SetLiquidSpeed(SignedEightEight speed) => fx.SetLiquidSpeed(speed);

    /// <inheritdoc/>
    public void MoveLiquidAfter(int delay, SignedEightEight speed) => fx.MoveLiquidAfter(delay, speed);

    /// <inheritdoc/>
    public void EndLiquidWaitNextUpdate() => fx.EndLiquidWaitNextUpdate();

    /// <inheritdoc/>
    public void HaltLiquid() => fx.HaltLiquid();

    /// <inheritdoc/>
    public void EnableWaterPhysics()
    {
        fx.EnableWaterPhysics();
        (samus() ?? throw new InvalidOperationException("Room water physics has no live Samus owner."))
            .LiquidPhysics.ConfigureFor(fx);
    }
}
