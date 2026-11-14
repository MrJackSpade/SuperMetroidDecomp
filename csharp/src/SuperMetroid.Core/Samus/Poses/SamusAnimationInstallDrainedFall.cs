namespace SuperMetroid.Core.Samus.Poses;

/// <summary>
/// Hands a drained Samus's movement to the drained fall, then continues the animation with the next frame.
/// Only the drained crouching poses' animations hold it.
/// </summary>
internal sealed record SamusAnimationInstallDrainedFall : SamusAnimationCue;
