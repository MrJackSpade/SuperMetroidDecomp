using SuperMetroid.Core.Samus.Poses;

namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>Shared constants used by shinespark.</summary>
public static class ShinesparkDefinitions
{
    /// <summary>The shinespark end check's minimum energy to sustain a shinespark.</summary>
    public const int MinimumSustainingEnergy = 30;

    /// <summary>How long a stored shine lasts, in updates, before it fades unused.</summary>
    public const int StoredShineFrames = 180;

    /// <summary>The updates after a shine is stored at which its ready sound plays.</summary>
    public const int StoredShineSoundDelayFrames = 10;

    /// <summary>How long the windup holds Samus before she launches, in updates.</summary>
    public const int WindupFrames = 30;

    /// <summary>The acceleration a shinespark windup starts its upward motion with: seven pixels an update.</summary>
    internal static readonly SignedSixteenSixteen WindupInitialVerticalAcceleration = SignedSixteenSixteen.FromParts(7, 0);

    /// <summary>How long the shinesparking shade lasts from the windup's start if nothing renews it.</summary>
    public const int WindupShadeFrames = 60;

    /// <summary>How long the shinesparking shade outlasts each update that moves the spark or runs its crash.</summary>
    public const int SparkShadeFrames = 15;

    /// <summary>How long the shinesparking shade outlasts the crash's end: it fades on the next update.</summary>
    public const int FinishedShadeFrames = 1;

    /// <summary>How long the crash's echoes stay on Samus once its orbit has drawn in, in updates.</summary>
    public const int CrashHoldFrames = 30;

    /// <summary>How far the crash's orbiting copies move out from or in to Samus on each update, in pixels.</summary>
    public const int CrashRadiusStep = 4;

    /// <summary>
    /// The radius from which the crash's next expansion starts the orbit: the copies orbit at
    /// <see cref="CrashRadiusStep"/> pixels beyond it.
    /// </summary>
    public const int CrashOrbitStartRadius = 12;

    /// <summary>The spark each directional shinespark pose launches: horizontal, vertical or diagonal, facing either way.</summary>
    public static readonly IReadOnlyDictionary<SamusPoseId, ShinesparkPhase> LaunchPhases = new Dictionary<SamusPoseId, ShinesparkPhase>
    {
        [SamusPoseId.ShinesparkHorizontalRightPose] = ShinesparkPhase.Horizontal,
        [SamusPoseId.ShinesparkHorizontalLeftPose] = ShinesparkPhase.Horizontal,
        [SamusPoseId.ShinesparkVerticalRightPose] = ShinesparkPhase.Vertical,
        [SamusPoseId.ShinesparkVerticalLeftPose] = ShinesparkPhase.Vertical,
        [SamusPoseId.ShinesparkDiagonalRightPose] = ShinesparkPhase.Diagonal,
        [SamusPoseId.ShinesparkDiagonalLeftPose] = ShinesparkPhase.Diagonal,
    };

    /// <summary>The steps of a 256-step turn the crash's orbiting copies turn by on each orbiting update.</summary>
    public const int CrashOrbitTurnSteps = 4;

    /// <summary>The run speed the windup gives Samus, which the launch carries: eight pixels an update.</summary>
    public static readonly UnsignedSixteenSixteen WindupRunSpeed = new(8, 0);

    /// <summary>The fastest a horizontal or diagonal spark runs, in whole pixels an update; its fraction is dropped there.</summary>
    public const int MostRunSpeed = 15;

    /// <summary>The most whole pixels a vertical or diagonal spark rises in an update, its fraction kept; its speed keeps growing.</summary>
    public const int MostRiseStep = 14;

    /// <summary>
    /// The most whole pixels a rising spark's move, outside forces included, carries Samus up toward terrain and enemies in
    /// an update, its fraction kept.
    /// </summary>
    public const int MostRiseCollisionStep = 15;

    /// <summary>The farthest, in pixels, the previous horizontal position may lag behind Samus after a spark's horizontal move.</summary>
    public const int MostPreviousXLag = 15;

    /// <summary>The farthest, in pixels, the previous vertical position may lie below Samus after a spark's vertical move.</summary>
    public const int MostPreviousYLag = 14;

    /// <summary>The hurt-flash count a moving spark holds, past the flash's colours and its damage sound.</summary>
    public const int MovingSparkHurtFlashCount = 8;

    /// <summary>Pixels a released crash echo's radius grows by on each update.</summary>
    public const int EchoRadiusStep = 8;

    /// <summary>How far right of or below the camera's corner a released echo may lie, in pixels, before it leaves the screen.</summary>
    public const int EchoScreenReach = 256;
}
