namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// The charging beam's sound, which resumes at the post-draw check after the one that ended what interrupted it (#2562):
/// a spin jump's end, or the hurt flash's recovery, arms the resumption while Samus holds a charge, and the next post-draw
/// check takes it, queueing the sound if Shoot is still held. Firing, a grapple's start and the drained fall cancel it.
/// </summary>
/// <remarks>
/// Evidence: the game sets its resume-charging-beam-sound flag where a spin jump ends and where the hurt flash recovers its
/// interrupted sounds, its post-draw sound check reads and clears the flag on its next run and queues the sound only while
/// Shoot is held, and firing a beam clears it.
/// Lifetime: Samus's.
/// </remarks>
[CompatibilityService("the charging beam's sound resumes at the post-draw check after the one that ended its interruption")]
public sealed partial class ChargingSoundResumption
{
    /// <summary>Whether the resumption is armed for the next post-draw check.</summary>
    internal bool IsArmed { get; set; }

    /// <summary>A spin's end or the hurt flash's recovery arms the resumption for the next post-draw check.</summary>
    internal void Arm() => IsArmed = true;

    /// <summary>Firing, a grapple's start or the drained fall cancels the resumption.</summary>
    internal void Cancel() => IsArmed = false;

    /// <summary>The post-draw check takes the resumption: whether it was armed, which it then no longer is.</summary>
    internal bool Take()
    {
        bool armed = IsArmed;
        IsArmed = false;
        return armed;
    }
}
