using SuperMetroid.Core.Rooms.Scrolling;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Enemies.EffectActors;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>The collaborators a <see cref="KraidBehavior"/> is constructed with, built once by the owner that creates it (#621).</summary>
[RoomLifetime]
internal sealed record KraidBehaviorDependencies
{
    /// <summary>The enemy pass's results, which this behavior writes.</summary>
    public required EnemyPassEvents PassEvents { get; init; }

    /// <summary>The room Kraid's fight is in: its area, its enemy definitions and the game services and random numbers its enemies use.</summary>
    public required EnemyRoomBinding Room { get; init; }

    /// <summary>The room's terrain, which Kraid's movement steps against.</summary>
    public required EnemyTerrain Terrain { get; init; }

    /// <summary>The enemy projectiles Kraid's fight fires and spawns.</summary>
    public required EnemyProjectileEngine Projectiles { get; init; }

    /// <summary>The room's earthquake, which Kraid's fight starts.</summary>
    public required RoomEarthquake Earthquake { get; init; }

    /// <summary>The enemy update's sound and music requests.</summary>
    public required EnemyAudioQueue Audio { get; init; }

    /// <summary>The room's effect actors, which Kraid's fight shows.</summary>
    public required RoomEffectActors EffectActors { get; init; }

    /// <summary>What the boss draws with: the room's presentation surfaces and the installed boss artwork.</summary>
    public required BossPresentation Presentation { get; init; }

    /// <summary>The enemies' touches, shots and bombs with Samus, which Kraid's fight reacts to.</summary>
    public required EnemyCollision Collision { get; init; }

    /// <summary>The enemies' death explosions and drops, which Kraid's fight spawns.</summary>
    public required EnemyDeathsAndDrops DeathsAndDrops { get; init; }

    /// <summary>The room camera's distance mode, which the fight requests.</summary>
    public required RoomCameraDistance CameraDistance { get; init; }

    /// <summary>Where the body offers its later parts the creation that builds each of them joining its encounter (#2554).</summary>
    public required EnemyPartOffers PartOffers { get; init; }

    /// <summary>The room's enemy places, where his body finds his other parts as his room lists them (#2554).</summary>
    public required EnemyPlaces Places { get; init; }
}
