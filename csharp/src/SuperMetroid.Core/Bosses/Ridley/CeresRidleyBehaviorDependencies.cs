using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Fx;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>The collaborators a <see cref="CeresRidleyBehavior"/> is constructed with, built once by the owner that creates it (#621).</summary>
[ArchitectureReview("wide-dependencies", "The composition seam for Ceres Ridley's five parts (flight, Baby scene, fade-in, hits, getaway), each built from a different subset of these collaborators; #2038 added the door passage that holds his opening wait.")]
[RoomLifetime]
internal sealed record CeresRidleyBehaviorDependencies
{
    /// <summary>The enemy pass's results, which this behavior writes.</summary>
    public required BossFrameResultBuilder FrameResults { get; init; }

    /// <summary>Sets up enemies in their places, spawns them and runs their AI.</summary>
    public required IEnemyLifecycle Lifecycle { get; init; }

    /// <summary>The room Ceres Ridley's fight is in: its area, its enemy definitions and the game services and random numbers its enemies use.</summary>
    public required EnemyRoomBinding Room { get; init; }

    /// <summary>The room's earthquake, which Ceres Ridley's fight starts.</summary>
    public required RoomEarthquake Earthquake { get; init; }

    /// <summary>The enemy update's sound and music requests.</summary>
    public required EnemyAudioQueue Audio { get; init; }

    /// <summary>The room's enemy places.</summary>
    public required EnemyPlaces Places { get; init; }

    /// <summary>What the boss draws with: the room\'s presentation surfaces and the installed boss artwork.</summary>
    public required BossPresentation Presentation { get; init; }

    /// <summary>The enemies' touches, shots and bombs with Samus, which Ceres Ridley's fight reacts to.</summary>
    public required EnemyCollision Collision { get; init; }

    /// <summary>The Ridley encounter state the Ceres and Lower Norfair fights share.</summary>
    public required RidleyEncounter RidleyEncounter { get; init; }

    /// <summary>Ridley's tail, its segments' flight and whip and the shots its joints block.</summary>
    public required RidleyTail RidleyTail { get; init; }

    /// <summary>The room-wide enemy state the game reads, which Ceres Ridley's fight sets.</summary>
    public required EnemyRoomStatus RoomStatus { get; init; }

    /// <summary>Samus's passage through a door, which holds Ridley's opening wait.</summary>
    public required DoorPassage DoorPassage { get; init; }

    /// <summary>The session's unauthored Speed Booster cadence, whose delays the getaway's wing frames leave behind.</summary>
    public required UnauthoredBoostCadence UnauthoredBoostCadence { get; init; }

}
