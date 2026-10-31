using SuperMetroid.Core.Bosses.Artwork;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Enemies.Drawing;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Mother Brain's false death: the fade to gray, the tubes collapsing and falling, the room rows and the fade back.</summary>
[RoomLifetime]
internal sealed class MotherBrainFakeDeath
{
    /// <summary>The enemy pass's results, which this behavior writes.</summary>
    private readonly BossFrameResultBuilder _frameResults;
    /// <summary>Routes Mother Brain's AI and projectile handlers through the room dispatch tables.</summary>
    private readonly IEnemyDispatch _dispatch;
    /// <summary>Provides room services, tiles, palette state, and effects used by the encounter.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Owns the bombs, glass, beams, turrets, and other encounter projectiles.</summary>
    private readonly EnemyProjectileEngine _projectiles;
    /// <summary>Publishes screen shake during the encounter's collapse and attacks.</summary>
    private readonly RoomEarthquake _earthquake;
    /// <summary>Draws encounter artwork and applies installed palette data.</summary>
    private readonly EnemyPresentation _presentation;
    /// <summary>Holds the shared body/head state for the current Mother Brain room.</summary>
    private readonly MotherBrainEncounter _motherBrainEncounter;

    /// <summary>Mother Brain's state lookups, body and brain program setters, timers and PLM requests.</summary>
    private readonly MotherBrainEncounterAccess _access;
    /// <summary>The session's installed boss artwork.</summary>
    private readonly BossArtworkBinding _bossArtwork;


    /// <summary>Builds the component from the collaborators it uses.</summary>
    internal MotherBrainFakeDeath(MotherBrainFakeDeathDependencies dependencies)
    {
        _bossArtwork = dependencies.BossArtwork;
        _frameResults = dependencies.FrameResults;
        _dispatch = dependencies.Dispatch;
        _room = dependencies.Room;
        _projectiles = dependencies.Projectiles;
        _earthquake = dependencies.Earthquake;
        _presentation = dependencies.Presentation;
        _motherBrainEncounter = dependencies.MotherBrainEncounter;

        _access = dependencies.Access;
    }

    /// <summary>
    /// $A9:8B5D/8B67/8B71: collision radii and landing floor selected by piece role. The two
    /// outer and two inner pieces share their values; the main tube has its own floor.
    /// </summary>
    internal static MotherBrainFallingTubeDefinition MotherBrainFallingTubeBody(int tubeIndex) =>
        tubeIndex switch
        {
            0 or 1 => new(new(16, 32), 248),
            2 or 3 => new(new(8, 24), 240),
            4 => new(new(16, 32), 246),
            _ => throw new InvalidDataException($"Mother Brain falling tube index {tubeIndex} is outside its five pieces."),
        };

    /// <summary>Steps Mother Brain's fake-death gray fade and advances when its palette cycle completes.</summary>
    internal void RunMotherBrainFadeToGray(MotherBrainEnemyState state)
    {
        CountdownStep timer = state.FunctionTimer.Decrement();
        state.FunctionTimer = timer.Remaining;
        if (timer.IsNegative)
        {
            state.FunctionTimer = WrappingCountdown.Start(8);
            int step = state.GrayFadeIndex++;
            if (step == MotherBrainFakeDeathPaletteDefinitions.FrameCount)
            {
                state.Function = MotherBrainBodyFunction.FakeDeathDescentCollapseTubes;
            }
            else
            {
                // Fake-death fade replaces colors 145..147 only. Copying a convenient
                // whole palette would visibly alter unrelated room art and is not what
                // `$AD:ED5A` does.
                if (_bossArtwork.MotherBrainRainbowColors is { } colors)
                    colors.ApplyFakeDeathToGrey(_presentation.Colours, step);
                else throw new InvalidOperationException(
                    "Mother Brain fake-death fade requires installed colors.");
            }
        }

        // `$A9:88CF` is an unconditional tail-call. Tube sequencing and dust therefore
        // begin during the grey fade rather than waiting for the eighth palette entry.
        RunMotherBrainTubeCollapse(state);
        RunMotherBrainFakeDeathExplosion(state);
    }

