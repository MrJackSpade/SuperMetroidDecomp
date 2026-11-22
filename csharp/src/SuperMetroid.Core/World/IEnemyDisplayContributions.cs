using System.Collections.Immutable;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies.MorphBallEye;
using SuperMetroid.Core.Escape;
using SuperMetroid.Core.Rooms.Backgrounds;

namespace SuperMetroid.Core.World;

/// <summary>
/// What the room's encounters show over the room, by what each is (#2571): the overlays written over the foreground and
/// the background, the turning floor an encounter flies over with the walls around it, and the beam cone the last latch
/// took. Reads are live.
/// </summary>
internal interface IEnemyDisplayContributions
{
    /// <summary>
    /// How the turning floor an encounter flies over appears as the flight last turned, zoomed and moved it, or null while
    /// no flight shows it (#2228).
    /// </summary>
    FloorView? FlownFloor { get; }

    /// <summary>The walls the flight shows around its turning floor (#1545), in the room's loaded artwork.</summary>
    ImmutableArray<PerspectiveFloorScenery> FlownFloorWalls { get; }

    /// <summary>The typewriter writing the foreground's warning, or none while it writes nothing.</summary>
    EscapeTypewriterState? ForegroundTypewriter { get; }

    /// <summary>The beam cone the last frame boundary latched, or none while no beam shows.</summary>
    MorphBallEyeBeamCone? LatchedBeamCone { get; }
}
