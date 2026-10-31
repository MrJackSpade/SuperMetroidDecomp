using SuperMetroid.Core.Bosses.Artwork;
using System.Diagnostics.CodeAnalysis;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.BossBodies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Rooms.Scrolling;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Bosses.Crocomire.Artwork;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Enemies.Drawing;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>Crocomire's setup, its body and tongue programs and their cues, and the invisible wall that holds Samus back.</summary>
[RoomLifetime]
internal sealed class CrocomireFight
{
    /// <summary>Accesses the active room, enemy slots, and shared RNG.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Moves Crocomire against the arena's collision blocks.</summary>
    private readonly EnemyTerrain _terrain;
    /// <summary>Spawns Crocomire's attacks, dust, smoke, and post-death drops.</summary>
    private readonly EnemyProjectileEngine _projectiles;
    /// <summary>Publishes the bridge and wall-break quake effects.</summary>
    private readonly RoomEarthquake _earthquake;
    /// <summary>Draws Crocomire's body, skeleton and melt, and holds the colours and background pictures they show.</summary>
    private readonly EnemyPresentation _presentation;
    /// <summary>Resolves Crocomire body and tongue contact with Samus.</summary>
    private readonly EnemyCollision _collision;
    /// <summary>Owns camera-distance mode changes during Crocomire's arena sequence.</summary>
    private readonly ZebetiteBehavior _zebetiteBehavior;
    /// <summary>Updates boss completion and room-state flags.</summary>
    private readonly EnemyRoomStatus _roomStatus;
    /// <summary>Crocomire's body layer: where it is placed, the canvas his death assembles it on, the tongue placed with it and the acid smoke drawn with it.</summary>
    private readonly CrocomireBodyLayer _bodyLayer;
    /// <summary>Crocomire's state lookups, body program start, PLMs, dust and random numbers, and the sound and music it last requested.</summary>
    private readonly CrocomireEncounterAccess _access;
    /// <summary>The session's installed boss artwork.</summary>
    private readonly BossArtworkBinding _bossArtwork;


    /// <summary>Builds the component from the collaborators it uses.</summary>
    internal CrocomireFight(CrocomireFightDependencies dependencies)
    {
        _bossArtwork = dependencies.BossArtwork;
        _room = dependencies.Room;
        _terrain = dependencies.Terrain;
        _projectiles = dependencies.Projectiles;
        _earthquake = dependencies.Earthquake;
        _presentation = dependencies.Presentation;
        _collision = dependencies.Collision;
        _zebetiteBehavior = dependencies.ZebetiteBehavior;
        _roomStatus = dependencies.RoomStatus;
        _bodyLayer = dependencies.BodyLayer;
        _access = dependencies.Access;
    }

