using SuperMetroid.Core.Rooms.Scrolling;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>The collaborators a <see cref="KraidRise"/> is constructed with, built once by its owner.</summary>
[RoomLifetime]
internal sealed record KraidRiseDependencies
{
    /// <summary>The enemy pass's results, which this behavior writes.</summary>
    public required EnemyPassEvents PassEvents { get; init; }

    /// <summary>Room services used throughout the Kraid fight.</summary>
    public required EnemyRoomBinding Room { get; init; }

    /// <summary>Owns rocks, lint, and other fight projectiles.</summary>
    public required EnemyProjectileEngine Projectiles { get; init; }

    /// <summary>Publishes arena shake during growth, death, and heavy movement.</summary>
    public required RoomEarthquake Earthquake { get; init; }


    /// <summary>Applies Kraid's colours, background pictures, and sprite presentation.</summary>
    public required EnemyPresentation Presentation { get; init; }

    /// <summary>The room camera's distance mode, which the fight requests.</summary>
    public required RoomCameraDistance CameraDistance { get; init; }


    /// <summary>Kraid's state and part lookups, part animations, phase waits, random numbers and the sound he last requested.</summary>
    public required KraidEncounterAccess Access { get; init; }

    /// <summary>Kraid's room background: its fade, color transitions and the ceiling rocks.</summary>
    public required KraidBackground Background { get; init; }

    /// <summary>The body layer Kraid draws himself, which the room shows (#2194).</summary>
    public required KraidBodyLayer BodyLayer { get; init; }

    /// <summary>Kraid's head: the mouth-open attack and reaction, the eye glow and the health colours.</summary>
    public required KraidHead Head { get; init; }

    /// <summary>The room's bosses' encounters, among which his body attaches the one it begins.</summary>
    public required RoomBossEncounters Encounters { get; init; }
}
