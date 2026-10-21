using SuperMetroid.Core.Bosses.Artwork;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>The collaborators a <see cref="PhantoonDeath"/> is constructed with, built once by its owner.</summary>
[RoomLifetime]
internal sealed record PhantoonDeathDependencies
{
    /// <summary>Room services and tilemap state used by the Wrecked Ship encounter.</summary>
    public required EnemyRoomBinding Room { get; init; }

    /// <summary>Owns the starting, casual, rain, rage, and spiral flame actors.</summary>
    public required EnemyProjectileEngine Projectiles { get; init; }

    /// <summary>Applies body palettes and BG2 tilemap changes.</summary>
    public required EnemyPresentation Presentation { get; init; }

    /// <summary>Supplies Samus state for eye tracking and attack selection.</summary>
    public required EnemyFrameContext FrameContext { get; init; }

    /// <summary>Handles Phantoon's damage, death, and flame drops.</summary>
    public required EnemyDeathsAndDrops DeathsAndDrops { get; init; }

    /// <summary>Phantoon's fades, health colors, wave amplitude, programs and function timer, shared by every phase of the fight.</summary>
    public required PhantoonPresentation PhantoonPresentation { get; init; }

    /// <summary>Phantoon's attack rounds: figure eights, eye tracking, swoops, flame rain and rage.</summary>
    public required PhantoonPatterns Patterns { get; init; }

    /// <summary>The session's installed boss artwork.</summary>
    public required BossArtworkBinding BossArtwork { get; init; }
}