    /// <summary>Ports <c>InitAI_Crocomire</c> at $A4:8A5A.</summary>
    internal void InitializeCrocomire(RoomEnemySlot slot)
    {
        _roomStatus.Boss = AreaBoss.Crocomire;
        var state = new CrocomireEnemyState(slot);
        slot.SetSpeciesState(state, state.ClearWithRecord);
        _access.Crocomire = state;
        // Crocomire shows his body on a cleared body layer (#1355, #1466): his body frames, and what his death assembles
        // on its working canvas, are drawn on it.
        BossBodySheet body = _bossArtwork.Body(BossBodyOwner.Crocomire);
        _access.CrocomireDeath = new CrocomireDeathState { WorkingBody = new BossBodyCanvas(body, BossBodyDefinitions.SharedBodyRows) };
        _presentation.AttachBossBody(new BossBodyCanvas(body, BossBodyDefinitions.SharedBodyRows));

        if (_room.Services.Bosses.IsDefeated(BossBits.AreaMiniBoss))
        {
            // The dead-room state keeps only Crocomire's skeleton display actor. Property
            // mask $7BFF and these radii/coordinates are literal writes at $A4:8AEA-$8B35.
            slot.Properties = slot.Properties.Replace(
                EnemyProperties.SolidToSamus |
                    EnemyProperties.IgnoreSamusCollision,
                EnemyProperties.IgnoreSamusCollision);
            state.DeathSequenceIndex = CrocomireDeathPhase.DefeatedRoomAdvance;
            CrocomireEncounterAccess.StartCrocomireBodyProgram(state, CrocomireBodyProgram.Dead);
            slot.Position = slot.Position.WithWhole(new(576, 144));
            slot.Radius = new(40, 28);
            _room.Services.RoomScroll.Write(0, RoomScrollState.Blue);
            _room.Services.RoomScroll.Write(1, RoomScrollState.Blue);
            _room.Services.RoomScroll.Write(2, RoomScrollState.Blue);
            _room.Services.RoomScroll.Write(3, RoomScrollState.Blue);
            _access.PublishCrocomirePlm(new Position16<RoomBlocks>(32, 3), PlmHeaderId.ClearCrocomireInvisibleWall);
            _access.PublishCrocomirePlm(new Position16<RoomBlocks>(30, 3), PlmHeaderId.ClearCrocomireInvisibleWall);
            _access.PublishCrocomirePlm(new Position16<RoomBlocks>(97, 11), PlmHeaderId.ClearCrocomireBridge);
            _bodyLayer.ShowCrocomireWorkingCells(0, BossBodyDefinitions.PageRows * BossBodyDefinitions.ColumnCount);
            return;
        }

        state.DeathSequenceIndex = CrocomireDeathPhase.Fighting;
        state.ReactionTimer = 0;
        state.FightFunction = CrocomireFightFunction.Sleeping;
        // $A4:8ABA-8ABD, on the living branch only.
        _zebetiteBehavior.CameraDistanceIndex = CameraDistanceMode.BossTracking;
        CrocomireEncounterAccess.StartCrocomireBodyProgram(state, CrocomireBodyProgram.Initial);
        slot.ExtraProperties = slot.ExtraProperties.With(
            EnemyExtraProperties.UsesExtendedSpritemap);
        _room.Services.RoomScroll.Write(0, RoomScrollState.RedBoundary);
        _room.Services.RoomScroll.Write(1, RoomScrollState.RedBoundary);

        // The initializer copies seventeen words, not sixteen: X starts at $20 and reaches
        // zero inclusively. Preserve that palette-boundary write because later fades compare
        // the exact target image produced by the cartridge.
        (_bossArtwork.Artwork?.CrocomireColors ?? throw new InvalidOperationException(
            "Crocomire requires installed color artwork.")).ApplyInitial(_presentation.Colours);
    }

    /// <summary>Ports <c>InitAI_CrocomireTongue</c> at $A4:F67A.</summary>
    internal void InitializeCrocomireTongue(RoomEnemySlot slot)
    {
        if (_room.Services.Bosses.IsDefeated(BossBits.AreaMiniBoss))
        {
            slot.Properties = slot.Properties.Replace(
                EnemyProperties.ProcessInstructions |
                    EnemyProperties.Deleted |
                    EnemyProperties.Invisible,
                EnemyProperties.Deleted | EnemyProperties.Invisible);
            return;
        }

        CrocomireEnemyState state = _access.Crocomire ??
            throw new InvalidOperationException("Crocomire's tongue initialized before its body.");
        state.Tongue = slot;
        CrocomireBodyLayer.StartCrocomireTongueProgram(state, CrocomireTongueProgram.Fight, WrappingCountdown.Start(1));
        slot.ExtraProperties = slot.ExtraProperties.With(EnemyExtraProperties.UsesExtendedSpritemap);
        var tongueState = new CrocomireTongueState { BodyXOffset = CrocomireBehavior.CrocomireTongueBodyXOffset };
        slot.SetSpeciesState(tongueState, tongueState.ClearWithRecord);
        slot.Colouring = new GraphicsSetMemberColouring(EnemyDefinitionId.Crocomire);
    }

