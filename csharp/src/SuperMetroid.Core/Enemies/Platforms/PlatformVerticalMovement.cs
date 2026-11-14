namespace SuperMetroid.Core.Enemies.Platforms;

/// <summary>
/// Two-phase vertical movement selected every enemy frame by the platform/Samus
/// overlap test.
/// </summary>
public enum PlatformVerticalMovement
{
    /// <summary>Phase zero, selected without an accepted rider overlap; accelerates upward until the target-height or collision condition leaves the platform vertically still.</summary>
    Rising = 0,
    /// <summary>Phase one, selected by the asymmetric Samus-above-platform overlap test; accelerates downward and publishes accepted whole-pixel X/Y movement as Samus's extra displacement.</summary>
    Sinking = 1,
}
