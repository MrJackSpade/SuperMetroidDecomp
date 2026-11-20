using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Enemies.MaridiaSnails;

/// <summary>The Oum species owner: its per-room state, behaviour and the interactions it registers.</summary>
[RoomLifetime]
internal sealed class OumBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.Oum];

    /// <inheritdoc/>
    public EnemyOccupant Create(RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common) => EnemyOccupant.Of(new OumEntity(this, occupant, place, common));

    /// <summary>Resolves floor, ledge, and wall contact during bounce and roll phases.</summary>
    private readonly EnemyTerrain _terrain;
    /// <summary>Runs common damaging touch behavior for selected shell hitboxes.</summary>
    private readonly EnemyCollision _collision;

    /// <summary>Receives the species' sound requests, each queued as the species makes it.</summary>
    private readonly EnemyAudioQueue _audio;

    /// <summary>Binds terrain and contact owners for Oum's bounce and push behavior.</summary>
    internal OumBehavior(EnemyTerrain terrain, EnemyCollision collision, EnemyAudioQueue audio)
    {
        _audio = audio;
        _terrain = terrain;
        _collision = collision;
    }

    /// <summary>Initializes the Oum.</summary>
    internal static void InitializeOum(RoomEnemyPlace place)
    {
        var state = new OumEnemyState(OumAction.Idle, OumBouncePhase.Falling)
        {
            Facing = HorizontalDirection.Right,
            ShowsSpawnIdle = true,
            AttackCooldown = UpdateCountdown.Lasting(OumMotionDefinitions.InitialAttackCooldown),
            Heading = HorizontalDirection.Right,
            RemainingBounces = OumMotionDefinitions.SettlingBounces,
        };
        place.SetSpeciesState(state);

        // The spawn's left-facing idle disagrees with the right-facing Oum it counts as (ShowsSpawnIdle), which is
        // observable until the snail first starts an animation.
        state.Playback = new(OumAnimations.StartingWith(OumSegment.FacingLeftIdle), AnimationPlace.StartOfAnimation);
    }

    /// <summary>
    /// Runs the Oum's main behaviour and actions. Ordering is exact: proximity and
    /// previous positions, behavior, Samus carrying, then the input-derived stop flag. The side of Samus pushing it and its
    /// position before the update are this update's own (#2473), read only by its later steps.
    /// </summary>
    private void RunOumMain(
        RoomEnemyPlace place,
        OumEnemyState state,
        SamusState samus,
        RoomLevelData level,
        GameplayAction controllerInput)
    {
        HorizontalDirection? pushSide = OumPushSide(place, samus);
        Position16<RoomPixels> previousPosition = place.Position.Whole;

        switch (state.Action)
        {
            case OumAction.Idle:
                RunOumIdle(place, state, samus, level);
                break;
            case OumAction.Rolling:
                RunOumRolling(place, state, samus, level, pushSide);
                break;
            case OumAction.Attacking:
                break;
            case OumAction.AttackFinished:
                RollOumAfterAttack(place, state);
                break;
            default:
                throw new InvalidDataException(
                    $"Maridia Large Snail has no behaviour for action {state.Action}.");
        }

        CarrySamusWithOum(place, state, samus, previousPosition, pushSide);
        UpdateOumInputStop(place, state, controllerInput, previousPosition, pushSide);
    }

    /// <summary>Completes landing bounces, faces Samus, then starts rolling within the pushing range.</summary>
    private void RunOumIdle(
        RoomEnemyPlace place,
        OumEnemyState state,
        SamusState samus,
        RoomLevelData level)
    {
        if (state.RemainingBounces != 0)
        {
            RunOumBounce(place, state, level);
            return;
        }

        state.Facing = SamusSide(samus, place);
        // Only Samus to its left turns the way it would roll; to its right it keeps the way it rolled last.
        if (state.Facing == HorizontalDirection.Left)
            state.Heading = HorizontalDirection.Left;
        StartOumAnimation(place, state, OumAnimationChoices.Idle(state.Facing));

        if (Math.Abs(samus.Position.X.Whole - place.Position.X.Whole) >= OumMotionDefinitions.PushingReachX)
        {
            return;
        }

        state.RollsBackwards = false;
        StartOumAnimation(place, state, OumRoll(state));
        state.Action = OumAction.Rolling;
    }

    /// <summary>Handles ledges, cooldown and attack selection before horizontal terrain movement.</summary>
    private void RunOumRolling(
        RoomEnemyPlace place,
        OumEnemyState state,
        SamusState samus,
        RoomLevelData level,
        HorizontalDirection? pushSide)
    {
        if (state.RemainingBounces != 0)
        {
            RunOumBounce(place, state, level);
        }
        else if (!_terrain.MoveEnemyVertically(level, place, OumMotionDefinitions.FloorProbe))
        {
            // No floor one pixel below means Oum walked off a ledge. Oum restarts the
            // complete settling sequence without skipping horizontal work.
            state.YSpeedCurvePlace = QuadraticSpeedCurvePlace.Start;
            state.BouncePhase = OumBouncePhase.Falling;
            state.RemainingBounces = OumMotionDefinitions.SettlingBounces;
        }

        // The stop is the one the previous update asked for; this update asks again only after it rolls.
        if (state.InputStop.IsHeld)
            return;

        // The cooldown lasts its whole duration and then stays run out until Oum attacks (#2340).
        if (state.AttackCooldown.Step())
        {
            bool mayAttack = Math.Abs(samus.Position.X.Whole - place.Position.X.Whole) < OumMotionDefinitions.AttackReach &&
                state.AttackAllowsRotation &&
                state.RemainingBounces == 0;
            if (mayAttack)
            {
                state.Heading = SamusSide(samus, place);
                state.AttackCooldown = UpdateCountdown.For(OumMotionDefinitions.InitialAttackCooldown);
                state.Facing = state.Heading;
                StartOumAnimation(place, state, OumAnimationChoices.Attacking(state.Facing));
                state.Action = OumAction.Attacking;
                return;
            }
        }

        MoveOumHorizontally(place, state, level, pushSide);
    }

    /// <summary>Resumes the rolling animation once the attack animation has finished.</summary>
    private static void RollOumAfterAttack(
        RoomEnemyPlace place,
        OumEnemyState state)
    {
        state.RollsBackwards = false;
        StartOumAnimation(place, state, OumRoll(state));
        state.Action = OumAction.Rolling;
    }

    /// <summary>Alternates quadratic-speed fall and rise phases, moving the place back after each impact.</summary>
    private void RunOumBounce(
        RoomEnemyPlace place,
        OumEnemyState state,
        RoomLevelData level)
    {
        switch (state.BouncePhase)
        {
            case OumBouncePhase.Falling:
                // Falling holds the place at its fastest fall place.
                state.YSpeedCurvePlace = QuadraticSpeedCurvePlace.Min(
                    state.YSpeedCurvePlace + OumMotionDefinitions.BounceAcceleration,
                    OumMotionDefinitions.FastestFallPlace);

                if (!_terrain.MoveEnemyVertically(
                        level,
                        place,
                        state.YSpeedCurvePlace.Speed.Positive))
                {
                    return;
                }

                state.RemainingBounces--;
                // An impact that leaves the place below the start ends the bounces; the negative place is kept.
                state.YSpeedCurvePlace -= OumMotionDefinitions.ImpactCurveLoss;
                if (state.YSpeedCurvePlace.IsNegative)
                    state.RemainingBounces = 0;
                state.BouncePhase = OumBouncePhase.Rising;
                return;

            case OumBouncePhase.Rising:
                state.YSpeedCurvePlace -= OumMotionDefinitions.BounceAcceleration;
                if (state.YSpeedCurvePlace.IsNegative)
                {
                    state.YSpeedCurvePlace = QuadraticSpeedCurvePlace.Start;
                    state.BouncePhase = OumBouncePhase.Falling;
                    return;
                }

                // The rising half moves at the same curve mark's speed upward; the place lies at or past the curve's start
                // here, so its whole marks are that mark.
                _terrain.MoveEnemyVertically(
                    level,
                    place,
                    state.YSpeedCurvePlace.Speed.Negative);
                return;

            default:
                throw new InvalidDataException(
                    $"Maridia Large Snail has no behaviour for bounce phase " +
                    $"{state.BouncePhase}.");
        }
    }

    /// <summary>Temporarily extends the collision probe toward Samus, then retracts it after wall resolution.</summary>
    private void MoveOumHorizontally(
        RoomEnemyPlace place,
        OumEnemyState state,
        RoomLevelData level,
        HorizontalDirection? pushSide)
    {
        SignedSixteenSixteen speed = OumMotionDefinitions.RollSpeed.Toward(state.Heading);
        bool temporarilyReachTowardSamus = state.Heading == pushSide;
        if (temporarilyReachTowardSamus)
        {
            // Temporarily extends the whole-pixel move toward Samus by the push's reach,
            // performs collision, then retracts the center. Net velocity remains its
            // roll speed while wall clipping naturally passes the lost distance to Samus.
            speed += OumMotionDefinitions.PushReach.Toward(state.Heading);
        }

        SignedSixteenSixteen displacement = speed;
        bool collided = _terrain.MoveEnemyHorizontallyTreatingSlopesAsWalls(
            level,
            place,
            displacement);

        if (temporarilyReachTowardSamus)
        {
            place.Position = place.Position.AdvanceX(OumMotionDefinitions.PushReach.Toward(state.Heading.Reversed()));
        }

        if (!collided)
            return;

        state.Heading = state.Heading.Reversed();
        state.RollsBackwards = !state.RollsBackwards;
        StartOumAnimation(place, state, OumRoll(state));
    }

    /// <summary>
    /// The side of Oum Samus is on while she is within its strict pushing range, or none while she is not (#2473).
    /// </summary>
    private static HorizontalDirection? OumPushSide(RoomEnemyPlace place, SamusState samus) =>
        Math.Abs(samus.Position.Y.Whole - place.Position.Y.Whole) >= OumMotionDefinitions.PushingReachY ||
        Math.Abs(samus.Position.X.Whole - place.Position.X.Whole) >= OumMotionDefinitions.PushingReachX
            ? null
            : SamusSide(samus, place);

    /// <summary>Publishes Oum's vertical platform motion and sideward push into Samus's extra displacement.</summary>
    private static void CarrySamusWithOum(
        RoomEnemyPlace place,
        OumEnemyState state,
        SamusState samus,
        Position16<RoomPixels> previousPosition,
        HorizontalDirection? pushSide)
    {
        state.InputStop.Release();
        if (EnemyCollision.IsSamusStandingOn(place, samus))
        {
            // Unlike most moving-platform producers, Oum assigns—not adds—the vertical
            // displacement. This preserves the last touching Oum's ownership. It assigns the whole pixels alone, so the
            // fraction of a pixel an earlier carry this update left stays.
            samus.Kinematics.ExtraDisplacement = samus.Kinematics.ExtraDisplacement.WithWholeY(
                place.Position.Y.Whole - previousPosition.Y);
        }

        if (pushSide is not { } side)
            return;

        int deltaX = place.Position.X.Whole - previousPosition.X;
        bool movedLeft = deltaX < 0;
        if (movedLeft != (side == HorizontalDirection.Left))
            return;

        samus.Kinematics.ExtraDisplacement += new Offset16<RoomPixels>(deltaX, 0);
    }

    /// <summary>Records an opposing controller direction to stop the following frame's roll.</summary>
    private static void UpdateOumInputStop(
        RoomEnemyPlace place,
        OumEnemyState state,
        GameplayAction controllerInput,
        Position16<RoomPixels> previousPosition,
        HorizontalDirection? pushSide)
    {
        if (pushSide is not { } side)
            return;

        int deltaX = place.Position.X.Whole - previousPosition.X;
        bool stop;
        if (deltaX == 0)
        {
            // Pushing against the way Oum rolls toward Samus stops it.
            stop = side == state.Heading &&
                controllerInput.HasAny(state.Heading == HorizontalDirection.Left ? GameplayAction.Right : GameplayAction.Left);
        }
        else if (deltaX < 0)
        {
            stop = controllerInput.HasAny(GameplayAction.Right);
        }
        else
        {
            stop = controllerInput.HasAny(GameplayAction.Left);
        }

        if (!stop)
            return;

        state.InputStop.HoldNextUpdate();
        place.Position = place.Position.WithWholeX(previousPosition.X);
    }

    /// <summary>Carries out one cue of Oum's animations: the splash sound, attack completion and the attack-rotation window.</summary>
    private void ApplyOumCue(OumEnemyState state, OumCue cue)
    {
        switch (cue.Kind)
        {
            case OumCueKind.PlaySplash:
                _audio.QueueEnemySound(EnemySoundEffectDefinitions.OumSplashSound, priority: SoundRequestPriority.Normal);
                break;
            case OumCueKind.FinishAttack:
                if (state.Action != OumAction.Attacking)
                    throw new InvalidOperationException($"Oum's attack finished while it was {state.Action}.");
                state.Action = OumAction.AttackFinished;
                break;
            case OumCueKind.AllowAttackRotation:
                state.AttackAllowsRotation = true;
                break;
            case OumCueKind.DisallowAttackRotation:
                state.AttackAllowsRotation = false;
                break;
            default:
                throw new InvalidOperationException($"Undefined {nameof(OumCueKind)} {cue.Kind}.");
        }
    }

    /// <summary>The roll Oum shows: facing as it faced when the roll began, forwards or, after a wall, backwards.</summary>
    private static OumSegment OumRoll(OumEnemyState state) => state.RollsBackwards
        ? OumAnimationChoices.RollingBackwards(state.Facing)
        : OumAnimationChoices.RollingForwards(state.Facing);

    /// <summary>The side of Oum Samus is on: left when she is to its left, otherwise right (#2338).</summary>
    private static HorizontalDirection SamusSide(SamusState samus, RoomEnemyPlace place) =>
        samus.Position.X.Whole < place.Position.X.Whole ? HorizontalDirection.Left : HorizontalDirection.Right;

    /// <summary>
    /// Starts <paramref name="segment"/>'s animation only when it differs from the one the snail plays. While the snail
    /// still shows its spawn idle it counts as playing the right-facing idle, and its first start of another animation, the
    /// left-facing idle included, starts it afresh (<see cref="OumEnemyState.ShowsSpawnIdle"/>).
    /// </summary>
    private static void StartOumAnimation(
        RoomEnemyPlace place,
        OumEnemyState state,
        OumSegment segment)
    {
        Animation<OumSegment> animation = OumAnimations.StartingWith(segment);
        if (!state.ShowsSpawnIdle)
        {
            state.Playback = EnemyAnimationPlayback.StartUnlessPlaying(place, state.Playback, animation);
            return;
        }
        if (segment == OumSegment.FacingRightIdle)
            return;
        state.ShowsSpawnIdle = false;
        state.Playback = EnemyAnimationPlayback.Start(place, animation);
    }

    /// <summary>
    /// Shoves Samus away from Oum when she touches it without standing on it, after any damage its hitbox does.
    /// </summary>
    private static void ResolveOumTouchAfterCommon(
        RoomEnemyPlace place,
        SamusState samus)
    {
        if (EnemyCollision.IsSamusStandingOn(place, samus))
            return;

        int shove = OumMotionDefinitions.ShoveDistance.Toward(
            samus.Position.X.Whole < place.Position.X.Whole ? HorizontalDirection.Left : HorizontalDirection.Right);
        samus.Kinematics.ExtraDisplacement += new Offset16<RoomPixels>(shove, 0);
    }

    /// <summary>
    /// Plays the Oum's shot sound after the common shot damage, for damaging, freezing, and zero-damage vulnerability
    /// results alike.
    /// </summary>
    private void ResolveOumShotAfterCommon() =>
        _audio.QueueEnemySound(EnemySoundEffectDefinitions.OumShotSound, priority: SoundRequestPriority.Normal);

    /// <summary>Publishes Oum's shot sound after common bomb damage.</summary>
    internal EnemyBombReaction OumBombReaction => field ??= new()
    {
        AfterCommon = _ => ResolveOumShotAfterCommon(),
    };

    /// <summary>Runs the Oum's main AI.</summary>
    internal void RunOumMainAi(RoomEnemyPlace place, IEnemyAiFrame frame)
    {
        RunOumMain(
            place,
            place.RequireSpeciesState<OumEnemyState>(),
            frame.Samus,
            frame.Level,
            frame.ControllerInput);
    }

    /// <summary>Advances the Oum's typed animation.</summary>
    internal void AdvanceOumTypedAnimation(RoomEnemyPlace place) =>
        EnemyAnimationPlayback.Advance(place, OumAnimations.Segments,
            static (OumEnemyState state) => state.Playback, static (state, playback) => state.Playback = playback,
            (state, cue) =>
            {
                ApplyOumCue(state, cue);
                return null;
            });

    /// <summary>A hit on the protected shell does nothing; only a vulnerable hitbox takes common damage.</summary>
    internal EnemyShotReaction OumShotReaction => field ??= new()
    {
        AfterSuperMissileQuake = (in EnemyShotContext c) =>
        {
            if (!c.UsesFrameHitboxes)
                throw new InvalidDataException("Maridia Large Snail's shots no longer meet its frames' hitboxes.");
            if (c.HitboxShot == EnemyReaction.Ignore)
            {
                // Frame-hitbox collision marks the projectile, and nothing more happens: the protected shell
                // takes no shot damage, makes no impact, plays no shot sound and has no vulnerability.
                c.Projectiles.EnemyImpacts.TouchEnemyWithoutImpact(c.Shot, c.MarksCollision);
                return true;
            }
            if (c.HitboxShot != OumReactions.Shot)
            {
                throw new InvalidDataException(
                    $"Maridia Large Snail has no response to shot reaction " +
                    $"{c.HitboxShot}.");
            }
            return false;
        },
        AfterCommon = _ => ResolveOumShotAfterCommon(),
    };

    /// <summary>Reacts to Samus touching a damaging or non-damaging Oum hitbox, each with its own push.</summary>
    internal void ReactToOumTouch(EnemyTouchContext context)
    {
        if (!context.UsesFrameHitboxes)
            throw new InvalidDataException("Maridia Large Snail's contact no longer meets its frames' hitboxes.");
        if ((context.HitboxTouch != OumReactions.DamagingTouch && context.HitboxTouch != OumReactions.NonDamagingTouch))
        {
            throw new InvalidDataException(
                $"Maridia Large Snail has no response to touch reaction " +
                $"{context.HitboxTouch}.");
        }

        // The damaging touch hurts Samus as any enemy does and then shoves her as the harmless touch does. A harmless
        // touch only shoves, which is how the closed shell carries and pushes Samus without its damage.
        if (context.HitboxTouch == OumReactions.DamagingTouch)
            _collision.ResolveOrdinaryTouch(context);
        ResolveOumTouchAfterCommon(context.Place, context.Samus);
    }

    /// <summary>The reaction of the first of the occupant's hitboxes the probe overlaps on the frame it shows.</summary>
    /// <exception cref="InvalidDataException">The frame shown has no Oum collision.</exception>
    internal static EnemyHitboxContact FindHitboxReaction(RoomEnemyPlace enemy, EnemyHitboxProbe probe)
    {
        // The frame's hitboxes are read in order, and the first the probe overlaps gives the reaction.
        foreach (EnemyCollisionHitbox hitbox in
                 OumCollisionDefinitions.Frames.Of(enemy.Frame))
        {
            int left = enemy.Position.X.Whole + hitbox.Left;
            int top = enemy.Position.Y.Whole + hitbox.Top;
            int right = enemy.Position.X.Whole + hitbox.Right;
            int bottom = enemy.Position.Y.Whole + hitbox.Bottom;
            if (!probe.Overlaps(left, top, right, bottom))
                continue;
            return EnemyHitboxContact.Struck(probe.SelectsShot ? hitbox.Shot : hitbox.Touch);
        }
        return EnemyHitboxContact.Missed;
    }
}
