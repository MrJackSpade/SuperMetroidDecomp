using SuperMetroid.Core.Areas.Tourian;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Scrolling;

namespace SuperMetroid.Core.World;

/// <summary>
/// The statue room's parts its statue sequence acts on (#2486), which the room's load hands it as it builds them: the
/// sequence reads the room it is built for, never the active room's views.
/// </summary>
[RoomScopeBoundary("A per-call input, built for one call and never kept.")]
internal sealed partial record TourianStatueSequenceRoom
{
    /// <summary>The statue room, whose statue the sequence lowers.</summary>
    public required TourianStatueRoom StatueRoom { get; init; }

    /// <summary>The entrance statue and the unlock effects in the statue room, which the statue animations drive.</summary>
    public required ITourianStatueActors Statue { get; init; }

    /// <summary>The room's level, where the way into Tourian opens.</summary>
    public required RoomLevelData Level { get; init; }

    /// <summary>The room's scroll grid, whose two screens the statues hold closed until the way opens.</summary>
    public required RoomScrollGrid Scrolls { get; init; }

    /// <summary>The frame each of the room's animations shows, which the statues' block animations select into.</summary>
    public required ArtworkFrameSelections ArtworkFrames { get; init; }

    /// <summary>The room's effect, whose earthquake sound the descent publishes.</summary>
    public required RoomFxOverlayState FxOverlay { get; init; }

    /// <summary>The room's colour effects, which the releases start.</summary>
    public required ColourEffectSystem ColourEffects { get; init; }

    /// <summary>The room's earthquake, which the descent shakes.</summary>
    public required RoomEarthquake Earthquake { get; init; }

    /// <summary>The room's room objects, which receive the Tourian access room object when the statues already stand open.</summary>
    public required RoomObjectSystem RoomObjects { get; init; }
}
