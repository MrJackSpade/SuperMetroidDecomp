using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>Bomb and Golden Torizo's hitboxes, touch and hurt, and what Samus's weapons do to each of them (#2240).</summary>
[RoomLifetime]
internal sealed class TorizoCombat
{
    /// <summary>Applies shot, bomb, and touch damage through the shared collision rules.</summary>
    private readonly EnemyCollision _collision;
    /// <summary>Bomb and Golden Torizo's body, which shows the Torizo's colouring and drips its low-health drool.</summary>
    private readonly TorizoBody _body;

    /// <summary>Builds the component from the collaborators it uses.</summary>
    internal TorizoCombat(EnemyCollision collision, TorizoBody body)
    {
        _collision = collision;
        _body = body;
    }

    /// <summary>
    /// Runs an already-colliding normal bomb through Bomb Torizo's hitbox reactions (#2240). The frame-hitbox collision has
    /// already set the bomb's explosion direction; this owns only the reaction and its common no-death damage. Bomb
    /// Torizo's stand-up and sit-down frames take no bomb at all, and while its animation is locked it takes none either.
    /// </summary>
    /// <param name="c">The bomb contact.</param>
    /// <param name="state">The Torizo's state.</param>
    internal void ReactToBombTorizoBomb(EnemyBombContext c, TorizoEnemyState state)
    {
        RoomEnemyPlace torizo = c.Enemy;
        if (c.SelectedShot == TorizoReactions.StandUpSitDownShot)
            return;
        // The reaction returns during the hurt flash, and rejects the bomb while the animation is locked.
        if (torizo.HurtFlash.IsRunning || state.AnimationLocked)
            return;
        DamageByNormalBomb(c.Occupant, state, c.PlacedBomb);
    }

    /// <summary>
    /// Runs an already-colliding normal bomb through Golden Torizo's hitbox reactions (#2240). Its stand-up and sit-down
    /// frames take common damage outside the hurt flash while its animation is unlocked, without arming its counterattack;
    /// its other frames take common damage outside the flash, a bomb first arming its stun while the animation is unlocked.
    /// </summary>
    /// <param name="c">The bomb contact.</param>
    /// <param name="state">The Torizo's state.</param>
    internal void ReactToGoldenTorizoBomb(EnemyBombContext c, TorizoEnemyState state)
    {
        RoomEnemyPlace torizo = c.Enemy;
        if (c.SelectedShot == TorizoReactions.StandUpSitDownShot)
        {
            if (torizo.HurtFlash.IsRunning || state.AnimationLocked)
                return;
            DamageByNormalBomb(c.Occupant, state, c.PlacedBomb);
            return;
        }
        if (torizo.HurtFlash.IsRunning)
            return;
        // A bomb is neither Missile nor Super Missile, so it stuns him before the common damage, whether or not he has
        // caught a Super Missile; while his animation is locked it does the common damage alone.
        if (!state.AnimationLocked)
            state.Stunned = true;
        DamageByNormalBomb(c.Occupant, state, c.PlacedBomb);
    }

    /// <summary>Applies a normal bomb's common no-death damage to the Torizo, starting its death once its health is gone.</summary>
    private void DamageByNormalBomb(RoomEnemyOccupant torizo, TorizoEnemyState state, SamusBombOccupant bomb)
    {
        _collision.ApplyNormalBombDamage(torizo, bomb);
        if (torizo.Place.Health == 0)
            BeginTorizoDeath(torizo.Place, state);
    }

    /// <summary>Shows Golden Torizo's body and belly in the colours of the health band <paramref name="health"/> is in.</summary>
    private static void ShowGoldenTorizoHealthShade(RoomEnemyPlace torizo, TorizoEnemyState state, int health) =>
        TorizoBody.ShowLook(torizo, state, TorizoLook.GoldenHealth(GoldenTorizoHealthBand.Of(Ensure.AtLeastZero(health))));

    /// <summary>Runs Bomb Torizo's hurt behaviour (#2240): the low-health drool, then white or its own colours on alternating flash updates.</summary>
    internal void ApplyBombTorizoHurt(RoomEnemyOccupant occupant, RoomEnemyPlace torizo, TorizoEnemyState torizoEnemyState) =>
        ApplyHurt(occupant, torizo, torizoEnemyState, unflashed: state => TorizoBody.ShowLook(torizo, state, new TorizoLook(TorizoLookKind.Normal)));

    /// <summary>Runs Golden Torizo's hurt behaviour (#2240): the low-health drool, then white or its health's colours on alternating flash updates.</summary>
    internal void ApplyGoldenTorizoHurt(RoomEnemyOccupant occupant, RoomEnemyPlace torizo, TorizoEnemyState torizoEnemyState) =>
        ApplyHurt(occupant, torizo, torizoEnemyState, unflashed: state => ShowGoldenTorizoHealthShade(torizo, state, torizo.Health));

    /// <summary>
    /// Runs a Torizo's hurt behaviour: the low-health drool, then on the flash's white updates white, and on the others the
    /// colours <paramref name="unflashed"/> shows.
    /// </summary>
    private void ApplyHurt(RoomEnemyOccupant occupant, RoomEnemyPlace torizo, TorizoEnemyState state, Action<TorizoEnemyState> unflashed)
    {
        _body.MaybeSpawnLowHealthDrool(occupant, torizo, state);
        if (!TorizoCadenceDefinitions.HurtWhiteShown.IsSetIn(torizo.HurtFlash.UpdatesLeft))
            unflashed(state);
        else
            TorizoBody.ShowLook(torizo, state, new TorizoLook(TorizoLookKind.White));
    }

    /// <summary>Enters the authored death list once and removes the dying statue from Samus collision.</summary>
    internal static void BeginTorizoDeath(RoomEnemyPlace torizo, TorizoEnemyState state)
    {
        if (state.DeathStarted)
            return;
        state.DeathStarted = true;
        state.Phase = TorizoPhase.Idle;
        TorizoBody.StartTorizoBodyAnimation(torizo, state, TorizoAnimations.Start(TorizoAnimation.Dying));
        state.GutBlownUp = true;
        state.FaceBlownUp = true;
        // The normal Torizo shot reaction makes the actor intangible when
        // entering the death animation. This removes the dying actor from Samus contact and
        // solid-enemy collision while its flashing/invisible animation frames continue.
        torizo.Contact = torizo.Contact.With(EnemyContact.IgnoresSamus);
    }


    /// <summary>Bomb Torizo's shot reaction (#2240): its hitbox reactions before impact, then common damage without its death.</summary>
    /// <param name="state">Bomb Torizo's state, which the reaction acts on.</param>
    internal static EnemyShotReaction BombTorizoShotReactionOf(TorizoEnemyState state) =>
        ShotReaction(state, (in EnemyShotContext c) => ResolveBombTorizoShotBeforeImpact(c, state));

    /// <summary>
    /// Golden Torizo's shot reaction (#2240): its own catches, dodges and stuns before impact, then common damage without its
    /// death. Its definition's own reaction and its body hitboxes' ordinary reaction both come here.
    /// </summary>
    /// <param name="state">Golden Torizo's state, which the reaction acts on.</param>
    internal static EnemyShotReaction GoldenTorizoShotReactionOf(TorizoEnemyState state) =>
        ShotReaction(state, (in EnemyShotContext c) => ResolveGoldenTorizoShotBeforeImpact(c, state));

    /// <summary>
    /// A Torizo's shot reaction, resolving each hit first by <paramref name="beforeImpact"/>. Its stand-up and sit-down
    /// frames' transitional reaction must stay distinguishable from the ordinary one, which the before-impact resolution
    /// tells apart.
    /// </summary>
    private static EnemyShotReaction ShotReaction(TorizoEnemyState state, EnemyShotStage beforeImpact) => new()
    {
        BeforeImpact = beforeImpact,
        ExemptFromGenericDeath = true,
        // Both Torizos' shot reactions run the common damage without the hit burst.
        ShowsHitBurst = EnemyShotReaction.NeverShowsHitBurst,
        AfterCommon = c =>
        {
            if (c.Enemy.Health == 0)
                BeginTorizoDeath(c.Enemy, state);
        },
    };

    /// <summary>Runs extended body touch damage without invoking the generic death animation.</summary>
    /// <param name="context">The touch.</param>
    /// <param name="state">The Torizo's state.</param>
    internal void ReactToTorizoTouch(EnemyTouchContext context, TorizoEnemyState state)
    {
        _collision.ResolveOrdinaryTouch(context, TouchDeathAnimation.Skip);
        if (context.Place.Health == 0)
            BeginTorizoDeath(context.Place, state);
    }

    /// <summary>
    /// Bomb Torizo's guards before shared shot damage (#2240): its stand-up and sit-down frames take no shot, nor does it
    /// during its hurt flash or while its animation is locked; each such hit is consumed. Returns whether it was.
    /// </summary>
    /// <param name="c">The shot.</param>
    /// <param name="torizoState">The Torizo's state.</param>
    internal static bool ResolveBombTorizoShotBeforeImpact(in EnemyShotContext c, TorizoEnemyState torizoState)
    {
        RoomEnemyPlace enemy = c.Enemy;
        if (c.HitboxShot == TorizoReactions.StandUpSitDownShot || enemy.HurtFlash.IsRunning || torizoState.AnimationLocked)
        {
            c.Projectiles.EnemyImpacts.TouchEnemyWithoutImpact(c.Shot, c.MarksCollision);
            return true;
        }
        return false;
    }

    /// <summary>
    /// Golden Torizo's guards and missile catches before shared shot damage (#2240); returns true when the hit was consumed
    /// by a special reaction.
    /// </summary>
    /// <param name="c">The shot.</param>
    /// <param name="torizoState">The Torizo's state.</param>
    internal static bool ResolveGoldenTorizoShotBeforeImpact(in EnemyShotContext c, TorizoEnemyState torizoState)
    {
        RoomEnemyPlace enemy = c.Enemy;
        // The stand-up and sit-down frames admit common no-death damage only while the flash timer is zero and the body's
        // animation is unlocked, and never reach the catches: missiles are not caught, Supers are not reflected, and the
        // counterattack is not armed through these transitional body rectangles.
        if (c.HitboxShot == TorizoReactions.StandUpSitDownShot && (enemy.HurtFlash.IsRunning || torizoState.AnimationLocked))
        {
            c.Projectiles.EnemyImpacts.TouchEnemyWithoutImpact(c.Shot, c.MarksCollision);
            return true;
        }
        // The reaction returns during a damage flash; a locked animation instead routes through common damage below.
        if (enemy.HurtFlash.IsRunning)
        {
            c.Projectiles.EnemyImpacts.TouchEnemyWithoutImpact(c.Shot, c.MarksCollision);
            return true;
        }

        // Once he has caught a Super Missile, any shot stuns him before the common damage, unless his animation is
        // locked or it strikes his stand-up and sit-down frames.
        if (c.HitboxShot == TorizoReactions.Shot && !torizoState.AnimationLocked && torizoState.CaughtSuperMissile)
            torizoState.Stunned = true;

        if (c.HitboxShot == TorizoReactions.Shot && !torizoState.AnimationLocked && !torizoState.CaughtSuperMissile)
        {
            if (c.Family == SamusProjectileFamily.Missile)
            {
                c.Projectiles.EnemyImpacts.TouchEnemyWithoutImpact(c.Shot, c.MarksCollision);
                c.Projectile.FlyOn();
                torizoState.Phase = TorizoPhase.Idle;
                TorizoBody.StartTorizoBodyAnimation(enemy, torizoState, TorizoAnimations.Start(TorizoFacing.FacesRight(torizoState)
                    ? TorizoAnimation.GoldenDodgeTurningRight
                    : TorizoAnimation.GoldenDodgeTurningLeft));
                return true;
            }

            if (c.Family == SamusProjectileFamily.SuperMissile)
            {
                if (TorizoFacing.FacesSamus(enemy, torizoState, c.Samus))
                {
                    c.Projectiles.EnemyImpacts.TouchEnemyWithoutImpact(c.Shot, c.MarksCollision);
                    torizoState.CaughtSuperMissile = true;
                    torizoState.Phase = TorizoPhase.Idle;
                    c.Projectile.Spend();
                    TorizoBody.StartTorizoBodyAnimation(enemy, torizoState, TorizoAnimations.Start(torizoState.Posture.LeadsWithFarFoot
                        ? TorizoFacing.FacesRight(torizoState)
                            ? TorizoAnimation.GoldenFacingRightCatchingSuperMissileLeftLegForward
                            : TorizoAnimation.GoldenFacingLeftCatchingSuperMissileRightLegForward
                        : TorizoFacing.FacesRight(torizoState)
                            ? TorizoAnimation.GoldenFacingRightCatchingSuperMissileRightLegForward
                            : TorizoAnimation.GoldenFacingLeftCatchingSuperMissileLeftLegForward));
                    return true;
                }
            }
            else
            {
                // A beam or any other shot that is neither a Missile nor a Super Missile stuns him, arming his
                // counterattack, before the common damage.
                torizoState.Stunned = true;
            }
        }
        return false;
    }

    /// <summary>The reaction of the first hitbox the probe overlaps on a Torizo's frame, which his own entity asks for (#2301).</summary>
    internal static EnemyHitboxContact FindTorizoHitboxReaction(RoomEnemyPlace enemy, EnemyHitboxProbe probe)
    {
        // The body's rectangles lie about Torizo's position; the probe meets their edges as every room position is
        // compared, by DistanceAcrossRoomEdge.LiesBefore, also an edge reaching past either end of the room.
        Position16<RoomPixels> origin = enemy.Position.Whole;
        IReadOnlyList<EnemyCollisionHitbox> rectangles = TorizoCollision.HitboxesAt(enemy.Frame).Rectangles;
        for (int index = 0; index < rectangles.Count; index++)
        {
            EnemyCollisionHitbox hitbox = rectangles[index];
            if (!probe.Overlaps(origin.X + hitbox.Left, origin.Y + hitbox.Top, origin.X + hitbox.Right, origin.Y + hitbox.Bottom))
                continue;
            return EnemyHitboxContact.Struck(probe.SelectsShot ? hitbox.Shot : hitbox.Touch);
        }
        return EnemyHitboxContact.Missed;
    }
}