    /// <summary>Ports the artificial left-side wall in <c>$A4:8C95</c>.</summary>
    internal void HandleCrocomireInvisibleWall(
        RoomEnemySlot body,
        CrocomireEnemyState state,
        SamusState? samus)
    {
        if (samus is null || state.DeathSequenceIndex != CrocomireDeathPhase.Fighting)
            return;
        int leftEdge = unchecked((ushort)(
            body.Position.X.Whole - body.Radius.X - samus.Kinematics.Radius.X));
        if (unchecked((short)(leftEdge - samus.Position.X.Whole)) >= 0)
            return;

        _collision.ResolveNormalEnemyTouch(body, samus);
        samus.Position = samus.Position.WithWholeX((ushort)leftEdge);
        samus.Kinematics.ExtraDisplacement = samus.Kinematics.ExtraDisplacement.WithWhole(new(-4, -1));
    }

    /// <summary>
    /// Runs the tongue's animation where the enemy instruction interpreter would, with the native timer gate
    /// and new-frame flag.
    /// </summary>
    internal void AdvanceCrocomireTongueAnimation(RoomEnemySlot tongue)
    {
        CrocomireEnemyState state = _access.Crocomire is { } owner && ReferenceEquals(owner.Tongue, tongue)
            ? owner
            : throw new InvalidOperationException("Crocomire's tongue has no body owner.");
        CountdownStep instructionTimer = tongue.InstructionTimer.Decrement();
        tongue.InstructionTimer = instructionTimer.Remaining;
        if (!instructionTimer.IsZero)
        {
            tongue.ExtraProperties = tongue.ExtraProperties.Without(EnemyExtraProperties.NewInstructionFrame);
            return;
        }

        var cursor = new AnimationProgramCursor<CrocomireTongueProgram>(state.TongueProgram, state.TonguePosition);
        AnimationProgramAdvance<CrocomireTongueFrame> advance = CrocomireAnimations.TonguePrograms.Advance(ref cursor,
            cue => throw new InvalidOperationException($"A Crocomire tongue program published cue {cue}."));
        state.TonguePosition = cursor.Position;
        // A hold is the native sleep: the timer wraps from zero and the last frame stays.
        if (advance is AnimationProgramAdvance<CrocomireTongueFrame>.Hold)
            return;
        if (advance is not AnimationProgramAdvance<CrocomireTongueFrame>.Show shown)
            throw new InvalidOperationException($"Crocomire's tongue program ended an update with {advance}.");
        tongue.InstructionTimer = WrappingCountdown.FromRaw((ushort)shown.Duration);
        tongue.Frame = CrocomireTongueFrameDefinitions.ShownFrames.Of(shown.Frame);
        tongue.ExtraProperties = tongue.ExtraProperties.With(EnemyExtraProperties.NewInstructionFrame);
    }

    /// <summary>
    /// Runs Crocomire's body program where the enemy instruction interpreter would: the frame expires on the
    /// update its native timer would, and a newly shown frame raises the new-frame flag exactly as a native
    /// timed frame does.
    /// </summary>
    internal void AdvanceCrocomireBodyAnimation(RoomEnemySlot body, SamusState? samus, RoomLevelData? level, int cameraX)
    {
        CrocomireEnemyState state = _access.RequireCrocomire(body);
        CountdownStep instructionTimer = body.InstructionTimer.Decrement();
        body.InstructionTimer = instructionTimer.Remaining;
        if (!instructionTimer.IsZero)
        {
            body.ExtraProperties = body.ExtraProperties.Without(EnemyExtraProperties.NewInstructionFrame);
            return;
        }

        AnimationProgramCursor<CrocomireBodyProgram> cursor = state.BodyCursor;
        WrappingCountdown counter = body.Timer;
        AnimationProgramAdvance<CrocomireBodyFrame> advance = CrocomireBodyAnimations.Programs.Advance(ref cursor, ref counter,
            (cue, continuation) => ApplyCrocomireBodyCue(state, samus, level, cameraX, cue,
                CrocomireBodyAnimations.Programs.ResidentProgram(continuation)));
        state.BodyCursor = cursor;
        body.Timer = counter;
        // A hold is the native sleep: the timer wraps from zero and the last frame stays. The body programs
        // have no waits or stops.
        if (advance is not AnimationProgramAdvance<CrocomireBodyFrame>.Show shown)
        {
            if (advance is not AnimationProgramAdvance<CrocomireBodyFrame>.Hold)
                throw new InvalidOperationException($"Crocomire's body program ended an update with {advance}.");
            return;
        }
        body.InstructionTimer = WrappingCountdown.FromRaw((ushort)shown.Duration);
        state.BodyFrame = shown.Frame;
        body.Frame = CrocomireBodyFrameSpritemaps.ShownFrames.Of(shown.Frame);
        body.ExtraProperties = body.ExtraProperties.With(EnemyExtraProperties.NewInstructionFrame);
    }