    /// <summary>Stops the palette effect and restores the encounter's steady room colors.</summary>
    internal void StopMotherBrainRoomPalette(MotherBrainEnemyState state)
    {
        state.RoomFlashFrame = null;
        state.RoomFlashTimer = 0;
        (_bossArtwork.MotherBrainRoomColors ?? throw new InvalidOperationException(
            "Mother Brain room finale requires installed colors."))
            .ApplyFinal(_presentation.Colours);
    }

    /// <summary>Emits timed explosions while the first-phase body appears to die.</summary>
    internal void RunMotherBrainFakeDeathExplosion(MotherBrainEnemyState state)
    {
        CountdownStep timer = state.FakeDeathExplosionTimer.Decrement();
        state.FakeDeathExplosionTimer = timer.Remaining;
        if (timer.IsNonNegative)
            return;

        state.FakeDeathExplosionTimer = WrappingCountdown.Start(8);
        // The index steps backward through the eight positions, from the first back to the last.
        state.FakeDeathExplosionIndex = (state.FakeDeathExplosionIndex + MotherBrainBehavior.MotherBrainFakeDeathExplosionPositions.Length - 1) %
            MotherBrainBehavior.MotherBrainFakeDeathExplosionPositions.Length;
        Position16<RoomPixels> explosion = MotherBrainBehavior.MotherBrainFakeDeathExplosionPositions[state.FakeDeathExplosionIndex];

        // `$A9:8904` samples the existing RNG word and never advances it. A host random
        // call here would desynchronize every later Rinka and phase-two attack decision.
        int sampledRandom = _room.Random.Read();
        MiscDustAnimation animation = sampledRandom < 16384 ? MiscDustAnimation.Smoke : MiscDustAnimation.SmallExplosion;
        _projectiles.SpawnRoomGraphicsDustExplosion(explosion, animation);
        state.LastSoundEffect = Library2Sound.SmallExplosion;
    }

