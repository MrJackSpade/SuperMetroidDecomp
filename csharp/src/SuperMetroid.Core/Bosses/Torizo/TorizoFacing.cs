using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Samus;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>
/// Which way a Torizo faces and which foot it leads with, read from the state its entity owns (#2030, #2237), and the side
/// of it Samus is on, as its attack branches test it.
/// </summary>
internal static class TorizoFacing
{
    /// <summary>The Torizo's posture: its facing, its leading foot and whether it is turning around.</summary>
    internal static TorizoPosture Posture(RoomEnemyPlace torizo, TorizoEnemyState torizoEnemyState) =>
        torizoEnemyState.Posture;

    /// <summary>Whether the Torizo faces right.</summary>
    internal static bool FacesRight(RoomEnemyPlace torizo, TorizoEnemyState torizoEnemyState) => Posture(torizo, torizoEnemyState).Facing == HorizontalDirection.Right;

    /// <summary>Tests whether the Torizo faces the side Samus is on, as its attack branches do.</summary>
    internal static bool FacesSamus(RoomEnemyPlace torizo, TorizoEnemyState torizoEnemyState, SamusState samus)
    {
        // A Torizo facing right faces Samus when she stands right of it; one facing left faces her otherwise, also when
        // she stands level with it.
        return FacesRight(torizo, torizoEnemyState) == torizo.Position.X.Whole < Ensure.NotNull(samus).Position.X.Whole;
    }
}
