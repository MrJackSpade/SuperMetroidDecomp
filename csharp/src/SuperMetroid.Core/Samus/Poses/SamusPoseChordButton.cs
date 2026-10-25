using SuperMetroid.Core.Input;

namespace SuperMetroid.Core.Samus.Poses;

/// <summary>One button of an authored canonical pose-input chord and the controller button that stands for it.</summary>
/// <param name="Chord">The canonical chord button the transition tables name.</param>
/// <param name="Button">The controller button that satisfies it.</param>
internal readonly record struct SamusPoseChordButton(CanonicalPoseButtons Chord, ControllerButton Button);
