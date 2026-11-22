namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// A Super Missile's invisible link (#1923): its own update leaves it where its Super Missile placed it, and a spent link
/// ends every Super Missile shot, its own Super Missile among them.
/// </summary>
internal sealed partial class SuperMissileLinkHold : SamusStatefulShotBehavior<SuperMissileShotState>
{
    /// <inheritdoc/>
    internal override void Run(SamusShotUpdate update, SuperMissileShotState state) =>
        update.Shots.Motion.HoldSuperMissileLink(update.Shot);
}