    /// <summary>Schedules glass and tube collapse phases with their authored waits.</summary>
    /// <remarks>Every spawn after the first ($A9:8983-$8ACB) begins with DEC timer : BPL return, so each waits out the
    /// $20 (or $02) its preceding clear step set.</remarks>
    internal void RunMotherBrainTubeCollapse(MotherBrainEnemyState state)
    {
        switch (state.TubeCollapseFunction)
        {
            case MotherBrainTubeCollapseFunction.WaitForFourFreeProjectileSlots:
                if (_projectiles.EnemyProjectiles.Count(projectile => !projectile.IsActive) < 4)
                    return;
                SpawnMotherBrainFallingTube(MotherBrainTubePiece.BottomLeft);
                state.TubeCollapseFunction = MotherBrainTubeCollapseFunction.ClearBottomLeftTube;
                return;

            case MotherBrainTubeCollapseFunction.ClearBottomLeftTube:
                RequestMotherBrainTubePlm(state, new(5, 9),
                    PlmHeaderId.ClearMotherBrainBottomLeftTube,
                    MotherBrainTubeCollapseFunction.SpawnTopRightTube, 32);
                return;
            case MotherBrainTubeCollapseFunction.SpawnTopRightTube:
                if (!DecrementMotherBrainTubeTimerPastZero(state))
                    return;
                SpawnMotherBrainTopTube(RoomEnemyProjectileKind.MotherBrainTopRightTube, new(152, 47));
                state.TubeCollapseFunction = MotherBrainTubeCollapseFunction.ClearCeilingColumn9;
                return;
            case MotherBrainTubeCollapseFunction.ClearCeilingColumn9:
                RequestMotherBrainTubePlm(state, new(9, 2),
                    PlmHeaderId.ClearMotherBrainCeilingBlock,
                    MotherBrainTubeCollapseFunction.SpawnTopLeftTube, 32);
                return;
            case MotherBrainTubeCollapseFunction.SpawnTopLeftTube:
                if (!DecrementMotherBrainTubeTimerPastZero(state))
                    return;
                SpawnMotherBrainTopTube(RoomEnemyProjectileKind.MotherBrainTopLeftTube, new(104, 47));
                state.TubeCollapseFunction = MotherBrainTubeCollapseFunction.ClearCeilingColumn6;
                return;
            case MotherBrainTubeCollapseFunction.ClearCeilingColumn6:
                RequestMotherBrainTubePlm(state, new(6, 2),
                    PlmHeaderId.ClearMotherBrainCeilingBlock,
                    MotherBrainTubeCollapseFunction.SpawnBottomRightTube, 32);
                return;
            case MotherBrainTubeCollapseFunction.SpawnBottomRightTube:
                if (!DecrementMotherBrainTubeTimerPastZero(state))
                    return;
                SpawnMotherBrainFallingTube(MotherBrainTubePiece.BottomRight);
                state.TubeCollapseFunction = MotherBrainTubeCollapseFunction.ClearBottomRightTube;
                return;
            case MotherBrainTubeCollapseFunction.ClearBottomRightTube:
                RequestMotherBrainTubePlm(state, new(10, 9),
                    PlmHeaderId.ClearMotherBrainBottomRightTube,
                    MotherBrainTubeCollapseFunction.SpawnBottomMiddleLeftTube, 32);
                return;
            case MotherBrainTubeCollapseFunction.SpawnBottomMiddleLeftTube:
                if (!DecrementMotherBrainTubeTimerPastZero(state))
                    return;
                SpawnMotherBrainFallingTube(MotherBrainTubePiece.BottomMiddleLeft);
                state.TubeCollapseFunction = MotherBrainTubeCollapseFunction.ClearBottomMiddleLeftTube;
                return;
            case MotherBrainTubeCollapseFunction.ClearBottomMiddleLeftTube:
                RequestMotherBrainTubePlm(state, new(6, 10),
                    PlmHeaderId.ClearMotherBrainBottomMiddleSideTube,
                    MotherBrainTubeCollapseFunction.SpawnTopMiddleLeftTube, 32);
                return;
            case MotherBrainTubeCollapseFunction.SpawnTopMiddleLeftTube:
                if (!DecrementMotherBrainTubeTimerPastZero(state))
                    return;
                SpawnMotherBrainTopTube(RoomEnemyProjectileKind.MotherBrainTopMiddleLeftTube, new(120, 59));
                state.TubeCollapseFunction = MotherBrainTubeCollapseFunction.ClearCeilingColumn7;
                return;
            case MotherBrainTubeCollapseFunction.ClearCeilingColumn7:
                RequestMotherBrainTubePlm(state, new(7, 2),
                    PlmHeaderId.ClearMotherBrainCeilingTube,
                    MotherBrainTubeCollapseFunction.SpawnTopMiddleRightTube, 32);
                return;
            case MotherBrainTubeCollapseFunction.SpawnTopMiddleRightTube:
                if (!DecrementMotherBrainTubeTimerPastZero(state))
                    return;
                SpawnMotherBrainTopTube(RoomEnemyProjectileKind.MotherBrainTopMiddleRightTube, new(136, 59));
                state.TubeCollapseFunction = MotherBrainTubeCollapseFunction.ClearCeilingColumn8;
                return;
            case MotherBrainTubeCollapseFunction.ClearCeilingColumn8:
                RequestMotherBrainTubePlm(state, new(8, 2),
                    PlmHeaderId.ClearMotherBrainCeilingTube,
                    MotherBrainTubeCollapseFunction.SpawnBottomMiddleRightTube, 32);
                return;
            case MotherBrainTubeCollapseFunction.SpawnBottomMiddleRightTube:
                if (!DecrementMotherBrainTubeTimerPastZero(state))
                    return;
                SpawnMotherBrainFallingTube(MotherBrainTubePiece.BottomMiddleRight);
                state.TubeCollapseFunction = MotherBrainTubeCollapseFunction.ClearBottomMiddleRightTube;
                return;
            case MotherBrainTubeCollapseFunction.ClearBottomMiddleRightTube:
                RequestMotherBrainTubePlm(state, new(9, 10),
                    PlmHeaderId.ClearMotherBrainBottomMiddleSideTube,
                    MotherBrainTubeCollapseFunction.SpawnMainTube, 2);
                return;
            case MotherBrainTubeCollapseFunction.SpawnMainTube:
                if (!DecrementMotherBrainTubeTimerPastZero(state))
                    return;
                SpawnMotherBrainFallingTube(MotherBrainTubePiece.Main);
                state.TubeCollapseFunction = MotherBrainTubeCollapseFunction.ClearBottomMiddleTubes;
                return;
            case MotherBrainTubeCollapseFunction.ClearBottomMiddleTubes:
                RequestMotherBrainTubePlm(state, new(7, 7),
                    PlmHeaderId.ClearMotherBrainBottomMiddleTubes,
                    MotherBrainTubeCollapseFunction.Finished, 0);
                return;
            case MotherBrainTubeCollapseFunction.Finished:
                return;
            default:
                throw new InvalidDataException(
                    $"Mother Brain tube function {state.TubeCollapseFunction} is not translated.");
        }
    }

