using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>The collaborators a <see cref="PhantoonDeath"/> is constructed with, built once by its owner.</summary>
internal sealed partial record PhantoonDeathDependencies
{
    /// <summary>Room services and background state used by the Wrecked Ship encounter.</summary>
    public required EnemyRoomBinding Room { get; init; }

    /// <summary>Owns the starting, casual, rain, rage, and spiral flame actors.</summary>
    public required EnemyProjectileEngine Projectiles { get; init; }

    /// <summary>Applies body colours and the background changes.</summary>
    public required EnemyPresentation Presentation { get; init; }

    /// <summary>The current enemy pass's clocks and flags.</summary>
    public required EnemyFrameContext FrameContext { get; init; }

    /// <summary>Handles Phantoon's damage, death, and flame drops.</summary>
    public required EnemyDeathsAndDrops DeathsAndDrops { get; init; }

    /// <summary>Phantoon's fades, health colors, wave amplitude and animations, shared by every phase of the fight.</summary>
    public required PhantoonPresentation PhantoonPresentation { get; init; }

    /// <summary>Phantoon's attack rounds: figure eights, eye tracking, swoops, flame rain and rage.</summary>
    public required PhantoonPatterns Patterns { get; init; }

    /// <summary>Receives the death's sounds and music change.</summary>
    public required EnemyAudioQueue Audio { get; init; }

    /// <summary>The bosses' results of the pass, where the death asks for the restored door.</summary>
    public required EnemyPassEvents PassEvents { get; init; }
}
