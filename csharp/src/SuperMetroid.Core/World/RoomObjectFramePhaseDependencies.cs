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

    /// <summary>Whether gameplay time is frozen, which holds the room objects.</summary>
    public required GameplayTimeFreeze Freeze { get; init; }

    /// <summary>The message box an item or station opens.</summary>
    public required GameplayMessageBoxState MessageBox { get; init; }

    /// <summary>What follows a message box closing, such as a save or a suit's transformation, which the room objects hand on.</summary>
    public required MessageBoxFlow MessageBoxes { get; init; }

    /// <summary>Samus, whom the room objects touch, and whether the X-ray scope holds the room objects still.</summary>
    public required SamusFrameState Player { get; init; }

    /// <summary>The suit transformation, which keeps a second suit from replacing one already running.</summary>
    public required SamusSuitPickupState SuitPickup { get; init; }
}
