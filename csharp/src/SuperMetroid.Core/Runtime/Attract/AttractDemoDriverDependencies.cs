using SuperMetroid.Core.Input;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.World;

namespace SuperMetroid.Core.Runtime.Attract;

/// <summary>The collaborators a <see cref="AttractDemoDriver"/> is constructed with, built once by the owner that creates it (#621).</summary>
internal sealed partial record AttractDemoDriverDependencies
{
    /// <summary>The attract demo's session: its scripted input and the scope that substitutes it for the player's controller.</summary>
    public required AttractDemoSession AttractSession { get; init; }

    /// <summary>The player's gameplay actions as the frame boundary latched them.</summary>
    public required GameplayActionInput PlayerActions { get; init; }

    /// <summary>Builds the demo's initial HUD from its record's health and ammunition.</summary>
    public required GameplayHudSetup HudSetup { get; init; }

    /// <summary>The accepted frame boundary.</summary>
    public required AcceptedFrameBoundary FrameBoundary { get; init; }

    /// <summary>Samus's frame state, which receives the demo's Samus and the pose or shinespark launch it selects.</summary>
    public required SamusFrameState Player { get; init; }

    /// <summary>The active room: the demo room's viewport, camera scrolls and load station.</summary>
    public required ActiveRoomState Room { get; init; }

    /// <summary>Sets up the demo's Samus and loads its room and door from its camera position.</summary>
    public required RoomLoader RoomLoader { get; init; }

    /// <summary>The game state the demo seeds its progression into, without reading or writing a save.</summary>
    public required SharedGameState System { get; init; }
}
