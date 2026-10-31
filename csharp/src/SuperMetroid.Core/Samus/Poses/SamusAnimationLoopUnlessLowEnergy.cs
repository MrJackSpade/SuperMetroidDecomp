namespace SuperMetroid.Core.Samus.Poses;

/// <summary>
/// Restarts a healthy Samus's breathing from frame zero; below the low-energy threshold the animation continues with the
/// next frame, the faster drained breathing that follows in the same program.
/// </summary>
internal sealed record SamusAnimationLoopUnlessLowEnergy : SamusAnimationInstruction;
