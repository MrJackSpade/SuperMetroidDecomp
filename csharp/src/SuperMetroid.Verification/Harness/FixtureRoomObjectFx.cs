using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Verification.Harness;

/// <summary>
/// The room FX of a room-object fixture whose room objects never touch the room's liquid (#2015): every read and write
/// fails at the dependency, as a check that reaches it needs the room's real FX.
/// </summary>
internal sealed class FixtureRoomObjectFx : IRoomObjectFx
{
    /// <inheritdoc/>
    public FixedCoordinate? BaseY => throw Missing();

    /// <inheritdoc/>
    public int? TargetSurface => throw Missing();

    /// <inheritdoc/>
    public void SetLiquidLevel(int line) => throw Missing();

    /// <inheritdoc/>
    public void SetLiquidSpeed(SignedEightEight speed) => throw Missing();

    /// <inheritdoc/>
    public void MoveLiquidAfter(int delay, SignedEightEight speed) => throw Missing();

    /// <inheritdoc/>
    public void EndLiquidWaitNextUpdate() => throw Missing();

    /// <inheritdoc/>
    public void HaltLiquid() => throw Missing();

    /// <inheritdoc/>
    public void EnableWaterPhysics() => throw Missing();

    /// <summary>The failure of a check whose room objects reached a room FX it did not install.</summary>
    private static InvalidOperationException Missing() =>
        new("The room-object fixture did not install the room's FX.");
}
