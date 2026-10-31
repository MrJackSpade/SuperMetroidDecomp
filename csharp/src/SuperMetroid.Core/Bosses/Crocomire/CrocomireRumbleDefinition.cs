namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>
/// One target in Crocomire's hidden-wall rumble schedule. A negative target also carries the cooldown and approach
/// delta loaded when its oscillation completes; a return target carries zeros there, which nothing reads.
/// </summary>
/// <param name="TargetYOffset">The vertical wall offset the rumble moves toward, in pixels.</param>
/// <param name="Cooldown">For a negative target, the oscillations the next phase repeats before moving on.</param>
/// <param name="Delta">For a negative target, the pixels per frame the next phase moves.</param>
internal readonly record struct CrocomireRumbleDefinition(
    int TargetYOffset,
    int Cooldown,
    int Delta);
