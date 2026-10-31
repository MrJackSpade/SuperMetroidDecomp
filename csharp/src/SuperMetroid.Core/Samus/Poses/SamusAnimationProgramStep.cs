namespace SuperMetroid.Core.Samus.Poses;

/// <summary>
/// One numbered frame of a Samus animation program: a picture held for a delay (<see cref="SamusAnimationDelay"/>), or
/// an instruction the animation runs when it reaches the frame (<see cref="SamusAnimationInstruction"/>).
/// </summary>
internal abstract record SamusAnimationProgramStep;
