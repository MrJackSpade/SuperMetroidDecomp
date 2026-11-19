using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Enemies.MaridiaSnails;

/// <summary>The Maridia large snail species owner: its per-room state, behaviour and the interactions it registers.</summary>
[RoomLifetime]
internal sealed class MaridiaLargeSnailBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.Oum];

    /// <inheritdoc/>
    public IEnemyEntity Create(RoomEnemyPlace place, EnemyCommonParticipation common) => new MaridiaLargeSnailEntity(this, place, common);

    /// <summary>Resolves floor, ledge, and wall contact during bounce and roll phases.</summary>
    private readonly EnemyTerrain _terrain;
    /// <summary>Runs common damaging touch behavior for selected shell hitboxes.</summary>
    private readonly EnemyCollision _collision;

    /// <summary>Receives the species' sound requests, each queued as the species makes it.</summary>
    private readonly EnemyAudioQueue _audio;

    /// <summary>Binds terrain and contact owners for Oum's bounce and push behavior.</summary>
    internal MaridiaLargeSnailBehavior(EnemyTerrain terrain, EnemyCollision collision, EnemyAudioQueue audio)
    {
        _audio = audio;
        _terrain = terrain;
        _collision = collision;
    }

    /// <summary>Initializes the Oum.</summary>
    internal static void InitializeMaridiaLargeSnail(RoomEnemyPlace place)
    {
        var state = new MaridiaLargeSnailEnemyState(MaridiaLargeSnailAction.Idle, MaridiaLargeSnailBouncePhase.Falling)
        {
            Facing = HorizontalDirection.Right,
            ShowsSpawnIdle = true,
            AttackCooldown = UpdateCountdown.Lasting(MaridiaLargeSnailMotionDefinitions.InitialAttackCooldown),
            Heading = HorizontalDirection.Right,
            RemainingBounces = MaridiaLargeSnailMotionDefinitions.SettlingBounces,
        };
        place.SetSpeciesState(state);

        // The spawn's left-facing idle disagrees with the right-facing Oum it counts as (ShowsSpawnIdle), which is
        // observable until the snail first starts an animation.
        state.Playback = new(MaridiaLargeSnailAnimations.StartingWith(MaridiaLargeSnailSegment.FacingLeftIdle), AnimationPlace.StartOfAnimation);
    }

    /// <summary>
    /// Runs the Oum's main behaviour and actions. Ordering is exact: proximity and
    /// previous positions, behavior, Samus carrying, then the input-derived stop flag. The side of Samus pushing it and its
    /// position before the update are this update's own (#2473), read only by its later steps.
    /// </summary>
    private void RunMaridiaLargeSnailMain(
        RoomEnemyPlace place,
        MaridiaLargeSnailEnemyState state,
        SamusState samus,
        RoomLevelData level,
        GameplayAction controllerInput)
    {
        HorizontalDirection? pushSide = MaridiaLargeSnailPushSide(place, samus);
        Position16<RoomPixels> previousPosition = place.Position.Whole;

        switch (state.Action)
        {
            case MaridiaLargeSnailAction.Idle:
                RunMaridiaLargeSnailIdle(place, state, samus, level);
                break;
            case MaridiaLargeSnailAction.Rolling:
                RunMaridiaLargeSnailRolling(place, state, samus, level, pushSide);
                break;
            case MaridiaLargeSnailAction.Attacking:
                break;
            case MaridiaLargeSnailAction.AttackFinished:
                RollMaridiaLargeSnailAfterAttack(place, state);
                break;
            default:
                throw new InvalidDataException(
                    $"Maridia Large Snail has no behaviour for action {state.Action}.");
        }

        CarrySamusWithMaridiaLargeSnail(place, state, samus, previousPosition, pushSide);
        UpdateMaridiaLargeSnailInputStop(place, state, controllerInput, previousPosition, pushSide);
    }

    /// <summary>Completes landing bounces, faces Samus, then starts rolling within the pushing range.</summary>
    private void RunMaridiaLargeSnailIdle(
        RoomEnemyPlace place,
        MaridiaLargeSnailEnemyState state,
        SamusState samus,
        RoomLevelData level)
    {
        if (state.RemainingBounces != 0)
        {
            RunMaridiaLargeSnailBounce(place, state, level);
            return;
        }

        state.Facing = SamusSide(samus, place);
        // Only Samus to its left turns the way it would roll; to its right it keeps the way it rolled last.
        if (state.Facing == HorizontalDirection.Left)
            state.Heading = HorizontalDirection.Left;
        StartMaridiaLargeSnailAnimation(place, state, MaridiaLargeSnailAnimationChoices.Idle(state.Facing));

        if (Math.Abs(samus.Position.X.Whole - place.Position.X.Whole) >= MaridiaLargeSnailMotionDefinitions.PushingReachX)
        {
            return;
        }

        state.RollsBackwards = false;
        StartMaridiaLargeSnailAnimation(place, state, MaridiaLargeSnailRoll(state));
        state.Action = MaridiaLargeSnailAction.Rolling;
    }

    /// <summary>Handles ledges, cooldown and attack selection before horizontal terrain movement.</summary>
    private void RunMaridiaLargeSnailRolling(
        RoomEnemyPlace place,
        MaridiaLargeSnailEnemyState state,
        SamusState samus,
        RoomLevelData level,
        HorizontalDirection? pushSide)
    {
        if (state.RemainingBounces != 0)
        {
            RunMaridiaLargeSnailBounce(place, state, level);
        }
        else if (!_terrain.MoveEnemyVertically(level, place, SignedSixteenSixteen.FromWholePixels(1)))
        {
            // No floor one pixel below means Oum walked off a ledge. Oum restarts the
            // complete three-impact settling sequence without skipping horizontal work.
            state.YSpeedCurvePlace = QuadraticSpeedCurvePlace.Start;
            state.BouncePhase = MaridiaLargeSnailBouncePhase.Falling;
            state.RemainingBounces = MaridiaLargeSnailMotionDefinitions.SettlingBounces;
        }

        // The stop is the one the previous update asked for; this update asks again only after it rolls.
        if (state.StopMovementNextFrame)
            return;

        // The cooldown lasts its whole duration and then stays run out until Oum attacks (#2340).
        if (state.AttackCooldown.Step())
        {
            bool mayAttack = Math.Abs(samus.Position.X.Whole - place.Position.X.Whole) < MaridiaLargeSnailMotionDefinitions.AttackReach &&
                state.AttackAllowsRotation &&
                state.RemainingBounces == 0;
            if (mayAttack)
            {
                state.Heading = SamusSide(samus, place);
                state.AttackCooldown = UpdateCountdown.For(MaridiaLargeSnailMotionDefinitions.InitialAttackCooldown);
                state.Facing = state.Heading;
                StartMaridiaLargeSnailAnimation(place, state, MaridiaLargeSnailAnimationChoices.Attacking(state.Facing));
                state.Action = MaridiaLargeSnailAction.Attacking;
                return;
            }
        }

        MoveMaridiaLargeSnailHorizontally(place, state, level, pushSide);
    }

    /// <summary>Resumes the rolling animation once the attack animation has finished.</summary>
    private static void RollMaridiaLargeSnailAfterAttack(
        RoomEnemyPlace place,
        MaridiaLargeSnailEnemyState state)
    {
        state.RollsBackwards = false;
        StartMaridiaLargeSnailAnimation(place, state, MaridiaLargeSnailRoll(state));
        state.Action = MaridiaLargeSnailAction.Rolling;
    }

    /// <summary>Alternates quadratic-speed fall and rise phases, moving the place back after each impact.</summary>
    private void RunMaridiaLargeSnailBounce(
        RoomEnemyPlace place,
        MaridiaLargeSnailEnemyState state,
        RoomLevelData level)
    {
        switch (state.BouncePhase)
        {
            case MaridiaLargeSnailBouncePhase.Falling:
                // Falling holds the place at mark 64.
                state.YSpeedCurvePlace = QuadraticSpeedCurvePlace.Min(
                    state.YSpeedCurvePlace + MaridiaLargeSnailMotionDefinitions.BounceAcceleration,
                    MaridiaLargeSnailMotionDefinitions.FastestFallPlace);

                if (!_terrain.MoveEnemyVertically(
                        level,
                        place,
                        state.YSpeedCurvePlace.Speed.Positive))
                {
                    return;
                }

                state.RemainingBounces--;
                // An impact that leaves the place below the start ends the bounces; the negative place is kept.
                state.YSpeedCurvePlace -= MaridiaLargeSnailMotionDefinitions.ImpactCurveLoss;
                if (state.YSpeedCurvePlace.IsNegative)
                    state.RemainingBounces = 0;
                state.BouncePhase = MaridiaLargeSnailBouncePhase.Rising;
                return;

            case MaridiaLargeSnailBouncePhase.Rising:
                state.YSpeedCurvePlace -= MaridiaLargeSnailMotionDefinitions.BounceAcceleration;
                if (state.YSpeedCurvePlace.IsNegative)
                {
                    state.YSpeedCurvePlace = QuadraticSpeedCurvePlace.Start;
                    state.BouncePhase = MaridiaLargeSnailBouncePhase.Falling;
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
    private void MoveMaridiaLargeSnailHorizontally(
        RoomEnemyPlace place,
        MaridiaLargeSnailEnemyState state,
        RoomLevelData level,
        HorizontalDirection? pushSide)
    {
        bool movingLeft = state.Heading == HorizontalDirection.Left;
        SignedSixteenSixteen speed = movingLeft ? -MaridiaLargeSnailMotionDefinitions.RollSpeed : MaridiaLargeSnailMotionDefinitions.RollSpeed;
        bool temporarilyReachTowardSamus = state.Heading == pushSide;
        if (temporarilyReachTowardSamus)
        {
            // Temporarily extends the whole-pixel move toward Samus by the push's reach,
            // performs collision, then retracts the center. Net velocity remains one
            // pixel per frame while wall clipping naturally passes the lost distance to Samus.
            speed = speed.WithWholePixels(speed.WholePixels + (movingLeft
                ? -MaridiaLargeSnailMotionDefinitions.PushReach
                : MaridiaLargeSnailMotionDefinitions.PushReach));
        }

        SignedSixteenSixteen displacement = speed;
        bool collided = _terrain.MoveEnemyHorizontallyTreatingSlopesAsWalls(
            level,
            place,
            displacement);

        if (temporarilyReachTowardSamus)
        {
            place.Position = place.Position.WithWholeX(
                place.Position.X.Whole + (movingLeft
                    ? MaridiaLargeSnailMotionDefinitions.PushReach
                    : -MaridiaLargeSnailMotionDefinitions.PushReach));
        }

        if (!collided)
            return;

        state.Heading = state.Heading.Reversed();
        state.RollsBackwards = !state.RollsBackwards;
        StartMaridiaLargeSnailAnimation(place, state, MaridiaLargeSnailRoll(state));
    }

    /// <summary>
    /// The side of Oum Samus is on while she is within its strict 24-by-32 pushing range, or none while she is not (#2473).
    /// </summary>
    private static HorizontalDirection? MaridiaLargeSnailPushSide(RoomEnemyPlace place, SamusState samus) =>
        Math.Abs(samus.Position.Y.Whole - place.Position.Y.Whole) >= MaridiaLargeSnailMotionDefinitions.PushingReachY ||
        Math.Abs(samus.Position.X.Whole - place.Position.X.Whole) >= MaridiaLargeSnailMotionDefinitions.PushingReachX
            ? null
            : SamusSide(samus, place);

    /// <summary>Publishes Oum's vertical platform motion and sideward push into Samus's extra displacement.</summary>
    private static void CarrySamusWithMaridiaLargeSnail(
        RoomEnemyPlace place,
        MaridiaLargeSnailEnemyState state,
        SamusState samus,
        Position16<RoomPixels> previousPosition,
        HorizontalDirection? pushSide)
    {
        state.StopMovementNextFrame = false;
        if (EnemyCollision.IsSamusStandingOn(place, samus))
        {
            // Unlike most moving-platform producers, Oum assigns—not adds—the vertical
            // displacement. This preserves the last touching Oum's ownership.
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
    private static void UpdateMaridiaLargeSnailInputStop(
        RoomEnemyPlace place,
        MaridiaLargeSnailEnemyState state,
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

        state.StopMovementNextFrame = true;
        place.Position = place.Position.WithWholeX(previousPosition.X);
    }

    /// <summary>Carries out one cue of Oum's animations: the splash sound, attack completion and the attack-rotation window.</summary>
    private void ApplyMaridiaLargeSnailCue(MaridiaLargeSnailEnemyState state, MaridiaLargeSnailCue cue)
    {
        switch (cue.Kind)
        {
            case MaridiaLargeSnailCueKind.PlaySplash:
                _audio.QueueEnemySound(EnemySoundEffectDefinitions.MaridiaLargeSnailSplashSound, priority: SoundRequestPriority.Normal);
                break;
            case MaridiaLargeSnailCueKind.FinishAttack:
                if (state.Action != MaridiaLargeSnailAction.Attacking)
                    throw new InvalidOperationException($"Oum's attack finished while it was {state.Action}.");
                state.Action = MaridiaLargeSnailAction.AttackFinished;
                break;
            case MaridiaLargeSnailCueKind.AllowAttackRotation:
                state.AttackAllowsRotation = true;
                break;
            case MaridiaLargeSnailCueKind.DisallowAttackRotation:
                state.AttackAllowsRotation = false;
                break;
            default:
                throw new InvalidOperationException($"Undefined {nameof(MaridiaLargeSnailCueKind)} {cue.Kind}.");
        }
    }

    /// <summary>The roll Oum shows: facing as it faced when the roll began, forwards or, after a wall, backwards.</summary>
    private static MaridiaLargeSnailSegment MaridiaLargeSnailRoll(MaridiaLargeSnailEnemyState state) => state.RollsBackwards
        ? MaridiaLargeSnailAnimationChoices.RollingBackwards(state.Facing)
        : MaridiaLargeSnailAnimationChoices.RollingForwards(state.Facing);

    /// <summary>The side of Oum Samus is on: left when she is to its left, otherwise right (#2338).</summary>
    private static HorizontalDirection SamusSide(SamusState samus, RoomEnemyPlace place) =>
        samus.Position.X.Whole < place.Position.X.Whole ? HorizontalDirection.Left : HorizontalDirection.Right;

    /// <summary>
    /// Starts <paramref name="segment"/>'s animation only when it differs from the one the snail plays. While the snail
    /// still shows its spawn idle it counts as playing the right-facing idle, and its first start of another animation, the
    /// left-facing idle included, starts it afresh (<see cref="MaridiaLargeSnailEnemyState.ShowsSpawnIdle"/>).
    /// </summary>
    internal static void StartMaridiaLargeSnailAnimation(
        RoomEnemyPlace place,
        MaridiaLargeSnailEnemyState state,
        MaridiaLargeSnailSegment segment)
    {
        Animation<MaridiaLargeSnailSegment> animation = MaridiaLargeSnailAnimations.StartingWith(segment);
        if (!state.ShowsSpawnIdle)
        {
            state.Playback = EnemyAnimationPlayback.StartUnlessPlaying(place, state.Playback, animation);
            return;
        }
        if (segment == MaridiaLargeSnailSegment.FacingRightIdle)
            return;
        state.ShowsSpawnIdle = false;
        state.Playback = EnemyAnimationPlayback.Start(place, animation);
    }

    /// <summary>
    /// Shoves Samus away from Oum when she touches it without standing on it, after any damage its hitbox does.
    /// </summary>
    private static void ResolveMaridiaLargeSnailTouchAfterCommon(
        RoomEnemyPlace place,
        SamusState samus)
    {
        if (EnemyCollision.IsSamusStandingOn(place, samus))
            return;

        int shove = samus.Position.X.Whole < place.Position.X.Whole
            ? -MaridiaLargeSnailMotionDefinitions.ShoveDistance
            : MaridiaLargeSnailMotionDefinitions.ShoveDistance;
        samus.Kinematics.ExtraDisplacement += new Offset16<RoomPixels>(shove, 0);
    }

    /// <summary>
    /// Plays the Oum's shot sound after the common shot damage, for damaging, freezing, and zero-damage vulnerability
    /// results alike.
    /// </summary>
    private void ResolveMaridiaLargeSnailShotAfterCommon() =>
        _audio.QueueEnemySound(EnemySoundEffectDefinitions.MaridiaLargeSnailShotSound, priority: SoundRequestPriority.Normal);

    /// <summary>Publishes Oum's shot sound after common bomb damage.</summary>
    internal EnemyBombReaction MaridiaLargeSnailBombReaction => field ??= new()
    {
        AfterCommon = _ => ResolveMaridiaLargeSnailShotAfterCommon(),
    };

    /// <summary>Runs the Maridia large snail's main AI.</summary>
    internal void RunMaridiaLargeSnailMainAi(RoomEnemyPlace place, IEnemyAiFrame frame)
    {
        RunMaridiaLargeSnailMain(
            place,
            place.RequireSpeciesState<MaridiaLargeSnailEnemyState>(),
            frame.Samus,
            frame.Level,
            frame.ControllerInput);
    }

    /// <summary>Advances the Maridia large snail's typed animation.</summary>
    internal void AdvanceMaridiaLargeSnailTypedAnimation(RoomEnemyPlace place, EnemyAnimationCueContext _) =>
        EnemyAnimationPlayback.Advance(place, MaridiaLargeSnailAnimations.Segments,
            static (MaridiaLargeSnailEnemyState state) => state.Playback, static (state, playback) => state.Playback = playback,
            (state, cue) =>
            {
                ApplyMaridiaLargeSnailCue(state, cue);
                return null;
            });

    /// <summary>A hit on the protected shell does nothing; only a vulnerable hitbox takes common damage.</summary>
    internal EnemyShotReaction MaridiaLargeSnailShotReaction => field ??= new()
    {
        AfterSuperMissileQuake = (in EnemyShotContext c) =>
        {
            if (!c.UsesFrameHitboxes)
                throw new InvalidDataException("Maridia Large Snail's shots no longer meet its frames' hitboxes.");
            if (c.HitboxShot == EnemyReaction.Ignore)
            {
                // Frame-hitbox collision marks the projectile, and nothing more happens: the protected shell
                // takes no shot damage, makes no impact, plays no shot sound and has no vulnerability.
                c.Projectiles.EnemyImpacts.ApplyEnemyCollisionPrelude(c.Projectile, c.MarksCollision);
                return true;
            }
            if (c.HitboxShot != MaridiaLargeSnailReactions.Shot)
            {
                throw new InvalidDataException(
                    $"Maridia Large Snail has no response to shot reaction " +
                    $"{c.HitboxShot?.ToString() ?? "(unnamed)"}.");
            }
            return false;
        },
        AfterCommon = _ => ResolveMaridiaLargeSnailShotAfterCommon(),
    };

    /// <summary>Reacts to Samus touching a damaging or non-damaging Oum hitbox, each with its own push.</summary>
    internal void ReactToMaridiaLargeSnailTouch(EnemyTouchContext context)
    {
        if (!context.UsesFrameHitboxes)
            throw new InvalidDataException("Maridia Large Snail's contact no longer meets its frames' hitboxes.");
        if ((context.HitboxTouch != MaridiaLargeSnailReactions.DamagingTouch && context.HitboxTouch != MaridiaLargeSnailReactions.NonDamagingTouch))
        {
            throw new InvalidDataException(
                $"Maridia Large Snail has no response to touch reaction " +
                $"{context.HitboxTouch?.ToString() ?? "(unnamed)"}.");
        }

        // The damaging touch hurts Samus as any enemy does and then shoves her as the harmless touch does. A harmless
        // touch only shoves, which is how the closed shell carries and pushes Samus without its 100 damage.
        if (context.HitboxTouch == MaridiaLargeSnailReactions.DamagingTouch)
            _collision.ResolveNormalEnemyTouch(context);
        ResolveMaridiaLargeSnailTouchAfterCommon(context.Place, context.Samus);
    }

    /// <summary>The reaction of the first of the occupant's hitboxes the probe overlaps on the frame it shows.</summary>
    /// <exception cref="InvalidDataException">The frame shown has no Oum collision.</exception>
    internal static bool? FindHitboxReaction(RoomEnemyPlace enemy, EnemyHitboxProbe probe, out EnemyReaction? reaction)
    {
        reaction = null;

        // The frame's hitboxes are read in order, and the first the probe overlaps gives the reaction.
        foreach (EnemyCollisionHitbox hitbox in
                 MaridiaLargeSnailCollisionDefinitions.Frames.Of(enemy.Frame))
        {
            int left = enemy.Position.X.Whole + hitbox.Left;
            int top = enemy.Position.Y.Whole + hitbox.Top;
            int right = enemy.Position.X.Whole + hitbox.Right;
            int bottom = enemy.Position.Y.Whole + hitbox.Bottom;
            if (!probe.Overlaps(left, top, right, bottom))
                continue;
            reaction = probe.SelectsShot ? hitbox.Shot : hitbox.Touch;
            return true;
        }
        return false;
    }
}