    /// <summary>Implements the tube phase's wraparound countdown transition.</summary>
    private static bool DecrementMotherBrainTubeTimerPastZero(MotherBrainEnemyState state)
    {
        CountdownStep timer = state.TubeCollapseTimer.Decrement();
        state.TubeCollapseTimer = timer.Remaining;
        return timer.IsNegative;
    }

    /// <summary>Queues the room tile change at the tube's native location.</summary>
    private void RequestMotherBrainTubePlm(
        MotherBrainEnemyState state,
        Position16<RoomBlocks> block,
        PlmHeaderId header,
        MotherBrainTubeCollapseFunction next,
        int timer)
    {
        _access.RequestMotherBrainPlm(block, header);
        state.TubeCollapseFunction = next;
        state.TubeCollapseTimer = WrappingCountdown.Start(timer);
    }

    /// <summary>Spawns the top tube projectile for the scripted collapse.</summary>
    internal void SpawnMotherBrainTopTube(
        RoomEnemyProjectileKind kind,
        Position16<RoomPixels> position)
    {
        RoomEnemyProjectileSlot? projectile = _projectiles.AllocateEnemyProjectile();
        if (projectile is null)
            return;

        MotherBrainProjectileStarts.Initialize(projectile, kind, graphics: SpriteGraphics.CharacterZeroIn(new EncounterSpriteColouring(MotherBrainColourDefinitions.TubeProjectiles)));
        projectile.Position = projectile.Position.WithWhole(position);
        projectile.Velocity = new(new SignedEightEight(0), new SignedEightEight(0));
        projectile.State = new MotherBrainTopTubeState();
    }

    /// <summary>Creates the falling tube enemy of <paramref name="piece"/>.</summary>
    internal void SpawnMotherBrainFallingTube(MotherBrainTubePiece piece)
    {
        // The bound source supplies the fixed cutscene placement; retail rooms resolve it without touching bank $A9.
        RoomEnemyPopulationRecord population = _room.LoadedDefinitions.ReadFallingTubePopulation(piece);
        if (population.DefinitionId != EnemyDefinitionId.MotherBrainTubes)
        {
            throw new InvalidDataException(
                $"Mother Brain tube record for {piece} names enemy " +
                $"{population.DefinitionId}, not the falling tube.");
        }

        // SpawnEnemy silently fails when all 32 native records are occupied.
        if (_dispatch.SpawnEnemy(population) is null)
            return;
        if (_motherBrainEncounter.MotherBrain is not null)
            _motherBrainEncounter.MotherBrain.SpawnedFallingTubeCount++;
    }

    /// <summary>Ports falling-tube initialization $A9:8B35.</summary>
    internal void InitializeMotherBrainFallingTube(RoomEnemySlot tube)
    {
        MotherBrainTubeSpawnSettings settings = tube.Spawn.Population.SpawnSettingsOf<MotherBrainTubeSpawnSettings>();
        MotherBrainTubePiece piece = settings.Piece;
        MotherBrainFallingTubeDefinition tubeBody = MotherBrainFallingTubeBody(MotherBrainFallingTubePopulationDefinitions.Pieces.PositionOf(piece));
        tube.Radius = tubeBody.Radius;
        var state = new MotherBrainFallingTubeState
        {
            Cursor = AnimationProgramCursor<MotherBrainTubePiece>.Start(piece),
            Function = piece == MotherBrainTubePiece.Main
                ? MotherBrainTubeFunction.MainTube_WaitingToFall
                : MotherBrainTubeFunction.NonMainTube,
            FloorY = tubeBody.Floor,
            FallDelay = WrappingCountdown.Start(settings.FallDelay),
        };
        tube.SetSpeciesState(state, state.ClearWithRecord);
    }

