namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// A Super Missile's invisible link (#1923): its own update leaves it where its Super Missile placed it, and a spent link
/// ends every Super Missile shot, its own Super Missile among them.
/// </summary>
/// <param name="state">The state the shot's kind keeps.</param>
internal sealed class SuperMissileLinkHold(SuperMissileShotState state) : SamusShotBehavior
{
    /// <inheritdoc/>
    internal override SamusShotKindState KindState => state;

    /// <inheritdoc/>
    internal override void Run(SamusShotUpdate update) =>
        update.Shots.Motion.HoldSuperMissileLink(update.Shot);
}
