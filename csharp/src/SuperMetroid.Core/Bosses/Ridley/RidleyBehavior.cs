using SuperMetroid.Core.Input;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Enemies.Drawing;
using SuperMetroid.Core.Bosses.Ridley.Artwork;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>The Ridley species owner: its per-room state, AI routines and the interactions it registers.</summary>
internal sealed class RidleyBehavior : IEnemySpecies
{
    /// <summary>Enemy dispatcher used for Ridley's registered native callbacks.</summary>
    private readonly IEnemyDispatch _dispatch;
    /// <summary>Room services, level state, and native RNG for the Norfair fight.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Spawns Ridley's fireballs and breakup projectiles.</summary>
    private readonly EnemyProjectileEngine _projectiles;
    /// <summary>Publishes tail-impact tremors to the room earthquake state.</summary>
    private readonly RoomEarthquake _earthquake;
    /// <summary>Queues roars, tail impacts, and other enemy-bank sounds.</summary>
    private readonly EnemyAudioQueue _audio;
    /// <summary>Resolves Ridley's physical enemy records and multipart slots.</summary>
    private readonly EnemySlotTable _slotTable;
    /// <summary>Publishes Ridley's health and transition palettes.</summary>
    private readonly EnemyPresentation _presentation;
    /// <summary>Handles common combat damage and enemy drops.</summary>
    private readonly EnemyDeathsAndDrops _deathsAndDrops;
    /// <summary>Owns fight state shared with cutscenes and room transitions.</summary>
    private readonly RidleyEncounter _ridleyEncounter;

    /// <summary>Advances Ridley's independent tail segments and their hit checks.</summary>
    private readonly RidleyTail _ridleyTail;

    /// <summary>Supplies shared Ceres and Norfair movement, fade, and animation behavior.</summary>
    private readonly CeresRidleyBehavior _ceresRidleyBehavior;
    /// <summary>Coordinates Ridley's dedicated projectile effects.</summary>
    private readonly RidleyProjectiles _ridleyProjectiles;
    /// <summary>Checks active door-transition state before the Norfair intro.</summary>
    private readonly EnemyRoomStatus _roomStatus;
    /// <summary>Raises the arena liquid during Ridley's reveal.</summary>
    private readonly EnemyRoomFx _roomFx;

    /// <summary>Wires Ridley's fight, movement, tail, projectile, audio, and room collaborators.</summary>
    internal RidleyBehavior(RidleyBehaviorDependencies dependencies)
    {
        _ceresRidleyBehavior = dependencies.CeresRidleyBehavior;
        _ridleyTail = dependencies.RidleyTail;
        _dispatch = dependencies.Dispatch;
        _room = dependencies.Room;
        _projectiles = dependencies.Projectiles;
        _earthquake = dependencies.Earthquake;
        _audio = dependencies.Audio;
        _slotTable = dependencies.SlotTable;
        _presentation = dependencies.Presentation;
        _deathsAndDrops = dependencies.DeathsAndDrops;
        _ridleyEncounter = dependencies.RidleyEncounter;
        _ridleyProjectiles = dependencies.RidleyProjectiles;
        _roomStatus = dependencies.RoomStatus;
        _roomFx = dependencies.RoomFx;
    }

    /// <summary>
    /// Runs Ridley's body program where the enemy instruction interpreter would: the frame expires on the update its
    /// native timer would, and a newly shown frame raises the new-frame flag.
    /// </summary>
    internal void AdvanceRidleyBodyAnimation(RoomEnemySlot slot, SamusState? samus)
    {
        if (!EnemyProgramAnimation.Expire(slot))
            return;
        RidleyEnemyState state = _ridleyEncounter.RequireRidley(slot);
        AnimationProgramCursor<RidleyBodyProgram> cursor = state.BodyProgram;
        // Ridley's body lists load no loop counter.
        EnemyProgramAnimation.Run(slot, RidleyAnimations.Programs, ref cursor, frame => (ushort)frame,
            (cue, continuation) => ApplyRidleyBodyCue(slot, state, samus, cue));
        state.BodyProgram = cursor;
    }

    /// <summary>Carries out one cue of Ridley's body program, returning the program a goto cue continues at.</summary>
    internal RidleyBodyProgram? ApplyRidleyBodyCue(RoomEnemySlot slot, RidleyEnemyState state, SamusState? samus, RidleyBodyCue cue)
    {
        switch (cue.Kind)
        {
            case RidleyBodyCueKind.GoToUnlessFacingLeft:
                return state.FacingDirection != RidleyFacing.Left ? cue.Target : null;
            case RidleyBodyCueKind.GoToIfCeresAndSamusLowEnergy:
                if (slot.EnemyDefinitionPointer == EnemyDefinitionId.Ridley)
                    return null;
                if (samus is null)
                {
                    throw new InvalidOperationException(
                        "Ceres Ridley fireball branch requires the active Samus actor.");
                }
                if (samus.Health >= 30)
                    return null;
                // $A6:E4E2 stores 8 to the Ridley timer at $7E:7800 before the goto,
                // cutting the fireball hover short.
                state.FunctionTimer = NativeCountdown.Start(8);
                return cue.Target;
            case RidleyBodyCueKind.Roar:
                state.Roaring = true;
                _audio.QueueEnemySound(SoundEffectId.FromCartridge(SoundEffectLibrary.Library2, RidleySoundDefinitions.Roar), maximumQueued: 6);
                return null;
            case RidleyBodyCueKind.ClearRoaringFlag:
                state.Roaring = false;
                return null;
            case RidleyBodyCueKind.SetFeetDistance:
                state.FeetDistanceIndex = cue.FeetDistanceIndex;
                return null;
            case RidleyBodyCueKind.Move:
                slot.Position = slot.Position.WithWholeX(unchecked((ushort)(slot.Position.X.Whole + cue.Displacement.X)));
                slot.Position = slot.Position.WithWholeY(unchecked((ushort)(slot.Position.Y.Whole + cue.Displacement.Y)));
                return null;
            case RidleyBodyCueKind.FaceForward:
                state.FacingDirection = RidleyFacing.Turning;
                return null;
            case RidleyBodyCueKind.FlipLeft:
                RidleyTail.MirrorRidleyTail(state);
                state.FacingDirection = RidleyFacing.Left;
                return null;
            case RidleyBodyCueKind.FlipRight:
                RidleyTail.MirrorRidleyTail(state);
                state.FacingDirection = RidleyFacing.Right;
                return null;
            case RidleyBodyCueKind.AimFireball:
                if (samus is null)
                {
                    throw new InvalidOperationException(
                        "Ceres Ridley fireball aim requires the active Samus actor.");
                }
                _ridleyProjectiles.CalculateRidleyFireballVelocity(slot, samus);
                return null;
            case RidleyBodyCueKind.SpawnFireballWithAfterburn:
                _ridleyProjectiles.SpawnRidleyFireball(slot, spawnAfterburn: true);
                return null;
            case RidleyBodyCueKind.SpawnFireballWithoutAfterburn:
                _ridleyProjectiles.SpawnRidleyFireball(slot, spawnAfterburn: false);
                return null;
            case RidleyBodyCueKind.CeresStartLiftoff:
            {
                RidleyEnemyState liftoff = _ridleyEncounter.RequireCeresRidley(slot);
                liftoff.Function = RidleyAiFunction.CeresLiftoffAccelerating;
                liftoff.Velocity = liftoff.Velocity.WithY(SnesSignedEightEight.FromSignedRaw(-352));
                return null;
            }
            case RidleyBodyCueKind.StartLiftoff:
            {
                // The shared roar/liftoff list hands control to the real fight at
                // $B2F3 and supplies the initial upward 8.8 velocity in the same tick.
                RidleyEnemyState norfairLiftoff = _ridleyEncounter.RequireNorfairRidley(slot);
                norfairLiftoff.Function = RidleyAiFunction.NorfairEnterArena;
                norfairLiftoff.Velocity = norfairLiftoff.Velocity.WithY(SnesSignedEightEight.FromSignedRaw(-352));
                return null;
            }
            default:
                throw new InvalidOperationException($"Undefined {nameof(RidleyBodyCueKind)} {cue.Kind}.");
        }
    }

    /// <summary>Includes both Ceres and Lower Norfair Ridley definition records.</summary>
    internal static bool IsRidleyDefinition(EnemyDefinitionId definitionPointer) =>
        definitionPointer is EnemyDefinitionId.RidleyCeres or EnemyDefinitionId.Ridley;

