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
    internal static TorizoPosture Posture(RoomEnemyPlace torizo) =>
        Ensure.NotNull(torizo).RequireSpeciesState<TorizoEnemyState>().Posture;

    /// <summary>Whether the Torizo faces right.</summary>
    internal static bool FacesRight(RoomEnemyPlace torizo) => Posture(torizo).Facing == HorizontalDirection.Right;

    /// <summary>Tests whether the Torizo faces the side Samus is on, as its attack branches do.</summary>
    internal static bool FacesSamus(RoomEnemyPlace torizo, SamusState samus)
    {
        // The awakening facing check compares the facing with whether the enemy's X minus Samus's is negative, rather
        // than the reverse difference the collision tests take.
        return FacesRight(torizo) == torizo.Position.X.Whole < Ensure.NotNull(samus).Position.X.Whole;
    }
}