    /// <summary>Ports the $A9:8B85 function-pointer dispatcher for physical tube enemies.</summary>
    internal void RunMotherBrainFallingTubeMain(RoomEnemySlot tube)
    {
        MotherBrainFallingTubeState tubeState = tube.RequireSpeciesState<MotherBrainFallingTubeState>();
        switch (tubeState.Function ?? throw new InvalidDataException("A falling tube has no function."))
        {
            case MotherBrainTubeFunction.NonMainTube:
                FallMotherBrainSideTube(tube);
                return;
            case MotherBrainTubeFunction.MainTube_WaitingToFall:
                // The main tube's fall delay expires when it passes zero.
                CountdownStep fallDelay = tubeState.FallDelay.Decrement();
                tubeState.FallDelay = fallDelay.Remaining;
                if (fallDelay.IsNonNegative)
                    return;
                tubeState.Function = MotherBrainTubeFunction.MainTube_Falling;
                FallMotherBrainMainTube(tube);
                return;
            case MotherBrainTubeFunction.MainTube_Falling:
                FallMotherBrainMainTube(tube);
                return;
            default:
                throw new InvalidDataException(
                    $"Mother Brain falling tube function {tubeState.Function} is not translated.");
        }
    }

    /// <summary>Drops a side tube until it reaches the floor and explodes, trailing smoke as it falls.</summary>
    private void FallMotherBrainSideTube(RoomEnemySlot tube)
    {
        MotherBrainFallingTubeState tubeState = AccelerateMotherBrainTube(tube);
        if (tube.Position.Y.Whole >= tubeState.FloorY)
            ExplodeMotherBrainTube(tube);
        else
            SpawnMotherBrainTubeSmoke(tube);
    }

    /// <summary>Accelerates a falling tube by six sub-pixels per update and moves it down by its fall speed.</summary>
    private static MotherBrainFallingTubeState AccelerateMotherBrainTube(RoomEnemySlot tube)
    {
        MotherBrainFallingTubeState tubeState = tube.RequireSpeciesState<MotherBrainFallingTubeState>();
        tubeState.FallSpeed = new SignedEightEight(unchecked((ushort)(tubeState.FallSpeed.RawValue + 6)));
        tube.Position = tube.Position.AdvanceY(tubeState.FallSpeed.ToSixteenSixteenDelta());
        return tubeState;
    }

    /// <summary>Drops the main tube, carrying Mother Brain's head with it and landing her body on the floor.</summary>
    private void FallMotherBrainMainTube(RoomEnemySlot tube)
    {
        AccelerateMotherBrainTube(tube);

        // Hiding the tube and moving the brain are independent native branches.
        // The brain follows the tube throughout its descent, including before hiding.
        if (tube.Position.Y.Whole >= 244)
            tube.Properties = tube.Properties.With(EnemyProperties.Invisible);
        MotherBrainEnemyState state = _motherBrainEncounter.MotherBrain ??
            throw new InvalidOperationException("A falling main tube has no Mother Brain encounter state.");
        state.Head.Position = state.Head.Position.WithWholeY(unchecked((ushort)(tube.Position.Y.Whole - 56)));
        if (state.Head.Position.Y.Whole < 196)
        {
            SpawnMotherBrainTubeSmoke(tube);
            return;
        }

        StopMotherBrainRoomPalette(state);
        _earthquake.EarthquakeType = 25;
        _earthquake.EarthquakeTimer = WrappingCountdown.Start(32);
        // `$A9:8C0C/$8C0F` zero the channel words of HDMA objects 0-3, deleting the room's
        // liquid scroll objects (and any X-ray object) for the rest of the encounter.
        _frameResults.RemoveLiquidEffects = true;
        state.Head.Position = state.Head.Position.WithWholeY(196);
        state.Body.Position = state.Body.Position.WithWhole(new(59, 279));

        // `$A9:903F` is part of the main-tube landing path, not phase-two setup. These
        // lengths and starting angles must exist before `$87A2` gets installed several
        // frames later or the first visible neck frame collapses every joint onto zero.
        MotherBrainNeckJointDefinitions.InitialDistances.CopyTo(state.NeckJointDistances);
        state.LowerNeckAngle = RotationAngle.FromRaw(18432);
        state.UpperNeckAngle = RotationAngle.FromRaw(20480);
        state.NeckAngleDelta = 256;
        state.Function = MotherBrainBodyFunction.FakeDeathAscentDrawRows2And3;
        ExplodeMotherBrainTube(tube);
    }