    /// <summary>
    /// Carries out one body-program cue. Movement, wall, sound, shake and dust cues keep their authored order;
    /// the fight AI and the spike-wall move may start another program, which the program set runs at once.
    /// </summary>
    /// <param name="state">The Crocomire encounter whose body runs the program.</param>
    /// <param name="samus">Samus, whom the fight AI measures against; null in fixtures without her.</param>
    /// <param name="level">The room's collision level, which the movement cues require.</param>
    /// <param name="cameraX">The camera's X, which the on-screen movement tests use.</param>
    /// <param name="cue">The cue to carry out.</param>
    /// <param name="continuation">The program the cursor rests in after the cue, which the fight AI compares.</param>
    /// <returns>The program to start, or null to continue after the cue.</returns>
    private CrocomireBodyProgram? ApplyCrocomireBodyCue(
        CrocomireEnemyState state,
        SamusState? samus,
        RoomLevelData? level,
        int cameraX,
        CrocomireBodyCue cue,
        CrocomireBodyProgram continuation)
    {
        RoomEnemySlot body = state.Body;
        switch (cue.Kind)
        {
            case CrocomireBodyCueKind.FightAi:
                return RunCrocomireFightInstruction(state, samus, continuation);

            case CrocomireBodyCueKind.MaybeStartProjectileAttack:
                if ((_access.ReadCrocomireRandom() & 4095) < 1024)
                {
                    state.FightFunction = CrocomireFightFunction.ProjectileAttack;
                    state.ProjectileCounter = 0;
                    return CrocomireBodyProgram.ProjectileAttack;
                }
                return null;

            case CrocomireBodyCueKind.Cry:
                _access.RequestCrocomireSound(CrocomireSoundDefinitions.Cry);
                return null;
            case CrocomireBodyCueKind.BigExplosionSound:
                _access.RequestCrocomireSound(CrocomireSoundDefinitions.BigExplosion);
                return null;
            case CrocomireBodyCueKind.SkeletonCollapseSound:
                _access.RequestCrocomireSound(CrocomireSoundDefinitions.SkeletonCollapse);
                return null;
            case CrocomireBodyCueKind.ShakeScreen:
                _earthquake.EarthquakeType = 4;
                _earthquake.EarthquakeTimer = WrappingCountdown.Start(5);
                _access.RequestCrocomireSound(CrocomireSoundDefinitions.ScreenShake);
                return null;

            case CrocomireBodyCueKind.MoveLeft:
                RequireCrocomireLevel(level);
                if ((state.FightFlags & CrocomireFightFlags.DamageReactionPending) == CrocomireFightFlags.None)
                    MoveCrocomire(body, level, -4);
                return null;
            case CrocomireBodyCueKind.MoveLeftWithDust:
                SpawnCrocomireRandomFootDust(state);
                RequireCrocomireLevel(level);
                if ((state.FightFlags & CrocomireFightFlags.DamageReactionPending) == CrocomireFightFlags.None)
                    MoveCrocomire(body, level, -4);
                return null;
            case CrocomireBodyCueKind.MoveLeftTowardSpikeWall:
                RequireCrocomireLevel(level);
                if (MoveCrocomire(body, level, -4))
                {
                    state.FightFunction = CrocomireFightFunction.BackingOffSpikeWall;
                    return CrocomireBodyProgram.BackOffFromSpikeWall;
                }
                int random = _access.ReadCrocomireRandom();
                int baseOffset = unchecked((short)(random - 2048)) >= 0 ? -32 : 32;
                _access.SpawnCrocomireDust(state, baseOffset + (random & 15));
                return null;
            case CrocomireBodyCueKind.MoveRightIfOnScreen:
                RequireCrocomireLevel(level);
                if (CrocomireBodyIsOnScreen(body, cameraX))
                    MoveCrocomire(body, level, 4);
                return null;
            case CrocomireBodyCueKind.MoveRight:
                RequireCrocomireLevel(level);
                MoveCrocomire(body, level, 4);
                return null;
            case CrocomireBodyCueKind.MoveRightIfOnScreenWithDust:
                SpawnCrocomireRandomFootDust(state);
                RequireCrocomireLevel(level);
                if (CrocomireBodyIsOnScreen(body, cameraX))
                    MoveCrocomire(body, level, 4);
                return null;
            case CrocomireBodyCueKind.MoveRightWithDust:
                SpawnCrocomireRandomFootDust(state);
                RequireCrocomireLevel(level);
                MoveCrocomire(body, level, 4);
                return null;
            case CrocomireBodyCueKind.Dust:
                _access.SpawnCrocomireDust(state, cue.DustOffset);
                return null;

            default:
                throw new InvalidOperationException($"Undefined {nameof(CrocomireBodyCueKind)} {cue.Kind}.");
        }
    }

