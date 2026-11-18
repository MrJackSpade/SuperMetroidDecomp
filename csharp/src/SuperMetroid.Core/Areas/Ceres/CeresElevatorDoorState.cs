using SuperMetroid.Core.Bosses;

namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>The state of the elevator room's blocked door (#1953).</summary>
internal sealed class CeresElevatorDoorState
{
    /// <summary>Where the door stands in its destruction.</summary>
    public CeresElevatorDoorPhase Phase { get; internal set; }

    /// <summary>
    /// Updates left in the door's destruction rumble, the one it ends on included: 49 when it starts, and the update that takes
    /// it to zero ends the destruction, which never counts it again (#2281).
    /// </summary>
    public int RumbleUpdatesLeft { get; internal set; }

    /// <summary>Updates left before the next destruction explosion, the one it bursts on included; reloaded as it bursts (#2281).</summary>
    public int ExplosionIntervalLeft { get; internal set; }

    /// <summary>
    /// Where the next destruction burst goes off (#2424): a place of
    /// <see cref="CeresDoorRumbleGeometryDefinitions.ExplosionOffsets"/>, the first until the rumble starts, each burst
    /// moving it on to the next and the first again after the last.
    /// </summary>
    public ListStep<Offset16<RoomPixels>> NextExplosion { get; internal set; } =
        ListStep<Offset16<RoomPixels>>.FirstOf(CeresDoorRumbleGeometryDefinitions.ExplosionOffsets);
}
