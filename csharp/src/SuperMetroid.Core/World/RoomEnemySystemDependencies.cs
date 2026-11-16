using SuperMetroid.Core.Bosses.Artwork;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.Doors;

namespace SuperMetroid.Core.World;

/// <summary>The collaborators a <see cref="RoomEnemySystem"/> is constructed with, built once by the room load that creates it (#621).</summary>
[RoomLifetime]
internal sealed record RoomEnemySystemDependencies
{
    /// <summary>The enemy state and installed artwork that outlive each room.</summary>
    public required EnemySession Session { get; init; }

    /// <summary>
    /// The compatibility services the room's enemies consult: the room's own, created with the system by its builder, and
    /// those that outlive every room, which the builder hands on.
    /// </summary>
    public required RoomEnemyCompatibility Compatibility { get; init; }

    /// <summary>The room's enemy definition source.</summary>
    public required IRoomEnemyDefinitionSource Definitions { get; init; }

    /// <summary>The game-state services the room's AI calls.</summary>
    public required IRoomEnemyServices Services { get; init; }

    /// <summary>The area the room is in.</summary>
    public required AreaId Area { get; init; }

    /// <summary>The session's installed boss artwork.</summary>
    public required BossArtworkBinding BossArtwork { get; init; }

    /// <summary>The room's room objects that show its enemy-breakable blocks breaking, which live as long as the room.</summary>
    public required EnemyBreakableTerrainObjects BreakableTerrain { get; init; }

    /// <summary>
    /// The room's effect state, which lives as long as the room: the liquid Ridley and the Chozo statue move, and the
    /// acid surface Crocomire's smoke rises from (#2062).
    /// </summary>
    public required RoomFxOverlayState RoomFx { get; init; }

    /// <summary>
    /// Samus's passage through a door, during which the elevator, Zebetites, Etecoons, face blocks' glow and Ridley's
    /// opening wait hold still.
    /// </summary>
    public required DoorPassage DoorPassage { get; init; }
}
