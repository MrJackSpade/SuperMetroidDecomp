namespace SuperMetroid.Core.Enemies.Platforms;

/// <summary>
/// Native two-entry vertical dispatcher selected every enemy frame by the platform/Samus
/// overlap test at <c>$A0:ABE7</c>.
/// </summary>
public enum PlatformVerticalMovement : ushort
{
    /// <summary>Dispatcher index zero, selected without an accepted rider overlap; accelerates upward until the target-height or collision condition leaves the platform vertically still.</summary>
    Rising = 0,
    /// <summary>Dispatcher index one, selected by the asymmetric Samus-above-platform overlap test; accelerates downward and publishes accepted whole-pixel X/Y movement as Samus's extra displacement.</summary>
    Sinking = 1,
}
