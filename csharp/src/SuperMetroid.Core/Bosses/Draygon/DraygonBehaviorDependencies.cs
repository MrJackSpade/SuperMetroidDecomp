using SuperMetroid.Core.Bosses.Artwork;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Enemies.EffectActors;

namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>The collaborators a <see cref="DraygonBehavior"/> is constructed with, built once by the owner that creates it (#621).</summary>
[RoomLifetime]
internal sealed record DraygonBehaviorDependencies
{
    /// <summary>The enemy pass's results, which this behavior writes.</summary>
    public required BossFrameResultBuilder FrameResults { get; init; }

    /// <summary>The room Draygon fights in: its services, its random numbers and whether he is already defeated.</summary>
    public required EnemyRoomBinding Room { get; init; }

    /// <summary>The enemy projectiles of his fight, his turrets' shots among them.</summary>
    public required EnemyProjectileEngine Projectiles { get; init; }

    /// <summary>The room's earthquake, which his fight starts.</summary>
    public required RoomEarthquake Earthquake { get; init; }

    /// <summary>The enemy update's sound and music requests.</summary>
    public required EnemyAudioQueue Audio { get; init; }

    /// <summary>The room's effect actors, which his intro, his movement, his grab and his death show.</summary>
    public required RoomEffectActors EffectActors { get; init; }

    /// <summary>The room's enemy presentation, which his body is shown through.</summary>
    public required EnemyPresentation Presentation { get; init; }

    /// <summary>The enemy update's clocks and flags.</summary>
    public required EnemyFrameContext FrameContext { get; init; }

    /// <summary>The enemy touch, which his body deals Samus.</summary>
    public required EnemyCollision Collision { get; init; }

    /// <summary>The enemies' drops, which his fight scatters.</summary>
    public required EnemyDeathsAndDrops DeathsAndDrops { get; init; }

    /// <summary>The cap on the camera's scrolling speed, which his grab sets while he holds Samus.</summary>
    public required SamusScrollCap ScrollCap { get; init; }

    /// <summary>The session's installed boss artwork.</summary>
    public required BossArtworkBinding BossArtwork { get; init; }
}
