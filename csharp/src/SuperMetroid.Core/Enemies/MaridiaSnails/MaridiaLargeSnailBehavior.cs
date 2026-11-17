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
            FacesLeft = false,
            RollsBackwards = false,
            ShowsSpawnIdle = true,
            AttackAnimationFinished = false,
            AttackAllowsRotation = false,
            SamusInPushingRange = false,
            SamusIsLeft = false,
            PreviousPosition = Position16<RoomPixels>.Zero,
            StopMovementNextFrame = false,
            YSpeedCurvePlace = QuadraticSpeedCurvePlace.Start,
            AttackCooldown = MaridiaLargeSnailMotionDefinitions.InitialAttackCooldown,
            MovingLeft = false,
            RemainingBounces = MaridiaLargeSnailMotionDefinitions.SettlingBounces,
        };
        place.SetSpeciesState(state);

        // The spawn's left-facing idle disagrees with the right-facing Oum it counts as (ShowsSpawnIdle), which is
        // observable until the snail first starts an animation.
        state.Playback = new(MaridiaLargeSnailAnimations.StartingWith(MaridiaLargeSnailSegment.FacingLeftIdle), AnimationPlace.StartOfAnimation);
    }

    /// <summary>
    /// Runs the Oum's main behaviour and actions. Ordering is exact: proximity and
    /// previous positions, behavior, Samus carrying, then the input-derived stop flag.
    /// </summary>
    private void RunMaridiaLargeSnailMain(
        RoomEnemyPlace place,
        MaridiaLargeSnailEnemyState state,
        SamusState samus,
        RoomLevelData level,
        GameplayAction controllerInput)
    {
        UpdateMaridiaLargeSnailSamusProximity(place, state, samus);
        state.PreviousPosition = place.Position.Whole;

        switch (state.Action)
        {
            case MaridiaLargeSnailAction.Idle:
                RunMaridiaLargeSnailIdle(place, state, samus, level);
                break;
            case MaridiaLargeSnailAction.Rolling:
                RunMaridiaLargeSnailRolling(place, state, samus, level);
                break;
            case MaridiaLargeSnailAction.Attacking:
                RunMaridiaLargeSnailAttack(place, state);
                break;
            default:
                throw new InvalidDataException(
                    $"Maridia Large Snail has no behaviour for action {state.Action}.");
        }

        CarrySamusWithMaridiaLargeSnail(place, state, samus);
        UpdateMaridiaLargeSnailInputStop(place, state, controllerInput);
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

        bool samusIsLeft = CoordinateAxis.Difference(samus.Position.X.Whole, place.Position.X.Whole) < 0;
        if (samusIsLeft)
            state.MovingLeft = true;
        state.FacesLeft = samusIsLeft;
        StartMaridiaLargeSnailAnimation(place, state, MaridiaLargeSnailAnimationChoices.Idle(state.FacesLeft));

        if (!EnemyMath.IsWithinStrictModularDistance(
                samus.Position.X.Whole,
                place.Position.X.Whole,
                MaridiaLargeSnailMotionDefinitions.PushingReachX))
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
        RoomLevelData level)
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

        // StopMovementNextFrame was produced at the end of the preceding main-AI call. The
        // current frame clears/recomputes it only after this behavior returns.
        if (state.StopMovementNextFrame)
            return;

        state.AttackCooldown--;
        bool cooldownExpired = state.AttackCooldown < 0;
        if (cooldownExpired)
        {
            state.AttackCooldown = 0;
            bool mayAttack = EnemyMath.IsWithinStrictModularDistance(
                    samus.Position.X.Whole,
                    place.Position.X.Whole,
                    MaridiaLargeSnailMotionDefinitions.AttackReach) &&
                state.AttackAllowsRotation &&
                state.RemainingBounces == 0;
            if (mayAttack)
            {
                state.MovingLeft = CoordinateAxis.Difference(samus.Position.X.Whole, place.Position.X.Whole) < 0;
                state.AttackCooldown = MaridiaLargeSnailMotionDefinitions.InitialAttackCooldown;
                state.FacesLeft = state.MovingLeft;
                StartMaridiaLargeSnailAnimation(place, state, MaridiaLargeSnailAnimationChoices.Attacking(state.FacesLeft));
                state.Action = MaridiaLargeSnailAction.Attacking;
                return;
            }
        }

        MoveMaridiaLargeSnailHorizontally(place, state, level);
    }

    /// <summary>Resumes the rolling animation after the attack animation marks itself complete.</summary>
    private static void RunMaridiaLargeSnailAttack(
        RoomEnemyPlace place,
        MaridiaLargeSnailEnemyState state)
    {
        if (!state.AttackAnimationFinished)
            return;

        state.AttackAnimationFinished = false;
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
                // The place stays within 3712 256ths of a mark below the start through mark 64, so the game's
                // signed comparisons are ordinary comparisons.
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

                // The rising half reads the negative member of the same curve mark. The place is not
                // negative here, so its whole marks are the mark the game selects by the speed's magnitude.
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
        RoomLevelData level)
    {
        SignedSixteenSixteen speed = state.MovingLeft ? -MaridiaLargeSnailMotionDefinitions.RollSpeed : MaridiaLargeSnailMotionDefinitions.RollSpeed;
        bool temporarilyReachTowardSamus = state.SamusInPushingRange &&
            state.MovingLeft == state.SamusIsLeft;
        if (temporarilyReachTowardSamus)
        {
            // Temporarily extends the whole-pixel move toward Samus by the push's reach,
            // performs collision, then retracts the center. Net velocity remains one
            // pixel per frame while wall clipping naturally passes the lost distance to Samus.
            speed = speed.WithWholePixels(speed.WholePixels + (state.MovingLeft
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
            place.Position = place.Position.WithWrappedWholeX(
                place.Position.X.Whole + (state.MovingLeft
                    ? MaridiaLargeSnailMotionDefinitions.PushReach
                    : -MaridiaLargeSnailMotionDefinitions.PushReach));
        }

        if (!collided)
            return;

        state.MovingLeft = !state.MovingLeft;
        state.RollsBackwards = !state.RollsBackwards;
        StartMaridiaLargeSnailAnimation(place, state, MaridiaLargeSnailRoll(state));
    }

    /// <summary>Captures whether Samus is within both push ranges and on which side.</summary>
    private static void UpdateMaridiaLargeSnailSamusProximity(
        RoomEnemyPlace place,
        MaridiaLargeSnailEnemyState state,
        SamusState samus)
    {
        state.SamusInPushingRange = false;
        state.SamusIsLeft = false;
        if (!EnemyMath.IsWithinStrictModularDistance(
                samus.Position.Y.Whole,
                place.Position.Y.Whole,
                MaridiaLargeSnailMotionDefinitions.PushingReachY) ||
            !EnemyMath.IsWithinStrictModularDistance(
                samus.Position.X.Whole,
                place.Position.X.Whole,
                MaridiaLargeSnailMotionDefinitions.PushingReachX))
        {
            return;
        }

        state.SamusInPushingRange = true;
        state.SamusIsLeft = CoordinateAxis.Difference(samus.Position.X.Whole, place.Position.X.Whole) < 0;
    }

    /// <summary>Publishes Oum's vertical platform motion and sideward push into Samus's extra displacement.</summary>
    private static void CarrySamusWithMaridiaLargeSnail(
        RoomEnemyPlace place,
        MaridiaLargeSnailEnemyState state,
        SamusState samus)
    {
        state.StopMovementNextFrame = false;
        if (EnemyCollision.IsSamusStandingOn(place, samus))
        {
            // Unlike most moving-platform producers, Oum assigns—not adds—the vertical
            // displacement. This preserves the last touching Oum's ownership.
            samus.Kinematics.ExtraDisplacement = samus.Kinematics.ExtraDisplacement.WithWholeY(CoordinateAxis.Difference(
                place.Position.Y.Whole, state.PreviousPosition.Y));
        }

        if (!state.SamusInPushingRange)
            return;

        int deltaX = CoordinateAxis.Wrap(place.Position.X.Whole - state.PreviousPosition.X);
        bool movedLeft = CoordinateAxis.AsSigned(deltaX) < 0;
        if (movedLeft != state.SamusIsLeft)
            return;

        samus.Kinematics.ExtraDisplacement += new Offset16<RoomPixels>(CoordinateAxis.AsSigned(deltaX), 0);
    }

    /// <summary>Records an opposing controller direction to stop the following frame's roll.</summary>
    private static void UpdateMaridiaLargeSnailInputStop(
        RoomEnemyPlace place,
        MaridiaLargeSnailEnemyState state,
        GameplayAction controllerInput)
    {
        if (!state.SamusInPushingRange)
            return;

        int deltaX = CoordinateAxis.Difference(place.Position.X.Whole, state.PreviousPosition.X);
        bool stop;
        if (deltaX == 0)
        {
            stop = state.SamusIsLeft
                ? state.MovingLeft &&
                    controllerInput.HasAny(GameplayAction.Right)
                : !state.MovingLeft &&
                    controllerInput.HasAny(GameplayAction.Left);
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
        place.Position = place.Position.WithWholeX(state.PreviousPosition.X);
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
                state.AttackAnimationFinished = true;
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
        ? MaridiaLargeSnailAnimationChoices.RollingBackwards(state.FacesLeft)
        : MaridiaLargeSnailAnimationChoices.RollingForwards(state.FacesLeft);

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
    /// Runs the common/non-damaging Oum hitbox tail after the frame-hitbox check selected a
    /// concrete frame hitbox. Damaging hitboxes call common touch before entering this tail.
    /// </summary>
    private static void ResolveMaridiaLargeSnailTouchAfterCommon(
        RoomEnemyPlace place,
        SamusState samus)
    {
        if (EnemyCollision.IsSamusStandingOn(place, samus))
            return;

        int shove = CoordinateAxis.Difference(samus.Position.X.Whole, place.Position.X.Whole) < 0
            ? -MaridiaLargeSnailMotionDefinitions.ShoveDistance
            : MaridiaLargeSnailMotionDefinitions.ShoveDistance;
        samus.Kinematics.ExtraDisplacement += new Offset16<RoomPixels>(shove, 0);
    }

    /// <summary>
    /// Runs the private tail of the Oum's shot reaction. It runs after common shot
    /// AI for damaging, freezing, and zero-damage vulnerability results alike.
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

    /// <summary>Preserves protected-shell no-op hits and runs common damage only for vulnerable hitboxes.</summary>
    internal EnemyShotReaction MaridiaLargeSnailShotReaction => field ??= new()
    {
        AfterSuperMissileQuake = (in EnemyShotContext c) =>
        {
            if (!c.UsesFrameHitboxes)
                throw new InvalidDataException("Maridia Large Snail's shots no longer meet its frames' hitboxes.");
            if (c.HitboxShot == EnemyReaction.Ignore)
            {
                // Frame-hitbox collision marks the projectile before calling the
                // no-op. It does not run normal shot AI, create an impact, play the shot sound,
                // or consult vulnerability for this protected shell component.
                c.Projectiles.EnemyImpacts.ApplyFrameHitboxCollisionPrelude(c.Projectile, c.MarksCollision);
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

        // The damaging touch enters common touch damage and falls directly through the harmless touch's
        // asymmetric shove. Harmless-touch frames execute only the shove, which is why the
        // closed shell can carry/push Samus without inflicting the definition's 100 HP.
        if (context.HitboxTouch == MaridiaLargeSnailReactions.DamagingTouch)
            _collision.ResolveNormalEnemyTouch(context);
        ResolveMaridiaLargeSnailTouchAfterCommon(context.Place, context.Samus);
    }

    /// <summary>The reaction of the first of the occupant's hitboxes the probe overlaps on the frame it shows.</summary>
    /// <exception cref="InvalidDataException">The frame shown has no compiled Oum collision.</exception>
    internal static bool? FindHitboxReaction(RoomEnemyPlace enemy, EnemyHitboxProbe probe, out EnemyReaction? reaction)
    {
        reaction = null;

        // The frame's hitboxes are read in order, and the first the probe overlaps gives the reaction.
        foreach (EnemyCollisionHitbox hitbox in
                 MaridiaLargeSnailCollisionDefinitions.Frames.Of(enemy.Frame))
        {
            int left = CoordinateAxis.Wrap(enemy.Position.X.Whole + hitbox.Left);
            int top = CoordinateAxis.Wrap(enemy.Position.Y.Whole + hitbox.Top);
            int right = CoordinateAxis.Wrap(enemy.Position.X.Whole + hitbox.Right);
            int bottom = CoordinateAxis.Wrap(enemy.Position.Y.Whole + hitbox.Bottom);
            if (!probe.Overlaps(left, top, right, bottom))
                continue;
            reaction = probe.SelectsShot ? hitbox.Shot : hitbox.Touch;
            return true;
        }
        return false;
    }
}