    /// <summary>Emits smoke at the falling tube's current position.</summary>
    private void SpawnMotherBrainTubeSmoke(RoomEnemySlot tube)
    {
        // The smoke cadence counts down and emits when it passes zero.
        MotherBrainFallingTubeState tubeState = tube.RequireSpeciesState<MotherBrainFallingTubeState>();
        CountdownStep smokeDelay = tubeState.SmokeDelay.Decrement();
        tubeState.SmokeDelay = smokeDelay.Remaining;
        if (smokeDelay.IsNonNegative)
            return;

        tubeState.SmokeDelay = WrappingCountdown.Start(8);
        tubeState.SmokePositionIndex = (tubeState.SmokePositionIndex + 1) & 3;
        _projectiles.SpawnRoomGraphicsDustExplosion(
            new(unchecked((ushort)(tube.Position.X.Whole + MotherBrainBehavior.MotherBrainFallingTubeSmokeXOffsets[tubeState.SmokePositionIndex])), 208),
            MiscDustAnimation.SmallDustCloud);
    }

    /// <summary>Finishes a falling tube with its explosion and tile update.</summary>
    private void ExplodeMotherBrainTube(RoomEnemySlot tube)
    {
        Position16<RoomPixels> position = tube.Position.Whole;
        tube.Properties = tube.Properties.With(EnemyProperties.Deleted);
        _projectiles.SpawnRoomGraphicsDustExplosion(position, MiscDustAnimation.SmallExplosion);
        if (_motherBrainEncounter.MotherBrain is not null)
            _motherBrainEncounter.MotherBrain.LastSoundEffect = Library2Sound.SmallExplosion;
    }

