namespace SuperMetroid.Core.Samus.Poses;

/// <summary>
/// The instruction that closes a run of delays in a Samus animation program: where the animation goes next, or which
/// transitional pose it publishes. Each kind is its own record; the animation interpreter dispatches on the kind.
/// </summary>
internal abstract record SamusAnimationInstruction : SamusAnimationProgramStep;
