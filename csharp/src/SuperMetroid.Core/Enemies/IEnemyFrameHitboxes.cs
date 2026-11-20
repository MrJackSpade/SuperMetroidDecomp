namespace SuperMetroid.Core.Enemies;

/// <summary>An enemy whose frame hitboxes are the ones the game defines rather than read from installed frames.</summary>
internal interface IEnemyFrameHitboxes
{
    /// <summary>
    /// The first of the occupant's hitboxes the probe overlaps, whose reaction runs, or none (#2558). Where the radius
    /// decides the contact instead, the reaction that runs is the one its definition names.
    /// </summary>
    /// <param name="probe">The colliding projectile's or Samus's edges, and whether the test selects the shot or the touch reaction.</param>
    /// <exception cref="InvalidDataException">The species' geometry does not cover the frame the occupant shows.</exception>
    EnemyHitboxContact FindHitboxReaction(EnemyHitboxProbe probe);
}