    /// <summary>
    /// Ports the area-two branch of the shared initializer at $A6:A0F5. A defeated Ridley
    /// is deleted before any shared workspace or palette side effects, matching the native
    /// boss-bit gate; a live fight begins in layer five below the visible arena.
    /// </summary>
    internal void InitializeNorfairRidley(RoomEnemySlot slot)
    {
        if (slot.SlotIndex != 0)
        {
            throw new InvalidDataException(
                $"Lower Norfair Ridley requires native enemy slot zero, not slot {slot.SlotIndex}.");
        }

        if (_room.Services.Bosses.IsDefeated(BossBits.AreaBoss))
        {
            slot.Properties = slot.Properties.With(
                EnemyProperties.IgnoreSamusCollision |
                EnemyProperties.Deleted |
                EnemyProperties.Invisible);
            return;
        }

        slot.Parameter1 = 0;
        slot.Parameter2 = 0;
        slot.PaletteIndex = EnemyPaletteBits.Palette7;
        slot.ExtraProperties = slot.ExtraProperties.With(EnemyExtraProperties.UsesExtendedSpritemap);

        // Native property $1000 blocks Plasma Beam penetration. Its meaning is established
        // only for this boss, so preserve the raw bit instead of widening the shared enum.
        slot.Properties = slot.Properties.With(EnemyProperties.BlocksPlasmaBeam);
        slot.Properties = slot.Properties.With(EnemyProperties.IgnoreSamusCollision);
        slot.Position = slot.Position.WithWhole(new(96, 394));
        slot.Layer = 5;

        _ridleyEncounter.Ridley = new RidleyEnemyState
        {
            Function = RidleyAiFunction.WaitForDoorTransition,
            FightMode = RidleyFightMode.NotFighting,
            SpritemapPaletteIndex = SnesObjPalettes.Index7,
            CommonDrawPaletteIndex = SnesObjPalettes.Index7,
            MovementAnimationEnabled = true,
            FacingDirection = RidleyFacing.Right,
            IdleTailWhipEnabled = false,
            TailDamage = 120,
            MovementMinimum = new Position16<RoomPixels>(64, 64),
            MovementMaximum = new Position16<RoomPixels>(224, 416),
            WingFrame = 0,
            WingAnimationTimer = NativeCountdown.Start(0),
            WingAnimationTimerDelta = 0,
            TailFunctionIndex = RidleyTailFunction.None,
            TailAngleDelta = 1,
            TailMinimumClockwiseAngle = RidleyTailDefinitions.InitialMinimumClockwise,
            TailMaximumCounterClockwiseAngle = RidleyTailDefinitions.InitialMaximumCounterClockwise,
            ClockwiseWhipTarget = null,
            CounterClockwiseWhipTarget = null,
            TailExtensionSpeed = 240,
            IdealInterSegmentTailAngle = RidleyTailDefinitions.IdealInterSegmentAngle,
            TailSegments = RidleyTail.CreateInitialRidleyTailSegments(),
        };
        CeresRidleyBehavior.StartRidleyProgram(_ridleyEncounter.Ridley, slot, RidleyBodyProgram.Initial);

        // WriteColorsToTargetPalette($140, $A6:E1CF, $20) installs both body and manual
        // tail/wing source palettes. The following loops clear the two fifteen-color OBJ
        // ranges that the reveal gradually fills; color zero remains transparent.
        (_room.Artwork?.NorfairRidleyColors ?? throw new InvalidOperationException(
            "Norfair Ridley requires installed colors."))
            .ApplyInitial(_presentation.Cgram);
        _presentation.BossPalettes.NorfairRidleyBody.Fill(Bgr555.Black);
        _presentation.BossPalettes.NorfairRidleyTail.Fill(Bgr555.Black);
    }

    /// <summary>
    /// Ports <c>Ridley_Main</c> at $A6:B227 and the area-two half of the shared reveal
    /// dispatcher. Composition is advanced after the selected function, exactly like the
    /// cartridge, so a velocity change affects the same rendered frame.
    /// </summary>
    private void RunNorfairRidleyMain(
        RoomEnemySlot slot,
        SamusState? samus,
        SnesButton controllerInput,
        RoomLevelData? level = null,
        SamusProjectileSystem? samusProjectiles = null,
        SamusBombProjectileSystem? sharedProjectiles = null,
        Position16<RoomPixels> camera = default)
    {
        RidleyEnemyState state = _ridleyEncounter.RequireNorfairRidley(slot);
        state.HurtMovementClamp = Math.Max(0, state.HurtMovementClamp - 4);

        UpdateNorfairRidleyIntangibility(slot, state, camera);
        PrepareNorfairRidleyCombatFrame(state, sharedProjectiles ?? throw new InvalidOperationException(
            "Ridley's power-bomb check ($A6:BD2C) requires the shared projectile owner."));

        RunNorfairRidleyFunction(slot, state, samus, controllerInput, level);

        if (state.MovementAnimationEnabled)
        {
            CeresRidleyBehavior.UpdateRidleyHurtFlashPalettes(slot, state, slot.FrameCounter);
            _ceresRidleyBehavior.IntegrateRidleyMovement(slot, state);
            CeresRidleyBehavior.TickRidleyWingAnimation(state);
            _ridleyTail.TickRidleyTail(slot, state, samus);
            if (samusProjectiles is not null)
                _ridleyTail.ResolveRidleyTailProjectileHits(slot, state, samusProjectiles);
            // Native Main places carried Samus after body/tail movement.
            if (state.GrabState != 0 && samus is not null)
                UpdateNorfairRidleyGrabbedSamus(slot, state, samus);
        }

        UpdateNorfairRidleyHealthPalette(slot, state);
    }

    /// <summary>Dispatches the current Norfair intro, flight, swoop, pogo, grab, or death state.</summary>
    internal void RunNorfairRidleyFunction(
        RoomEnemySlot slot,
        RidleyEnemyState state,
        SamusState? samus,
        SnesButton controllerInput,
        RoomLevelData? level)
    {
        switch (state.Function)
        {
            case RidleyAiFunction.WaitForDoorTransition:
                if (_roomStatus.EnemyDoorTransitionActive)
                    return;
                state.Function = RidleyAiFunction.InitialDelay;
                state.FunctionTimer = NativeCountdown.Start(170);
                CeresRidleyBehavior.TickCeresRidleyInitialDelay(state);
                return;

            case RidleyAiFunction.InitialDelay:
                CeresRidleyBehavior.TickCeresRidleyInitialDelay(state);
                return;

            case RidleyAiFunction.FadeInEyes:
                _ceresRidleyBehavior.TickCeresRidleyEyeFade(state);
                return;

            case RidleyAiFunction.FadeInBody:
                _ceresRidleyBehavior.TickCeresRidleyBodyFade(slot, state);
                if (state.Function == RidleyAiFunction.WaitBeforeRoar)
                    slot.Layer = 2;
                return;

            case RidleyAiFunction.WaitBeforeRoar:
                state.FunctionTimer = state.FunctionTimer.Decrement().Remaining;
                if (state.FunctionTimer.IsNegative)
                {
                    CeresRidleyBehavior.StartRidleyProgram(state, slot, RidleyBodyProgram.OpeningRoar);
                    state.FunctionTimer = NativeCountdown.Start(0);
                    state.Function = RidleyAiFunction.WaitBeforeLiftoff;
                }
                return;

            case RidleyAiFunction.WaitBeforeLiftoff:
                TickNorfairRidleyArenaReveal(slot, state);
                return;

            case RidleyAiFunction.ClearVelocity:
                state.Velocity = new EightEightVector<RoomPixels>(0, 0);
                return;

            case RidleyAiFunction.NorfairEnterArena:
                MoveNorfairRidleyToward(slot, state, new Position16<RoomPixels>(64, 256), divisorIndex: 14);
                if (CeresRidleyBehavior.IsWithinRidleyRectangle(slot, new(64, 256), new(8, 8)))
                {
                    state.FightMode = RidleyFightMode.Fighting;
                    state.Function = RidleyAiFunction.NorfairSelectAttack;
                    SelectNorfairRidleyAttack(slot, state, samus, controllerInput, level);
                }
                return;

            case RidleyAiFunction.NorfairSelectAttack:
                SelectNorfairRidleyAttack(slot, state, samus, controllerInput, level);
                return;

            case RidleyAiFunction.NorfairHoverSetup:
                state.Function = RidleyAiFunction.NorfairHover;
                state.FunctionTimer = NativeCountdown.Start(128);
                TickNorfairRidleyHover(slot, state);
                return;

            case RidleyAiFunction.NorfairHover:
                TickNorfairRidleyHover(slot, state);
                return;

            case RidleyAiFunction.NorfairSwoopSetup:
                state.Function = RidleyAiFunction.NorfairSwoopMoveToStart;
                state.SwoopPhaseTimer = NativeCountdown.Start(10);
                state.SwoopAngleAccumulator = SnesAngle.Zero;
                TickNorfairRidleySwoopMoveToStart(slot, state);
                return;

            case RidleyAiFunction.NorfairSwoopMoveToStart:
                TickNorfairRidleySwoopMoveToStart(slot, state);
                return;

            case RidleyAiFunction.NorfairSwoopAimDown:
                TickNorfairRidleySwoopPhase(
                    state,
                    state.FacingDirection != RidleyFacing.Left ? 32 : -32,
                    state.FacingDirection != RidleyFacing.Left ? 512 : -512,
                    1152,
                    RidleyAiFunction.NorfairSwoopAimSideways,
                    20);
                return;

            case RidleyAiFunction.NorfairSwoopAimSideways:
                if (TickNorfairRidleySwoopPhase(
                        state,
                        state.FacingDirection != RidleyFacing.Left ? 320 : -320,
                        state.FacingDirection != RidleyFacing.Left ? 16384 : -16384,
                        1280,
                        RidleyAiFunction.NorfairSwoopAimUp,
                        16))
                {
                    state.TailWhipRequest = 1;
                }
                return;

            case RidleyAiFunction.NorfairSwoopAimUp:
                TickNorfairRidleySwoopPhase(
                    state,
                    state.FacingDirection != RidleyFacing.Left ? 512 : -512,
                    state.FacingDirection != RidleyFacing.Left ? 30720 : -30720,
                    768,
                    RidleyAiFunction.NorfairSwoopClimb,
                    32);
                return;

            case RidleyAiFunction.NorfairSwoopClimb:
                if (TickNorfairRidleySwoopPhase(
                        state,
                        state.FacingDirection != RidleyFacing.Left ? 1024 : -1024,
                        state.FacingDirection != RidleyFacing.Left ? 30720 : -30720,
                        768,
                        RidleyAiFunction.NorfairSwoopRecover,
                        32))
                {
                    SelectNorfairRidleyFacingInstruction(slot, state);
                }
                return;

            case RidleyAiFunction.NorfairSwoopRecover:
                CeresRidleyBehavior.UpdateRidleySwoopVelocity(state, 0, short.MinValue, 448);
                if (!state.SwoopPhaseTimer.IsZero)
                {
                    state.SwoopPhaseTimer = state.SwoopPhaseTimer.Decrement().Remaining;
                    return;
                }
                state.Function = SamusMovementUsesRidleyGrab(samus)
                    ? RidleyAiFunction.NorfairGrabApproach
                    : RidleyAiFunction.NorfairSelectAttack;
                return;

            case RidleyAiFunction.NorfairPogoSetup:
                state.IdealInterSegmentTailAngle = 11;
                state.TailExtensionSpeed = 384;
                state.Function = RidleyAiFunction.NorfairPogoDescending;
                state.FunctionTimer = NativeCountdown.FromRaw(unchecked((ushort)(_room.Random.Read() % 32 + 32)));
                TickNorfairRidleyPogo(slot, state, samus, descending: true);
                return;

            case RidleyAiFunction.NorfairPogoDescending:
                TickNorfairRidleyPogo(slot, state, samus, descending: true);
                return;

            case RidleyAiFunction.NorfairPogoAscending:
                TickNorfairRidleyPogo(slot, state, samus, descending: false);
                return;

            case RidleyAiFunction.NorfairFireballSetup:
                BeginNorfairRidleyGroundAttack(state);
                return;

            case RidleyAiFunction.NorfairFireballMoveToSide:
                TickNorfairRidleyGroundAttackMoveToSide(slot, state, samus);
                return;

            case RidleyAiFunction.NorfairFireballMoveToHeight:
                TickNorfairRidleyGroundAttackMoveToHeight(slot, state, samus);
                return;

            case RidleyAiFunction.NorfairFireballAttack:
                TickNorfairRidleyGroundAttack(slot, state, samus, level);
                return;

            case RidleyAiFunction.NorfairFireballRecover:
                TickNorfairRidleyGroundAttackRecovery(state, samus);
                return;

            case RidleyAiFunction.NorfairGrabApproach:
                TickNorfairRidleyGrabApproach(slot, state, samus);
                return;

            case RidleyAiFunction.NorfairReturnToArena:
                TickNorfairRidleyPowerBombDodge(slot, state);
                return;

            case RidleyAiFunction.NorfairCarrySetup:
                BeginNorfairRidleyCarry(slot, state);
                return;

            case RidleyAiFunction.NorfairCarryMoveToAnchor:
                MoveNorfairRidleyToward(slot, state, state.Target, 0);
                if (TickRidleyFunctionTimer(state))
                {
                    state.Function = RidleyAiFunction.NorfairCarryRise;
                    state.FunctionTimer = NativeCountdown.Start(32);
                }
                return;

            case RidleyAiFunction.NorfairCarryRise:
                if (TickRidleyFunctionTimer(state))
                {
                    state.IdealInterSegmentTailAngle = RidleyTailDefinitions.CarryReleaseInterSegmentAngle;
                    state.TailExtensionSpeed = RidleyTailDefinitions.CarryReleaseExtensionSpeed;
                    ReleaseNorfairRidleyGrab(state, samus);
                    state.Function = RidleyAiFunction.NorfairCarryRelease;
                    state.FunctionTimer = NativeCountdown.Start(64);
                }
                else
                    MoveNorfairRidleyToward(slot, state, state.Target.WithY(256), 0);
                return;

            case RidleyAiFunction.NorfairCarryRelease:
                if (TickRidleyFunctionTimer(state))
                {
                    state.IdealInterSegmentTailAngle = RidleyTailDefinitions.IdealInterSegmentAngle;
                    state.TailExtensionSpeed = RidleyTailDefinitions.CarryReleaseExtensionSpeed;
                    state.Function = RidleyAiFunction.NorfairSelectAttack;
                }
                else
                {
                    ushort releaseX = RidleyMovementTargets.CarryReleaseX(state.FacingDirection);
                    MoveNorfairRidleyToward(slot, state, new Position16<RoomPixels>(releaseX, 224), 0);
                }
                return;

            case RidleyAiFunction.NorfairReleaseSamus:
                TickNorfairRidleyMoveToDeathSpot(slot, state);
                return;

            case RidleyAiFunction.NorfairDeathStart:
                BeginNorfairRidleyDeathRoar(slot, state);
                return;

            case RidleyAiFunction.NorfairDeathExplosions:
                TickNorfairRidleyDeathRoar(slot, state);
                return;

            case RidleyAiFunction.NorfairDeathFall:
                TickNorfairRidleyDeathExplosions(slot, state, samus);
                return;

            case RidleyAiFunction.NorfairDeathImpact:
                BeginNorfairRidleyBreakup(slot, state);
                return;

            case RidleyAiFunction.NorfairDeathWait:
                TickNorfairRidleyBreakupWait(state);
                return;

            case RidleyAiFunction.NorfairDeathFinish:
                TickNorfairRidleyDeathFinish(slot, state);
                return;

            case RidleyAiFunction.NorfairDeathComplete:
                return;

            default:
                throw new InvalidDataException(
                    $"Lower Norfair Ridley function $A6:{(ushort)state.Function:X4} is not translated.");
        }
    }

