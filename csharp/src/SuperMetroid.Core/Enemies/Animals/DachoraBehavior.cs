using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Animation;
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
    public IEnemyEntity Create(RoomEnemyPlace place, EnemyCommonParticipation common) => new DachoraEntity(this, place, common);

    /// <summary>Probes floors, walls, and ceiling during the run and shinespark arc.</summary>
    private readonly EnemyTerrain _terrain;
    /// <summary>The room's enemy places, where the Dachora finds its four echoes when it is placed.</summary>
    private readonly EnemyPlaces _places;
    /// <summary>The four speed-boost echoes each Dachora found when it was placed, which its room lists right after it.</summary>
    private readonly EnemyPartLinks<RoomEnemyPlace[]> _echoes = new();

    /// <summary>Receives the species' sound requests, each queued as the species makes it.</summary>
    private readonly EnemyAudioQueue _audio;

    /// <summary>
    /// Binds the terrain and echo slots used by Dachora's AI behaviours; its glows are shades its draws name (#1512), so
    /// it needs no colour artwork.
    /// </summary>
    internal DachoraBehavior(EnemyTerrain terrain, EnemyPlaces places, EnemyAudioQueue audio)
    {
        _audio = audio;
        _terrain = terrain;
        _places = places;
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
    /// <summary>Enemy-library silence queued at the scripted wall turn, stopping the run's Speed Booster sound.</summary>
    private static readonly SoundEffectId DachoraSpeedBoosterSoundStop =
        EnemySoundEffectDefinitions.DachoraSpeedBoosterSoundStop;
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
    internal void InitializeDachora(RoomEnemyPlace place)
    {
        DachoraSpawnSettings settings = place.SpawnSettings<DachoraSpawnSettings>();
        var state = new DachoraEnemyState(settings.Role == DachoraRole.SpeedBoostEcho ? DachoraAction.Echo : DachoraAction.WaitingForSamus)
        {
            FacingRight = settings.Facing == HorizontalDirection.Right || settings.Role == DachoraRole.SpeedBoostEcho,
        };
        place.SetSpeciesState(state);

        // The animating property both disables Samus collision and admits enemy animations.
        // The population's tangibility property already excludes these actors from collisions.
        place.Processing = place.Processing.With(EnemyProcessing.Animates);
        place.Frame = EnemyFrameId.Empty;
        place.AnimationClock.Restart();

        if (settings.Role == DachoraRole.SpeedBoostEcho)
        {
            // An echo faces the way the body does; each trail placement later gives it an eight-update life.
            state.Playback = new(DachoraAnimations.StartingWith(settings.Facing == HorizontalDirection.Right
                ? DachoraSegment.EchoRight
                : DachoraSegment.EchoLeft), AnimationPlace.StartOfAnimation);
            return;
        }

        _echoes.Place(place, _places.ListedRunAfter(place, DachoraEchoCount));
        state.Playback = new(DachoraAnimations.StartingWith(settings.Facing == HorizontalDirection.Right
            ? DachoraSegment.IdleRight
            : DachoraSegment.IdleLeft), AnimationPlace.StartOfAnimation);
    }

    /// <summary>Runs the Dachora's current action.</summary>
    private void RunDachoraMain(
        RoomEnemyPlace place,
        DachoraEnemyState state,
        SamusState samus,
        RoomLevelData level,
        int frameCycle)
    {
        // Echo AI needs only the frame counter's parity. Every body state needs both the active
        // Samus gravity values and authentic room collision, so fail loudly if the family
        // runs without the state that code reads globally.
        if (state.Action == DachoraAction.Echo)
        {
            RunDachoraEcho(place, state, frameCycle);
            return;
        }

        SamusState activeSamus = samus;
        RoomLevelData activeLevel = level;
        switch (state.Action)
        {
            case DachoraAction.WaitingForSamus:
                RunDachoraWaiting(place, state, activeSamus, activeLevel);
                break;
            case DachoraAction.BlinkingBeforeRun:
                RunDachoraBlinking(place, state);
                break;
            case DachoraAction.RunningLeft:
                RunDachoraLeft(place, state, activeLevel);
                break;
            case DachoraAction.RunningRight:
                RunDachoraRight(place, state, activeLevel);
                break;
            case DachoraAction.ChargingShinespark:
                RunDachoraCharging(place, state);
                break;
            case DachoraAction.Shinesparking:
                RunDachoraShinespark(place, state, activeSamus, activeLevel);
                break;
            case DachoraAction.Falling:
                RunDachoraFalling(place, state, activeSamus, activeLevel);
                break;
            case DachoraAction.Echo:
            default:
                throw new InvalidDataException(
                    $"Dachora has no behaviour for action {state.Action}.");
        }
    }

    /// <summary>Waits for Samus, including the one-pixel floor probe before proximity.</summary>
    private void RunDachoraWaiting(
        RoomEnemyPlace place,
        DachoraEnemyState state,
        SamusState samus,
        RoomLevelData level)
    {
        _terrain.MoveEnemyVertically(level, place, SignedSixteenSixteen.FromWholePixels(1));
        if (!DachoraWithinAxis(place.Position.Y.Whole, samus.Position.Y.Whole, DachoraActivationYDistance) ||
            !DachoraWithinAxis(place.Position.X.Whole, samus.Position.X.Whole, DachoraActivationXDistance))
        {
            return;
        }

        StartDachoraAnimation(
            place,
            state,
            state.FacingRight
                ? DachoraSegment.BlinkRight
                : DachoraSegment.BlinkLeft);
        state.Action = DachoraAction.BlinkingBeforeRun;
        state.StartBlinking(DachoraBlinkDuration);
        _audio.QueueEnemySound(DachoraActivationSound, priority: SoundRequestPriority.Normal);
    }

    /// <summary>Runs the 120-frame warning countdown.</summary>
    private static void RunDachoraBlinking(RoomEnemyPlace place, DachoraEnemyState state)
    {
        if (--state.PhaseTimer != 0)
            return;

        StartDachoraAnimation(
            place,
            state,
            state.FacingRight
                ? DachoraSegment.RunningRight
                : DachoraSegment.RunningLeft);
        state.Action = state.FacingRight
            ? DachoraAction.RunningRight
            : DachoraAction.RunningLeft;
        state.SetGlowCycle(frame: 0, ticks: 1);
    }

    /// <summary>Runs left. Leftward speed is the whole and fractional speed exactly negated.</summary>
    private void RunDachoraLeft(
        RoomEnemyPlace place,
        DachoraEnemyState state,
        RoomLevelData level)
    {
        SignedSixteenSixteen displacement = -AccelerateDachora(place, state, level);
        bool hitWall = _terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(
            level,
            place,
            displacement);
        if (!hitWall)
            EnemyTerrain.AlignEnemyYWithNonSquareSlope(level, place);

        if (!hitWall && CoordinateAxis.Difference(place.Position.X.Whole, DachoraLeftTurnX) >= 0)
            return;

        StartDachoraAnimation(place, state, DachoraSegment.RunningRight);
        state.Action = DachoraAction.RunningRight;
        state.SetGlowCycle(frame: 0, ticks: 1);
        state.FacingRight = true;
        state.Speed = SignedSixteenSixteen.Zero;
        ShowDachoraColours(place, DachoraColourPhase.Default, frame: 0);
    }

    /// <summary>Runs right, including the hard-coded tutorial launch coordinate.</summary>
    private void RunDachoraRight(
        RoomEnemyPlace place,
        DachoraEnemyState state,
        RoomLevelData level)
    {
        SignedSixteenSixteen displacement = AccelerateDachora(place, state, level);
        if (_terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(level, place, displacement))
        {
            _audio.QueueEnemySound(DachoraSpeedBoosterSoundStop, priority: SoundRequestPriority.Normal);
            StartDachoraAnimation(place, state, DachoraSegment.RunningLeft);
            state.Action = DachoraAction.RunningLeft;
            state.FacingRight = false;
            state.Speed = SignedSixteenSixteen.Zero;
            state.SetGlowCycle(frame: 0, ticks: 0);
            ShowDachoraColours(place, DachoraColourPhase.Default, frame: 0);
            return;
        }

        EnemyTerrain.AlignEnemyYWithNonSquareSlope(level, place);
        if (CoordinateAxis.Difference(place.Position.X.Whole, DachoraShinesparkX) < 0)
            return;

        StartDachoraAnimation(place, state, DachoraSegment.ChargeRight);
        state.Action = DachoraAction.ChargingShinespark;
        state.StartCharging(DachoraChargeDuration);
        state.SetGlowCycle(frame: 0, ticks: 0);
        place.Position = place.Position.WithWrappedWholeY(place.Position.Y.Whole + 8);
        _audio.QueueEnemySound(DachoraChargeSound, priority: SoundRequestPriority.Normal);
    }

    /// <summary>
    /// Accelerates the running Dachora. This intentionally retains the
    /// speed glow's cycle (DachoraGlowCycle): after frame three is reached, the game shows that brightest
    /// speed glow frame again every sixteen frames instead of wrapping to frame zero.
    /// </summary>
    internal SignedSixteenSixteen AccelerateDachora(
        RoomEnemyPlace place,
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
                ShowDachoraColours(place, DachoraColourPhase.Speed, glowFrame);
                state.AdvanceSpeedGlow();
            }
        }

        // Presses the body down one pixel every running frame before changing speed.
        // The collision result is discarded; the probe keeps Dachora glued to room slopes.
        _terrain.MoveEnemyVertically(level, place, SignedSixteenSixteen.FromWholePixels(1));

        // The game compares the whole and fraction parts separately, each by whether its difference
        // from the cap's is negative; a fraction of half a pixel or more therefore also counts as below the cap.
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
                state.Playback = state.Playback with { Animation = DachoraAnimations.StartingWith(FasterDachoraRun(state.Playback.PlayingSegment)) };
        }
        else
        {
            state.Speed = DachoraMaximumSpeed;
        }

        return state.Speed;
    }

    /// <summary>Runs the stored-shine countdown and echo activation.</summary>
    private void RunDachoraCharging(RoomEnemyPlace place, DachoraEnemyState state)
    {
        StepDachoraShineGlow(place, state);
        if (--state.PhaseTimer != 0)
            return;

        // The charge animation holds its crouch until the launch, which starts the right-facing echo animation the
        // four trails also show.
        StartDachoraAnimation(place, state, DachoraSegment.EchoRight);
        state.Action = DachoraAction.Shinesparking;

        RoomEnemyPlace[] echoes = RequireDachoraEchoPlaces(place);
        echoes[0].RequireSpeciesState<DachoraEnemyState>().EchoPositionTimer = 0;
        foreach (RoomEnemyPlace echo in echoes)
            echo.RequireSpeciesState<DachoraEnemyState>().EchoLifetime = 0;
        state.VerticalAcceleration = SignedSixteenSixteen.Zero;
        place.Position = place.Position.WithWrappedWholeY(place.Position.Y.Whole - 8);
        _audio.QueueEnemySound(DachoraLaunchSound, priority: SoundRequestPriority.Normal);

        DachoraSegment echoList = state.FacingRight
            ? DachoraSegment.EchoRight
            : DachoraSegment.EchoLeft;
        foreach (RoomEnemyPlace echo in echoes)
            StartDachoraAnimation(echo, echo.RequireSpeciesState<DachoraEnemyState>(), echoList);
    }

    /// <summary>Runs upward acceleration, trail placement, and ceiling collision.</summary>
    private void RunDachoraShinespark(
        RoomEnemyPlace place,
        DachoraEnemyState state,
        SamusState samus,
        RoomLevelData level)
    {
        StepDachoraShineGlow(place, state);
        UpdateDachoraEchoPositions(place);

        state.VerticalAcceleration += SamusGravity(samus);
        state.Speed += state.VerticalAcceleration;

        // The cap replaces only the whole pixels, compared by their wrapped difference read as signed; the fraction is kept.
        SignedSixteenSixteen speed = state.Speed;
        SignedSixteenSixteen capped = speed.WholePixels >= DachoraShinesparkMaximumWholeSpeed
            ? speed.WithWholePixels(DachoraShinesparkMaximumWholeSpeed)
            : speed;
        SignedSixteenSixteen upwardDisplacement = -capped;
        if (!_terrain.MoveEnemyVertically(level, place, upwardDisplacement))
            return;

        // Ceiling impact reverses Dachora's facing before the fall, a visible retail quirk.
        if (!state.FacingRight)
        {
            StartDachoraAnimation(place, state, DachoraSegment.FallingRight);
            state.FacingRight = true;
        }
        else
        {
            StartDachoraAnimation(place, state, DachoraSegment.FallingLeft);
            state.FacingRight = false;
        }
        state.Action = DachoraAction.Falling;
        state.Speed = SignedSixteenSixteen.Zero;
        state.SetGlowCycle(frame: 0, ticks: 0);
        ShowDachoraColours(place, DachoraColourPhase.Default, frame: 0);
        _audio.QueueEnemySound(DachoraCeilingImpactSound, priority: SoundRequestPriority.Normal);
    }

    /// <summary>Runs the gravity-capped return to the ground.</summary>
    private void RunDachoraFalling(
        RoomEnemyPlace place,
        DachoraEnemyState state,
        SamusState samus,
        RoomLevelData level)
    {
        state.Speed += SamusGravity(samus);
        if (state.Speed.WholePixels >= DachoraMaximumFallSpeed.WholePixels)
            state.Speed = DachoraMaximumFallSpeed;

        if (!_terrain.MoveEnemyVertically(
                level,
                place,
                state.Speed))
        {
            return;
        }

        StartDachoraAnimation(
            place,
            state,
            state.FacingRight
                ? DachoraSegment.RunningRight
                : DachoraSegment.RunningLeft);
        state.Action = state.FacingRight
            ? DachoraAction.RunningRight
            : DachoraAction.RunningLeft;
        state.Speed = SignedSixteenSixteen.Zero;
    }

    /// <summary>Moves the Dachora's four echoes, each new echo taking the first free one.</summary>
    private void UpdateDachoraEchoPositions(RoomEnemyPlace body)
    {
        RoomEnemyPlace[] echoes = RequireDachoraEchoPlaces(body);
        DachoraEnemyState first = echoes[0].RequireSpeciesState<DachoraEnemyState>();
        if (first.EchoPositionTimer != 0)
        {
            first.EchoPositionTimer--;
            return;
        }

        first.EchoPositionTimer = DachoraEchoPositionInterval;
        for (int trailPlace = 0; trailPlace < echoes.Length; trailPlace++)
        {
            RoomEnemyPlace echo = echoes[trailPlace];
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
        RoomEnemyPlace place,
        DachoraEnemyState state,
        int frameCycle)
    {
        if (state.EchoLifetime != 0)
        {
            state.EchoLifetime--;
            bool visible = DachoraCadenceDefinitions.EchoOddUpdate.IsSetIn(frameCycle) == state.ShowsOnOddFrames;
            place.Presence = visible
                ? place.Presence.Without(EnemyPresence.Hidden)
                : place.Presence.With(EnemyPresence.Hidden);
            return;
        }

        place.Presence = place.Presence.With(EnemyPresence.Hidden);
    }

    /// <summary>Runs the four-frame shine glow.</summary>
    internal static void StepDachoraShineGlow(RoomEnemyPlace place, DachoraEnemyState state)
    {
        int glowFrame = state.GlowFrame;
        if ((uint)glowFrame >= 4)
        {
            throw new InvalidDataException(
                $"Dachora shine glow frame {glowFrame} is not one of its four.");
        }
        ShowDachoraColours(place, DachoraColourPhase.Shine, glowFrame);
        state.AdvanceShineGlow();
    }

    /// <summary>
    /// Shows Dachora in one frame of a colour phase (#1512): its normal colours, or a frame of its speed or stored-shine
    /// glow, which the import pre-coloured its pictures in. Only the Dachora shows it (#1544).
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">The phase has no such frame.</exception>
    internal static void ShowDachoraColours(
        RoomEnemyPlace place, DachoraColourPhase phase, int frame)
    {
        int lastFrame = phase == DachoraColourPhase.Default ? 0 : DachoraColorDefinitions.AnimatedFrameCount - 1;
        place.Shading = PictureShading.Shaded(DachoraShades.Of(phase, Ensure.BetweenInclusive(0, lastFrame, frame)));
    }

    /// <summary>The four echoes the Dachora found when it was placed, rejecting a malformed formation.</summary>
    private RoomEnemyPlace[] RequireDachoraEchoPlaces(RoomEnemyPlace body)
    {
        RoomEnemyPlace[] echoes = _echoes.Of(body);
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
    private static void StartDachoraAnimation(RoomEnemyPlace place, DachoraEnemyState state, DachoraSegment segment)
    {
        state.Playback = new(DachoraAnimations.StartingWith(segment), AnimationPlace.StartOfAnimation);
        place.AnimationClock.StepOnNextUpdate();
    }

    /// <summary>
    /// The faster clone of each run loop that speeds up, where the speed-up jump lands at the same pose. The very fast
    /// runs are the fastest.
    /// </summary>
    private static readonly Dictionary<DachoraSegment, DachoraSegment> FasterRuns = new()
    {
        [DachoraSegment.RunningLeft] = DachoraSegment.RunningLeftFast,
        [DachoraSegment.RunningLeftFast] = DachoraSegment.RunningLeftVeryFast,
        [DachoraSegment.RunningRight] = DachoraSegment.RunningRightFast,
        [DachoraSegment.RunningRightFast] = DachoraSegment.RunningRightVeryFast,
    };

    /// <summary>The faster clone of a run loop, where the speed-up jump lands at the same pose.</summary>
    /// <exception cref="InvalidOperationException"><paramref name="segment"/> has no faster run.</exception>
    private static DachoraSegment FasterDachoraRun(DachoraSegment segment) =>
        FasterRuns.TryGetValue(segment, out DachoraSegment faster)
            ? faster
            : throw new InvalidOperationException($"Dachora speed-up from {segment}, which has no faster run.");

    /// <summary>Uses wrapped signed positions and a strict proximity threshold on one axis.</summary>
    private static bool DachoraWithinAxis(int actor, int samus, int distance) =>
        CoordinateAxis.Distance(actor, samus) < distance;

    /// <summary>Samus's current gravity per update, which the shinespark and fall accumulate.</summary>
    private static SignedSixteenSixteen SamusGravity(SamusState samus) => samus.Kinematics.YAcceleration.AsDisplacement();

    /// <summary>Advances Dachora's typed animation.</summary>
    internal static void AdvanceDachoraTypedAnimation(RoomEnemyPlace place, EnemyAnimationCueContext _) =>
        EnemyAnimationPlayback.Advance(place, DachoraAnimations.Segments,
            static (DachoraEnemyState state) => state.Playback, static (state, playback) => state.Playback = playback,
            static (_, cue) => throw new InvalidOperationException($"A cue-free Dachora animation raised {cue}."));

    /// <summary>Runs Dachora's main AI.</summary>
    internal void RunDachoraMainAi(RoomEnemyPlace place, IEnemyAiFrame frame)
    {
        RunDachoraMain(
            place,
            place.RequireSpeciesState<DachoraEnemyState>(),
            frame.Samus,
            frame.Level,
            frame.Clock.ShortCounter);
    }
}
