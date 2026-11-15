using SuperMetroid.Core.Input;
using SuperMetroid.Core.Gameplay;
using SuperMetroid.Core.Samus;

namespace SuperMetroid.Core.World;

/// <summary>The collaborators a <see cref="SamusPoseTransitionPhase"/> is constructed with, built by its <see cref="SamusFramePhases"/> owner.</summary>
internal sealed record SamusPoseTransitionPhaseDependencies
{

    /// <summary>The player's gameplay actions as the frame boundary latched them.</summary>
    public required GameplayActionInput PlayerActions { get; init; }
    /// <summary>Freeze.</summary>
    public required GameplayTimeFreeze Freeze { get; init; }
    /// <summary>The frame clock.</summary>
    public required FrameClock Clock { get; init; }
    /// <summary>Options.</summary>
    public required GameplayOptions Options { get; init; }
    /// <summary>Player.</summary>
    public required SamusFrameState Player { get; init; }
    /// <summary>Room.</summary>
    public required ActiveRoomState Room { get; init; }
}
