using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Gameplay;
using SuperMetroid.Core.Samus;

namespace SuperMetroid.Core.World;

/// <summary>The collaborators a <see cref="SamusFramePhases"/> is constructed with, built once by the owner that creates it (#621).</summary>
internal sealed partial record SamusFramePhasesDependencies
{
    /// <summary>The attract demo's input, which replaces the player's while the demo plays Samus.</summary>
    public required IAttractSamusInput AttractDemo { get; init; }

    /// <summary>The player's gameplay actions as the frame boundary latched them.</summary>
    public required GameplayActionInput PlayerActions { get; init; }

    /// <summary>Whether gameplay time is frozen, which holds Samus's movement and animation.</summary>
    public required GameplayTimeFreeze Freeze { get; init; }

    /// <summary>The game's clock, whose elapsed updates a shinespark reads while it moves Samus.</summary>
    public required GameTimeState GameTime { get; init; }

    /// <summary>The game's frame counters, which time enemies, animations and blinking.</summary>
    public required FrameClock Clock { get; init; }

    /// <summary>The in-game options, whose controls Samus's pose changes read.</summary>
    public required GameplayOptions Options { get; init; }

    /// <summary>The runtime's assists; a shinespark reads whether Samus cannot die.</summary>
    public required ActiveAssists Assists { get; init; }

    /// <summary>Samus, her input and movement modes and what her update keeps.</summary>
    public required SamusFrameState Player { get; init; }

    /// <summary>The session's shared game state, which Samus's movement and shots consult.</summary>
    public required SharedGameState System { get; init; }

    /// <summary>Samus's passage through a door, during which her input handling and radius update do not run.</summary>
    public required DoorPassage DoorPassage { get; init; }
}
