using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Samus;

namespace SuperMetroid.Core.World;

/// <summary>The collaborators a <see cref="RoomMainRoutines"/> is constructed with, built once by the owner that creates it (#621).</summary>
internal sealed record RoomMainRoutinesDependencies
{
    /// <summary>Where the layers stand, which Crocomire's hidden-wall rumble shakes.</summary>
    public required BackgroundScrollState BackgroundScroll { get; init; }

    /// <summary>Samus, whose X-ray scope holds the block animations still.</summary>
    public required SamusFrameState Player { get; init; }

    /// <summary>The loaded room, whose own behaviour and area choose what runs.</summary>
    public required ActiveRoomState Room { get; init; }

    /// <summary>The session's shared game state: the defeated bosses and the random number the rooms' behaviours read.</summary>
    public required SharedGameState System { get; init; }
}
