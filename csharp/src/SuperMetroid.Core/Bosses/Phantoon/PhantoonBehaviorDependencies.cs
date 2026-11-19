using SuperMetroid.Core.Bosses.Artwork;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>The collaborators a <see cref="PhantoonBehavior"/> is constructed with, built once by the owner that creates it (#621).</summary>
[RoomLifetime]
internal sealed record PhantoonBehaviorDependencies
{
    /// <summary>The room Phantoon's fight is in: its area, its enemy definitions and the game services and random numbers its enemies use.</summary>
    public required EnemyRoomBinding Room { get; init; }

    /// <summary>The enemy projectiles Phantoon's fight fires and spawns.</summary>
    public required EnemyProjectileEngine Projectiles { get; init; }

    /// <summary>The room's earthquake, which Phantoon's fight starts.</summary>
    public required RoomEarthquake Earthquake { get; init; }

    /// <summary>The enemy update's sound and music requests.</summary>
    public required EnemyAudioQueue Audio { get; init; }

    /// <summary>The room's enemy presentation, which Phantoon's fight is shown through.</summary>
    public required EnemyPresentation Presentation { get; init; }

    /// <summary>The enemy update's random number, frame counter and flags.</summary>
    public required EnemyFrameContext FrameContext { get; init; }

    /// <summary>The enemies' touches, shots and bombs with Samus, which Phantoon's fight reacts to.</summary>
    public required EnemyCollision Collision { get; init; }

    /// <summary>The enemies' death explosions and drops, which Phantoon's fight spawns.</summary>
    public required EnemyDeathsAndDrops DeathsAndDrops { get; init; }

    /// <summary>The session's installed boss artwork.</summary>
    public required BossArtworkBinding BossArtwork { get; init; }

    /// <summary>The bosses' results of the pass, where the fight asks for its door changes.</summary>
    public required EnemyPassEvents PassEvents { get; init; }

    /// <summary>The encounters of the room's bosses, to which his body's setup attaches his (#2194).</summary>
    public required RoomBossEncounters Encounters { get; init; }

    /// <summary>The room's enemy places, among which his body finds his parts listed after it and each part his body before it (#2252).</summary>
    public required EnemyPlaces Places { get; init; }
}