    /// <summary>Whether the body's left edge lies within 260 pixels right of the camera: the step-back moves' on-screen test.</summary>
    private static bool CrocomireBodyIsOnScreen(RoomEnemySlot body, int cameraX) =>
        unchecked((short)(body.Position.X.Whole - body.Radius.X - 260 - cameraX)) < 0;

    /// <summary>Ports $A4:86B3-$8A39, including the shipped but normally unused states.</summary>
    private CrocomireBodyProgram? RunCrocomireFightInstruction(
        CrocomireEnemyState state,
        SamusState? samus,
        CrocomireBodyProgram continuation)
    {
        switch (state.FightFunction)
        {
            case CrocomireFightFunction.ResetAnimation:
                state.Body.InstructionTimer = WrappingCountdown.Start(1);
                return CrocomireBodyProgram.Initial;

            case CrocomireFightFunction.StepForward:
                state.FightFunction = CrocomireFightFunction.Sleeping;
                return CrocomireBodyProgram.StepForward;

            case CrocomireFightFunction.Sleeping:
                if (samus is not null && EnemyMath.WrappedMagnitude(unchecked((ushort)(
                        state.Body.Position.X.Whole - samus.Position.X.Whole))) < 224)
                {
                    state.FightFlags |= CrocomireFightFlags.Engaged;
                    state.FightFunction = CrocomireFightFunction.WaitingForFirstDamage;
                    return CrocomireBodyProgram.WaitForFirstSecondDamage;
                }
                return null;

            case CrocomireFightFunction.SteppingForward:
                if ((state.FightFlags & CrocomireFightFlags.DamageReactionPending) != CrocomireFightFlags.None)
                {
                    state.FightFlags &= ~CrocomireFightFlags.DamageReactionPending;
                    if (state.StepCounter != 0)
                    {
                        state.FightFunction = CrocomireFightFunction.SteppingBack;
                        return CrocomireBodyProgram.StepBack;
                    }
                }
                if (unchecked((short)(
                        state.Body.Position.X.Whole - CrocomireBehavior.CrocomireSpikeWallThreshold)) < 0)
                {
                    state.FightFunction = CrocomireFightFunction.NearSpikeWallCharge;
                    return CrocomireBodyProgram.NearSpikeWallCharge;
                }
                return !CrocomireBodyProgramGroups.BeforeSteppingBack.Contains(continuation)
                    ? CrocomireBodyProgram.StepForward
                    : null;

            case CrocomireFightFunction.ProjectileAttack:
                if ((state.FightFlags & CrocomireFightFlags.DamageReactionPending) != CrocomireFightFlags.None)
                {
                    state.FightFlags &= ~CrocomireFightFlags.DamageReactionPending;
                    state.FightFunction = CrocomireFightFunction.SteppingBack;
                    return CrocomireBodyProgram.StepBack;
                }
                if (state.ProjectileCounter < CrocomireBehavior.LastProjectileSpawnParameter)
                {
                    state.ProjectileCounter += 2;
                    SpawnCrocomireProjectile(state.Body, state.ProjectileCounter);
                    _access.RequestCrocomireSound(CrocomireSoundDefinitions.ProjectileSpit);
                    return null;
                }
                state.FightFunction = CrocomireFightFunction.SteppingForward;
                return CrocomireBodyProgram.StepForwardAfterDelay;

            case CrocomireFightFunction.NearSpikeWallCharge:
                if ((state.FightFlags & CrocomireFightFlags.DamageReactionPending) != CrocomireFightFlags.None)
                {
                    state.FightFlags &= ~CrocomireFightFlags.DamageReactionPending;
                    state.FightFunction = CrocomireFightFunction.SteppingBack;
                    return CrocomireBodyProgram.StepBack;
                }
                return null;

            case CrocomireFightFunction.SteppingBack:
                if (state.StepCounter != 0)
                    state.StepCounter--;
                if (state.StepCounter != 0)
                    return CrocomireBodyProgram.SteppingBack;
                state.FightFunction = CrocomireFightFunction.SteppingForward;
                return CrocomireBodyProgram.StepForward;

            case CrocomireFightFunction.BackingOffSpikeWall:
                if (unchecked((short)(
                        state.Body.Position.X.Whole - CrocomireBehavior.CrocomireSpikeWallThreshold)) >= 0)
                {
                    state.FightFunction = CrocomireFightFunction.SteppingForward;
                    return CrocomireBodyProgram.StepForward;
                }
                return null;

            case CrocomireFightFunction.RoarAndStepForwardUnused:
                state.FightFunction = CrocomireFightFunction.SteppingForward;
                return CrocomireBodyProgram.Roar;

            case CrocomireFightFunction.WaitingForFirstDamage:
                return RunCrocomireWaitingForDamage(
                    state,
                    continuation,
                    CrocomireFightFunction.WaitingForSecondDamage);

            case CrocomireFightFunction.WaitingForSecondDamage:
            case CrocomireFightFunction.WaitingForSecondDamageUnused:
                return RunCrocomireWaitingForDamage(
                    state,
                    continuation,
                    CrocomireFightFunction.SteppingBack);

            case CrocomireFightFunction.PowerBombCharge:
            {
                // The charge consumes one step and ends once fewer than two remain; a count that would drop below
                // zero ends it the same way, as the cartridge's wrapped word reads negative.
                int remaining = state.StepCounter - 1;
                if (remaining < 2)
                {
                    state.StepCounter = 0;
                    state.FightFunction = CrocomireFightFunction.SteppingForward;
                    return CrocomireBodyProgram.StepForward;
                }
                state.StepCounter = remaining;
                return null;
            }

            case CrocomireFightFunction.NearSpikeWallChargeUnused:
                if ((state.FightFlags & CrocomireFightFlags.DamageReactionPending) != CrocomireFightFlags.None)
                {
                    state.FightFlags = (state.FightFlags & (CrocomireFightFlags.UnusedPreferClawAttack | CrocomireFightFlags.UnusedAttackSequenceStarted |
                        CrocomireFightFlags.UnusedClawsMoving | CrocomireFightFlags.DamageReactionPending | CrocomireFightFlags.UnusedKeptByChargeReset)) |
                        CrocomireFightFlags.Engaged | CrocomireFightFlags.UnusedForwardStepsDone;
                    state.StepCounter = 1;
                    state.ReactionTimer = 10;
                    state.FightFunction = CrocomireFightFunction.SteppingBack;
                    _access.RequestCrocomireSound(CrocomireSoundDefinitions.UnusedChargeInterrupted);
                    return null;
                }
                state.FightFunction = CrocomireFightFunction.NearSpikeWallCharge;
                return CrocomireBodyProgram.RoarCloseMouth;

            case CrocomireFightFunction.ResetAnimationUnused:
                state.Body.InstructionTimer = WrappingCountdown.Start(1);
                state.FightFlags |= CrocomireFightFlags.UnusedAttackSequenceStarted;
                state.StepCounter = 32;
                state.FightFunction = CrocomireFightFunction.ChooseAttackUnused;
                return CrocomireBodyProgram.Initial;

            case CrocomireFightFunction.ChooseAttackUnused:
                if ((state.FightFlags & CrocomireFightFlags.UnusedPreferClawAttack) != CrocomireFightFlags.None)
                {
                    state.Body.InstructionTimer = WrappingCountdown.Start(1);
                    state.StepCounter = 16;
                    state.FightFunction = CrocomireFightFunction.MoveUntilSamusUnused;
                    return CrocomireBodyProgram.Initial;
                }
                state.FightFunction = CrocomireFightFunction.StepForwardUnused;
                return CrocomireBodyProgram.UnusedChargeForwardOneStep;

            case CrocomireFightFunction.StepForwardUnused:
                state.Body.InstructionTimer = WrappingCountdown.Start(1);
                if (state.StepCounter == 0)
                {
                    state.FightFlags |= CrocomireFightFlags.UnusedForwardStepsDone;
                    state.FightFunction = CrocomireFightFunction.MoveClawsUnused;
                    return CrocomireBodyProgram.StepForward;
                }
                return CrocomireBodyProgram.Initial;

            case CrocomireFightFunction.MoveUntilSamusUnused:
                if (unchecked((short)(state.Body.Position.X.Whole - 672)) < 0)
                {
                    state.FightFunction = CrocomireFightFunction.MoveClawsUnused;
                    state.StepCounter = 3;
                    return CrocomireBodyProgram.StepForward;
                }
                if ((state.FightFlags & CrocomireFightFlags.ClawTouchedSamus) == CrocomireFightFlags.None)
                {
                    state.FightFunction = CrocomireFightFunction.StepForwardVariantUnused;
                    state.FightFlags &= ~CrocomireFightFlags.UnusedClawsMoving;
                    return CrocomireBodyProgram.MovingClaws;
                }
                state.StepCounter = 5;
                state.SavedFightFunction = state.FightFunction;
                state.FightFunction = CrocomireFightFunction.UnwrittenTableEntry;
                return CrocomireBodyProgram.MovingClaws;

            case CrocomireFightFunction.MoveClawsUnused:
                if (state.StepCounter == 0 || --state.StepCounter == 0)
                {
                    state.FightFunction = CrocomireFightFunction.MovingClawsUnused;
                    state.FightFlags &= ~CrocomireFightFlags.UnusedClawsMoving;
                    return CrocomireBodyProgram.StepForward;
                }
                state.FightFunction = CrocomireFightFunction.MoveClawsUnused;
                state.FightFlags |= CrocomireFightFlags.UnusedClawsMoving;
                return CrocomireBodyProgram.MovingClaws;

            case CrocomireFightFunction.StepForwardVariantUnused:
                if ((state.FightFlags & CrocomireFightFlags.UnusedForwardStepsDone) == CrocomireFightFlags.None)
                    state.FightFlags &= ~(CrocomireFightFlags.UnusedPreferClawAttack | CrocomireFightFlags.UnusedAttackSequenceStarted);
                state.FightFunction = CrocomireFightFunction.MovingClawsUnused;
                return CrocomireBodyProgram.StepForward;

            case CrocomireFightFunction.MovingClawsUnused:
                if (state.StepCounter == 0)
                {
                    state.FightFlags &= ~CrocomireFightFlags.ClawTouchedSamus;
                    state.Body.InstructionTimer = WrappingCountdown.Start(1);
                    state.FightFunction = state.SavedFightFunction;
                    return CrocomireBodyProgram.MovingClaws;
                }
                if ((state.FightFlags & CrocomireFightFlags.ClawTouchedSamus) != CrocomireFightFlags.None)
                {
                    state.StepCounter--;
                    _access.RequestCrocomireSound(CrocomireSoundDefinitions.ClawStrike);
                    return CrocomireBodyProgram.MovingClaws;
                }
                state.FightFlags &= ~CrocomireFightFlags.ClawTouchedSamus;
                state.FightFunction = CrocomireFightFunction.SteppingBack;
                return null;

            case CrocomireFightFunction.UnwrittenTableEntry:
            default:
                throw new InvalidDataException(
                    $"Crocomire fight function {state.FightFunction} is not translated.");
        }
    }

