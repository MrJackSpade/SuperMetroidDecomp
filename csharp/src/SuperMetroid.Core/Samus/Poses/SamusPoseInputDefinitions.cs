using SuperMetroid.Core.Input;

namespace SuperMetroid.Core.Samus.Poses;

/// <summary>The buttons the pose input transitions are authored in.</summary>
internal static class SamusPoseInputDefinitions
{
    /// <summary>
    /// Each canonical chord button and the controller button that stands for it: the transition tables name the
    /// default action buttons (R and L aim, X shoots, A jumps) for their bindings.
    /// </summary>
    internal static readonly SamusPoseChordButton[] ChordButtons =
    [
        new(CanonicalPoseButtons.AimUp, ControllerButton.R),
        new(CanonicalPoseButtons.AimDown, ControllerButton.L),
        new(CanonicalPoseButtons.Shoot, ControllerButton.X),
        new(CanonicalPoseButtons.Jump, ControllerButton.A),
        new(CanonicalPoseButtons.Right, ControllerButton.Right),
        new(CanonicalPoseButtons.Left, ControllerButton.Left),
        new(CanonicalPoseButtons.Down, ControllerButton.Down),
        new(CanonicalPoseButtons.Up, ControllerButton.Up),
    ];
}
