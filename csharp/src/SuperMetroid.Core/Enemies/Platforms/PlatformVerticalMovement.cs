namespace SuperMetroid.Core.Enemies.Platforms;

/// <summary>
/// A platform's vertical movement, chosen every enemy frame by whether Samus rides it.
/// </summary>
public enum PlatformVerticalMovement
{
    /// <summary>No rider: accelerates upward until it reaches its target height or collides, then holds still vertically.</summary>
    Rising,
    /// <summary>Samus rides it, by the asymmetric Samus-above-platform overlap test: accelerates downward and carries Samus by its whole-pixel X and Y movement.</summary>
    Sinking,
}
