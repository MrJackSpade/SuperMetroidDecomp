namespace SuperMetroid.Core.Enemies.Platforms;

/// <summary>
/// Two-entry vertical dispatcher selected every enemy frame by the platform/Samus
/// overlap test.
/// </summary>
public enum PlatformVerticalMovement
{
    /// <summary>Dispatcher index zero, selected without an accepted rider overlap; accelerates upward until the target-height or collision condition leaves the platform vertically still.</summary>
    Rising = 0,
    /// <summary>Dispatcher index one, selected by the asymmetric Samus-above-platform overlap test; accelerates downward and publishes accepted whole-pixel X/Y movement as Samus's extra displacement.</summary>
    Sinking = 1,
}