    /// <summary>
    /// $A6:A478 advances an area-two-only fourteen-color fade every third dispatcher call.
    /// A zero pointer terminates the table and activates the shared tail/wing composition.
    /// </summary>
    internal void TickNorfairRidleyArenaReveal(RoomEnemySlot slot, RidleyEnemyState state)
    {
        state.FunctionTimer = state.FunctionTimer.Decrement().Remaining;
        if (!state.FunctionTimer.IsNegative)
            return;

        state.FunctionTimer = NativeCountdown.Start(2);
        int row = state.FadePaletteOffset;
        bool hasPalette;
        var colors = _room.Artwork?.NorfairRidleyColors ?? throw new InvalidOperationException(
            "Norfair Ridley reveal requires installed colors.");
        if (row > NorfairRidleyPaletteRomData.RevealRowCount)
            throw new InvalidDataException($"Norfair Ridley reveal row {row} escaped its native terminator.");
        hasPalette = row < NorfairRidleyPaletteRomData.RevealRowCount;
        if (hasPalette)
            colors.ApplyReveal(_presentation.Cgram, row);
        state.FadePaletteOffset++;
        if (hasPalette)
            return;

        _roomFx.PublishRidleyLiquidMotion(state, RidleyLiquidRomData.BattleHeight,
            RidleyLiquidRomData.RiseVelocity, RidleyLiquidRomData.RiseDelay);
        state.FadePaletteOffset = 0;
        CeresRidleyBehavior.StartRidleyProgram(state, slot, RidleyBodyProgram.TransitionToFlying);
        state.WingAnimationTimer = NativeCountdown.Start(8);
        state.WingAnimationTimerDelta = 8;
        foreach (RidleyTailSegment segment in state.TailSegments)
            segment.Active = true;
        state.TailFunctionIndex = RidleyTailFunction.Neutral;
        state.Function = RidleyAiFunction.ClearVelocity;
    }

    /// <summary>Chooses a native attack table by Samus movement, Ridley health, and arena position.</summary>
    internal void SelectNorfairRidleyAttack(
        RoomEnemySlot slot,
        RidleyEnemyState state,
        SamusState? samus,
        SnesButton controllerInput,
        RoomLevelData? level)
    {
        RidleyAttackSituation situation;
        if (samus?.PoseMetadata.ReadMovementType() == SamusMovementType.SpinJumping)
        {
            situation = RidleyAttackSituation.SpinJumping;
        }
        else if (slot.Health == 0)
        {
            situation = RidleyAttackSituation.ZeroHealth;
            state.ZeroHealthLungeCount++;
        }
        else if (slot.Health < 14400)
        {
            situation = RidleyAttackSituation.BelowHalfHealth;
        }
        else if (samus is not null && samus.Position.Y.Whole >= 352)
        {
            situation = RidleyAttackSituation.PogoZone;
        }
        else if (SamusMovementUsesRidleyGrab(samus))
        {
            situation = RidleyAttackSituation.DamageBoosting;
        }
        else
        {
            situation = slot.Health < 9000 ? RidleyAttackSituation.AboveHalfHealth : RidleyAttackSituation.BelowHalfHealth;
        }

        int choice = _room.Random.Generate() & 7;
        state.Function = RidleyAttackChoices.Resolve(situation, choice);

        // The native selector tail-calls the chosen routine. Retain that same-frame setup
        // so timers, instruction changes, and velocity all begin on the selected frame.
        RunNorfairRidleyFunction(slot, state, samus, controllerInput, level);
    }

    /// <summary>Steers Ridley to the facing-dependent hover point until timeout or arrival.</summary>
    internal static void TickNorfairRidleyHover(RoomEnemySlot slot, RidleyEnemyState state)
    {
        if (TickRidleyFunctionTimer(state))
        {
            state.Function = RidleyAiFunction.NorfairSelectAttack;
            return;
        }

        ushort targetX = state.FacingDirection != RidleyFacing.Left ? (ushort)96 : (ushort)192;
        MoveNorfairRidleyToward(
            slot,
            state,
            new Position16<RoomPixels>(targetX, 256),
            ReadRidleyHealthMovementDivisorIndex(state));
        if (CeresRidleyBehavior.IsWithinRidleyRectangle(slot, new(targetX, 256), new(8, 8)))
            state.Function = RidleyAiFunction.NorfairSelectAttack;
    }

    /// <summary>Moves to the high side point and initializes the swoop angle timer.</summary>
    private static void TickNorfairRidleySwoopMoveToStart(RoomEnemySlot slot, RidleyEnemyState state)
    {
        ushort targetX = state.FacingDirection != RidleyFacing.Left ? (ushort)64 : (ushort)192;
        MoveNorfairRidleyToward(slot, state, new Position16<RoomPixels>(targetX, 128), divisorIndex: 1);
        if (!CeresRidleyBehavior.IsWithinRidleyRectangle(slot, new(targetX, 128), new(8, 8)))
            return;

        state.Function = RidleyAiFunction.NorfairSwoopAimDown;
        state.SwoopPhaseTimer = NativeCountdown.Start(32);
        state.SwoopAngleAccumulator = SnesAngle.Zero;
    }

    /// <summary>Advances the swoop velocity arc and changes phase when its timer expires.</summary>
    private static bool TickNorfairRidleySwoopPhase(
        RidleyEnemyState state,
        int angleDelta,
        int targetAngle,
        int targetMagnitude,
        RidleyAiFunction nextFunction,
        ushort nextTimer)
    {
        CeresRidleyBehavior.UpdateRidleySwoopVelocity(state, angleDelta, targetAngle, targetMagnitude);
        if (!state.SwoopPhaseTimer.IsZero)
        {
            state.SwoopPhaseTimer = state.SwoopPhaseTimer.Decrement().Remaining;
            return false;
        }

        state.Function = nextFunction;
        state.SwoopPhaseTimer = NativeCountdown.FromRaw(nextTimer);
        return true;
    }

