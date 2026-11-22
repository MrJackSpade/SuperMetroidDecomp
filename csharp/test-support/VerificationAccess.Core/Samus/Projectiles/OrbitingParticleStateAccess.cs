namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// Measures how a special attack particle turns round its circle (#2554): a check reads the change the particle's own turn
/// makes to its angle, not the turn it holds.
/// </summary>
internal static class OrbitingParticleStateAccess
{
    /// <summary>The turn a particle makes.</summary>
    extension(OrbitingParticleState particle)
    {
        /// <summary>
        /// The change the particle's turn round its circle makes to its angle on its next update: its angle after one turn
        /// less its angle before, which it is then put back to.
        /// </summary>
        internal TrigonometryAngle MeasuredTurn
        {
            get
            {
                TrigonometryAngle before = particle.Angle;
                particle.Turn();
                TrigonometryAngle turn = particle.Angle - before;
                particle.Angle = before;
                return turn;
            }
        }
    }
}
