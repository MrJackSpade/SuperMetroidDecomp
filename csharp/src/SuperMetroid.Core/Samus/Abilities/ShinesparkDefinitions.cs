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
}
