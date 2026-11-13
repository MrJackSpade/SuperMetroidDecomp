namespace SuperMetroid.Core.Enemies.Zoa;

/// <summary>
/// Zoa's shooting and rising animations as typed programs (#641), one per animation a spawn, branch or resumed sleep enters, in authored order.
/// Their steps are <see cref="ZoaAnimations"/>.
/// </summary>
internal enum ZoaSegment
{
    /// <summary>The facing left shooting animation.</summary>
    FacingLeftShooting,
    /// <summary>The facing left rising animation.</summary>
    FacingLeftRising,
    /// <summary>The facing right shooting animation.</summary>
    FacingRightShooting,
    /// <summary>The facing right rising animation.</summary>
    FacingRightRising,
}
