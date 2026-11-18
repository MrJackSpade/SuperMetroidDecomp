namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>The Power Bomb fuse the Chainsaw shot runs over itself (#2346), created when the shot is fired.</summary>
/// <remarks>
/// Where a fuse that has not burned out comes from (#2467): the game keeps the Chainsaw's fuse in the word each shot
/// place keeps for its own projectile, which only a projectile's deletion and the clearing of Samus's shots zero, and
/// firing leaves it as it finds it. A shot fired into a free place finds it zero, so its fuse starts burned out. A beam
/// fired while every place is held and the shot count is still below the place count (<see cref="SamusShotCount"/>)
/// lands in the last place over the shot holding it, and a Chainsaw shot fired so starts from the word that shot left
/// there: a Missile's or Super Missile's ignition and link, or a crash echo's angle. That fuse counts down to the fast
/// stage (<see cref="ChainsawFuseAnimationShift{TCursor}"/>) and to the Power Bomb blast, so both stay.
/// Beam firing here refuses a shot with every place held, so that path is not reproduced yet and every fuse starts at
/// zero.
/// </remarks>
internal sealed class ChainsawFuseShotState : SamusShotKindState
{
    /// <summary>
    /// The updates the fuse has left, zero when the shot is fired into a free place: a fuse already burned out (#2458), so
    /// the shot is deleted on its first update unless a Power Bomb's blast is under way.
    /// </summary>
    internal int FuseUpdatesLeft
    {
        get;
        set => field = Ensure.AtLeastZero(value);
    }
}
