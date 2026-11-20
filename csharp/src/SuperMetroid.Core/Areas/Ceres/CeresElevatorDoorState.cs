namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>The state of the elevator room's blocked door (#1953).</summary>
internal sealed class CeresElevatorDoorState
{
    /// <summary>Where the door stands in its destruction.</summary>
    public CeresElevatorDoorPhase Phase { get; internal set; }

    /// <summary>
    /// The door's destruction rumble (#2281): <see cref="CeresElevatorDoorEntity.RumbleDuration"/> when it starts, and the update that ends it ends the destruction,
    /// which never counts it again.
    /// </summary>
    public UpdateCountdown Rumble { get; internal set; } = UpdateCountdown.Ended();

    /// <summary>The wait before the next destruction explosion, over on the update it bursts and restarted as it bursts (#2281).</summary>
    public UpdateCountdown ExplosionWait { get; internal set; } = UpdateCountdown.Ended();

    /// <summary>
    /// Where the next destruction burst goes off (#2424): a place of
    /// <see cref="CeresDoorRumbleGeometryDefinitions.ExplosionOffsets"/>, the first until the rumble starts, each burst
    /// moving it on to the next and the first again after the last.
    /// </summary>
    internal CyclingSequence<Offset16<RoomPixels>> NextExplosion { get; set; } =
        CyclingSequence<Offset16<RoomPixels>>.StartOf(CeresDoorRumbleGeometryDefinitions.ExplosionOffsets);
}