    /// <summary>
    /// Ports $B5E5/$B613. These two states alternate side targets while Samus is spin
    /// jumping; any other movement type immediately advances into the low arena attack.
    /// </summary>
    internal void TickNorfairRidleyPogo(
        RoomEnemySlot slot,
        RidleyEnemyState state,
        SamusState? samus,
        bool descending)
    {
        RidleyFacing facing = state.FacingDirection;
        ushort targetX = descending ? RidleyMovementTargets.DescendingPogoX(facing)
            : RidleyMovementTargets.AscendingPogoX(facing);
        ushort targetY = samus is null ? (ushort)352 : Math.Min(samus.Position.Y.Whole, (ushort)352);
        MoveNorfairRidleyToward(
            slot,
            state,
            new Position16<RoomPixels>(targetX, targetY),
            ReadRidleyHealthMovementDivisorIndex(state));
        state.TailWhipRequest = 1;

        if (samus?.PoseMetadata.ReadMovementType() != SamusMovementType.SpinJumping)
        {
            BeginNorfairRidleyGroundAttack(state);
            return;
        }

        // $A6:B669 uses the existing RNG byte; it does not advance the generator.
        if ((SnesLittleEndian.LowByte(_room.Random.Read())) >= 128 && !state.Roaring && state.FacingDirection != RidleyFacing.Turning)
            CeresRidleyBehavior.StartRidleyProgram(state, slot, RidleyBodyProgram.Fireballing);

        if (!TickRidleyFunctionTimer(state))
            return;

        state.Function = descending
            ? RidleyAiFunction.NorfairPogoAscending
            : RidleyAiFunction.NorfairPogoDescending;
        state.FunctionTimer = NativeCountdown.Start(128);
        SelectNorfairRidleyFacingInstruction(slot, state);
    }

    /// <summary>Resets tail extension and enters the ground fireball/pogo setup.</summary>
    private static void BeginNorfairRidleyGroundAttack(RidleyEnemyState state)
    {
        state.TailExtensionSpeed = 240;
        state.IdealInterSegmentTailAngle = 16;
        state.TailFunctionIndex = RidleyTailFunction.Neutral;
        state.Function = RidleyAiFunction.NorfairFireballMoveToSide;
    }

    /// <summary>Moves Ridley toward the facing-dependent side before descending to attack height.</summary>
    internal void TickNorfairRidleyGroundAttackMoveToSide(
        RoomEnemySlot slot,
        RidleyEnemyState state,
        SamusState? samus)
    {
        if (unchecked((short)(slot.Position.Y.Whole - 288)) < 0)
        {
            SelectNorfairRidleyFacingInstruction(slot, state);
            state.Function = RidleyAiFunction.NorfairFireballMoveToHeight;
            state.FunctionTimer = NativeCountdown.Start(32);
            TickNorfairRidleyGroundAttackMoveToHeight(slot, state, samus);
            return;
        }

        ushort targetX = RidleyMovementTargets.GroundAttackX(state.FacingDirection);
        MoveNorfairRidleyToward(slot, state, new Position16<RoomPixels>(targetX, 288), divisorIndex: 0);
    }

    /// <summary>Settles at Y=288, sets the pogo tail, and starts a randomized attack timer.</summary>
    internal void TickNorfairRidleyGroundAttackMoveToHeight(
        RoomEnemySlot slot,
        RidleyEnemyState state,
        SamusState? samus)
    {
        MoveNorfairRidleyToward(slot, state, slot.Position.Whole.WithY(288), divisorIndex: 0);
        if (!TickRidleyFunctionTimer(state))
            return;

        state.TailFunctionIndex = RidleyTailFunction.PogoSetup;
        _ridleyTail.TickRidleyPogoTail(slot, state, samus);
        InitializeNorfairRidleyPogoVelocity(state);
        state.Function = RidleyAiFunction.NorfairFireballAttack;
        state.FunctionTimer = NativeCountdown.FromRaw(unchecked((ushort)(_room.Random.Read() % 64 + 128)));
    }

    /// <summary>Checks claw grab, accelerates downward, and bounces the tail on terrain contact.</summary>
    internal void TickNorfairRidleyGroundAttack(
        RoomEnemySlot slot,
        RidleyEnemyState state,
        SamusState? samus,
        RoomLevelData? level)
    {
        if (samus is not null && SamusMovementUsesRidleyGrab(samus) &&
            RidleyClawOverlapsSamus(slot, state, samus, new Extent16<RoomPixels>(4, 4)))
        {
            state.Velocity = state.Velocity.WithY(new SnesSignedEightEight(unchecked((ushort)-Math.Max(
                512,
                Math.Abs((int)state.Velocity.Y.SignedRawValue)))));
            state.TailFunctionIndex = RidleyTailFunction.Neutral;
            state.TailAngleDelta = 1;
            BeginNorfairRidleyGrab(slot, state, samus);
            return;
        }

        int nextVertical = state.Velocity.Y.SignedRawValue + state.PogoDownwardAcceleration;
        state.Velocity = state.Velocity.WithY(new SnesSignedEightEight(unchecked((ushort)Math.Min(nextVertical, 1536))));

        if (!RidleyTail.RidleyTailTouchesTerrain(state, level))
            return;

        RidleyTailSegment tip = state.TailSegments[6];
        SpawnRidleyDust(tip.Position + new Offset16<RoomPixels>(0, 12), variant: 9);
        _audio.QueueEnemySound(SoundEffectLibrary2Sounds.RidleyTailTerrainImpact, maximumQueued: 6);
        _earthquake.EarthquakeType = 13;
        _earthquake.EarthquakeTimer = NativeCountdown.Start(4);
        SetRidleyPogoHorizontalDirection(slot, state, samus);
        InitializeNorfairRidleyPogoVelocity(state);
        for (int index = 0; index < state.TailSegments.Length; index++)
        {
            state.TailSegments[index].Distance = RidleyTailDefinitions.RestDistance(index);
            state.TailSegments[index].TargetDistance = RidleyTailDefinitions.BounceDistance;
        }
        state.TailFunctionIndex = RidleyTailFunction.Pogo;
        state.PogoBounceCount++;
        if (state.PogoBounceCount >= 2)
        {
            state.PogoBounceCount = 0;
            if (state.FacingDirection != RidleyFacing.Turning)
                CeresRidleyBehavior.StartRidleyProgram(state, slot, RidleyBodyProgram.Fireballing);
        }
        state.Function = RidleyAiFunction.NorfairFireballRecover;
    }

    /// <summary>Recovers upward after a pogo impact or returns to attack selection.</summary>
    private static void TickNorfairRidleyGroundAttackRecovery(
        RidleyEnemyState state,
        SamusState? samus)
    {
        if (samus is null || samus.Position.Y.Whole < 352 || TickRidleyFunctionTimer(state))
        {
            state.TailFunctionIndex = RidleyTailFunction.Neutral;
            state.TailAngleDelta = 1;
            state.Function = RidleyAiFunction.NorfairSelectAttack;
            return;
        }

        int nextVertical = state.Velocity.Y.SignedRawValue + state.PogoUpwardAcceleration;
        if (nextVertical >= 0)
        {
            state.Velocity = state.Velocity.WithY(new SnesSignedEightEight(0));
            state.Function = RidleyAiFunction.NorfairFireballAttack;
        }
        else
        {
            state.Velocity = state.Velocity.WithY(new SnesSignedEightEight(unchecked((ushort)nextVertical)));
        }
    }

    /// <summary>
    /// Ports the four ROM-selected velocity tables consumed by $A6:B90F. The signedness
    /// of the previous horizontal velocity chooses direction; magnitudes and both vertical
    /// accelerations are compiled cartridge definitions indexed by Ridley's health stage.
    /// </summary>
    internal void InitializeNorfairRidleyPogoVelocity(RidleyEnemyState state)
    {
        // The native routine samples the current word; it does not advance shared RNG.
        int randomIndex = _room.Random.Read() & 3;
        var definition = RidleyPogoDefinitions.Read(randomIndex, Math.Min(state.HealthStage, 3) + 2);
        state.PogoUpwardAcceleration = definition.UpwardAcceleration;
        state.PogoDownwardAcceleration = definition.DownwardAcceleration;
        state.Velocity = state.Velocity.WithY(new SnesSignedEightEight(definition.Y));
        ushort horizontalMagnitude = definition.X;
        state.Velocity = state.Velocity.WithX(new SnesSignedEightEight(state.Velocity.X.SignedRawValue < 0
            ? unchecked((ushort)-(short)horizontalMagnitude)
            : horizontalMagnitude));
    }

    /// <summary>Lunges toward a grabbable Samus, ending on passage, boundary, height, or claw contact.</summary>
    internal void TickNorfairRidleyGrabApproach(
        RoomEnemySlot slot,
        RidleyEnemyState state,
        SamusState? samus)
    {
        if (samus is null || !SamusMovementUsesRidleyGrab(samus))
        {
            HandleNorfairRidleyMissedLunge(slot, state);
            return;
        }

        // The native lunge is a passing attack, not indefinite pursuit. Test the
        // previous movement's boundary result before applying another acceleration.
        short deltaX = unchecked((short)(slot.Position.X.Whole - samus.Position.X.Whole));
        bool passedSamus = state.FacingDirection == RidleyFacing.Right ? deltaX >= 0 : deltaX < 0;
        if (state.HitRoomBoundary ||
            (passedSamus && unchecked((short)(Math.Abs((int)deltaX) - RidleyLungeDefinitions.PassDistance)) >= 0) ||
            unchecked((short)(slot.Position.Y.Whole + RidleyLungeDefinitions.ClawHeight - samus.Position.Y.Whole)) >= 0)
        {
            HandleNorfairRidleyMissedLunge(slot, state);
            return;
        }

        int sideOffset = state.FacingDirection != RidleyFacing.Left ? 16 : -16;
        ushort targetY = unchecked((ushort)(samus.Position.Y.Whole - 4));
        int divisorIndex = RidleyMovementTargets.GrabDivisorIndexes(Math.Min(state.HealthStage, 3));
        MoveNorfairRidleyToward(
            slot,
            state,
            new Position16<RoomPixels>(unchecked((ushort)(samus.Position.X.Whole + sideOffset)), targetY),
            divisorIndex);

        if (RidleyClawOverlapsSamus(slot, state, samus, new Extent16<RoomPixels>(8, 12)))
        {
            // $A6:BB56-$BB5D reverses the lunge before the carry setup accelerates.
            state.Velocity = state.Velocity.WithY(new SnesSignedEightEight(unchecked((ushort)-state.Velocity.Y.RawValue)));
            BeginNorfairRidleyGrab(slot, state, samus);
        }
    }

