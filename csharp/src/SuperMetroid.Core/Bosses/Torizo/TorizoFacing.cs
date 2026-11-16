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
    internal static TorizoPostureFlags Posture(RoomEnemySlot torizo) =>
        Ensure.NotNull(torizo).RequireSpeciesState<TorizoEnemyState>().Posture;

    /// <summary>Whether the Torizo faces right.</summary>
    internal static bool FacesRight(RoomEnemySlot torizo) => Posture(torizo).HasFlag(TorizoPostureFlags.FacingRight);

    /// <summary>Tests whether the Torizo faces the side Samus is on, as its attack branches do.</summary>
    internal static bool FacesSamus(RoomEnemySlot torizo, SamusState samus)
    {
        // The awakening facing check compares the facing bit with whether
        // the enemy's X minus Samus's is negative, rather than the reverse difference the walkers test.
        return FacesRight(torizo) == CoordinateAxis.Difference(torizo.Position.X.Whole, Ensure.NotNull(samus).Position.X.Whole) < 0;
    }
}
