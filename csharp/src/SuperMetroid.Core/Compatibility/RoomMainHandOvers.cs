namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// What one room's main hands the next (#2572): each hand-over named for the behaviour it carries, all three over the one
/// value the room mains share (<see cref="RoomMainCarryover"/>). A room's composition hands each main only its own: the
/// Ceres elevator shaft and the pre-elevator hall <see cref="ShaftTurnToDebris"/>, the Zebes escape's shaking rooms and
/// their two clearing setups <see cref="EscapeQuake"/>, the Maridia elevatube <see cref="ElevatubeFraction"/>.
/// </summary>
/// <remarks>
/// Created once with the active room, for the session's every room load. Evidence: the original game's room mains share
/// one working word that room loading never clears (<see cref="RoomMainCarryover"/>), which each of the three hand-overs
/// reads and leaves.
/// </remarks>
public sealed partial class RoomMainHandOvers
{
    /// <summary>Starts the session's hand-overs over a shared value no main has left yet.</summary>
    public RoomMainHandOvers()
    {
        var shared = new RoomMainCarryover();
        ShaftTurnToDebris = new ShaftTurnSetsDebrisWait(shared);
        EscapeQuake = new EscapeQuakeCarriedAcrossRooms(shared);
        ElevatubeFraction = new ElevatubeInheritedFraction(shared);
    }

    /// <summary>The Ceres elevator shaft's turn, which sets the pre-elevator hall's first debris wait.</summary>
    public ShaftTurnSetsDebrisWait ShaftTurnToDebris { get; }

    /// <summary>The Zebes escape's diagonal quake, carried from room to room.</summary>
    public EscapeQuakeCarriedAcrossRooms EscapeQuake { get; }

    /// <summary>The Maridia elevatube's fraction, adopted from the previous room's main.</summary>
    public ElevatubeInheritedFraction ElevatubeFraction { get; }
}