    /// <summary>Locks Samus to the claw and enters carry or the zero-health death approach.</summary>
    private static void BeginNorfairRidleyGrab(
        RoomEnemySlot slot,
        RidleyEnemyState state,
        SamusState samus)
    {
        ushort clawX = GetNorfairRidleyClawX(slot, state);
        ushort clawY = GetNorfairRidleyClawY(slot, state);
        state.GrabOffset = samus.Position.Whole - new Position16<RoomPixels>(clawX, clawY);
        state.GrabState = 1;
        samus.ScriptLocks.SetStationaryScriptControlLock(true);
        slot.Properties = slot.Properties.With(EnemyProperties.IgnoreSamusCollision);
        if (slot.Health == 0)
        {
            StartNorfairRidleyDeathSequence(slot, state);
            // $A6:BB8C tail-jumps to $C538 on the same successful zero-health grab.
            TickNorfairRidleyMoveToDeathSpot(slot, state);
            return;
        }
        // $BB8F falls through $BBC4 on the grabbing update.
        BeginNorfairRidleyCarry(slot, state);
    }

    /// <summary>Chooses a facing-dependent carry anchor and starts Ridley's 32-frame move.</summary>
    internal static void BeginNorfairRidleyCarry(RoomEnemySlot slot, RidleyEnemyState state)
    {
        state.Target = new Position16<RoomPixels>(
            RidleyMovementTargets.CarryAnchorX(state.FacingDirection),
            unchecked((short)(slot.Position.Y.Whole - 320)) < 0
                ? (ushort)256
                : unchecked((ushort)(slot.Position.Y.Whole - 64)));
        state.Function = RidleyAiFunction.NorfairCarryMoveToAnchor;
        state.FunctionTimer = NativeCountdown.Start(32);
        MoveNorfairRidleyToward(slot, state, state.Target, 0);
        TickRidleyFunctionTimer(state);
    }

    /// <summary>Releases Samus's script lock and grants movement-dependent intangibility outside death.</summary>
    internal static void ReleaseNorfairRidleyGrab(RidleyEnemyState state, SamusState? samus)
    {
        state.GrabState = 0;
        samus?.ScriptLocks.SetStationaryScriptControlLock(false);
        state.TailWhipRequest = 1;
        state.TailFunctionIndex = RidleyTailFunction.Neutral;
        // $A6:BC8F-BC93 leaves the timer untouched once death owns the fight.
        if (state.FightMode != RidleyFightMode.Dying)
        {
            SamusMovementType movement = samus?.PoseMetadata.ReadMovementType() ?? SamusMovementType.Standing;
            state.IntangibilityTimer = NativeCountdown.FromRaw(RidleySamusInteractionDefinitions.ReleaseIntangibilityFrames(movement));
        }
    }

    /// <summary>Decays Samus's grab offset and positions her at Ridley's moving claw.</summary>
    internal static void UpdateNorfairRidleyGrabbedSamus(
        RoomEnemySlot slot,
        RidleyEnemyState state,
        SamusState samus)
    {
        state.GrabOffset = new Offset16<RoomPixels>(
            DecayRidleyGrabOffset(state.GrabOffset.X),
            DecayRidleyGrabOffset(state.GrabOffset.Y));
        samus.Position = samus.Position.WithWhole(
            new Position16<RoomPixels>(GetNorfairRidleyClawX(slot, state), GetNorfairRidleyClawY(slot, state)) +
            state.GrabOffset);
    }

    /// <summary>Moves one signed grab-offset component four pixels toward zero.</summary>
    private static short DecayRidleyGrabOffset(short offset)
    {
        if (offset > 0)
            return (short)Math.Max(0, offset - 4);
        if (offset < 0)
            return (short)Math.Min(0, offset + 4);
        return offset;
    }

    /// <summary>Tests Samus's radius against the facing and foot-frame adjusted claw point.</summary>
    internal static bool RidleyClawOverlapsSamus(
        RoomEnemySlot slot,
        RidleyEnemyState state,
        SamusState samus,
        Extent16<RoomPixels> radius) =>
        Math.Abs(unchecked((short)(samus.Position.X.Whole - GetNorfairRidleyClawX(slot, state)))) <
            samus.Kinematics.Radius.X + radius.X &&
        Math.Abs(unchecked((short)(samus.Position.Y.Whole - GetNorfairRidleyClawY(slot, state)))) <
            samus.Kinematics.Radius.Y + radius.Y;

    /// <summary>Applies the facing-dependent claw X offset to Ridley's body position.</summary>
    private static ushort GetNorfairRidleyClawX(RoomEnemySlot slot, RidleyEnemyState state) =>
        unchecked((ushort)(slot.Position.X.Whole + RidleyClawOffsets.ReadX(state.FacingDirection)));

    /// <summary>Applies the current feet-frame claw Y offset to Ridley's body position.</summary>
    private static ushort GetNorfairRidleyClawY(RoomEnemySlot slot, RidleyEnemyState state) =>
        unchecked((ushort)(slot.Position.Y.Whole + RidleyClawOffsets.ReadY(state.FeetDistanceIndex)));

    /// <summary>Decrements a native word timer and reports signed underflow.</summary>
    private static bool TickRidleyFunctionTimer(RidleyEnemyState state)
    {
        state.FunctionTimer = state.FunctionTimer.Decrement().Remaining;
        return state.FunctionTimer.IsNegative;
    }

    /// <summary>Starts a turn only when Ridley faces away from the arena half identified by X bit seven.</summary>
    internal static void SelectNorfairRidleyFacingInstruction(RoomEnemySlot slot, RidleyEnemyState state)
    {
        if (state.FacingDirection == RidleyFacing.Turning)
            return;
        // D955 reads the word at Enemy.XPosition-1: its sign is bit 7 of
        // the low X byte, not the sign of the full coordinate. Keep an inward
        // facing actor's current animation and timer untouched.
        bool rightHalf = SnesLittleEndian.LowByte(slot.Position.X.Whole) >= 128;
        if (state.FacingDirection == RidleyFacing.Left ? rightHalf : !rightHalf)
            return;
        CeresRidleyBehavior.StartRidleyProgram(
            state,
            slot,
            state.FacingDirection == RidleyFacing.Left
                ? RidleyBodyProgram.TurnFromLeftToRight
                : RidleyBodyProgram.TurnFromRightToLeft);
        slot.InstructionTimer = NativeCountdown.Start(2);
    }

    /// <summary>Selects a hover acceleration divisor from Ridley's capped health stage.</summary>
    internal static int ReadRidleyHealthMovementDivisorIndex(RidleyEnemyState state) =>
        RidleyMovementTargets.HoverDivisorIndexes(Math.Min(state.HealthStage, 3));

    /// <summary>Checks the validated pose movement type against Ridley's grabbable states.</summary>
    internal static bool SamusMovementUsesRidleyGrab(SamusState? samus)
    {
        if (samus is null)
            return false;
        SamusMovementType movement = samus.PoseMetadata.ReadMovementType();
        return RidleySamusInteractionDefinitions.CanGrab(movement);
    }

    /// <summary>Accelerates both 8.8 velocity axes toward a target using native inertia divisors.</summary>
    internal static void MoveNorfairRidleyToward(
        RoomEnemySlot slot,
        RidleyEnemyState state,
        Position16<RoomPixels> target,
        int divisorIndex,
        ushort reversalBoost = 0)
    {
        ushort divisor = RidleyInertiaDefinitions.Divisor(divisorIndex);

        state.Velocity = new EightEightVector<RoomPixels>(
            new SnesSignedEightEight(AccelerateNorfairRidleyAxis(
                state.Velocity.X.RawValue,
                slot.Position.X.Whole, target.X,
                divisor,
                reversalBoost)),
            new SnesSignedEightEight(AccelerateNorfairRidleyAxis(
                state.Velocity.Y.RawValue,
                slot.Position.Y.Whole, target.Y,
                divisor,
                reversalBoost)));
    }

    /// <summary>Preserves ADC/SBC carry across reversal and normal acceleration steps on one axis.</summary>
    private static ushort AccelerateNorfairRidleyAxis(
        ushort velocityWord,
        ushort position,
        ushort target,
        ushort divisor,
        ushort reversalBoost)
    {
        short distance = unchecked((short)(position - target));
        if (distance == 0) return velocityWord;
        ushort step = (ushort)Math.Max(1, Math.Abs((int)distance) / divisor);
        bool carry = position >= target;
        ushort velocity = velocityWord;

        // $A6:D559..D5A5 and D5CF..D61B retain carry/borrow across the chained
        // ADC/SBC instructions. Crossing zero can add one more acceleration unit;
        // combining these operations into integer arithmetic changes the trajectory.
        if (distance > 0)
        {
            if ((short)velocity >= 0)
            {
                carry = true;
                Subtract(reversalBoost);
                carry = true;
                Subtract(RidleyInertiaDefinitions.ReversalAcceleration);
                Subtract(step);
            }
            Subtract(step);
            if (unchecked((short)(velocity - RidleyInertiaDefinitions.MinimumVelocity)) < 0) velocity = RidleyInertiaDefinitions.MinimumVelocity;
        }
        else
        {
            if ((short)velocity < 0)
            {
                carry = false;
                Add(reversalBoost);
                carry = false;
                Add(RidleyInertiaDefinitions.ReversalAcceleration);
                Add(step);
            }
            Add(step);
            if (unchecked((short)(velocity - RidleyInertiaDefinitions.MaximumVelocity)) >= 0) velocity = RidleyInertiaDefinitions.MaximumVelocity;
        }
        return velocity;

        void Add(ushort operand)
        {
            int result = velocity + operand + (carry ? 1 : 0);
            velocity = unchecked((ushort)result);
            carry = result > ushort.MaxValue;
        }
        void Subtract(ushort operand)
        {
            int result = velocity - operand - (carry ? 0 : 1);
            velocity = unchecked((ushort)result);
            carry = result >= 0;
        }
    }

