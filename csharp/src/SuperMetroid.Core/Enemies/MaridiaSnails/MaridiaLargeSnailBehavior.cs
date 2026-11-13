using SuperMetroid.Core.Programs;
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
    public IEnemyEntity Create(RoomEnemySlot slot, EnemyCommonParticipation common) => new MaridiaLargeSnailEntity(this, slot, common);

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

    /// <summary>Strict horizontal range in which Oum can push or carry Samus.</summary>
    private const int MaridiaLargeSnailPushingXDistance = 24;
    /// <summary>Strict vertical range required for Samus to be in Oum's pushing band.</summary>
    private const int MaridiaLargeSnailPushingYDistance = 32;
    /// <summary>Strict horizontal range used when a completed cooldown permits attack.</summary>
    private const int MaridiaLargeSnailAttackXDistance = 32;
    /// <summary>Frames initially held before Oum may rotate into an attack.</summary>
    private const int MaridiaLargeSnailInitialAttackCooldown = 128;
    /// <summary>Number of impacts in Oum's landing or ledge-fall settling sequence.</summary>
    private const int MaridiaLargeSnailInitialBounceCount = 3;
    /// <summary>Per-frame 8.8 index change in the quadratic vertical speed table.</summary>
    private static readonly EnemyQuadraticSpeedIndex MaridiaLargeSnailBounceAcceleration = EnemyQuadraticSpeedIndex.FromTwoHundredFiftySixthsOfRow(384);
    /// <summary>Maximum falling speed-table index before the next impact.</summary>
    private static readonly EnemyQuadraticSpeedIndex MaridiaLargeSnailMaximumFallIndex = EnemyQuadraticSpeedIndex.FromRows(64);
    /// <summary>Speed-table index removed at each floor impact.</summary>
    private static readonly EnemyQuadraticSpeedIndex MaridiaLargeSnailImpactIndexLoss = EnemyQuadraticSpeedIndex.FromRows(16);

    /// <summary>Enemy-library sound queued by Oum's attack splash.</summary>
    private static readonly SoundEffectId MaridiaLargeSnailSplashSound =
        EnemySoundEffectDefinitions.MaridiaLargeSnailSplashSound;
    /// <summary>Enemy-library sound queued after a shot hits Oum.</summary>
    private static readonly SoundEffectId MaridiaLargeSnailShotSound =
        EnemySoundEffectDefinitions.MaridiaLargeSnailShotSound;

    /// <summary>Initializes the Oum.</summary>
    internal static void InitializeMaridiaLargeSnail(RoomEnemySlot slot)
    {
        var state = new MaridiaLargeSnailEnemyState
        {
            RequestedAnimation = MaridiaLargeSnailAnimation.FacingRightIdle,
            InstalledAnimation = MaridiaLargeSnailAnimation.FacingRightIdle,
            AttackAnimationFinished = false,
            AttackAllowsRotation = false,
            SamusInPushingRange = false,
            SamusIsLeft = false,
            PreviousPosition = Position16<RoomPixels>.Zero,
            StopMovementNextFrame = false,
            Function = MaridiaLargeSnailEnemyFunction.Idle,
            YSpeedTableIndex = EnemyQuadraticSpeedIndex.Start,
            AttackCooldown = MaridiaLargeSnailInitialAttackCooldown,
            MovingLeft = false,
            RemainingBounces = MaridiaLargeSnailInitialBounceCount,
            BounceFunction = MaridiaLargeSnailBounceFunction.Falling,
        };
        slot.SetSpeciesState(state);

        // The initial left-facing animation intentionally disagrees with the installed right-facing idle. Because asking for
        // the installed animation again starts nothing, this oddity is observable until the snail first asks for another.
        state.Playback = new(MaridiaLargeSnailAnimations.StartingWith(MaridiaLargeSnailProgram.FacingLeftIdle), AnimationPlace.StartOfAnimation);
    }

    /// <summary>
    /// Runs the Oum's main behaviour and functions. Ordering is exact: proximity and
    /// previous positions, behavior, Samus carrying, then the input-derived stop flag.
    /// </summary>
    private void RunMaridiaLargeSnailMain(
        RoomEnemySlot slot,
        MaridiaLargeSnailEnemyState state,
        SamusState? samus,
        RoomLevelData? level,
        ControllerButton controllerInput)
    {
        if (samus is null)
            throw new InvalidOperationException("Maridia Large Snail AI requires the active Samus actor.");
        if (level is null)
            throw new InvalidOperationException("Maridia Large Snail AI requires room level data.");

        UpdateMaridiaLargeSnailSamusProximity(slot, state, samus);
        state.PreviousPosition = slot.Position.Whole;

        switch (state.Function)
        {
            case MaridiaLargeSnailEnemyFunction.Idle:
                RunMaridiaLargeSnailIdle(slot, state, samus, level);
                break;
            case MaridiaLargeSnailEnemyFunction.Rolling:
                RunMaridiaLargeSnailRolling(slot, state, samus, level);
                break;
            case MaridiaLargeSnailEnemyFunction.Attacking:
                RunMaridiaLargeSnailAttack(slot, state);
                break;
            default:
                throw new InvalidDataException(
                    $"Maridia Large Snail function {state.Function?.ToString() ?? "none"} is not translated.");
        }

        CarrySamusWithMaridiaLargeSnail(slot, state, samus);
        UpdateMaridiaLargeSnailInputStop(slot, state, controllerInput);
    }

    /// <summary>Completes landing bounces, faces Samus, then starts rolling within the pushing range.</summary>
    private void RunMaridiaLargeSnailIdle(
        RoomEnemySlot slot,
        MaridiaLargeSnailEnemyState state,
        SamusState samus,
        RoomLevelData level)
    {
        if (state.RemainingBounces != 0)
        {
            RunMaridiaLargeSnailBounce(slot, state, level);
            return;
        }

        bool samusIsLeft = CoordinateAxis.Difference(samus.Position.X.Whole, slot.Position.X.Whole) < 0;
        if (samusIsLeft)
            state.MovingLeft = true;
        state.RequestedAnimation = MaridiaLargeSnailPrograms.Idle(samusIsLeft);
        InstallMaridiaLargeSnailAnimationScript(slot, state);

        if (!EnemyMath.IsWithinStrictModularDistance(
                samus.Position.X.Whole,
                slot.Position.X.Whole,
                MaridiaLargeSnailPushingXDistance))
        {
            return;
        }

        state.RequestedAnimation = MaridiaLargeSnailPrograms.RollingFrom(state.RequestedAnimation);
        InstallMaridiaLargeSnailAnimationScript(slot, state);
        state.Function = MaridiaLargeSnailEnemyFunction.Rolling;
    }

    /// <summary>Handles ledges, cooldown and attack selection before horizontal terrain movement.</summary>
    private void RunMaridiaLargeSnailRolling(
        RoomEnemySlot slot,
        MaridiaLargeSnailEnemyState state,
        SamusState samus,
        RoomLevelData level)
    {
        if (state.RemainingBounces != 0)
        {
            RunMaridiaLargeSnailBounce(slot, state, level);
        }
        else if (!_terrain.MoveEnemyVertically(level, slot, SignedSixteenSixteen.FromWholePixels(1)))
        {
            // No floor one pixel below means Oum walked off a ledge. Retail restarts the
            // complete three-impact settling sequence without skipping horizontal work.
            state.YSpeedTableIndex = EnemyQuadraticSpeedIndex.Start;
            state.BounceFunction = MaridiaLargeSnailBounceFunction.Falling;
            state.RemainingBounces = MaridiaLargeSnailInitialBounceCount;
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
                    slot.Position.X.Whole,
                    MaridiaLargeSnailAttackXDistance) &&
                state.AttackAllowsRotation &&
                state.RemainingBounces == 0;
            if (mayAttack)
            {
                state.MovingLeft = CoordinateAxis.Difference(samus.Position.X.Whole, slot.Position.X.Whole) < 0;
                state.AttackCooldown = MaridiaLargeSnailInitialAttackCooldown;
                state.RequestedAnimation = MaridiaLargeSnailPrograms.Attacking(state.MovingLeft);
                InstallMaridiaLargeSnailAnimationScript(slot, state);
                state.Function = MaridiaLargeSnailEnemyFunction.Attacking;
                return;
            }
        }

        MoveMaridiaLargeSnailHorizontally(slot, state, level);
    }

    /// <summary>Resumes the rolling list after the attack animation marks itself complete.</summary>
    private static void RunMaridiaLargeSnailAttack(
        RoomEnemySlot slot,
        MaridiaLargeSnailEnemyState state)
    {
        if (!state.AttackAnimationFinished)
            return;

        state.AttackAnimationFinished = false;
        state.RequestedAnimation = MaridiaLargeSnailPrograms.RollingAfter(state.RequestedAnimation);
        InstallMaridiaLargeSnailAnimationScript(slot, state);
        state.Function = MaridiaLargeSnailEnemyFunction.Rolling;
    }

    /// <summary>Alternates quadratic-speed fall and rise phases, reducing the index after each impact.</summary>
    private void RunMaridiaLargeSnailBounce(
        RoomEnemySlot slot,
        MaridiaLargeSnailEnemyState state,
        RoomLevelData level)
    {
        switch (state.BounceFunction)
        {
            case MaridiaLargeSnailBounceFunction.Falling:
                // The record stays within 3712 256ths of a record below the start through record 64, so the game's
                // signed comparisons are ordinary comparisons.
                state.YSpeedTableIndex = EnemyQuadraticSpeedIndex.Min(
                    state.YSpeedTableIndex + MaridiaLargeSnailBounceAcceleration,
                    MaridiaLargeSnailMaximumFallIndex);

                if (!_terrain.MoveEnemyVertically(
                        level,
                        slot,
                        state.YSpeedTableIndex.Row.Positive))
                {
                    return;
                }

                state.RemainingBounces--;
                // An impact that leaves the record below zero ends the bounces; the negative record is kept.
                state.YSpeedTableIndex -= MaridiaLargeSnailImpactIndexLoss;
                if (state.YSpeedTableIndex.IsNegative)
                    state.RemainingBounces = 0;
                state.BounceFunction = MaridiaLargeSnailBounceFunction.Rising;
                return;

            case MaridiaLargeSnailBounceFunction.Rising:
                state.YSpeedTableIndex -= MaridiaLargeSnailBounceAcceleration;
                if (state.YSpeedTableIndex.IsNegative)
                {
                    state.YSpeedTableIndex = EnemyQuadraticSpeedIndex.Start;
                    state.BounceFunction = MaridiaLargeSnailBounceFunction.Falling;
                    return;
                }

                // The rising half reads the negative member of the same quadratic row. The record is not
                // negative here, so its whole records are the row the game's sign-clearing AND selects.
                _terrain.MoveEnemyVertically(
                    level,
                    slot,
                    state.YSpeedTableIndex.Row.Negative);
                return;

            default:
                throw new InvalidDataException(
                    $"Maridia Large Snail bounce function " +
                    $"{state.BounceFunction?.ToString() ?? "none"} is not translated.");
        }
    }

    /// <summary>Temporarily extends the collision probe toward Samus, then retracts it after wall resolution.</summary>
    private void MoveMaridiaLargeSnailHorizontally(
        RoomEnemySlot slot,
        MaridiaLargeSnailEnemyState state,
        RoomLevelData level)
    {
        EnemyLinearSpeedRow speeds = EnemyLinearSpeeds.Row(EnemyLinearSpeedDefinitions.OnePixelPerFrameRow);
        SignedSixteenSixteen speed = state.MovingLeft ? speeds.Negative : speeds.Positive;
        bool temporarilyReachTowardSamus = state.SamusInPushingRange &&
            state.MovingLeft == state.SamusIsLeft;
        if (temporarilyReachTowardSamus)
        {
            // Temporarily extends the whole-pixel move sixteen pixels toward Samus,
            // performs collision, then retracts the center. Net velocity remains the table
            // speed while wall clipping naturally transfers the lost distance to Samus.
            speed = speed.WithWholePixels(speed.WholePixels + (state.MovingLeft ? -16 : 16));
        }

        SignedSixteenSixteen displacement = speed;
        bool collided = _terrain.MoveEnemyHorizontallyTreatingSlopesAsWalls(
            level,
            slot,
            displacement);

        if (temporarilyReachTowardSamus)
        {
            slot.Position = slot.Position.WithWrappedWholeX(
                slot.Position.X.Whole + (state.MovingLeft ? 16 : -16));
        }

        if (!collided)
            return;

        state.MovingLeft = !state.MovingLeft;
        state.RequestedAnimation = MaridiaLargeSnailPrograms.Reversed(state.RequestedAnimation);
        InstallMaridiaLargeSnailAnimationScript(slot, state);
    }

    /// <summary>Captures whether Samus is within both push ranges and on which side.</summary>
    private static void UpdateMaridiaLargeSnailSamusProximity(
        RoomEnemySlot slot,
        MaridiaLargeSnailEnemyState state,
        SamusState samus)
    {
        state.SamusInPushingRange = false;
        state.SamusIsLeft = false;
        if (!EnemyMath.IsWithinStrictModularDistance(
                samus.Position.Y.Whole,
                slot.Position.Y.Whole,
                MaridiaLargeSnailPushingYDistance) ||
            !EnemyMath.IsWithinStrictModularDistance(
                samus.Position.X.Whole,
                slot.Position.X.Whole,
                MaridiaLargeSnailPushingXDistance))
        {
            return;
        }

        state.SamusInPushingRange = true;
        state.SamusIsLeft = CoordinateAxis.Difference(samus.Position.X.Whole, slot.Position.X.Whole) < 0;
    }

    /// <summary>Publishes Oum's vertical platform motion and sideward push into Samus's extra displacement.</summary>
    private static void CarrySamusWithMaridiaLargeSnail(
        RoomEnemySlot slot,
        MaridiaLargeSnailEnemyState state,
        SamusState samus)
    {
        state.StopMovementNextFrame = false;
        if (MaridiaLargeSnailTouchesSamusFromBelow(slot, samus))
        {
            // Unlike most moving-platform producers, Oum assigns—not adds—the vertical
            // displacement. This preserves the last touching Oum's ownership.
            samus.Kinematics.ExtraDisplacement = samus.Kinematics.ExtraDisplacement.WithWholeY(CoordinateAxis.Difference(
                slot.Position.Y.Whole, state.PreviousPosition.Y));
        }

        if (!state.SamusInPushingRange)
            return;

        int deltaX = CoordinateAxis.Wrap(slot.Position.X.Whole - state.PreviousPosition.X);
        bool movedLeft = CoordinateAxis.AsSigned(deltaX) < 0;
        if (movedLeft != state.SamusIsLeft)
            return;

        samus.Kinematics.ExtraDisplacement += new Offset16<RoomPixels>(CoordinateAxis.AsSigned(deltaX), 0);
    }

    /// <summary>Records an opposing controller direction to stop the following frame's roll.</summary>
    private static void UpdateMaridiaLargeSnailInputStop(
        RoomEnemySlot slot,
        MaridiaLargeSnailEnemyState state,
        ControllerButton controllerInput)
    {
        if (!state.SamusInPushingRange)
            return;

        int deltaX = CoordinateAxis.Difference(slot.Position.X.Whole, state.PreviousPosition.X);
        bool stop;
        if (deltaX == 0)
        {
            stop = state.SamusIsLeft
                ? state.MovingLeft &&
                    controllerInput.HasAny(ControllerButton.Right)
                : !state.MovingLeft &&
                    controllerInput.HasAny(ControllerButton.Left);
        }
        else if (deltaX < 0)
        {
            stop = controllerInput.HasAny(ControllerButton.Right);
        }
        else
        {
            stop = controllerInput.HasAny(ControllerButton.Left);
        }

        if (!stop)
            return;

        state.StopMovementNextFrame = true;
        slot.Position = slot.Position.WithWholeX(state.PreviousPosition.X);
    }

    /// <summary>
    /// Exact asymmetric "touching from below" predicate used for Oum carrying and
    /// its post-contact shove. It is intentionally not interchangeable with radius overlap.
    /// </summary>
    private static bool MaridiaLargeSnailTouchesSamusFromBelow(
        RoomEnemySlot slot,
        SamusState samus)
    {
        int xDistance = CoordinateAxis.Distance(
            samus.Position.X.Whole, slot.Position.X.Whole);
        bool centerWithinSamusRadius = xDistance < samus.Kinematics.Radius.X;
        int remainingX = CoordinateAxis.Wrap(xDistance - samus.Kinematics.Radius.X);
        if (!centerWithinSamusRadius && remainingX >= slot.Radius.X)
            return false;

        int biasedSamusY = CoordinateAxis.Wrap(samus.Position.Y.Whole + 3);
        if (CoordinateAxis.Difference(biasedSamusY, slot.Position.Y.Whole) >= 0)
            return false;

        int verticalDistance = CoordinateAxis.Wrap(slot.Position.Y.Whole - biasedSamusY);
        int remainingY = CoordinateAxis.Wrap(verticalDistance - samus.Kinematics.Radius.Y);
        return verticalDistance < samus.Kinematics.Radius.Y ||
            remainingY == slot.Radius.Y ||
            remainingY < slot.Radius.Y;
    }

    /// <summary>Carries out one cue of Oum's animations: the splash sound, attack completion and the attack-rotation window.</summary>
    private void ApplyMaridiaLargeSnailCue(MaridiaLargeSnailEnemyState state, MaridiaLargeSnailCue cue)
    {
        switch (cue.Kind)
        {
            case MaridiaLargeSnailCueKind.PlaySplash:
                _audio.QueueEnemySound(MaridiaLargeSnailSplashSound, priority: SoundRequestPriority.Normal);
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

    /// <summary>Starts the requested animation only when it differs from the one the snail last started.</summary>
    internal static void InstallMaridiaLargeSnailAnimationScript(
        RoomEnemySlot slot,
        MaridiaLargeSnailEnemyState state)
    {
        if (state.RequestedAnimation == state.InstalledAnimation)
            return;
        state.InstalledAnimation = state.RequestedAnimation;
        state.Playback = EnemyProgramAnimation.Start(slot, MaridiaLargeSnailAnimations.StartingWith(MaridiaLargeSnailPrograms
            .Program(state.RequestedAnimation)));
    }

    /// <summary>
    /// Runs the common/non-damaging Oum hitbox tail after the frame-hitbox dispatcher selected a
    /// concrete frame hitbox. Damaging hitboxes call common touch before entering this tail.
    /// </summary>
    private static void ResolveMaridiaLargeSnailTouchAfterCommon(
        RoomEnemySlot slot,
        SamusState samus)
    {
        if (MaridiaLargeSnailTouchesSamusFromBelow(slot, samus))
            return;

        int shove = CoordinateAxis.Difference(samus.Position.X.Whole, slot.Position.X.Whole) < 0
            ? -4
            : 4;
        samus.Kinematics.ExtraDisplacement += new Offset16<RoomPixels>(shove, 0);
    }

    /// <summary>
    /// Runs the private tail of the Oum's shot reaction. It runs after common shot
    /// AI for damaging, freezing, and zero-damage vulnerability results alike.
    /// </summary>
    private void ResolveMaridiaLargeSnailShotAfterCommon() =>
        _audio.QueueEnemySound(MaridiaLargeSnailShotSound, priority: SoundRequestPriority.Normal);

    /// <summary>Publishes Oum's shot sound after common bomb damage.</summary>
    internal void AddMaridiaLargeSnailBombReactions(EnemySharedReactions table)
    {
        table.AddBombCommon(new EnemyBombReaction(c => c.Enemy.DefinitionId == EnemyDefinitionId.Oum &&
                                                       c.SelectedShotAi == EnemyReaction.MaridiaLargeSnailShot)
        {
            AfterCommon = _ => ResolveMaridiaLargeSnailShotAfterCommon(),
        });
    }

    /// <summary>Runs the Maridia large snail's main AI.</summary>
    internal void RunMaridiaLargeSnailMainAi(RoomEnemySlot slot, IEnemyAiFrame frame)
    {
        RunMaridiaLargeSnailMain(
            slot,
            slot.RequireSpeciesState<MaridiaLargeSnailEnemyState>(),
            frame.Samus,
            frame.Level,
            frame.ControllerInput);
    }

    /// <summary>Advances the Maridia large snail's typed animation.</summary>
    internal void AdvanceMaridiaLargeSnailTypedAnimation(RoomEnemySlot slot, EnemyInstructionContext _) =>
        EnemyProgramAnimation.Advance(slot, MaridiaLargeSnailAnimations.Segments,
            static (MaridiaLargeSnailEnemyState state) => state.Playback, static (state, playback) => state.Playback = playback,
            (state, cue) =>
            {
                ApplyMaridiaLargeSnailCue(state, cue);
                return null;
            });

    /// <summary>Preserves protected-shell no-op hits and runs common damage only for vulnerable hitboxes.</summary>
    internal void AddMaridiaLargeSnailShotReactions(EnemySharedReactions table)
    {
        table.AddShot(new EnemyShotReaction(enemy => enemy.DefinitionId == EnemyDefinitionId.Oum &&
                                                     EnemyCollision.ShotAi(enemy) == EnemyReaction.MaridiaLargeSnailShot)
        {
            AfterSuperMissileQuake = (in EnemyShotContext c) =>
            {
                if (!c.UsesFrameHitboxes)
                    throw new InvalidDataException("Maridia Large Snail's shots no longer meet its frames' hitboxes.");
                if (c.HitboxShotAi == EnemyReaction.Ignore)
                {
                    // Frame-hitbox collision marks the projectile before calling the
                    // no-op. It does not run normal shot AI, create an impact, play the shot sound,
                    // or consult vulnerability for this protected shell component.
                    c.Projectiles.EnemyImpacts.ApplyFrameHitboxCollisionPrelude(c.Projectile, c.MarksCollision);
                    return true;
                }
                if (c.HitboxShotAi != EnemyReaction.MaridiaLargeSnailShot)
                {
                    throw new InvalidDataException(
                        $"Maridia Large Snail hitbox shot AI " +
                        $"{c.HitboxShotAi?.ToString() ?? "(unnamed)"} is not translated.");
                }
                return false;
            },
            AfterCommon = _ => ResolveMaridiaLargeSnailShotAfterCommon(),
        });
    }

    /// <summary>Dispatches damaging and non-damaging Oum hitboxes with their distinct Samus pushes.</summary>
    internal void AddMaridiaLargeSnailTouch(EnemySharedReactions table)
    {
        table.AddTouch(slot => slot.DefinitionId == EnemyDefinitionId.Oum &&
                               EnemyCollision.TouchAi(slot) == EnemyReaction.MaridiaLargeSnailNonDamagingTouch,
            context =>
            {
                if (!context.UsesFrameHitboxes)
                    throw new InvalidDataException("Maridia Large Snail's contact no longer meets its frames' hitboxes.");
                if (context.HitboxTouchAi is not (
                        EnemyReaction.MaridiaLargeSnailDamagingTouch or
                        EnemyReaction.MaridiaLargeSnailNonDamagingTouch))
                {
                    throw new InvalidDataException(
                        $"Maridia Large Snail hitbox touch AI " +
                        $"{context.HitboxTouchAi?.ToString() ?? "(unnamed)"} is not translated.");
                }

                // The damaging touch enters common touch damage and falls directly through the harmless touch's
                // asymmetric shove. Harmless-touch frames execute only the shove, which is why the
                // closed shell can carry/push Samus without inflicting the definition's 100 HP.
                if (context.HitboxTouchAi == EnemyReaction.MaridiaLargeSnailDamagingTouch)
                    _collision.ResolveNormalEnemyTouch(context.Slot, context.Samus);
                ResolveMaridiaLargeSnailTouchAfterCommon(context.Slot, context.Samus);
            });
    }

    /// <summary>The reaction of the first of the occupant's hitboxes the probe overlaps on the frame it shows.</summary>
    /// <exception cref="InvalidDataException">The frame shown has no compiled Oum collision.</exception>
    internal static bool? FindHitboxReaction(RoomEnemySlot enemy, EnemyHitboxProbe probe, out EnemyReaction? reaction)
    {
        reaction = null;

        // Oum's 30 frames each have one (0,0) component, but the
        // ordered regions change with roll and attack direction. Keep
        // those rectangles and callback identities independent of the drawn sprite pieces.
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
