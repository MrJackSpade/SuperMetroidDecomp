namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>Which of the two echoes a shinespark crash releases an echo is (#2553).</summary>
public enum ShinesparkCrashEchoRelease
{
    /// <summary>The first echo, whose flight leaves its height for the crash's orbit and clears it once it leaves the screen.</summary>
    First,
    /// <summary>The second echo, whose flight keeps its height.</summary>
    Second,
}
