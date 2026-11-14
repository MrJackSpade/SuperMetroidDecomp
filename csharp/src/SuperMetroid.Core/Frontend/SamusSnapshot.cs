using SuperMetroid.Core.Escape;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Poses;

namespace SuperMetroid.Core.Frontend;

/// <summary>What Samus, the player's controller and the escape countdown are doing at one moment.</summary>
public sealed class SamusSnapshot
{
    /// <summary>Samus's pose.</summary>
    public required SamusPoseId Pose { get; init; }

    /// <summary>The step of the pose's animation the animation is on.</summary>
    public required int AnimationFrame { get; init; }

    /// <summary>Frames left before the animation's next step.</summary>
    public required int AnimationFramesRemaining { get; init; }

    /// <summary>The pose whose animation is playing, or null when no animation has started.</summary>
    public required SamusPoseId? AnimatedPose { get; init; }

    /// <summary>Samus's position in the room, in whole pixels.</summary>
    public required Position16<RoomPixels> Position { get; init; }

    /// <summary>The buttons held on the player's controller.</summary>
    public required ControllerButton HeldButtons { get; init; }

    /// <summary>The buttons newly pressed on the player's controller this update.</summary>
    public required ControllerButton NewlyPressedButtons { get; init; }

    /// <summary>Whether the game has taken control of Samus away from the player.</summary>
    public required bool InputLocked { get; init; }

    /// <summary>How far Samus is through being drained and getting up again.</summary>
    public required DrainedSamusPhase DrainedPhase { get; init; }

    /// <summary>How a drained Samus gets back up.</summary>
    public required DrainedGetUpCheck DrainedGetUp { get; init; }

    /// <summary>The escape countdown's phase.</summary>
    public required EscapeTimerPhase EscapeTimer { get; init; }

    /// <summary>The room's liquid or weather effect Samus moves in.</summary>
    public required RoomFxType Fx { get; init; }

    /// <summary>The medium Samus's physics currently use.</summary>
    public required SamusLiquidMedium Medium { get; init; }

    /// <summary>The water surface's height in the room, or none when the room has no water surface.</summary>
    public required int? WaterSurfaceY { get; init; }

    /// <summary>The lava or acid surface's height in the room, or none when the room has no lava or acid surface.</summary>
    public required int? LavaAcidSurfaceY { get; init; }
}
