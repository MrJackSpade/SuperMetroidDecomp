using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Animals.Artwork;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Enemies.Animals;

/// <summary>The Dachora species owner: its per-room state, behaviour and the interactions it registers.</summary>
[RoomLifetime]
internal sealed class DachoraBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.Dachora];

    /// <inheritdoc/>
    public IEnemyEntity Create(RoomEnemySlot slot, EnemyCommonParticipation common) => new DachoraEntity(this, slot, common);

    /// <summary>Probes floors, walls, and ceiling during the run and shinespark arc.</summary>
    private readonly EnemyTerrain _terrain;
    /// <summary>The room's enemy places, where the Dachora finds its four echoes when it is placed.</summary>
    private readonly EnemySlotTable _slotTable;
    /// <summary>The four speed-boost echoes each Dachora found when it was placed, which its room lists right after it.</summary>
    private readonly EnemyPartLinks<RoomEnemySlot[]> _echoes = new();

    /// <summary>Receives the species' sound requests, each queued as the species makes it.</summary>
    private readonly EnemyAudioQueue _audio;

    /// <summary>
    /// Binds the terrain and echo slots used by Dachora's AI callbacks; its glows are shades its draws name (#1512), so
    /// it needs no colour artwork.
    /// </summary>
    internal DachoraBehavior(EnemyTerrain terrain, EnemySlotTable slotTable, EnemyAudioQueue audio)
    {
        _audio = audio;
        _terrain = terrain;
        _slotTable = slotTable;
    }

    /// <summary>Horizontal proximity threshold for activating the bird.</summary>
    private const int DachoraActivationXDistance = 96;
    /// <summary>Vertical proximity threshold for activating the bird.</summary>
    private const int DachoraActivationYDistance = 64;
    /// <summary>Warning interval before Dachora starts its leftward run.</summary>
    private const int DachoraBlinkDuration = 120;
    /// <summary>Stored-shine countdown before Dachora launches.</summary>
    private const int DachoraChargeDuration = 60;
    /// <summary>Accepted-frame interval between echo position captures.</summary>
    private const int DachoraEchoPositionInterval = 1;
    /// <summary>Accepted-frame lifetime of each Dachora shine echo.</summary>
    private const int DachoraEchoLifetime = 8;
    /// <summary>The bird's run speed cap: eight pixels per update.</summary>
    private static readonly SignedSixteenSixteen DachoraMaximumSpeed = SignedSixteenSixteen.FromParts(8, 0);
    /// <summary>Dachora's running acceleration: a sixteenth of a pixel per update, every update.</summary>
    private static readonly SignedSixteenSixteen DachoraAcceleration = SignedSixteenSixteen.FromParts(0, 4096);
    /// <summary>The shinespark's upward speed cap in whole pixels per update; the fraction is kept.</summary>
    private const int DachoraShinesparkMaximumWholeSpeed = 15;
    /// <summary>The fall's speed cap: ten pixels per update.</summary>
    private static readonly SignedSixteenSixteen DachoraMaximumFallSpeed = SignedSixteenSixteen.FromParts(10, 0);
    /// <summary>How many speed-boost echoes trail a Dachora.</summary>
    private const int DachoraEchoCount = 4;
    /// <summary>Scripted room X where Dachora reverses its leftward run.</summary>
    private const int DachoraLeftTurnX = 96;
    /// <summary>Scripted room X where Dachora begins its stored-shine demonstration.</summary>
    private const int DachoraShinesparkX = 1152;

    /// <summary>Enemy-library sound queued when Samus wakes the tutorial Dachora.</summary>
    private static readonly SoundEffectId DachoraActivationSound =
        EnemySoundEffectDefinitions.DachoraActivationSound;
    /// <summary>Enemy-library sound queued at the scripted wall turn.</summary>
    private static readonly SoundEffectId DachoraWallImpactSound =
        EnemySoundEffectDefinitions.DachoraWallImpactSound;
    /// <summary>Enemy-library sound queued while Dachora stores its shinespark.</summary>
    private static readonly SoundEffectId DachoraChargeSound =
        EnemySoundEffectDefinitions.DachoraChargeSound;
    /// <summary>Enemy-library sound accompanying Dachora's Speed Booster run.</summary>
    private static readonly SoundEffectId DachoraSpeedBoosterSound =
        EnemySoundEffectDefinitions.DachoraSpeedBoosterSound;
    /// <summary>Enemy-library sound queued when Dachora launches upward.</summary>
    private static readonly SoundEffectId DachoraLaunchSound =
        EnemySoundEffectDefinitions.DachoraLaunchSound;
    /// <summary>Enemy-library sound queued on Dachora's ceiling contact.</summary>
    private static readonly SoundEffectId DachoraCeilingImpactSound =
        EnemySoundEffectDefinitions.DachoraCeilingImpactSound;

    /// <summary>Initializes the Dachora.</summary>
    internal void InitializeDachora(RoomEnemySlot slot)
    {
        DachoraSpawnSettings settings = slot.SpawnSettings<DachoraSpawnSettings>();
        var state = new DachoraEnemyState
        {
            FacingRight = settings.Facing == HorizontalDirection.Right || settings.Role == DachoraRole.SpeedBoostEcho,
        };
        slot.SetSpeciesState(state);

        // The animation-script property both disables Samus collision and admits enemy animation scripts.
        // The population's tangibility property already excludes these actors from host interaction.
        slot.Processing = slot.Processing.With(EnemyProcessing.Animates);
        slot.Frame = EnemyFrameId.Empty;
        slot.AnimationClock.Restart();

        if (settings.Role == DachoraRole.SpeedBoostEcho)
        {
            // An echo faces the way the body does; each trail placement later gives it an eight-update life.
            state.Playback = new(DachoraAnimations.StartingWith(settings.Facing == HorizontalDirection.Right
                ? DachoraSegment.EchoRight
                : DachoraSegment.EchoLeft), AnimationPlace.StartOfAnimation);
            state.Function = DachoraAiFunction.Echo;
            return;
        }

        _echoes.Place(slot, _slotTable.ListedRunAfter(slot, DachoraEchoCount));
        state.Playback = new(DachoraAnimations.StartingWith(settings.Facing == HorizontalDirection.Right
            ? DachoraSegment.IdleRight
            : DachoraSegment.IdleLeft), AnimationPlace.StartOfAnimation);
        state.Function = DachoraAiFunction.WaitingForSamus;
    }

    /// <summary>Dispatches the Dachora's main AI through its indirect jump.</summary>
    private void RunDachoraMain(
        RoomEnemySlot slot,
        DachoraEnemyState state,
        SamusState? samus,
        RoomLevelData? level,
        int frameCycle)
    {
        // Echo AI needs only the frame counter's parity. Every body state needs both the active
        // Samus gravity values and authentic room collision, so fail loudly if a caller
        // attempts to run the family without the state that code reads globally.
        if (state.Function == DachoraAiFunction.Echo)
        {
            RunDachoraEcho(slot, state, frameCycle);
            return;
        }

        SamusState activeSamus = samus ?? throw new InvalidOperationException(
            "Dachora body AI requires the active Samus actor.");
        RoomLevelData activeLevel = level ?? throw new InvalidOperationException(
            "Dachora body AI requires active room collision data.");
        switch (state.Function)
        {
            case DachoraAiFunction.WaitingForSamus:
                RunDachoraWaiting(slot, state, activeSamus, activeLevel);
                break;
            case DachoraAiFunction.BlinkingBeforeRun:
                RunDachoraBlinking(slot, state);
                break;
            case DachoraAiFunction.RunningLeft:
                RunDachoraLeft(slot, state, activeLevel);
                break;
            case DachoraAiFunction.RunningRight:
                RunDachoraRight(slot, state, activeLevel);
                break;
            case DachoraAiFunction.ChargingShinespark:
                RunDachoraCharging(slot, state);
                break;
            case DachoraAiFunction.Shinesparking:
                RunDachoraShinespark(slot, state, activeSamus, activeLevel);
                break;
            case DachoraAiFunction.Falling:
                RunDachoraFalling(slot, state, activeSamus, activeLevel);
                break;
            case DachoraAiFunction.Echo:
            default:
                throw new InvalidDataException(
                    $"Dachora function {state.Function?.ToString() ?? "none"} is not translated.");
        }
    }

    /// <summary>Waits for Samus, including the one-pixel floor probe before proximity.</summary>
    private void RunDachoraWaiting(
        RoomEnemySlot slot,
        DachoraEnemyState state,
        SamusState samus,
        RoomLevelData level)
    {
        _terrain.MoveEnemyVertically(level, slot, SignedSixteenSixteen.FromWholePixels(1));
        if (!DachoraWithinAxis(slot.Position.Y.Whole, samus.Position.Y.Whole, DachoraActivationYDistance) ||
            !DachoraWithinAxis(slot.Position.X.Whole, samus.Position.X.Whole, DachoraActivationXDistance))
        {
            return;
        }

        StartDachoraAnimation(
            slot,
            state,
            state.FacingRight
                ? DachoraSegment.BlinkRight
                : DachoraSegment.BlinkLeft);
        state.Function = DachoraAiFunction.BlinkingBeforeRun;
        state.StartBlinking(DachoraBlinkDuration);
        _audio.QueueEnemySound(DachoraActivationSound, priority: SoundRequestPriority.Normal);
    }

    /// <summary>Runs the 120-frame warning countdown.</summary>
    private static void RunDachoraBlinking(RoomEnemySlot slot, DachoraEnemyState state)
    {
        if (--state.PhaseTimer != 0)
            return;

        StartDachoraAnimation(
            slot,
            state,
            state.FacingRight
                ? DachoraSegment.RunningRight
                : DachoraSegment.RunningLeft);
        state.Function = state.FacingRight
            ? DachoraAiFunction.RunningRight
            : DachoraAiFunction.RunningLeft;
        state.SetGlowCycle(frame: 0, ticks: 1);
    }

    /// <summary>Runs left. Leftward speed is the exact two's complement of the whole and fractional speed.</summary>
    private void RunDachoraLeft(
        RoomEnemySlot slot,
        DachoraEnemyState state,
        RoomLevelData level)
    {
        SignedSixteenSixteen displacement = -AccelerateDachora(slot, state, level);
        bool hitWall = _terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(
            level,
            slot,
            displacement);
        if (!hitWall)
            EnemyTerrain.AlignEnemyYWithNonSquareSlope(level, slot);

        if (!hitWall && CoordinateAxis.Difference(slot.Position.X.Whole, DachoraLeftTurnX) >= 0)
            return;

        StartDachoraAnimation(slot, state, DachoraSegment.RunningRight);
        state.Function = DachoraAiFunction.RunningRight;
        state.SetGlowCycle(frame: 0, ticks: 1);
        state.FacingRight = true;
        state.Speed = SignedSixteenSixteen.Zero;
        ShowDachoraColours(slot, DachoraColourPhase.Default, frame: 0);
    }

    /// <summary>Runs right, including the hard-coded tutorial launch coordinate.</summary>
    private void RunDachoraRight(
        RoomEnemySlot slot,
        DachoraEnemyState state,
        RoomLevelData level)
    {
        SignedSixteenSixteen displacement = AccelerateDachora(slot, state, level);
        if (_terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(level, slot, displacement))
        {
            _audio.QueueEnemySound(DachoraWallImpactSound, priority: SoundRequestPriority.Normal);
            StartDachoraAnimation(slot, state, DachoraSegment.RunningLeft);
            state.Function = DachoraAiFunction.RunningLeft;
            state.FacingRight = false;
            state.Speed = SignedSixteenSixteen.Zero;
            state.SetGlowCycle(frame: 0, ticks: 0);
            ShowDachoraColours(slot, DachoraColourPhase.Default, frame: 0);
            return;
        }

        EnemyTerrain.AlignEnemyYWithNonSquareSlope(level, slot);
        if (CoordinateAxis.Difference(slot.Position.X.Whole, DachoraShinesparkX) < 0)
            return;

        StartDachoraAnimation(slot, state, DachoraSegment.ChargeRight);
        state.Function = DachoraAiFunction.ChargingShinespark;
        state.StartCharging(DachoraChargeDuration);
        state.SetGlowCycle(frame: 0, ticks: 0);
        slot.Position = slot.Position.WithWrappedWholeY(slot.Position.Y.Whole + 8);
        _audio.QueueEnemySound(DachoraChargeSound, priority: SoundRequestPriority.Normal);
    }

    /// <summary>
    /// Accelerates the running Dachora. This intentionally retains the
    /// speed glow's cycle (DachoraGlowCycle): after frame three is reached, the game shows that brightest
    /// speed glow frame again every sixteen frames instead of wrapping to frame zero.
    /// </summary>
    internal SignedSixteenSixteen AccelerateDachora(
        RoomEnemySlot slot,
        DachoraEnemyState state,
        RoomLevelData level)
    {
        if (state.Speed.WholePixels >= DachoraMaximumSpeed.WholePixels)
        {
            if (state.GlowFrame == 0 && state.GlowTicks == 1)
                _audio.QueueEnemySound(DachoraSpeedBoosterSound, priority: SoundRequestPriority.Normal);

            if (state.TickGlowCycle())
            {
                int glowFrame = state.GlowFrame;
                if ((uint)glowFrame >= 4)
                {
                    throw new InvalidDataException(
                        $"Dachora speed glow frame {glowFrame} is not one of its four.");
                }
                ShowDachoraColours(slot, DachoraColourPhase.Speed, glowFrame);
                state.AdvanceSpeedGlow();
            }
        }

        // Presses the body down one pixel every running frame before changing speed.
        // The collision result is discarded; the probe keeps Dachora glued to room slopes.
        _terrain.MoveEnemyVertically(level, slot, SignedSixteenSixteen.FromWholePixels(1));

        // The game compares the whole and fraction parts separately, each by the sign of its difference
        // from the cap's; a fraction of half a pixel or more therefore also counts as below the cap.
        SignedSixteenSixteen speed = state.Speed;
        bool belowMaximum = speed.WholePixels < DachoraMaximumSpeed.WholePixels ||
            WrappingCounter.IsBehind(speed.Fraction, DachoraMaximumSpeed.Fraction);
        if (belowMaximum)
        {
            speed += DachoraAcceleration;
            state.Speed = speed;

            // The game's speed-up jump lands at the matching pose in the faster clone of the
            // current six-pose run loop: every run plays one six-pose phase, so the animation keeps its place and timing.
            if ((speed.WholePixels is 4 or 8) && speed.Fraction == 0)
                state.Playback = state.Playback with { Animation = DachoraAnimations.StartingWith(FasterDachoraRun(Playing(state.Playback))) };
        }
        else
        {
            state.Speed = DachoraMaximumSpeed;
        }

        return state.Speed;
    }

    /// <summary>Runs the stored-shine countdown and echo activation.</summary>
    private void RunDachoraCharging(RoomEnemySlot slot, DachoraEnemyState state)
    {
        StepDachoraShineGlow(slot, state);
        if (--state.PhaseTimer != 0)
            return;

        // The charge animation is asleep at its launch wait. Stepping past the sleep enters the same
        // right-facing echo animation used by the four trails, exactly as the game does on launch.
        state.Playback = new(DachoraAnimations.StartingWith(DachoraAnimations.AfterSleep(Playing(state.Playback))), AnimationPlace.StartOfAnimation);
        slot.AnimationClock.StepOnNextUpdate();
        state.Function = DachoraAiFunction.Shinesparking;

        RoomEnemySlot[] echoes = RequireDachoraEchoSlots(slot);
        echoes[0].RequireSpeciesState<DachoraEnemyState>().EchoPositionTimer = 0;
        foreach (RoomEnemySlot echo in echoes)
            echo.RequireSpeciesState<DachoraEnemyState>().EchoLifetime = 0;
        state.VerticalAcceleration = SignedSixteenSixteen.Zero;
        slot.Position = slot.Position.WithWrappedWholeY(slot.Position.Y.Whole - 8);
        _audio.QueueEnemySound(DachoraLaunchSound, priority: SoundRequestPriority.Normal);

        DachoraSegment echoList = state.FacingRight
            ? DachoraSegment.EchoRight
            : DachoraSegment.EchoLeft;
        foreach (RoomEnemySlot echo in echoes)
            StartDachoraAnimation(echo, echo.RequireSpeciesState<DachoraEnemyState>(), echoList);
    }

    /// <summary>Runs upward acceleration, trail placement, and ceiling collision.</summary>
    private void RunDachoraShinespark(
        RoomEnemySlot slot,
        DachoraEnemyState state,
        SamusState samus,
        RoomLevelData level)
    {
        StepDachoraShineGlow(slot, state);
        UpdateDachoraEchoPositions(slot);

        state.VerticalAcceleration += SamusGravity(samus);
        state.Speed += state.VerticalAcceleration;

        // The cap replaces only the whole pixels, compared by the sign of their wrapped difference; the fraction is kept.
        SignedSixteenSixteen speed = state.Speed;
        SignedSixteenSixteen capped = speed.WholePixels >= DachoraShinesparkMaximumWholeSpeed
            ? speed.WithWholePixels(DachoraShinesparkMaximumWholeSpeed)
            : speed;
        SignedSixteenSixteen upwardDisplacement = -capped;
        if (!_terrain.MoveEnemyVertically(level, slot, upwardDisplacement))
            return;

        // Ceiling impact reverses the facing bit before the fall, a visible retail quirk.
        if (!state.FacingRight)
        {
            StartDachoraAnimation(slot, state, DachoraSegment.FallingRight);
            state.FacingRight = true;
        }
        else
        {
            StartDachoraAnimation(slot, state, DachoraSegment.FallingLeft);
            state.FacingRight = false;
        }
        state.Function = DachoraAiFunction.Falling;
        state.Speed = SignedSixteenSixteen.Zero;
        state.SetGlowCycle(frame: 0, ticks: 0);
        ShowDachoraColours(slot, DachoraColourPhase.Default, frame: 0);
        _audio.QueueEnemySound(DachoraCeilingImpactSound, priority: SoundRequestPriority.Normal);
    }

    /// <summary>Runs the gravity-capped return to the ground.</summary>
    private void RunDachoraFalling(
        RoomEnemySlot slot,
        DachoraEnemyState state,
        SamusState samus,
        RoomLevelData level)
    {
        state.Speed += SamusGravity(samus);
        if (state.Speed.WholePixels >= DachoraMaximumFallSpeed.WholePixels)
            state.Speed = DachoraMaximumFallSpeed;

        if (!_terrain.MoveEnemyVertically(
                level,
                slot,
                state.Speed))
        {
            return;
        }

        StartDachoraAnimation(
            slot,
            state,
            state.FacingRight
                ? DachoraSegment.RunningRight
                : DachoraSegment.RunningLeft);
        state.Function = state.FacingRight
            ? DachoraAiFunction.RunningRight
            : DachoraAiFunction.RunningLeft;
        state.Speed = SignedSixteenSixteen.Zero;
    }

    /// <summary>Runs the four-slot, first-free echo pipeline.</summary>
    private void UpdateDachoraEchoPositions(RoomEnemySlot body)
    {
        RoomEnemySlot[] echoes = RequireDachoraEchoSlots(body);
        DachoraEnemyState first = echoes[0].RequireSpeciesState<DachoraEnemyState>();
        if (first.EchoPositionTimer != 0)
        {
            first.EchoPositionTimer--;
            return;
        }

        first.EchoPositionTimer = DachoraEchoPositionInterval;
        for (int trailPlace = 0; trailPlace < echoes.Length; trailPlace++)
        {
            RoomEnemySlot echo = echoes[trailPlace];
            DachoraEnemyState echoState = echo.RequireSpeciesState<DachoraEnemyState>();
            if (echoState.EchoLifetime != 0)
                continue;

            echo.Position = echo.Position.WithWhole(body.Position.Whole);
            echoState.EchoLifetime = DachoraEchoLifetime;
            // Neighbouring echoes flicker on alternate updates, the first on odd ones.
            echoState.ShowsOnOddFrames = trailPlace % 2 == 0;
            return;
        }
    }

    /// <summary>Runs the alternating, lifetime-owned echo visibility.</summary>
    private static void RunDachoraEcho(
        RoomEnemySlot slot,
        DachoraEnemyState state,
        int frameCycle)
    {
        if (state.EchoLifetime != 0)
        {
            state.EchoLifetime--;
            bool visible = DachoraCadenceDefinitions.EchoOddUpdate.IsSetIn(frameCycle) == state.ShowsOnOddFrames;
            slot.Presence = visible
                ? slot.Presence.Without(EnemyPresence.Hidden)
                : slot.Presence.With(EnemyPresence.Hidden);
            return;
        }

        slot.Presence = slot.Presence.With(EnemyPresence.Hidden);
    }

    /// <summary>Runs the four-frame shine glow.</summary>
    internal static void StepDachoraShineGlow(RoomEnemySlot slot, DachoraEnemyState state)
    {
        int glowFrame = state.GlowFrame;
        if ((uint)glowFrame >= 4)
        {
            throw new InvalidDataException(
                $"Dachora shine glow frame {glowFrame} is not one of its four.");
        }
        ShowDachoraColours(slot, DachoraColourPhase.Shine, glowFrame);
        state.AdvanceShineGlow();
    }

    /// <summary>
    /// Shows Dachora in one frame of a colour phase (#1512): its normal colours, or a frame of its speed or stored-shine
    /// glow, which the import pre-coloured its pictures in. Only the Dachora shows it (#1544).
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">The phase has no such frame.</exception>
    internal static void ShowDachoraColours(
        RoomEnemySlot slot, DachoraColourPhase phase, int frame)
    {
        int lastFrame = phase == DachoraColourPhase.Default ? 0 : DachoraColorDefinitions.AnimatedFrameCount - 1;
        slot.Shading = PictureShading.Shaded(DachoraShades.Of(phase, Ensure.BetweenInclusive(0, lastFrame, frame)));
    }

    /// <summary>The four echoes the Dachora found when it was placed, rejecting a malformed formation.</summary>
    private RoomEnemySlot[] RequireDachoraEchoSlots(RoomEnemySlot body)
    {
        RoomEnemySlot[] echoes = _echoes.Of(body);
        for (int echoIndex = 0; echoIndex < echoes.Length; echoIndex++)
        {
            if (echoes[echoIndex].DefinitionId != EnemyDefinitionId.Dachora)
            {
                throw new InvalidDataException(
                    $"Dachora echo {echoIndex} is enemy {echoes[echoIndex].DefinitionId}, expected Dachora.");
            }
        }
        return echoes;
    }

    /// <summary>Starts an authored Dachora animation at its first tick.</summary>
    private static void StartDachoraAnimation(RoomEnemySlot slot, DachoraEnemyState state, DachoraSegment segment)
    {
        state.Playback = new(DachoraAnimations.StartingWith(segment), AnimationPlace.StartOfAnimation);
        slot.AnimationClock.StepOnNextUpdate();
    }

    /// <summary>The faster clone of a run loop, where the speed-up jump lands at the same pose.</summary>
    private static DachoraSegment FasterDachoraRun(DachoraSegment segment) => segment switch
    {
        DachoraSegment.RunningLeft
            or DachoraSegment.RunningLeftFast
            or DachoraSegment.RunningRight
            or DachoraSegment.RunningRightFast => EnumOrder.Next(segment),
        DachoraSegment.RunningLeftVeryFast or
            DachoraSegment.IdleLeft or
            DachoraSegment.BlinkLeft or
            DachoraSegment.EchoLeft or
            DachoraSegment.FallingLeft or
            DachoraSegment.RunningRightVeryFast or
            DachoraSegment.IdleRight or
            DachoraSegment.BlinkRight or
            DachoraSegment.ChargeRight or
            DachoraSegment.EchoRight or
            DachoraSegment.FallingRight => throw new InvalidOperationException($"Dachora speed-up from {segment}, which has no faster run."),
        _ => throw new InvalidOperationException($"Dachora speed-up from {segment}, which has no faster run."),
    };

    /// <summary>Uses wrapped signed positions and a strict proximity threshold on one axis.</summary>
    private static bool DachoraWithinAxis(int actor, int samus, int distance) =>
        CoordinateAxis.Distance(actor, samus) < distance;

    /// <summary>Samus's current gravity per update, which the shinespark and fall accumulate.</summary>
    private static SignedSixteenSixteen SamusGravity(SamusState samus) =>
        SignedSixteenSixteen.FromParts(
            samus.Kinematics.YAcceleration.Whole,
            samus.Kinematics.YAcceleration.Fraction);

    /// <summary>The segment <paramref name="playback"/> is playing: the one its current phase plays, as a sleep rests in it.</summary>
    private static DachoraSegment Playing(AnimationPlayback<DachoraSegment> playback) =>
        playback.Animation.Phases[playback.Place.Phase].Segment;

    /// <summary>Advances Dachora's typed animation.</summary>
    internal static void AdvanceDachoraTypedAnimation(RoomEnemySlot slot, EnemyAnimationCueContext _) =>
        EnemyAnimationPlayback.Advance(slot, DachoraAnimations.Segments,
            static (DachoraEnemyState state) => state.Playback, static (state, playback) => state.Playback = playback,
            static (_, cue) => throw new InvalidOperationException($"A cue-free Dachora animation raised {cue}."));

    /// <summary>Runs Dachora's main AI.</summary>
    internal void RunDachoraMainAi(RoomEnemySlot slot, IEnemyAiFrame frame)
    {
        RunDachoraMain(
            slot,
            slot.RequireSpeciesState<DachoraEnemyState>(),
            frame.Samus,
            frame.Level,
            frame.Clock.ShortCounter);
    }
}