    /// <summary>Holds the damage-reaction instruction until its native timer permits the next step.</summary>
    private static CrocomireBodyProgram? RunCrocomireWaitingForDamage(
        CrocomireEnemyState state,
        CrocomireBodyProgram continuation,
        CrocomireFightFunction damagedDestination)
    {
        if ((state.FightFlags & CrocomireFightFlags.DamageReactionPending) != CrocomireFightFlags.None)
        {
            state.FightFlags &= ~CrocomireFightFlags.DamageReactionPending;
            state.FightFunction = damagedDestination;
            return CrocomireBodyProgram.StepBack;
        }
        return !CrocomireBodyProgramGroups.BeforeRoarCloseMouthLoop.Contains(continuation)
            ? CrocomireBodyProgram.Roar
            : null;
    }

    /// <summary>Moves Crocomire through ordinary ignore-slopes collision with 16.16 displacement.</summary>
    private bool MoveCrocomire(RoomEnemySlot slot, RoomLevelData level, int pixels) =>
        // The instruction callbacks pass INT16_SHL16(±4) into the ordinary ignore-slopes
        // enemy mover; retaining 16.16 here preserves collision-edge alignment.
        slot.DefinitionId == EnemyDefinitionId.Crocomire &&
        _terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(level, slot, SixteenSixteen.FromWhole(pixels));

