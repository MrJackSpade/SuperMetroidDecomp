using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.MaridiaSnails.Artwork;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Enemies.MaridiaSnails;

/// <summary>The Maridia large snail species owner: its per-room state, AI routines and the interactions it registers.</summary>
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

    /// <summary>Binds terrain and contact owners for Oum's bounce and push behavior.</summary>
    internal MaridiaLargeSnailBehavior(EnemyTerrain terrain, EnemyCollision collision)
    {
        _terrain = terrain;
        _collision = collision;
    }

    /// <summary>The animation selector's left-facing choice: the right-facing program's selector plus one.</summary>
    private const int MaridiaLargeSnailFacingSelector = 1;
    /// <summary>Added to an idle selector to choose the same facing's forward roll.</summary>
    private const int MaridiaLargeSnailRollingSelector = 2;
    /// <summary>Selects the same facing's attack from idle, and toggles a roll between forwards and backwards.</summary>
    private const int MaridiaLargeSnailAttackSelector = 4;

    /// <summary>Strict horizontal range in which Oum can push or carry Samus.</summary>
    private const ushort MaridiaLargeSnailPushingXDistance = 24;
    /// <summary>Strict vertical range required for Samus to be in Oum's pushing band.</summary>
    private const ushort MaridiaLargeSnailPushingYDistance = 32;
    /// <summary>Strict horizontal range used when a completed cooldown permits attack.</summary>
    private const ushort MaridiaLargeSnailAttackXDistance = 32;
    /// <summary>Frames initially held before Oum may rotate into an attack.</summary>
    private const ushort MaridiaLargeSnailInitialAttackCooldown = 128;
    /// <summary>Number of impacts in Oum's landing or ledge-fall settling sequence.</summary>
    private const ushort MaridiaLargeSnailInitialBounceCount = 3;
    /// <summary>Per-frame 8.8 index change in the quadratic vertical speed table.</summary>
    private const int MaridiaLargeSnailBounceAcceleration = 384;
    /// <summary>Maximum falling speed-table index before the next impact.</summary>
    private const int MaridiaLargeSnailMaximumFallIndex = 16384;
    /// <summary>Speed-table index removed at each floor impact.</summary>
    private const int MaridiaLargeSnailImpactIndexLoss = 4096;

    /// <summary>Library-two sound queued by Oum's attack splash.</summary>
    private static readonly SoundEffectId MaridiaLargeSnailSplashSound =
        SoundEffectId.FromCartridge(SoundEffectLibrary.Library2, EnemySoundEffectDefinitions.MaridiaLargeSnailSplashSound);
    /// <summary>Library-two sound queued after a shot hits Oum.</summary>
    private static readonly SoundEffectId MaridiaLargeSnailShotSound =
        SoundEffectId.FromCartridge(SoundEffectLibrary.Library2, EnemySoundEffectDefinitions.MaridiaLargeSnailShotSound);

    /// <summary>Most recent library-two sound requested by Oum this frame.</summary>
    public SoundEffectId? LastMaridiaLargeSnailSoundEffect { get; private set; }

    /// <summary>Ports <c>InitAI_Oum</c> at $A2:CCD4.</summary>
    internal static void InitializeMaridiaLargeSnail(RoomEnemySlot slot)
    {
        var state = new MaridiaLargeSnailEnemyState
        {
            RequestedInstructionListIndex = 0,
            InstalledInstructionListIndex = 0,
            AttackAnimationFinished = false,
            AttackAllowsRotation = false,
            SamusInPushingRange = false,
            SamusIsLeft = false,
            PreviousPosition = Position16<RoomPixels>.Zero,
            StopMovementNextFrame = false,
            Function = MaridiaLargeSnailEnemyFunction.Idle,
            YSpeedTableIndex = 0,
            AttackCooldown = WrappingCountdown.Start(MaridiaLargeSnailInitialAttackCooldown),
            MovingLeft = false,
            RemainingBounces = WrappingCountdown.Start(MaridiaLargeSnailInitialBounceCount),
            BounceFunction = MaridiaLargeSnailBounceFunction.Falling,
        };
        slot.SetSpeciesState(state, state.ClearWithRecord);

        // The initial left-facing animation intentionally disagrees with installed selector zero,
        // whose table entry is right-facing idle. Because SetOumInstList suppresses equal
        // selectors, this oddity is observable until Samus first selects a different index.
        state.Program = AnimationProgramCursor<MaridiaLargeSnailProgram>.Start(MaridiaLargeSnailProgram.FacingLeftIdle);
    }

    /// <summary>
    /// Ports <c>MainAI_Oum</c> and functions $CD23-$CFFF. Ordering is exact: proximity and
    /// previous positions, behavior, Samus carrying, then the input-derived stop flag.
    /// </summary>
    private void RunMaridiaLargeSnailMain(
        RoomEnemySlot slot,
        MaridiaLargeSnailEnemyState state,
        SamusState? samus,
        RoomLevelData? level,
        SnesButton controllerInput)
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
        if (!state.RemainingBounces.IsZero)
        {
            RunMaridiaLargeSnailBounce(slot, state, level);
            return;
        }

        state.RequestedInstructionListIndex = 0;
        if (unchecked((short)(samus.Position.X.Whole - slot.Position.X.Whole)) < 0)
        {
            state.MovingLeft = true;
            state.RequestedInstructionListIndex = 1;
        }
        InstallMaridiaLargeSnailInstructionList(slot, state);

        if (!EnemyMath.IsWithinStrictModularDistance(
                samus.Position.X.Whole,
                slot.Position.X.Whole,
                MaridiaLargeSnailPushingXDistance))
        {
            return;
        }

        state.RequestedInstructionListIndex |= MaridiaLargeSnailRollingSelector;
        InstallMaridiaLargeSnailInstructionList(slot, state);
        state.Function = MaridiaLargeSnailEnemyFunction.Rolling;
    }

    /// <summary>Handles ledges, cooldown and attack selection before horizontal terrain movement.</summary>
    private void RunMaridiaLargeSnailRolling(
        RoomEnemySlot slot,
        MaridiaLargeSnailEnemyState state,
        SamusState samus,
        RoomLevelData level)
    {
        if (!state.RemainingBounces.IsZero)
        {
            RunMaridiaLargeSnailBounce(slot, state, level);
        }
        else if (!_terrain.MoveEnemyVertically(level, slot, 1 << 16))
        {
            // No floor one pixel below means Oum walked off a ledge. Retail restarts the
            // complete three-impact settling sequence without skipping horizontal work.
            state.YSpeedTableIndex = 0;
            state.BounceFunction = MaridiaLargeSnailBounceFunction.Falling;
            state.RemainingBounces = WrappingCountdown.Start(MaridiaLargeSnailInitialBounceCount);
        }

        // StopMovementNextFrame was produced at the end of the preceding main-AI call. The
        // current frame clears/recomputes it only after this behavior returns.
        if (state.StopMovementNextFrame)
            return;

        state.AttackCooldown = state.AttackCooldown.Decrement().Remaining;
        bool cooldownExpired = state.AttackCooldown.IsNegative;
        if (cooldownExpired)
        {
            state.AttackCooldown = WrappingCountdown.Start(0);
            bool mayAttack = EnemyMath.IsWithinStrictModularDistance(
                    samus.Position.X.Whole,
                    slot.Position.X.Whole,
                    MaridiaLargeSnailAttackXDistance) &&
                state.AttackAllowsRotation &&
                state.RemainingBounces.IsZero;
            if (mayAttack)
            {
                state.RequestedInstructionListIndex = 0;
                state.MovingLeft = false;
                if (unchecked((short)(samus.Position.X.Whole - slot.Position.X.Whole)) < 0)
                {
                    state.RequestedInstructionListIndex = 1;
                    state.MovingLeft = true;
                }

                state.AttackCooldown = WrappingCountdown.Start(MaridiaLargeSnailInitialAttackCooldown);
                state.RequestedInstructionListIndex =
                    (state.RequestedInstructionListIndex & MaridiaLargeSnailFacingSelector) | MaridiaLargeSnailAttackSelector;
                InstallMaridiaLargeSnailInstructionList(slot, state);
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
        state.RequestedInstructionListIndex -= MaridiaLargeSnailRollingSelector;
        InstallMaridiaLargeSnailInstructionList(slot, state);
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
                // The record stays within -3712 through 16384, so the cartridge's signed word comparisons are
                // ordinary comparisons.
                state.YSpeedTableIndex = Math.Min(
                    state.YSpeedTableIndex + MaridiaLargeSnailBounceAcceleration,
                    MaridiaLargeSnailMaximumFallIndex);

                if (!_terrain.MoveEnemyVertically(
                        level,
                        slot,
                        EnemyMath.ReadQuadraticEnemySpeed(
                            checked((ushort)SnesEightEight.WholeOf(state.YSpeedTableIndex)),
                            negative: false)))
                {
                    return;
                }

                state.RemainingBounces = state.RemainingBounces.Decrement().Remaining;
                // An impact that leaves the record below zero ends the bounces; the negative record is kept.
                state.YSpeedTableIndex -= MaridiaLargeSnailImpactIndexLoss;
                if (state.YSpeedTableIndex < 0)
                    state.RemainingBounces = WrappingCountdown.Start(0);
                state.BounceFunction = MaridiaLargeSnailBounceFunction.Rising;
                return;

            case MaridiaLargeSnailBounceFunction.Rising:
                state.YSpeedTableIndex -= MaridiaLargeSnailBounceAcceleration;
                if (state.YSpeedTableIndex < 0)
                {
                    state.YSpeedTableIndex = 0;
                    state.BounceFunction = MaridiaLargeSnailBounceFunction.Falling;
                    return;
                }

                // The rising half reads the negative member of the same quadratic row. The
                // native AND $7F00 is retained even though valid bounce indexes stay below
                // $4000; it documents the exact table-index formation seen in a debugger.
                ushort tableIndex = checked((ushort)
                    SnesEightEight.WholeOf(state.YSpeedTableIndex & MaridiaLargeSnailMasks.SpeedIndexWholeBits));
                _terrain.MoveEnemyVertically(
                    level,
                    slot,
                    EnemyMath.ReadQuadraticEnemySpeed(tableIndex, negative: true));
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
        ushort byteOffset = state.MovingLeft ? (ushort)132 : (ushort)128;
        SnesSignedSixteenSixteen speed = EnemyMath.ReadLinearEnemySpeed(byteOffset);
        bool temporarilyReachTowardSamus = state.SamusInPushingRange &&
            state.MovingLeft == state.SamusIsLeft;
        if (temporarilyReachTowardSamus)
        {
            // Native temporarily extends the whole-pixel move sixteen pixels toward Samus,
            // performs collision, then retracts the center. Net velocity remains the table
            // speed while wall clipping naturally transfers the lost distance to Samus.
            speed = speed.WithWholePixels(unchecked((short)(speed.WholePixels + (state.MovingLeft ? -16 : 16))));
        }

        int displacement = speed.RawValue;
        bool collided = _terrain.MoveEnemyHorizontallyTreatingSlopesAsWalls(
            level,
            slot,
            displacement);

        if (temporarilyReachTowardSamus)
        {
            slot.Position = slot.Position.WithWholeX(unchecked((ushort)(
                slot.Position.X.Whole + (state.MovingLeft ? 16 : -16))));
        }

        if (!collided)
            return;

        state.MovingLeft = !state.MovingLeft;
        state.RequestedInstructionListIndex ^= MaridiaLargeSnailAttackSelector;
        InstallMaridiaLargeSnailInstructionList(slot, state);
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
        state.SamusIsLeft = unchecked((short)(samus.Position.X.Whole - slot.Position.X.Whole)) < 0;
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
            // displacement word. This preserves the last touching Oum's native ownership.
            samus.Kinematics.ExtraDisplacement = samus.Kinematics.ExtraDisplacement.WithWholeY(unchecked((short)(
                slot.Position.Y.Whole - state.PreviousPosition.Y)));
        }

        if (!state.SamusInPushingRange)
            return;

        ushort deltaX = unchecked((ushort)(slot.Position.X.Whole - state.PreviousPosition.X));
        bool movedLeft = unchecked((short)deltaX) < 0;
        if (movedLeft != state.SamusIsLeft)
            return;

        samus.Kinematics.ExtraDisplacement += new Offset16<RoomPixels>(unchecked((short)(deltaX)), 0);
    }

    /// <summary>Records an opposing controller direction to stop the following frame's roll.</summary>
    private static void UpdateMaridiaLargeSnailInputStop(
        RoomEnemySlot slot,
        MaridiaLargeSnailEnemyState state,
        SnesButton controllerInput)
    {
        if (!state.SamusInPushingRange)
            return;

        short deltaX = unchecked((short)(slot.Position.X.Whole - state.PreviousPosition.X));
        bool stop;
        if (deltaX == 0)
        {
            stop = state.SamusIsLeft
                ? state.MovingLeft &&
                    (controllerInput & SnesButton.Right) != 0
                : !state.MovingLeft &&
                    (controllerInput & SnesButton.Left) != 0;
        }
        else if (deltaX < 0)
        {
            stop = (controllerInput & SnesButton.Right) != 0;
        }
        else
        {
            stop = (controllerInput & SnesButton.Left) != 0;
        }

        if (!stop)
            return;

        state.StopMovementNextFrame = true;
        slot.Position = slot.Position.WithWholeX(state.PreviousPosition.X);
    }

    /// <summary>
    /// Exact $A0:ABE7 asymmetric "touching from below" predicate used for Oum carrying and
    /// its post-contact shove. It is intentionally not interchangeable with radius overlap.
    /// </summary>
    private static bool MaridiaLargeSnailTouchesSamusFromBelow(
        RoomEnemySlot slot,
        SamusState samus)
    {
        ushort xDistance = EnemyMath.WrappedMagnitude(unchecked((ushort)(
            samus.Position.X.Whole - slot.Position.X.Whole)));
        bool centerWithinSamusRadius = xDistance < samus.Kinematics.Radius.X;
        ushort remainingX = unchecked((ushort)(xDistance - samus.Kinematics.Radius.X));
        if (!centerWithinSamusRadius && remainingX >= slot.Radius.X)
            return false;

        ushort biasedSamusY = unchecked((ushort)(samus.Position.Y.Whole + 3));
        if (unchecked((short)(biasedSamusY - slot.Position.Y.Whole)) >= 0)
            return false;

        ushort verticalDistance = unchecked((ushort)(slot.Position.Y.Whole - biasedSamusY));
        ushort remainingY = unchecked((ushort)(verticalDistance - samus.Kinematics.Radius.Y));
        return verticalDistance < samus.Kinematics.Radius.Y ||
            remainingY == slot.Radius.Y ||
            remainingY < slot.Radius.Y;
    }

    /// <summary>Carries out one cue of Oum's animations: the splash sound, attack completion and the attack-rotation window.</summary>
    private MaridiaLargeSnailProgram? ApplyMaridiaLargeSnailCue(MaridiaLargeSnailEnemyState state, MaridiaLargeSnailCue cue)
    {
        switch (cue.Kind)
        {
            case MaridiaLargeSnailCueKind.PlaySplash:
                LastMaridiaLargeSnailSoundEffect = MaridiaLargeSnailSplashSound;
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
        return null;
    }

    /// <summary>Starts one of eight Oum animations only when the requested index changes.</summary>
    internal static void InstallMaridiaLargeSnailInstructionList(
        RoomEnemySlot slot,
        MaridiaLargeSnailEnemyState state)
    {
        if (state.RequestedInstructionListIndex == state.InstalledInstructionListIndex)
            return;
        if (state.RequestedInstructionListIndex >= 8)
        {
            throw new InvalidDataException(
                $"Maridia Large Snail list index {state.RequestedInstructionListIndex} " +
                "is outside the retail eight-entry table.");
        }

        state.InstalledInstructionListIndex = state.RequestedInstructionListIndex;
        state.Program = AnimationProgramCursor<MaridiaLargeSnailProgram>.Start(MaridiaLargeSnailInstructionDefinitions
            .Program(state.RequestedInstructionListIndex));
        slot.InstructionTimer = WrappingCountdown.Start(1);
        slot.Timer = WrappingCountdown.Start(0);
    }

    /// <summary>
    /// Runs the common/non-damaging Oum hitbox tail after the multibox dispatcher selected a
    /// concrete frame hitbox. Damaging hitboxes call common touch before entering this tail.
    /// </summary>
    private static void ResolveMaridiaLargeSnailTouchAfterCommon(
        RoomEnemySlot slot,
        SamusState samus)
    {
        if (MaridiaLargeSnailTouchesSamusFromBelow(slot, samus))
            return;

        ushort shove = unchecked((short)(samus.Position.X.Whole - slot.Position.X.Whole)) < 0
            ? unchecked((ushort)-4)
            : (ushort)4;
        samus.Kinematics.ExtraDisplacement += new Offset16<RoomPixels>(unchecked((short)(shove)), 0);
    }

    /// <summary>
    /// Ports the private tail of <c>EnemyShot_Oum</c> at $A2:D3B8. It runs after common shot
    /// AI for damaging, freezing, and zero-damage vulnerability results alike.
    /// </summary>
    private void ResolveMaridiaLargeSnailShotAfterCommon() =>
        LastMaridiaLargeSnailSoundEffect = MaridiaLargeSnailShotSound;

    /// <summary>Publishes Oum's native shot sound after common bomb damage.</summary>
    internal void RegisterMaridiaLargeSnailBombReactions(EnemySharedReactions table)
    {
        table.AddBombCommon(new EnemyBombReaction(c => c.Enemy.EnemyDefinitionPointer == EnemyDefinitionId.Oum &&
                                                       c.SelectedShotAi == EnemyInteractionCallback.MaridiaLargeSnailShot)
        {
            AfterCommon = _ => ResolveMaridiaLargeSnailShotAfterCommon(),
        });
    }

    /// <summary>Ports MaridiaLargeSnail's main AI.</summary>
    internal void RunMaridiaLargeSnailMainAi(RoomEnemySlot slot, IEnemyAiFrame frame)
    {
        RunMaridiaLargeSnailMain(
            slot,
            slot.RequireSpeciesState<MaridiaLargeSnailEnemyState>(),
            frame.Samus,
            frame.Level,
            frame.ControllerInput);
    }

    /// <summary>Advances MaridiaLargeSnail's typed animation.</summary>
    internal void AdvanceMaridiaLargeSnailTypedAnimation(RoomEnemySlot slot, EnemyInstructionContext _) =>
        EnemyProgramAnimation.Advance(slot, MaridiaLargeSnailAnimations.Programs, MaridiaLargeSnailFrameDefinitions.ShownFrames.Of,
            static (MaridiaLargeSnailEnemyState state) => state.Program, static (state, cursor) => state.Program = cursor,
            (state, cue, _) => ApplyMaridiaLargeSnailCue(state, cue));

    /// <summary>Preserves protected-shell no-op hits and runs common damage only for vulnerable hitboxes.</summary>
    internal void RegisterMaridiaLargeSnailShotReactions(EnemySharedReactions table)
    {
        table.AddShot(new EnemyShotReaction(enemy => enemy.EnemyDefinitionPointer == EnemyDefinitionId.Oum &&
                                                     EnemyCollision.ShotAi(enemy) == EnemyInteractionCallback.MaridiaLargeSnailShot)
        {
            AfterSuperMissileQuake = (in EnemyShotContext c) =>
            {
                if (!c.UsesExtendedHitboxes)
                    throw new InvalidDataException("Maridia Large Snail lost its required extended-spritemap property.");
                if (c.HitboxShotAi == EnemyInteractionCallback.NoOp)
                {
                    // Native multibox collision marks the projectile before calling the
                    // no-op. It does not run normal shot AI, create an impact, play $57,
                    // or consult vulnerability for this protected shell component.
                    c.Projectiles.EnemyImpacts.ApplyExtendedEnemyCollisionPrelude(c.Projectile.SlotIndex, c.MarksCollision);
                    return true;
                }
                if (c.HitboxShotAi != EnemyInteractionCallback.MaridiaLargeSnailShot)
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
    internal void RegisterMaridiaLargeSnailTouch(EnemySharedReactions table)
    {
        table.AddTouch(slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.Oum &&
                               EnemyCollision.TouchAi(slot) == EnemyInteractionCallback.MaridiaLargeSnailNonDamagingTouch,
            context =>
            {
                if (!context.UsesExtendedHitboxes)
                    throw new InvalidDataException("Maridia Large Snail lost its required extended-spritemap property.");
                if (context.HitboxTouchAi is not (
                        EnemyInteractionCallback.MaridiaLargeSnailDamagingTouch or
                        EnemyInteractionCallback.MaridiaLargeSnailNonDamagingTouch))
                {
                    throw new InvalidDataException(
                        $"Maridia Large Snail hitbox touch AI " +
                        $"{context.HitboxTouchAi?.ToString() ?? "(unnamed)"} is not translated.");
                }

                // $D388 enters common touch damage and falls directly through $D38C's
                // asymmetric shove. $D38C frames execute only the shove, which is why the
                // closed shell can carry/push Samus without inflicting the header's 100 HP.
                if (context.HitboxTouchAi == EnemyInteractionCallback.MaridiaLargeSnailDamagingTouch)
                    _collision.ResolveNormalEnemyTouch(context.Slot, context.Samus);
                ResolveMaridiaLargeSnailTouchAfterCommon(context.Slot, context.Samus);
            });
    }

    /// <inheritdoc/>
    public void BeginEnemyFrame(RoomEnemyFrame frame)
    {
        LastMaridiaLargeSnailSoundEffect = null;
    }

    /// <summary>Queues this frame's published sound effects through the shared enemy audio queue, in the retail queue limits.</summary>
    internal void QueueLegacySounds(EnemyAudioQueue audio)
    {
        if (LastMaridiaLargeSnailSoundEffect is SoundEffectId sound)
            audio.QueueEnemySound(sound, maximumQueued: 6);
    }

    /// <summary>The callback of the first of the occupant's hitboxes the probe overlaps, or null outside its compiled frames.</summary>
    internal static bool? FindHitboxCallback(RoomEnemySlot enemy, EnemyHitboxProbe probe, out EnemyInteractionCallback? callback)
    {
        callback = null;
        if (!(enemy.Definition.Bank == MaridiaLargeSnailCollisionDefinitions.Bank &&
            (MaridiaLargeSnailCollisionDefinitions.Frames.Contains(enemy.Frame) ||
             probe.ArtworkInstalled)))
            return null;

        // Oum's 30 native frames each have one (0,0) component, but the
        // ordered regions change with roll and attack direction. Keep
        // those rectangles and callback identities independent of OAM.
        foreach (MaridiaLargeSnailCollisionHitbox hitbox in
                 MaridiaLargeSnailCollisionDefinitions.Frames.Of(enemy.Frame))
        {
            ushort left = unchecked((ushort)(enemy.Position.X.Whole + hitbox.Left));
            ushort top = unchecked((ushort)(enemy.Position.Y.Whole + hitbox.Top));
            ushort right = unchecked((ushort)(enemy.Position.X.Whole + hitbox.Right));
            ushort bottom = unchecked((ushort)(enemy.Position.Y.Whole + hitbox.Bottom));
            if (!probe.Overlaps(left, top, right, bottom))
                continue;
            callback = probe.SelectShotCallback ? hitbox.ShotAi : hitbox.TouchAi;
            return true;
        }
        return false;
    
    }
}
