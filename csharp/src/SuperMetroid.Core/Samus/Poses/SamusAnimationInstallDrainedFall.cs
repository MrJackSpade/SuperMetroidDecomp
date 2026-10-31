namespace SuperMetroid.Core.Samus.Poses;

/// <summary>
/// Hands a drained Samus's movement to the drained falling handler, then continues the animation with the next frame.
/// Only the drained crouching poses' programs hold it.
/// </summary>
internal sealed record SamusAnimationInstallDrainedFall : SamusAnimationInstruction;
