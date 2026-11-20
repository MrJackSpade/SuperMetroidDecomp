namespace SuperMetroid.Core.Samus;

/// <summary>
/// The holds each owner of Samus's movement ends as it takes her movement or gives it back to her pose
/// (<see cref="SamusMovementHandOver"/>), in the order it ends them. An owner not listed ends no other hold that way, and
/// handing her movement over through it fails.
/// </summary>
internal static class SamusMovementOwnerDefinitions
{
    /// <summary>
    /// The holds each owner ends as it takes her movement. The windup and Draygon's grab take her from Crystal Flash, and a
    /// shinespark Draygon grabs keeps flying beneath the hold until the release ends it. Crystal Flash takes her from every
    /// other hold that keeps its state: a shinespark, Draygon's hold and the drain. Ceres Ridley's shove, a bomb jump, the
    /// drained fall, the X-ray scope and an elevator ride take her from a shinespark and from Crystal Flash: an uncrashed
    /// spark keeps its boost as the Blue Suit, and Crystal Flash keeps its suit colouring, but neither moves her again.
    /// Her pose, the grapple and a knockback take her movement by their own state.
    /// </summary>
    internal static readonly IReadOnlyDictionary<SamusMovementOwner, SamusMovementHoldEnd[]> HoldsEndedTaking =
        new Dictionary<SamusMovementOwner, SamusMovementHoldEnd[]>
        {
            [SamusMovementOwner.Shinespark] = [SamusMovementHoldEnd.CrystalFlash],
            [SamusMovementOwner.DraygonHold] = [SamusMovementHoldEnd.CrystalFlash],
            [SamusMovementOwner.CrystalFlash] = [SamusMovementHoldEnd.Shinespark, SamusMovementHoldEnd.DraygonHold, SamusMovementHoldEnd.Drain],
            [SamusMovementOwner.CeresRidleyEjection] = [SamusMovementHoldEnd.Shinespark, SamusMovementHoldEnd.CrystalFlash],
            [SamusMovementOwner.BombJump] = [SamusMovementHoldEnd.Shinespark, SamusMovementHoldEnd.CrystalFlash],
            [SamusMovementOwner.Drained] = [SamusMovementHoldEnd.Shinespark, SamusMovementHoldEnd.CrystalFlash],
            [SamusMovementOwner.Xray] = [SamusMovementHoldEnd.Shinespark, SamusMovementHoldEnd.CrystalFlash],
            [SamusMovementOwner.Elevator] = [SamusMovementHoldEnd.Shinespark, SamusMovementHoldEnd.CrystalFlash],
        };

    /// <summary>
    /// The holds each owner ends as it gives her movement back to her pose. Draygon's release (and death, which runs it
    /// even when Samus was never held) ends a spark or Crystal Flash the hold kept beneath it, without resetting the spark's
    /// boost or colouring. A knockback's end takes her from a shinespark windup begun after the end was decided but leaves
    /// a stored shine stored. The end of the shinespark's crash frees a Samus drained mid-crash. The X-ray scope's closing
    /// ends an active spark X-Mode left holding her, the spark's colouring timer surviving as the Blue Suit. The other
    /// owners give her movement back by their own state alone.
    /// </summary>
    internal static readonly IReadOnlyDictionary<SamusMovementOwner, SamusMovementHoldEnd[]> HoldsEndedReturning =
        new Dictionary<SamusMovementOwner, SamusMovementHoldEnd[]>
        {
            [SamusMovementOwner.DraygonHold] = [SamusMovementHoldEnd.Shinespark, SamusMovementHoldEnd.CrystalFlash],
            [SamusMovementOwner.Knockback] = [SamusMovementHoldEnd.ShinesparkMovingHer],
            [SamusMovementOwner.Shinespark] = [SamusMovementHoldEnd.Drain],
            [SamusMovementOwner.Xray] = [SamusMovementHoldEnd.Shinespark],
        };
}
