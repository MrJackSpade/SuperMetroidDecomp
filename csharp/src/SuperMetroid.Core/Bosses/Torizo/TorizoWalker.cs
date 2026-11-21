using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>
/// How a Torizo walks in one of its states (#633, #1891, #2335, #2472): Bomb Torizo with its face, Bomb Torizo without it,
/// or Golden Torizo. Its walking steps carry it, and it names the turn away from the way the Torizo faces, which a wall in
/// its way starts and which its wait behind Samus ends in, and how long that wait lasts.
/// </summary>
/// <param name="TurnFacingRight">The turn a Torizo facing right turns about with.</param>
/// <param name="TurnFacingLeft">The turn a Torizo facing left turns about with.</param>
/// <param name="TurnDelay">The updates it waits before it turns toward a Samus behind it.</param>
internal sealed partial record TorizoWalker(TorizoAnimation TurnFacingRight, TorizoAnimation TurnFacingLeft, int TurnDelay)
{
    /// <summary>The turn <paramref name="torizo"/> turns about with, away from the way it faces.</summary>
    internal TorizoAnimation TurnAbout(RoomEnemyPlace torizo) =>
        TorizoFacing.FacesRight(torizo) ? TurnFacingRight : TurnFacingLeft;
}