    /// <summary>Queues the BG2 room rows revealed by the tube collapse.</summary>
    internal void RequestMotherBrainRoomRows(
        MotherBrainEnemyState state,
        PlmHeaderId firstHeader,
        PlmHeaderId secondHeader,
        MotherBrainBodyFunction next)
    {
        int firstRow = state.Function switch
        {
            MotherBrainBodyFunction.FakeDeathAscentDrawRows2And3 => 2,
            MotherBrainBodyFunction.FakeDeathAscentDrawRows4And5 => 4,
            MotherBrainBodyFunction.FakeDeathAscentDrawRows6And7 => 6,
            MotherBrainBodyFunction.FakeDeathAscentDrawRows8And9 => 8,
            MotherBrainBodyFunction.FakeDeathAscentDrawRowsAAndB => 10,
            MotherBrainBodyFunction.FakeDeathAscentDrawRowsCAndD => 12,
            MotherBrainBodyFunction.FirstPhase or
                MotherBrainBodyFunction.FakeDeathDescentInitialPause or
                MotherBrainBodyFunction.FakeDeathDescentPauseBeforeLock or
                MotherBrainBodyFunction.FakeDeathDescentPauseBeforeMusic or
                MotherBrainBodyFunction.FakeDeathDescentPauseBeforeUnlock or
                MotherBrainBodyFunction.FakeDeathDescentPauseBeforeFlash or
                MotherBrainBodyFunction.FakeDeathDescentFadeToGray or
                MotherBrainBodyFunction.FakeDeathDescentCollapseTubes or
                MotherBrainBodyFunction.FakeDeathAscentSetupPhase2Graphics or
                MotherBrainBodyFunction.FakeDeathAscentSetupPhase2Brain or
                MotherBrainBodyFunction.FakeDeathAscentPauseForSuspense or
                MotherBrainBodyFunction.FakeDeathAscentPrepareForRising or
                MotherBrainBodyFunction.FakeDeathAscentLoadLegTiles or
                MotherBrainBodyFunction.FakeDeathAscentContinuePausing or
                MotherBrainBodyFunction.FakeDeathAscentStartMusicAndEarthquake or
                MotherBrainBodyFunction.FakeDeathAscentRaiseMotherBrain or
                MotherBrainBodyFunction.FakeDeathAscentWaitUntilUncrouched or
                MotherBrainBodyFunction.FakeDeathAscentTransitionFromGray or
                MotherBrainBodyFunction.SecondPhaseStretchingShakeHead or
                MotherBrainBodyFunction.SecondPhaseStretchingBringHeadUp or
                MotherBrainBodyFunction.SecondPhaseStretchingFinish or
                MotherBrainBodyFunction.SecondPhaseThinking or
                MotherBrainBodyFunction.SecondPhaseTryAttack or
                MotherBrainBodyFunction.SecondPhaseBombDecideWalking or
                MotherBrainBodyFunction.SecondPhaseBombWalkingBackwards or
                MotherBrainBodyFunction.SecondPhaseBombCrouch or
                MotherBrainBodyFunction.SecondPhaseBombFired or
                MotherBrainBodyFunction.SecondPhaseBombStandUp or
                MotherBrainBodyFunction.SecondPhaseLaserPositionHeadQuickly or
                MotherBrainBodyFunction.SecondPhaseLaserPositionHeadSlowlyAndFire or
                MotherBrainBodyFunction.SecondPhaseLaserFinishAttack or
                MotherBrainBodyFunction.SecondPhaseHandBeam or
                MotherBrainBodyFunction.SecondPhaseRainbowExtendNeck or
                MotherBrainBodyFunction.SecondPhaseRainbowStartCharging or
                MotherBrainBodyFunction.SecondPhaseRainbowRetractNeck or
                MotherBrainBodyFunction.SecondPhaseRainbowWaitForCharge or
                MotherBrainBodyFunction.SecondPhaseRainbowExtendNeckDown or
                MotherBrainBodyFunction.SecondPhaseRainbowStartFiring or
                MotherBrainBodyFunction.SecondPhaseRainbowMoveSamusTowardWall or
                MotherBrainBodyFunction.SecondPhaseRainbowOneFrameDelay or
                MotherBrainBodyFunction.SecondPhaseRainbowStartDrainingSamus or
                MotherBrainBodyFunction.SecondPhaseRainbowDrainingSamus or
                MotherBrainBodyFunction.SecondPhaseRainbowFinishFiring or
                MotherBrainBodyFunction.SecondPhaseRainbowLetSamusFall or
                MotherBrainBodyFunction.SecondPhaseRainbowWaitForSamusToLand or
                MotherBrainBodyFunction.SecondPhaseRainbowLowerHead or
                MotherBrainBodyFunction.SecondPhaseRainbowDecideNextAction or
                MotherBrainBodyFunction.SecondPhaseFinishSamusOff or
                MotherBrainBodyFunction.SecondPhaseFinishSamusOffStandUp or
                MotherBrainBodyFunction.SecondPhaseFinishSamusOffAdmire or
                MotherBrainBodyFunction.SecondPhaseFinishSamusOffChargeFinalBeam or
                MotherBrainBodyFunction.SecondPhaseFinishSamusOffLoadBabyTiles or
                MotherBrainBodyFunction.SecondPhaseFinishSamusOffFireFinalBeam or
                MotherBrainBodyFunction.SecondPhaseFinalRainbowBeamHolding or
                MotherBrainBodyFunction.SecondPhaseDrainedByBabyTakenAback or
                MotherBrainBodyFunction.SecondPhaseDrainedByBabyRegainBalance or
                MotherBrainBodyFunction.SecondPhaseDrainedByBabyFiringRainbowBeam or
                MotherBrainBodyFunction.SecondPhaseDrainedByBabyRainbowBeamRunOut or
                MotherBrainBodyFunction.SecondPhaseDrainedByBabyMoveToBackOfRoom or
                MotherBrainBodyFunction.SecondPhaseDrainedByBabyGoIntoLowPowerMode or
                MotherBrainBodyFunction.SecondPhaseDrainedByBabyPrepareTransitionToGrey or
                MotherBrainBodyFunction.SecondPhaseDrainedByBabyTransitionToGrey or
                MotherBrainBodyFunction.SecondPhaseReviveInanimateGrey or
                MotherBrainBodyFunction.SecondPhaseReviveShowSignsOfLife or
                MotherBrainBodyFunction.SecondPhaseReviveTransitionFromGrey or
                MotherBrainBodyFunction.SecondPhaseReviveWakeUp or
                MotherBrainBodyFunction.SecondPhaseReviveWakeUpStretch or
                MotherBrainBodyFunction.SecondPhaseReviveWalkUpToBaby or
                MotherBrainBodyFunction.SecondPhaseRevivePrepareNeckForBabyDeath or
                MotherBrainBodyFunction.SecondPhaseReviveFinishPreparingForBabyDeath or
                MotherBrainBodyFunction.SecondPhaseMurderBabyAttack or
                MotherBrainBodyFunction.SecondPhaseMurderBabyAttackCooldown or
                MotherBrainBodyFunction.SecondPhasePrepareForFinalBabyAttack or
                MotherBrainBodyFunction.SecondPhaseExecuteFinalBabyAttack or
                MotherBrainBodyFunction.SecondPhaseFinalBabyAttackHolding or
                MotherBrainBodyFunction.ThirdPhaseRecoverMakeSomeDistance or
                MotherBrainBodyFunction.ThirdPhaseRecoverSetupForFighting or
                MotherBrainBodyFunction.ThirdPhaseFightingMain or
                MotherBrainBodyFunction.ThirdPhaseFightingAttackCooldown or
                MotherBrainBodyFunction.ThirdPhaseDeathMoveToBackOfRoom or
                MotherBrainBodyFunction.ThirdPhaseDeathIdleWhilstExploding or
                MotherBrainBodyFunction.ThirdPhaseDeathStumbleToMiddleOfRoom or
                MotherBrainBodyFunction.ThirdPhaseDeathDisableBrainEffects or
                MotherBrainBodyFunction.ThirdPhaseDeathSetupBodyFadeOut or
                MotherBrainBodyFunction.ThirdPhaseDeathFadeOutBody or
                MotherBrainBodyFunction.ThirdPhaseDeathFinalFewExplosions or
                MotherBrainBodyFunction.ThirdPhaseDeathRealizeDecapitation or
                MotherBrainBodyFunction.ThirdPhaseDeathBrainFallsToGround or
                MotherBrainBodyFunction.ThirdPhaseDeathLoadCorpseTiles or
                MotherBrainBodyFunction.ThirdPhaseDeathSetupFadeToGrey or
                MotherBrainBodyFunction.ThirdPhaseDeathFadeToGrey or
                MotherBrainBodyFunction.ThirdPhaseDeathCorpseTipsOver or
                MotherBrainBodyFunction.ThirdPhaseDeathCorpseRotsAway or
                MotherBrainBodyFunction.ThirdPhaseDeath20FrameDelay or
                MotherBrainBodyFunction.ThirdPhaseDeathLoadEscapeTimerTiles or
                MotherBrainBodyFunction.ThirdPhaseDeathStartEscape or
                MotherBrainBodyFunction.ThirdPhaseDeathSpawnTimeBombSetSubtitle or
                MotherBrainBodyFunction.ThirdPhaseDeathTypeOutZebesEscapeText or
                MotherBrainBodyFunction.ThirdPhaseDeathDoorExplodingStartTimer or
                MotherBrainBodyFunction.ThirdPhaseDeathBlowUpEscapeDoor or
                MotherBrainBodyFunction.ThirdPhaseDeathKeepEarthquakeGoing => throw new InvalidOperationException("Mother Brain row draw called outside fake ascent."),
            _ => throw new InvalidOperationException("Mother Brain row draw called outside fake ascent."),
        };
        _access.RequestMotherBrainPlm(new(2, (ushort)firstRow), firstHeader);
        _access.RequestMotherBrainPlm(new(2, (ushort)(firstRow + 1)), secondHeader);
        state.Function = next;
    }

