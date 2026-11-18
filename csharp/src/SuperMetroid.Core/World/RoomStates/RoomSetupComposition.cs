using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.World.RoomStates;

/// <summary>
/// What a room's setup reaches as it composes its own parts into the room being loaded (#2485): once the room's effect is
/// loaded and before the room composes its main.
/// </summary>
[RoomScopeBoundary("A per-call input, built for one call and never kept.")]
internal sealed record RoomSetupComposition
{
    /// <summary>The room being loaded, which the setup composes its parts into.</summary>
    public required RoomScopeConstruction Scope { get; init; }

    /// <summary>The session's progress, whose defeated bosses the Ceres haze reads.</summary>
    public required SharedGameState System { get; init; }

    /// <summary>The room's effect, whose water the Tourian statues' room reads.</summary>
    public required RoomFxOverlayState FxOverlay { get; init; }

    /// <summary>Whether the room's load shows its first view at once, rather than scrolling it in through a door.</summary>
    public required bool DisplaysInitialViewport { get; init; }
}
