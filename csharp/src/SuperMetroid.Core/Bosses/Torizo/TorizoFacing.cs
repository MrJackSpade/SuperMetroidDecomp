using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Samus;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>
/// Which way a Torizo faces and which leg it has forward, read from the state its entity owns (#2030), and the side of it
/// Samus is on, as its attack branches test it.
/// </summary>
internal static class TorizoFacing
{
    /// <summary>The Torizo's posture: its facing, its forward leg and whether it is turning around.</summary>
    internal static TorizoPostureFlags Posture(RoomEnemyPlace torizo) =>
        Ensure.NotNull(torizo).RequireSpeciesState<TorizoEnemyState>().Posture;

    /// <summary>Whether the Torizo faces right.</summary>
    internal static bool FacesRight(RoomEnemyPlace torizo) => Posture(torizo).HasFlag(TorizoPostureFlags.FacingRight);

    /// <summary>
    /// The posture a step <paramref name="direction"/> on <paramref name="foot"/> leaves (#2165): facing that way, with the
    /// opposite leg forward when the foot is not on that side. It replaces all three latches.
    /// </summary>
    internal static TorizoPostureFlags AfterStep(HorizontalDirection direction, HorizontalDirection foot) =>
        (direction == HorizontalDirection.Right ? TorizoPostureFlags.FacingRight : TorizoPostureFlags.None)
        | (foot != direction ? TorizoPostureFlags.OppositeLegForward : TorizoPostureFlags.None);

    /// <summary>
    /// Whether the step that left <paramref name="posture"/> was on the right foot (#2175): the foot on the facing side, or
    /// the other with the opposite leg forward.
    /// </summary>
    internal static bool SteppedOnRightFoot(TorizoPostureFlags posture) =>
        posture.HasFlag(TorizoPostureFlags.FacingRight) != posture.HasFlag(TorizoPostureFlags.OppositeLegForward);

    /// <summary>Tests whether the Torizo faces the side Samus is on, as its attack branches do.</summary>
    internal static bool FacesSamus(RoomEnemyPlace torizo, SamusState samus)
    {
        // The awakening facing check compares the facing bit with whether
        // the enemy's X minus Samus's is negative, rather than the reverse difference the walkers test.
        return FacesRight(torizo) == CoordinateAxis.Difference(torizo.Position.X.Whole, Ensure.NotNull(samus).Position.X.Whole) < 0;
    }
}
