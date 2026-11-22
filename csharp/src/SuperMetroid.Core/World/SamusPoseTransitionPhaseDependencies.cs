using SuperMetroid.Core.Input;
using SuperMetroid.Core.Gameplay;
using SuperMetroid.Core.Samus;

namespace SuperMetroid.Core.World;

/// <summary>The collaborators a <see cref="SamusPoseTransitionPhase"/> is constructed with, built by its <see cref="SamusFramePhases"/> owner.</summary>
internal sealed partial record SamusPoseTransitionPhaseDependencies
{
    /// <summary>The player's gameplay actions as the frame boundary latched them.</summary>
    public required GameplayActionInput PlayerActions { get; init; }
    /// <summary>Whether gameplay time is frozen, which holds Samus's pose changes.</summary>
    public required GameplayTimeFreeze Freeze { get; init; }
    /// <summary>The game's frame counters, which time enemies, animations and blinking.</summary>
    public required FrameClock Clock { get; init; }
    /// <summary>The in-game options, whose controls Samus's pose changes read.</summary>
    public required GameplayOptions Options { get; init; }
    /// <summary>Samus, her input and movement modes and her pose.</summary>
    public required SamusFrameState Player { get; init; }
}