    /// <summary>Selects three damage-color rows at 9000, 5400, and 1800 health.</summary>
    internal void UpdateNorfairRidleyHealthPalette(RoomEnemySlot slot, RidleyEnemyState state)
    {
        state.HealthStage = slot.Health switch
        {
            < 1800 => 3,
            < 5400 => 2,
            < 9000 => 1,
            _ => 0,
        };
        if (state.HealthStage == 0)
            return;

        // Norfair and Ceres select the same authored $A6:E46A health rows. Keep
        // Norfair's health thresholds here, but use the installed palette so
        // editing that one asset updates both encounters consistently.
        int row = state.HealthStage - 1;
        (_presentation.CeresRidleyColors ?? throw new InvalidOperationException(
            "Ridley damage requires installed health colors."))
            .ApplyHealth(_presentation.Cgram, row);
    }

    /// <summary>Ports Lower Norfair Ridley's initialization AI.</summary>
    internal void RunRidleyInitializationAi(RoomEnemySlot slot, RoomLevelData? _, SamusState? _1, SnesButton _2, ushort _3) =>
        InitializeNorfairRidley(slot);

    /// <summary>Ports the breakup explosion actor's initialization AI.</summary>
    internal void RunRidleyExplosionInitializationAi(RoomEnemySlot slot, RoomLevelData? _, SamusState? _1, SnesButton _2, ushort _3) =>
        InitializeNorfairRidleyExplosion(
            slot,
            _slotTable._slots[0],
            _ridleyEncounter.Ridley ?? throw new InvalidOperationException(
                "A Ridley breakup actor has no shared Ridley owner."));

    /// <summary>Ports Lower Norfair Ridley's main AI.</summary>
    internal void RunRidleyMainAi(RoomEnemySlot slot, IEnemyAiFrame frame) =>
        RunNorfairRidleyMain(
            slot,
            frame.Samus,
            frame.ControllerInput,
            frame.Level,
            frame.SamusProjectiles,
            frame.SharedProjectiles,
            frame.Camera);

    /// <summary>Ports the breakup explosion actor's main AI.</summary>
    internal void RunRidleyExplosionMainAi(RoomEnemySlot slot, IEnemyAiFrame _) => RunNorfairRidleyExplosionMain(slot);

    /// <summary>Runs Ceres and Lower Norfair Ridley's body as typed programs in place of their bank-$A6 lists (#641).</summary>
    internal void AdvanceRidleyBodyTypedAnimation(RoomEnemySlot slot, EnemyInstructionContext context) =>
        AdvanceRidleyBodyAnimation(slot, context.Samus);

    /// <summary>Runs the breakup explosion actor's typed program.</summary>
    internal void AdvanceRidleyBreakupTypedAnimation(RoomEnemySlot slot, EnemyInstructionContext _) => AdvanceRidleyBreakupAnimation(slot);
    /// <summary>
    /// <c>LungeIfPowerBombActiveAndNotHoldingSamus</c> at $A6:BD2C, run before the function on
    /// every main and even hurt frame. It reads $0CEE, which is set as soon as a power bomb is
    /// laid, so Ridley lunges in the same frame Samus lays one.
    /// </summary>
    internal static void PrepareNorfairRidleyCombatFrame(
        RidleyEnemyState state,
        SamusBombProjectileSystem sharedProjectiles)
    {
        if (state.FightMode is RidleyFightMode.NotFighting or RidleyFightMode.Dying or RidleyFightMode.PowerBombDodge)
            return;
        if (!sharedProjectiles.PowerBombExplosion.IsArmed || state.GrabState != 0)
            return;

        // Function $B84D resets both tail control words before the lunge.
        state.TailFunctionIndex = RidleyTailFunction.Neutral;
        state.TailAngleDelta = 1;
        state.Function = RidleyAiFunction.NorfairGrabApproach;
    }

    /// <summary>
    /// Ports hurt AI $A6:B297. Function, movement, wing, and tail updates execute only on
    /// even actor frames; tail armor, flash/palette work, and the grabbed-Samus lag counter
    /// remain active on every hurt frame.
    /// </summary>
    internal void RunNorfairRidleyHurt(
        RoomEnemySlot body,
        SamusState? samus,
        SnesButton controllerInput,
        RoomLevelData? level,
        SamusProjectileSystem? samusProjectiles,
        SamusBombProjectileSystem sharedProjectiles,
        Position16<RoomPixels> camera)
    {
        RidleyEnemyState state = _ridleyEncounter.RequireNorfairRidley(body);
        if ((body.FrameCounter & 1) == 0)
        {
            UpdateNorfairRidleyIntangibility(body, state, camera);
            PrepareNorfairRidleyCombatFrame(state, sharedProjectiles);
            RunNorfairRidleyFunction(body, state, samus, controllerInput, level);
            if (state.MovementAnimationEnabled)
            {
                _ceresRidleyBehavior.IntegrateRidleyMovement(body, state);
                CeresRidleyBehavior.TickRidleyWingAnimation(state);
                _ridleyTail.TickRidleyTail(body, state, samus);
            }
        }

        if (samusProjectiles is not null && state.MovementAnimationEnabled)
            _ridleyTail.ResolveRidleyTailProjectileHits(body, state, samusProjectiles);
        CeresRidleyBehavior.UpdateRidleyHurtFlashPalettes(body, state, body.FrameCounter);
        UpdateNorfairRidleyHealthPalette(body, state);

        state.HurtMovementClamp = Math.Min(8, state.HurtMovementClamp + 1);
        if (state.GrabState != 0 && state.HurtMovementClamp >= 8 && samus is not null)
            UpdateNorfairRidleyGrabbedSamus(body, state, samus);
    }

    /// <summary>
    /// Ports the collision-relevant portion of <c>HandleRidleySamusInteractionBit</c> at
    /// $A6:BCB4. Off-screen bodies disable collision and suspend the release timer.
    /// On-screen fight AI clears the gate before ticking that timer. EnemyMain has
    /// already consumed the previous gate value for this update's body collision.
    /// </summary>
    internal static void UpdateNorfairRidleyIntangibility(
        RoomEnemySlot body,
        RidleyEnemyState state,
        Position16<RoomPixels> camera)
    {
        if (state.FightMode == RidleyFightMode.Dying)
            return;
        if (state.FightMode != RidleyFightMode.NotFighting)
        {
            if (RidleyCollisionDefinitions.IsOutsideInteractionWindow(body.Position.Whole, camera))
            {
                body.Properties = body.Properties.With(EnemyProperties.IgnoreSamusCollision);
                return;
            }
            body.Properties = body.Properties.Without(EnemyProperties.IgnoreSamusCollision);
        }
        if (!state.IntangibilityTimer.IsZero)
        {
            state.IntangibilityTimer = state.IntangibilityTimer.Decrement().Remaining;
            if (state.IntangibilityTimer.IsZero)
                body.Properties = body.Properties.Without(EnemyProperties.IgnoreSamusCollision);
        }
    }

