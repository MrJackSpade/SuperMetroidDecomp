using F = SuperMetroid.Core.Enemies.Artwork.EnemyFrameId;

namespace SuperMetroid.Core.Enemies.Chozo;

/// <summary>
/// The poses of one Chozo statue's hand as it takes Samus, flexes its fingers and lets her go (#2477), and the way it carries
/// her. Each statue's grab, flex and release are written once over its hand, as a Work Robot's strides are written once over
/// its feet (#2451).
/// </summary>
/// <param name="Carries">The way the hand carries Samus as it grips and releases her.</param>
/// <param name="HandsCupped">Its hands cupped, where Samus sits.</param>
/// <param name="HandReaching">Its hand reaching.</param>
/// <param name="HandOutstretched">Its hand outstretched.</param>
/// <param name="HandOpen">Its hand open.</param>
/// <param name="HandOpenEyeGlowing">Its hand open, its eye glowing.</param>
/// <param name="FingersCurling">Its fingers curling.</param>
/// <param name="FingersCurled">Its fingers curled.</param>
/// <param name="FingersClenched">Its fingers clenched.</param>
internal sealed record ChozoStatueHand(
    HorizontalDirection Carries, F HandsCupped, F HandReaching, F HandOutstretched, F HandOpen, F HandOpenEyeGlowing,
    F FingersCurling, F FingersCurled, F FingersClenched)
{
    /// <summary>The Lower Norfair statue's hand, which carries Samus right.</summary>
    internal static ChozoStatueHand LowerNorfair { get; } = new(
        HorizontalDirection.Right, F.ChozoStatueLowerNorfairHandsCupped, F.ChozoStatueLowerNorfairHandReaching,
        F.ChozoStatueLowerNorfairHandOutstretched, F.ChozoStatueLowerNorfairHandOpen, F.ChozoStatueLowerNorfairHandOpenEyeGlowing,
        F.ChozoStatueLowerNorfairFingersCurling, F.ChozoStatueLowerNorfairFingersCurled, F.ChozoStatueLowerNorfairFingersClenched);

    /// <summary>The Wrecked Ship statue's hand, which carries Samus left.</summary>
    internal static ChozoStatueHand WreckedShip { get; } = new(
        HorizontalDirection.Left, F.ChozoStatueWreckedShipHandsCupped, F.ChozoStatueWreckedShipHandReaching,
        F.ChozoStatueWreckedShipHandOutstretched, F.ChozoStatueWreckedShipHandOpen, F.ChozoStatueWreckedShipHandOpenEyeGlowing,
        F.ChozoStatueWreckedShipFingersCurling, F.ChozoStatueWreckedShipFingersCurled, F.ChozoStatueWreckedShipFingersClenched);
}