    /// <summary>Restores the combat palette after the gray fake-death phase.</summary>
    internal void TransitionMotherBrainFromGray(MotherBrainEnemyState state)
    {
        if (!MotherBrainEncounterAccess.DecrementMotherBrainTimerPastZero(state))
            return;

        state.FunctionTimer = WrappingCountdown.Start(4);
        int paletteStep = state.GrayTransitionCounter++;
        if (paletteStep < MotherBrainFakeDeathPaletteDefinitions.FrameCount)
        {
            // Fake-death restoration changes only brain sprite colors one through three.
            if (_bossArtwork.MotherBrainRainbowColors is { } colors)
                colors.ApplyFakeDeathFromGrey(_presentation.Colours, paletteStep);
            else throw new InvalidOperationException(
                "Mother Brain revival requires installed colors.");
            return;
        }

        state.BrainPaletteHandlingEnabled = true;
        state.Form = MotherBrainForm.SecondPhase;
        state.DroolGenerationEnabled = true;
        state.LowerNeckMovementIndex = MotherBrainNeckMovement.Lower;
        state.UpperNeckMovementIndex = MotherBrainNeckMovement.Lower;
        state.NeckAngleDelta = 1280;
        state.Function = MotherBrainBodyFunction.SecondPhaseStretchingShakeHead;
        state.FunctionTimer = WrappingCountdown.Start(23);
    }
}