    /// <summary>Chooses a foot-dust offset from room RNG and spawns the matching effect.</summary>
    private void SpawnCrocomireRandomFootDust(CrocomireEnemyState state)
    {
        int random = _access.ReadCrocomireRandom();
        int offset = random & 31;
        if (unchecked((short)(random - 4096)) >= 0)
            offset = -offset;
        _access.SpawnCrocomireDust(state, offset);
    }

    /// <summary>Rejects death and melting operations when the arena collision level is unavailable.</summary>
    private static void RequireCrocomireLevel([NotNull] RoomLevelData? level)
    {
        if (level is null)
            throw new InvalidOperationException("Crocomire movement requires room level data.");
    }

    /// <summary>Ports <c>EprojInit_CrocomireProjectile</c> at $86:9023.</summary>
    internal void SpawnCrocomireProjectile(RoomEnemySlot body, int spawnParameter)
    {
        RoomEnemyProjectileSlot? projectile = _projectiles.AllocateEnemyProjectile();
        if (projectile is null)
            return;

        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
            projectile,
            RoomEnemyProjectileKind.CrocomireProjectile,
            graphics: SpriteGraphics.CharacterZeroIn(SharedSpriteColouring.EnemyProjectiles));
        projectile.StartAnimation(new CrocomireProjectileAnimation(CrocomireProjectileFrameKind.Mouth));
        projectile.Velocity = new(SignedEightEight.FromSignedRaw(-512), new SignedEightEight(1));
        projectile.Position = projectile.Position.WithWhole(new(unchecked((ushort)(body.Position.X.Whole - 32)), unchecked((ushort)(body.Position.Y.Whole - 16))));
        projectile.GeneralTimer = 0;
        projectile.Position = projectile.Position.WithFractionX(0).WithFractionY(0);
        projectile.State = new CrocomireProjectileState(spawnParameter);
    }
}
