namespace SuperMetroid.Core.Samus.Poses;

/// <summary>
/// One step of a Samus animation: a picture held for a delay (<see cref="SamusAnimationDelay"/>), or
/// a cue the animation runs when it reaches the frame (<see cref="SamusAnimationCue"/>).
/// </summary>
internal abstract partial record SamusPoseAnimationStep;
