namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// The pose an animation's pose-transition cue names for Samus (#2562), which outranks every other pose transition: the
/// update's pose handling, after the animation, installs it. Installing a pose, a scripted pose and Draygon's grab forget it.
/// </summary>
/// <remarks>
/// Evidence: the game's pose-transition animation cue writes the prospective pose its pose handling gives precedence to,
/// the pose handling after the animation installs it, and pose initialization clears it.
/// Lifetime: Samus's.
/// </remarks>
/// <typeparam name="TPose">Samus's pose.</typeparam>
[CompatibilityService("an animation's pose-transition cue names the pose her pose handling installs")]
public sealed class AnimationCuePose<TPose>
    where TPose : struct
{
    /// <summary>The pose the cue named, or null when no cue named one.</summary>
    public TPose? Named { get; private set; }

    /// <summary>The animation's cue names <paramref name="pose"/>.</summary>
    internal void Name(TPose pose) => Named = pose;

    /// <summary>A pose installation, a scripted pose or Draygon's grab forgets the named pose.</summary>
    internal void Forget() => Named = null;
}
