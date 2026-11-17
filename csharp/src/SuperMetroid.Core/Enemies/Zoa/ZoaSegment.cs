namespace SuperMetroid.Core.Enemies.Zoa;

/// <summary>
/// Zoa's shooting and rising animations (#641): one segment per animation a spawn, branch or wake enters, in authored order.
/// Their steps are <see cref="ZoaAnimations"/>.
/// </summary>
public enum ZoaSegment
{
    /// <summary>
    /// Darting left: its tail tucked as it launches slowly, kicking as it speeds up and stretched out at full speed,
    /// repeating.
    /// </summary>
    FacingLeftShooting,
    /// <summary>Swimming up facing left, its legs paddling in and out, repeating.</summary>
    FacingLeftRising,
    /// <summary>
    /// Darting right: its tail tucked as it launches slowly, kicking as it speeds up and stretched out at full speed,
    /// repeating.
    /// </summary>
    FacingRightShooting,
    /// <summary>Swimming up facing right, its legs paddling in and out, repeating.</summary>
    FacingRightRising,
}
