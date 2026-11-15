using SuperMetroid.Core.Input;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Gameplay;

namespace SuperMetroid.Core.World;

/// <summary>The collaborators a <see cref="RoomObjectFramePhase"/> is constructed with, built once by the owner that creates it (#621).</summary>
internal sealed record RoomObjectFramePhaseDependencies
{
    /// <summary>The player's gameplay actions as the frame boundary latched them.</summary>
    public required GameplayActionInput PlayerActions { get; init; }

    /// <summary>The physical controller buttons, which the message box's own reads continue from.</summary>
    public required ControllerInputState PlayerButtons { get; init; }

    /// <summary>The options whose controller bindings the message panels name.</summary>
    public required GameplayOptions Options { get; init; }

    /// <summary>Enemy room effects.</summary>
    public required EnemyRoomEffects EnemyRoomEffects { get; init; }

    /// <summary>Freeze.</summary>
    public required GameplayTimeFreeze Freeze { get; init; }

    /// <summary>Message box.</summary>
    public required GameplayMessageBoxState MessageBox { get; init; }

    /// <summary>Message boxes.</summary>
    public required MessageBoxFlow MessageBoxes { get; init; }

    /// <summary>Player.</summary>
    public required SamusFrameState Player { get; init; }


    /// <summary>Room.</summary>
    public required ActiveRoomState Room { get; init; }

    /// <summary>Suit pickup.</summary>
    public required SamusSuitPickupState SuitPickup { get; init; }

}
