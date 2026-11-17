namespace SuperMetroid.Core.Enemies.WreckedShip;

/// <summary>
/// The Wrecked Ship ghost's (Coven) floating loop as a typed animation (#641): one segment per animation a spawn, branch or wake enters, in authored order.
/// Their steps are <see cref="WreckedShipGhostAnimations"/>.
/// </summary>
internal enum WreckedShipGhostSegment
{
    /// <summary>Floating, its wisps at rest, curling and stretched, repeating.</summary>
    Floating,
}