    /// <summary>Allocates enemy projectile $E509 with one ROM-authored dust variant.</summary>
    internal void SpawnRidleyDust(Position16<RoomPixels> position, ushort variant)
    {
        RoomEnemyProjectileSlot? dust = _projectiles.AllocateEnemyProjectile();
        if (dust is null)
            return;

        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
            dust,
            RoomEnemyProjectileKind.MiscDustExplosion,
            graphicsIndex: 0);
        dust.Position = dust.Position.WithWhole(position);
        dust.StartAnimation(new MiscDustProjectileAnimation(MiscDustProjectileDefinitions.Program(
            (MiscDustAnimation)Math.Min(variant, (ushort)MiscDustAnimation.BigExplosion))));
    }
    /// <summary>Last item-drop request published by Ridley's terminal routine.</summary>
    public bool RidleyDeathDropRequested { get; private set; }

    /// <summary>On zero health, makes Ridley intangible and enters the Samus-release death state.</summary>
    private static void StartNorfairRidleyDeathSequence(
        RoomEnemySlot body,
        RidleyEnemyState state)
    {
        if (body.Health != 0 || state.FightMode == RidleyFightMode.Dying)
            return;

        state.FightMode = RidleyFightMode.Dying;
        body.Properties = body.Properties.With(EnemyProperties.IgnoreSamusCollision);
        state.Function = RidleyAiFunction.NorfairReleaseSamus;
    }

    /// <summary>
    /// $A6:C538 keeps the already-grabbed Samus attached while accelerating Ridley toward
    /// the four-pixel rectangle centered at (128,328). Reaching it immediately tail-calls
    /// the roar setup instead of spending a host-only transition frame.
    /// </summary>
    internal static void TickNorfairRidleyMoveToDeathSpot(
        RoomEnemySlot body,
        RidleyEnemyState state)
    {
        MoveNorfairRidleyToward(body, state, new Position16<RoomPixels>(128, 328),
            RidleyInertiaDefinitions.DeathDivisorIndex, RidleyInertiaDefinitions.DeathReversalBoost);
        if (CeresRidleyBehavior.IsWithinRidleyRectangle(body, new(128, 328), new(4, 4)))
            BeginNorfairRidleyDeathRoar(body, state);
    }

    /// <summary>Ports $A6:C53E: select the death-roar list and arm its 32-frame move.</summary>
    private static void BeginNorfairRidleyDeathRoar(
        RoomEnemySlot body,
        RidleyEnemyState state)
    {
        CeresRidleyBehavior.StartRidleyProgram(state, body, RidleyBodyProgram.DeathRoar);
        state.Function = RidleyAiFunction.NorfairDeathExplosions;
        state.FunctionTimer = NativeCountdown.Start(32);
    }

    /// <summary>
    /// Ports $A6:C551. Ridley continues converging on the death point throughout the roar;
    /// expiry freezes velocity and starts lowering the room acid before entering the long
    /// small-explosion phase in the same dispatcher call.
    /// </summary>
    internal void TickNorfairRidleyDeathRoar(
        RoomEnemySlot body,
        RidleyEnemyState state)
    {
        MoveNorfairRidleyToward(body, state, new Position16<RoomPixels>(128, 328),
            RidleyInertiaDefinitions.DeathDivisorIndex, RidleyInertiaDefinitions.DeathReversalBoost);
        if (!TickRidleyFunctionTimer(state))
            return;

        state.Velocity = new EightEightVector<RoomPixels>(0, 0);
        _roomFx.PublishRidleyLiquidMotion(state, RidleyLiquidRomData.DrainedHeight,
            RidleyLiquidRomData.DrainVelocity, RidleyLiquidRomData.DrainDelay);
        state.DeathExplosionTimer = NativeCountdown.Start(0);
        state.DeathExplosionCount = 0;
        state.Function = RidleyAiFunction.NorfairDeathFall;
        state.FunctionTimer = NativeCountdown.Start(160);
        TickNorfairRidleyDeathExplosions(body, state, samus: null);
    }

    /// <summary>
    /// Ports $A6:C588. A ROM-positioned small explosion appears every five calls while the
    /// acid lowers. When the 160-frame clock expires, Samus is released and Ridley's twelve
    /// independently moving body/tail breakup enemies are allocated from the real slot pool.
    /// </summary>
    internal void TickNorfairRidleyDeathExplosions(
        RoomEnemySlot body,
        RidleyEnemyState state,
        SamusState? samus)
    {
        SpawnSmallExplosionNearNorfairRidley(body, state);
        if (!TickRidleyFunctionTimer(state))
            return;

        if (state.GrabState != 0)
            ReleaseNorfairRidleyGrab(state, samus);
        state.Function = RidleyAiFunction.NorfairDeathImpact;
        state.Velocity = new EightEightVector<RoomPixels>(0, 0);
        SpawnNorfairRidleyBreakupActors(body, state);
    }

    /// <summary>
    /// $A6:C5A8 performs one final small-explosion tick, hides the intact body, and disables
    /// shared wing/tail movement so only the breakup actors remain visible.
    /// </summary>
    private void BeginNorfairRidleyBreakup(
        RoomEnemySlot body,
        RidleyEnemyState state)
    {
        SpawnSmallExplosionNearNorfairRidley(body, state);
        state.MovementAnimationEnabled = false;
        body.Properties = body.Properties.With(EnemyProperties.Invisible);
        state.Function = RidleyAiFunction.NorfairDeathWait;
        state.FunctionTimer = NativeCountdown.Start(32);
    }

    /// <summary>$A6:C5C8's deliberate 32-frame pre-wait before the final 256 frames.</summary>
    private static void TickNorfairRidleyBreakupWait(RidleyEnemyState state)
    {
        if (!TickRidleyFunctionTimer(state))
            return;
        state.Function = RidleyAiFunction.NorfairDeathFinish;
        state.FunctionTimer = NativeCountdown.Start(256);
    }

    /// <summary>
    /// Ports $A6:C5DA. Boss persistence is published before the slot is deleted, exactly
    /// when the retail item drop and delayed music-track-three request occur.
    /// </summary>
    private void TickNorfairRidleyDeathFinish(
        RoomEnemySlot body,
        RidleyEnemyState state)
    {
        if (!TickRidleyFunctionTimer(state))
            return;

        _room.Services.Bosses.SetDefeated(BossBits.AreaBoss);
        state.BossDefeatPublished = true;

        // $A0:B8AC scatters sixteen pickups through Ridley's authored 128x64 arena
        // rectangle. The body is deleted immediately afterward, so the drop actors must
        // retain header $E17F themselves.
        _deathsAndDrops.SpawnEnemyDropScatter(
            EnemyDefinitionId.Ridley,
            count: 16,
            xBase: 64,
            xMask: 127,
            yBase: 320,
            yMask: SnesLittleEndian.Word(0, 63));
        RidleyDeathDropRequested = true;
        state.MusicRequest = MusicCommand.SelectTrack(3);
        body.Properties = body.Properties.With(EnemyProperties.Deleted);
        state.Function = RidleyAiFunction.NorfairDeathComplete;
    }

    /// <summary>Ports $A6:C623 and its ten signed X/Y offset pairs at $A6:C66E.</summary>
    internal void SpawnSmallExplosionNearNorfairRidley(
        RoomEnemySlot body,
        RidleyEnemyState state)
    {
        if (!state.DeathExplosionTimer.IsZero)
        {
            state.DeathExplosionTimer = state.DeathExplosionTimer.Decrement().Remaining;
            return;
        }

        state.DeathExplosionTimer = NativeCountdown.Start(4);
        state.DeathExplosionCount++;
        if (state.DeathExplosionCount >= 10)
            state.DeathExplosionCount = 0;

        SpawnRidleyDust(
            body.Position.Whole + RidleyExplosionDefinitions.DeathExplosionPlacement(state.DeathExplosionCount),
            variant: 3);
        state.LastDeathSoundEffect = SoundEffectId.FromCartridge(SoundEffectLibrary.Library2, RidleySoundDefinitions.DeathExplosion);
    }
    /// <summary>Allocates the body and tail pieces that continue moving after the intact body hides.</summary>
    internal void SpawnNorfairRidleyBreakupActors(
        RoomEnemySlot body,
        RidleyEnemyState state)
    {
        if (state.DeathBreakupSpawned)
            return;
        state.DeathBreakupSpawned = true;

        RoomEnemyDefinition definition = _room.LoadedDefinitions.ReadEnemyDefinition(EnemyDefinitionId.RidleyExplosion);
        RidleyExplosionDefinitions.SpawnInNativeOrder(SpawnPart);

        void SpawnPart(ushort parameter)
        {
            int slotIndex = Array.FindIndex(
                _slotTable._slots,
                candidate => candidate.EnemyDefinitionPointer == 0);
            if (slotIndex < 0)
            {
                throw new InvalidOperationException(
                    "Lower Norfair Ridley breakup exhausted the 32-slot enemy pool.");
            }

            // The embedded records at $A6:C987-$CA46 all use the same zero position,
            // property $2C00, and one even parameter. Initialization replaces position and
            // instruction from the still-live shared Ridley joints below.
            RoomEnemyPopulationRecord population = new(
                EnemyDefinitionId.RidleyExplosion,
                Position: Position16<RoomPixels>.Zero,
                InitializationParameter: 0,
                Properties: new EnemyPropertyWord(
                    EnemyProperties.ProcessInstructions |
                    EnemyProperties.ProcessOffScreen |
                    EnemyProperties.IgnoreSamusCollision),
                ExtraProperties: new EnemyExtraPropertyWord(EnemyExtraProperties.None),
                Parameter1: parameter,
                Parameter2: 0);
            RoomEnemySlot fragment = _slotTable._slots[slotIndex];
            _dispatch.InitializeSlotFromDefinition(fragment, population, definition);
            InitializeNorfairRidleyExplosion(fragment, body, state);
            // SpawnEnemy's post-init $A0:93D9 write precedes the first instruction tick.
            fragment.SpritemapPointer = CommonEnemyEmptyExtendedFrameDefinitions.EmptySpritemap;
            _slotTable.EnemyCount = unchecked((ushort)Math.Max(_slotTable.EnemyCount, slotIndex + 1));
            _slotTable.FirstFreeEnemyIndex = unchecked((ushort)((slotIndex + 1) * RoomEnemySystem.NativeSlotSize));
        }
    }

    /// <summary>Ports initialization AI $A6:C696 for one parameter $00..$16.</summary>
    internal void InitializeNorfairRidleyExplosion(
        RoomEnemySlot fragment,
        RoomEnemySlot body,
        RidleyEnemyState state)
    {
        RidleyExplosionPart part = ClosedNativeWords.Decode<RidleyExplosionPart>(
            checked((ushort)fragment.Parameter1), "Ridley explosion part");

        fragment.InstructionTimer = NativeCountdown.Start(1);
        fragment.Timer = NativeCountdown.Start(0);
        fragment.VramTilesIndex = 0;
        fragment.PaletteIndex = EnemyPaletteBits.Palette7;
        fragment.VariableF = RidleyExplosionDefinitions.GetPart(checked((ushort)fragment.Parameter1)).Lifetime;

        ushort random = _room.Random.Generate();
        ushort horizontalMagnitude = unchecked((ushort)(random & RidleyExplosionDefinitions.FragmentSpeedMask));
        fragment.VariableB = unchecked((short)random) < 0
            ? unchecked((ushort)-horizontalMagnitude)
            : horizontalMagnitude;
        fragment.VariableC = 0;

        if (part <= RidleyExplosionPart.TailTip)
        {
            int tailIndex = (int)part >> 1;
            RidleyTailSegment tail = state.TailSegments[tailIndex];
            fragment.Position = fragment.Position.WithWhole(tail.Position);
            int tailTipOrientation =
                unchecked((byte)(unchecked((byte)tail.Angle) +
                    unchecked((byte)state.TailSegments[5].Angle) + 8)) / 16;
            StartRidleyBreakupProgram(state, fragment, RidleyExplosionDefinitions.SelectTailFrame(
                part,
                tailTipOrientation));
            return;
        }

        RidleyExplosionBodyPartDefinition bodyPart =
            RidleyExplosionDefinitions.SelectBodyPart(
                part,
                facingRight: state.FacingDirection != RidleyFacing.Left);
        fragment.Position = fragment.Position.WithWhole(body.Position.Whole + bodyPart.Offset);
        StartRidleyBreakupProgram(state, fragment, bodyPart.Frame);
    }

    /// <summary>Starts a breakup fragment's typed program for <paramref name="frame"/>.</summary>
    private static void StartRidleyBreakupProgram(RidleyEnemyState state, RoomEnemySlot fragment, RidleyBreakupFrame frame) =>
        state.BreakupPrograms[fragment.SlotIndex] = AnimationProgramCursor<RidleyBreakupFrame>.Start(frame);

    /// <summary>
    /// Runs a breakup fragment's program where the enemy instruction interpreter would: its pose shows for one update
    /// and then holds, raising the new-frame flag when shown.
    /// </summary>
    internal void AdvanceRidleyBreakupAnimation(RoomEnemySlot fragment)
    {
        if (!EnemyProgramAnimation.Expire(fragment))
            return;
        RidleyEnemyState state = _ridleyEncounter.Ridley ?? throw new InvalidOperationException(
            $"Ridley breakup fragment in slot {fragment.SlotIndex} requires Ridley's encounter state.");
        if (!state.BreakupPrograms.TryGetValue(fragment.SlotIndex, out AnimationProgramCursor<RidleyBreakupFrame> cursor))
            throw new InvalidOperationException($"Ridley breakup fragment in slot {fragment.SlotIndex} has no program.");
        EnemyProgramAnimation.Run(fragment, RidleyBreakupAnimations.Programs, ref cursor, RidleyBreakupFrameDefinitions.Spritemap,
            (cue, continuation) => throw new InvalidOperationException("Ridley's breakup programs publish no cues."));
        state.BreakupPrograms[fragment.SlotIndex] = cursor;
    }

    /// <summary>Ports main AI $A6:C8D4: flicker, drag, gravity, movement, and lifetime.</summary>
    internal void RunNorfairRidleyExplosionMain(RoomEnemySlot fragment)
    {
        if ((fragment.FrameCounter & 1) != 0)
            fragment.Properties = fragment.Properties.With(EnemyProperties.Invisible);
        else
            fragment.Properties = fragment.Properties.Without(EnemyProperties.Invisible);
        fragment.FrameCounter &= 1;

        int horizontal = unchecked((short)fragment.VariableB);
        int magnitude = Math.Max(0, Math.Abs(horizontal) - 4);
        fragment.VariableB = unchecked((ushort)(horizontal < 0 ? -magnitude : magnitude));
        fragment.VariableC = unchecked((ushort)(fragment.VariableC + 4));
        fragment.Position = fragment.Position.Advance(new(new SnesSignedEightEight(fragment.VariableB).ToSixteenSixteenDelta(), new SnesSignedEightEight(fragment.VariableC).ToSixteenSixteenDelta()));

        NativeWordCounterStep lifetime = NativeWordCounter.Decrement(fragment.VariableF);
        fragment.VariableF = lifetime.Value;
        if (lifetime.IsNonNegative)
            return;

        // $A6:C90B calls EnemyDeathAnimation(0): clear the slot immediately and
        // retain the fragment header/position in the shared explosion-to-pickup actor.
        _deathsAndDrops.StartGenericEnemyDeath(fragment, (ushort)EnemyDeathAnimation.SmallExplosion);
    }
    /// <summary>Ports $A6:BA85: death has priority over power-bomb dodge and ordinary retreat.</summary>
    private void HandleNorfairRidleyMissedLunge(RoomEnemySlot slot, RidleyEnemyState state)
    {
        if (state.ZeroHealthLungeCount >= RidleyLungeDefinitions.DeathLungeLimit)
        {
            StartNorfairRidleyDeathSequence(slot, state);
            BeginNorfairRidleyDeathRoar(slot, state);
            return;
        }
        if (_audio.PowerBomb is { IsArmed: true })
        {
            state.Function = RidleyAiFunction.NorfairReturnToArena;
            TickNorfairRidleyPowerBombDodge(slot, state);
            return;
        }
        state.Function = RidleyAiFunction.NorfairHoverSetup;
        state.TailWhipRequest = 1;
    }

    /// <summary>Ports $A6:BD4E, the power-bomb branch of missed-lunge recovery.</summary>
    private void TickNorfairRidleyPowerBombDodge(RoomEnemySlot slot, RidleyEnemyState state)
    {
        if (_audio.PowerBomb is not { IsArmed: true } bomb)
        {
            state.FightMode = RidleyFightMode.Fighting;
            state.Function = state.GrabState == 0
                ? RidleyAiFunction.NorfairSelectAttack : RidleyAiFunction.NorfairCarrySetup;
            return;
        }
        state.FightMode = RidleyFightMode.PowerBombDodge;
        ushort targetX = unchecked((short)(bomb.Position.X - RidleyLungeDefinitions.DodgeSplitX)) >= 0
            ? RidleyLungeDefinitions.DodgeLeftX : RidleyLungeDefinitions.DodgeRightX;
        ushort targetY = unchecked((short)(bomb.Position.Y - RidleyLungeDefinitions.DodgeSplitY)) >= 0
            ? RidleyLungeDefinitions.DodgeUpperY : RidleyLungeDefinitions.DodgeLowerY;
        MoveNorfairRidleyToward(slot, state, new Position16<RoomPixels>(targetX, targetY), ReadRidleyHealthMovementDivisorIndex(state));
    }
    /// <summary>$A6:B8A9 chooses the next bounce's direction from position and the current seed.</summary>
    internal void SetRidleyPogoHorizontalDirection(RoomEnemySlot slot, RidleyEnemyState state, SamusState? samus)
    {
        if (state.Velocity.X.RawValue == 0)
            state.Velocity = state.Velocity.WithX(SnesSignedEightEight.FromSignedRaw(SnesLittleEndian.LowByte(slot.Position.X.Whole) < 128 ? (short)192 : (short)-192));
        bool negative = state.Velocity.X.SignedRawValue < 0;
        bool reverse;
        if ((short)(slot.Position.X.Whole - state.MovementMinimum.X) < 0) reverse = negative;
        else if ((short)(slot.Position.X.Whole - state.MovementMaximum.X) >= 0) reverse = !negative;
        else
        {
            bool oppositeSigns = ((ushort)(slot.Position.X.Whole - (samus?.Position.X.Whole ?? 0)) ^ state.Velocity.X.RawValue) > short.MaxValue;
            reverse = oppositeSigns ? _room.Random.Read() < 1365 : _room.Random.Read() >= 1365;
        }
        if (reverse) state.Velocity = state.Velocity.WithX(new SnesSignedEightEight(unchecked((ushort)-state.Velocity.X.RawValue)));
    }

    /// <inheritdoc/>
    public void ResetForRoomLoad(IRoomEnemyLoad request)
    {
        RidleyDeathDropRequested = false;
    }

    /// <inheritdoc/>
    public void BeginEnemyFrame(RoomEnemyFrame frame)
    {
        // These are one-shot publications made by Ridley's current AI call, matching
        // the global QueueMusic/QueueSfx calls in bank $A6. Clear them at the same
        // enemy-frame boundary as every other Last* request above.
        if (_ridleyEncounter.Ridley is not null)
        {
            _ridleyEncounter.Ridley.LastDeathSoundEffect = null;
            _ridleyEncounter.Ridley.MusicRequest = null;
        }
    }

    /// <summary>Requires the extended Ridley hitbox callback and suppresses generic death on shots.</summary>
    internal static void RegisterRidleyShotReactions(EnemySharedReactions table)
    {
        table.AddShot(new EnemyShotReaction(enemy => enemy.EnemyDefinitionPointer == EnemyDefinitionId.Ridley &&
                                                     EnemyCollision.ShotAi(enemy) == EnemyInteractionCallback.RidleyShot)
        {
            BeforeSuperMissileQuake = (in EnemyShotContext c) =>
            {
                if (!c.UsesExtendedHitboxes || c.HitboxShotAi != EnemyInteractionCallback.RidleyShot)
                {
                    throw new InvalidDataException(
                        $"Lower Norfair Ridley requires extended shot AI " +
                        $"$A6:{EnemyAiCodePointers.BankA6.RidleyShot:X4}; extended={c.UsesExtendedHitboxes}, " +
                        $"selected=$A6:{c.HitboxShotAiPointer:X4}, map=$A6:{c.Enemy.SpritemapPointer:X4}.");
                }
                return false;
            },
            ExemptFromGenericDeath = true,
        });
    }

    /// <summary>Routes bombs through Ridley's native shot callback without generic death.</summary>
    internal static void RegisterRidleyBombReactions(EnemySharedReactions table)
    {
        table.AddBombCommon(new EnemyBombReaction(c => c.Enemy.EnemyDefinitionPointer == EnemyDefinitionId.Ridley &&
                                                       c.SelectedShotAi == EnemyInteractionCallback.RidleyShot)
        {
            RunsGenericDeath = false,
        });
    }

    // $A6:B297 owns hurt frames instead of falling back to ordinary main
    // AI. Movement/function work runs only on even actor frames, while tail
    // projectile armor and hurt palettes remain live on every frame.
    /// <summary>Registers Ridley's hurt callback for the post-hit AI window.</summary>
    internal void RegisterRidleyHurtAi(EnemySharedReactions table) =>
        table.AddHurt(slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.Ridley, (slot, frame) => RunNorfairRidleyHurt(
            slot,
            frame.Samus,
            frame.ControllerInput,
            frame.Level,
            frame.SamusProjectiles,
            frame.SharedProjectiles ?? throw new InvalidOperationException(
                "Ridley's power-bomb check ($A6:BD2C) requires the shared projectile owner."),
            frame.Camera));

    /// <summary>The callback of the first Ridley hitbox the probe overlaps, or null outside its compiled frames.</summary>
    internal static bool? FindRidleyHitboxCallback(RoomEnemySlot enemy, EnemyHitboxProbe probe, out ushort callback)
    {
        callback = 0;
        if (!(RidleyBehavior.IsRidleyDefinition(enemy.EnemyDefinitionPointer) &&
            enemy.Definition.Bank == RidleyCollisionDefinitions.Bank &&
            (RidleyCollisionDefinitions.HasFrame(enemy.SpritemapPointer) ||
            probe.ArtworkInstalled)))
            return null;

        // The OAM component offsets in an installed composition are editable.
        // Contact and shot collision instead use the eleven fixed cartridge
        // body frames and their engine-owned hitbox lists.
        foreach (RidleyCollisionComponent component in
                 RidleyCollisionDefinitions.ComponentsAt(enemy.SpritemapPointer))
        {
            Position16<RoomPixels> componentOrigin = enemy.Position.Whole + component.Offset;
            ushort componentX = componentOrigin.X;
            ushort componentY = componentOrigin.Y;
            foreach (RidleyCollisionHitbox hitbox in component.Hitboxes)
            {
                ushort left = unchecked((ushort)(componentX + hitbox.Left));
                ushort top = unchecked((ushort)(componentY + hitbox.Top));
                ushort right = unchecked((ushort)(componentX + hitbox.Right));
                ushort bottom = unchecked((ushort)(componentY + hitbox.Bottom));
                if (!probe.Overlaps(left, top, right, bottom))
                    continue;
                callback = RidleyHitboxCallbackCodec.ToCallbackWord(probe.SelectShotCallback);
                return true;
            }
        }
        return false;
    
    }

    /// <summary>Ridley's Power Bomb reaction, exempt from the generic death.</summary>
    internal static void RegisterRidleyPowerBombReactions(EnemySharedReactions table) =>
        table.AddPowerBomb(new EnemyPowerBombReaction(enemy => enemy.EnemyDefinitionPointer is EnemyDefinitionId.Ridley or EnemyDefinitionId.RidleyCeres &&
                                                               EnemyCollision.PowerBombReaction(enemy) == EnemyInteractionCallback.RidleyPowerBomb)
        {
            ExemptFromGenericDeath = true,
        });
}
