using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>Bomb and Golden Torizo's hitboxes, touch and hurt, and what Samus's weapons do to each of them (#2240).</summary>
[RoomLifetime]
internal sealed partial class TorizoCombat
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
    internal void ReactToBombTorizoBomb(EnemyBombContext c)
    {
        RoomEnemyPlace torizo = c.Enemy;
        TorizoEnemyState state = torizo.RequireSpeciesState<TorizoEnemyState>();
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
    internal void ReactToGoldenTorizoBomb(EnemyBombContext c)
    {
        RoomEnemyPlace torizo = c.Enemy;
        TorizoEnemyState state = torizo.RequireSpeciesState<TorizoEnemyState>();
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
    internal void ApplyBombTorizoHurt(RoomEnemyOccupant occupant, RoomEnemyPlace torizo) =>
        ApplyHurt(occupant, torizo, unflashed: state => TorizoBody.ShowLook(torizo, state, new TorizoLook(TorizoLookKind.Normal)));

    /// <summary>Runs Golden Torizo's hurt behaviour (#2240): the low-health drool, then white or its health's colours on alternating flash updates.</summary>
    internal void ApplyGoldenTorizoHurt(RoomEnemyOccupant occupant, RoomEnemyPlace torizo) =>
        ApplyHurt(occupant, torizo, unflashed: state => ShowGoldenTorizoHealthShade(torizo, state, torizo.Health));

    /// <summary>
    /// Runs a Torizo's hurt behaviour: the low-health drool, then on the flash's white updates white, and on the others the
    /// colours <paramref name="unflashed"/> shows.
    /// </summary>
    private void ApplyHurt(RoomEnemyOccupant occupant, RoomEnemyPlace torizo, Action<TorizoEnemyState> unflashed)
    {
        _body.MaybeSpawnLowHealthDrool(occupant, torizo);
        TorizoEnemyState state = torizo.RequireSpeciesState<TorizoEnemyState>();
        if (!TorizoCadenceDefinitions.HurtWhiteShown.IsInSecondHalfAt(torizo.HurtFlash.UpdatesLeft))
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
        state.Breakage = TorizoBreakage.GutAndFaceBlownUp;
        // The normal Torizo shot reaction makes the actor intangible when
        // entering the death animation. This removes the dying actor from Samus contact and
        // solid-enemy collision while its flashing/invisible animation frames continue.
        torizo.Contact = torizo.Contact.With(EnemyContact.IgnoresSamus);
    }


    /// <summary>Bomb Torizo's shot reaction (#2240): its hitbox reactions before impact, then common damage without its death.</summary>
    internal EnemyShotReaction BombTorizoShotReaction => field ??= ShotReaction(ResolveBombTorizoShotBeforeImpact);

    /// <summary>
    /// Golden Torizo's shot reaction (#2240): its own catches, dodges and stuns before impact, then common damage without its
    /// death. Its definition's own reaction and its body hitboxes' ordinary reaction both come here.
    /// </summary>
    internal EnemyShotReaction GoldenTorizoShotReaction => field ??= ShotReaction(ResolveGoldenTorizoShotBeforeImpact);

    /// <summary>
    /// A Torizo's shot reaction, resolving each hit first by <paramref name="beforeImpact"/>. Its stand-up and sit-down
    /// frames' transitional reaction must stay distinguishable from the ordinary one, which the before-impact resolution
    /// tells apart.
    /// </summary>
    private static EnemyShotReaction ShotReaction(EnemyShotStage beforeImpact) => new()
    {
        BeforeImpact = beforeImpact,
        Lethal = EnemyShotLethal.SpeciesOwned,
        // Both Torizos' shot reactions run the common damage without the hit burst.
        ShowsHitBurst = EnemyShotReaction.NeverShowsHitBurst,
        AfterCommon = c =>
        {
            if (c.Enemy.Health == 0)
                BeginTorizoDeath(c.Enemy, c.Enemy.RequireSpeciesState<TorizoEnemyState>());
        },
    };

    /// <summary>Runs extended body touch damage without invoking the generic death animation.</summary>
    internal void ReactToTorizoTouch(EnemyTouchContext context)
    {
        _collision.ResolveOrdinaryTouch(context, TouchDeathAnimation.Skip);
        if (context.Place.Health == 0)
            BeginTorizoDeath(context.Place, context.Place.RequireSpeciesState<TorizoEnemyState>());
    }

    /// <summary>
    /// Bomb Torizo's guards before shared shot damage (#2240): its stand-up and sit-down frames take no shot, nor does it
    /// during its hurt flash or while its animation is locked; each such hit is consumed. Returns whether it was.
    /// </summary>
    internal static bool ResolveBombTorizoShotBeforeImpact(in EnemyShotContext c)
    {
        RoomEnemyPlace enemy = c.Enemy;
        TorizoEnemyState torizoState = enemy.RequireSpeciesState<TorizoEnemyState>();
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
    internal static bool ResolveGoldenTorizoShotBeforeImpact(in EnemyShotContext c)
    {
        RoomEnemyPlace enemy = c.Enemy;
        TorizoEnemyState torizoState = enemy.RequireSpeciesState<TorizoEnemyState>();
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
                c.Shot.FlyOn();
                torizoState.Phase = TorizoPhase.Idle;
                TorizoBody.StartTorizoBodyAnimation(enemy, torizoState, TorizoAnimations.Start(TorizoFacing.FacesRight(enemy)
                    ? TorizoAnimation.GoldenDodgeTurningRight
                    : TorizoAnimation.GoldenDodgeTurningLeft));
                return true;
            }

            if (c.Family == SamusProjectileFamily.SuperMissile)
            {
                if (TorizoFacing.FacesSamus(enemy, c.Samus))
                {
                    c.Projectiles.EnemyImpacts.TouchEnemyWithoutImpact(c.Shot, c.MarksCollision);
                    torizoState.CaughtSuperMissile = true;
                    torizoState.Phase = TorizoPhase.Idle;
                    c.Shot.Spend();
                    TorizoBody.StartTorizoBodyAnimation(enemy, torizoState, TorizoAnimations.Start(TorizoFacing.Posture(enemy).LeadsWithFarFoot
                        ? TorizoFacing.FacesRight(enemy)
                            ? TorizoAnimation.GoldenFacingRightCatchingSuperMissileLeftLegForward
                            : TorizoAnimation.GoldenFacingLeftCatchingSuperMissileRightLegForward
                        : TorizoFacing.FacesRight(enemy)
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
            return EnemyHitboxContact.Struck(probe.Contact == HitboxContactKind.Shot ? hitbox.Shot : hitbox.Touch);
        }
        return EnemyHitboxContact.Missed;
    }
}
