using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>The collaborators a <see cref="TorizoBehavior"/> is constructed with, built once by the owner that creates it (#621).</summary>
[RoomLifetime]
internal sealed partial record TorizoBehaviorDependencies
{
    /// <summary>The room the Torizo's fight is in: its area, its enemy definitions and the game services and random numbers its enemies use.</summary>
    public required EnemyRoomBinding Room { get; init; }

    /// <summary>The room's terrain, which the Torizo's movement steps against.</summary>
    public required EnemyTerrain Terrain { get; init; }

    /// <summary>The enemy projectiles the Torizo's fight fires and spawns.</summary>
    public required EnemyProjectileEngine Projectiles { get; init; }

    /// <summary>The room's earthquake, which the Torizo's fight starts.</summary>
    public required RoomEarthquake Earthquake { get; init; }

    /// <summary>The enemy update's sound and music requests.</summary>
    public required EnemyAudioQueue Audio { get; init; }

    /// <summary>The room's enemy presentation, which the Torizo's fight is shown through.</summary>
    public required EnemyPresentation Presentation { get; init; }

    /// <summary>The enemy update's random number, frame counter and flags.</summary>
    public required EnemyFrameContext FrameContext { get; init; }

    /// <summary>The enemies' touches, shots and bombs with Samus, which the Torizo's fight reacts to.</summary>
    public required EnemyCollision Collision { get; init; }

    /// <summary>The enemies' death explosions and drops, which the Torizo's fight spawns.</summary>
    public required EnemyDeathsAndDrops DeathsAndDrops { get; init; }

    /// <summary>The room effects the bosses request this update: the fight's start asks for the belly's glow.</summary>
    public required EnemyPassEvents PassEvents { get; init; }
}
