using SuperMetroid.Core.Bosses.Artwork;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>The collaborators a <see cref="MotherBrainFakeDeath"/> is constructed with, built once by its owner.</summary>
[RoomLifetime]
internal sealed record MotherBrainFakeDeathDependencies
{
    /// <summary>The enemy pass's results, which this behavior writes.</summary>
    public required BossFrameResultBuilder FrameResults { get; init; }

    /// <summary>Routes Mother Brain's AI and projectile handlers through the room dispatch tables.</summary>
    public required IEnemyDispatch Dispatch { get; init; }

    /// <summary>Provides room scratch memory, tiles, palette state, and effects used by the encounter.</summary>
    public required EnemyRoomBinding Room { get; init; }

    /// <summary>Owns the bombs, glass, beams, turrets, and other encounter projectiles.</summary>
    public required EnemyProjectileEngine Projectiles { get; init; }

    /// <summary>Publishes screen shake during the encounter's collapse and attacks.</summary>
    public required RoomEarthquake Earthquake { get; init; }

    /// <summary>Draws encounter artwork and applies installed palette data.</summary>
    public required EnemyPresentation Presentation { get; init; }

    /// <summary>Holds the shared body/head state for the current Mother Brain room.</summary>
    public required MotherBrainEncounter MotherBrainEncounter { get; init; }

    /// <summary>The words each physical slot keeps across MotherBrain occupants and rooms.</summary>
    public required MotherBrainExtensionWords[] Extensions { get; init; }

    /// <summary>Mother Brain's state lookups, body and brain program setters, timers and PLM requests.</summary>
    public required MotherBrainEncounterAccess Access { get; init; }

    /// <summary>The session's installed boss artwork.</summary>
    public required BossArtworkBinding BossArtwork { get; init; }
}
