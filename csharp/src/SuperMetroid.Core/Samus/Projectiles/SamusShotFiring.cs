namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// What firing a beam shot does once its muzzle collision has run (#1923, #2553): it gives the shot the behaviour it runs
/// (<see cref="SamusShotBehavior"/>), or, for a Spazer and Plasma shot whose effect lies outside the emulation scope limit,
/// refuses the shot (<see cref="ShotRefusedWhenFired"/>, #1500).
/// </summary>
internal abstract class SamusShotFiring
{
    /// <summary>Fires the shot <paramref name="held"/> holds.</summary>
    /// <param name="held">The fired shot.</param>
    internal abstract void Fire(SamusProjectileOccupant held);
}
