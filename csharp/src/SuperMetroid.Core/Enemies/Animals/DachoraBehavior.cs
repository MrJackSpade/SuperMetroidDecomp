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
internal sealed class DachoraBehavior : IEnemySpecies, IEnemySpawnFactory<DachoraSpawnSettings>
{

    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.Dachora];

    /// <summary>
    /// A Dachora's body, or one of the speed-boost echoes its placement names (#2283). A body offers the entries its room
    /// lists after it the creation that makes each echo join its trail (#2554); an echo no body was listed before runs its
    /// own flicker with nothing to place it, as the game's does.
    /// </summary>
    public EnemyOccupant Create(RoomEnemyOccupant occupant, DachoraSpawnSettings settings, RoomEnemyPlace place, EnemyCommonParticipation common)
    {
        if (Ensure.NotNull(settings).Role == DachoraRole.SpeedBoostEcho)
            return EnemyOccupant.Running(new DachoraEchoEntity(occupant, place, common, settings));
        var body = new DachoraEntity(this, occupant, place, common, settings);
        _partOffers.Offer<DachoraSpawnSettings>(EnemyDefinitionId.Dachora, (later, laterSettings, laterPlace, laterCommon) =>
            laterSettings.Role == DachoraRole.SpeedBoostEcho
                ? EnemyOccupant.Running(body.State.Echoes.Join(new DachoraEchoEntity(later, laterPlace, laterCommon, laterSettings)))
                : Create(later, laterSettings, laterPlace, laterCommon));
        return EnemyOccupant.Running(body);
    }

    /// <summary>Probes floors, walls, and ceiling during the run and shinespark arc.</summary>
    private readonly EnemyTerrain _terrain;
    /// <summary>Where a body offers the entries listed after it the creation that makes its echoes join its trail.</summary>
    private readonly EnemyPartOffers _partOffers;

    /// <summary>Receives the species' sound requests, each queued as the species makes it.</summary>
    private readonly EnemyAudioQueue _audio;

    /// <summary>
    /// Binds the terrain used by Dachora's AI behaviours and where its bodies offer their echoes their trail; its glows are
    /// shades its draws name (#1512), so it needs no colour artwork.
    /// </summary>
    internal DachoraBehavior(EnemyTerrain terrain, EnemyPartOffers partOffers, EnemyAudioQueue audio)
    {
        _audio = audio;
        _terrain = terrain;
        _partOffers = partOffers;
    }

    /// <summary>Initializes a speed-boost echo: it faces the way the body does; each trail placement later gives it its life (<see cref="DachoraTimingDefinitions.EchoLifetime"/>).</summary>
    /// <returns>The state the enemy holds from its setup.</returns>
    internal static DachoraEchoState InitializeDachoraEcho(DachoraSpawnSettings settings, RoomEnemyPlace place)
    {
        var echo = new DachoraEchoState();
        PrepareDachoraPlace(place);
        echo.Playback = new(DachoraAnimations.StartingWith(settings.Facing == HorizontalDirection.Right
            ? DachoraSegment.EchoRight
            : DachoraSegment.EchoLeft), AnimationPlace.StartOfAnimation);
        return echo;
    }

    /// <summary>Initializes the Dachora's body, whose echoes, listed after it, join its trail as each is built.</summary>
    /// <returns>The state the enemy holds from its setup.</returns>
    internal static DachoraEnemyState InitializeDachora(DachoraSpawnSettings settings, RoomEnemyPlace place)
    {
        var state = new DachoraEnemyState(DachoraAction.WaitingForSamus)
        {
            Facing = settings.Facing,
        };
        PrepareDachoraPlace(place);
        state.Playback = new(DachoraAnimations.StartingWith(settings.Facing == HorizontalDirection.Right
            ? DachoraSegment.IdleRight
            : DachoraSegment.IdleLeft), AnimationPlace.StartOfAnimation);
        return state;
    }

    /// <summary>
    /// Readies a Dachora body's or echo's place to animate; its placement already keeps it from touching Samus or being
    /// shot.
    /// </summary>
    private static void PrepareDachoraPlace(RoomEnemyPlace place)
    {
        place.Processing = place.Processing.With(EnemyProcessing.Animates);
        place.Frame = EnemyFrameId.PointAtOrigin;
        place.AnimationClock.Restart();
    }

    /// <summary>What <see cref="RunDachoraMain"/> does for each <see cref="DachoraAction"/> member, each member's own arm.</summary>
    private IdentityTable<DachoraAction, Action<RoomEnemyPlace, DachoraEnemyState, SamusState, RoomLevelData>> RunDachoraMainForDachoraAction => field ??= new(new Dictionary<DachoraAction, Action<RoomEnemyPlace, DachoraEnemyState, SamusState, RoomLevelData>>
    {
        [DachoraAction.WaitingForSamus] = (place, state, samus, level) => RunDachoraWaiting(place, state, samus, level),
        [DachoraAction.BlinkingBeforeRun] = (place, state, samus, level) => RunDachoraBlinking(place, state),
        [DachoraAction.RunningLeft] = (place, state, samus, level) => RunDachoraLeft(place, state, level),
        [DachoraAction.RunningRight] = (place, state, samus, level) => RunDachoraRight(place, state, level),
        [DachoraAction.ChargingShinespark] = (place, state, samus, level) => RunDachoraCharging(place, state),
        [DachoraAction.Shinesparking] = (place, state, samus, level) => RunDachoraShinespark(place, state, samus, level),
        [DachoraAction.Falling] = (place, state, samus, level) => RunDachoraFalling(place, state, samus, level),
    });

    /// <summary>Runs the Dachora body's current action.</summary>
    private void RunDachoraMain(
        RoomEnemyPlace place,
        DachoraEnemyState state,
        SamusState samus,
        RoomLevelData level)
    {
        RunDachoraMainForDachoraAction[state.Action](place, state, samus, level);
    }

    /// <summary>Waits for Samus, including the one-pixel floor probe before proximity.</summary>
    private void RunDachoraWaiting(
        RoomEnemyPlace place,
        DachoraEnemyState state,
        SamusState samus,
        RoomLevelData level)
    {
        _terrain.MoveEnemyVertically(level, place, DachoraMotionDefinitions.WaitingFloorProbe);
        if (!DachoraWithinAxis(place.Position.Y.Whole, samus.Position.Y.Whole, DachoraMotionDefinitions.ActivationYDistance) ||
            !DachoraWithinAxis(place.Position.X.Whole, samus.Position.X.Whole, DachoraMotionDefinitions.ActivationXDistance))
        {
            return;
        }

        StartDachoraAnimation(
            place,
            state,
            state.Facing == HorizontalDirection.Right
                ? DachoraSegment.BlinkRight
                : DachoraSegment.BlinkLeft);
        state.Action = DachoraAction.BlinkingBeforeRun;
        state.StartBlinking(DachoraTimingDefinitions.BlinkDuration);
        _audio.QueueEnemySound(EnemySoundEffectDefinitions.DachoraActivationSound, priority: SoundRequestPriority.Normal);
    }

    /// <summary>Runs the warning blink (<see cref="DachoraTimingDefinitions.BlinkDuration"/>).</summary>
    private static void RunDachoraBlinking(RoomEnemyPlace place, DachoraEnemyState state)
    {
        if (!state.PhaseTimer.StepEnds())
            return;

        StartDachoraAnimation(
            place,
            state,
            state.Facing == HorizontalDirection.Right
                ? DachoraSegment.RunningRight
                : DachoraSegment.RunningLeft);
        state.Action = state.Facing == HorizontalDirection.Right
            ? DachoraAction.RunningRight
            : DachoraAction.RunningLeft;
        state.SetGlowCycle(DachoraGlowCycleDefinitions.RunStart);
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

        if (!hitWall && place.Position.X.Whole >= DachoraMotionDefinitions.LeftTurnX)
            return;

        StartDachoraAnimation(place, state, DachoraSegment.RunningRight);
        state.Action = DachoraAction.RunningRight;
        state.SetGlowCycle(DachoraGlowCycleDefinitions.RunStart);
        state.Facing = HorizontalDirection.Right;
        state.Speed = SignedSixteenSixteen.Zero;
        ShowDachoraColours(place, DachoraColourPhase.Default, frame: 0);
    }

    /// <summary>Runs right, including the fixed tutorial launch coordinate.</summary>
    private void RunDachoraRight(
        RoomEnemyPlace place,
        DachoraEnemyState state,
        RoomLevelData level)
    {
        SignedSixteenSixteen displacement = AccelerateDachora(place, state, level);
        if (_terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(level, place, displacement))
        {
            _audio.QueueEnemySound(EnemySoundEffectDefinitions.DachoraSpeedBoosterSoundStop, priority: SoundRequestPriority.Normal);
            StartDachoraAnimation(place, state, DachoraSegment.RunningLeft);
            state.Action = DachoraAction.RunningLeft;
            state.Facing = HorizontalDirection.Left;
            state.Speed = SignedSixteenSixteen.Zero;
            state.SetGlowCycle(DachoraGlowCycleDefinitions.Cleared);
            ShowDachoraColours(place, DachoraColourPhase.Default, frame: 0);
            return;
        }

        EnemyTerrain.AlignEnemyYWithNonSquareSlope(level, place);
        if (place.Position.X.Whole < DachoraMotionDefinitions.ShinesparkX)
            return;

        StartDachoraAnimation(place, state, DachoraSegment.ChargeRight);
        state.Action = DachoraAction.ChargingShinespark;
        state.StartCharging(DachoraTimingDefinitions.ChargeDuration);
        state.SetGlowCycle(DachoraGlowCycleDefinitions.Cleared);
        place.Position = place.Position.WithWholeY(place.Position.Y.Whole + DachoraMotionDefinitions.ChargeCrouchDepth);
        _audio.QueueEnemySound(EnemySoundEffectDefinitions.DachoraChargeSound, priority: SoundRequestPriority.Normal);
    }

    /// <summary>
    /// Accelerates the running Dachora. This intentionally retains the
    /// speed glow's cycle (DachoraGlowCycle): once its last frame is reached, the game shows that brightest
    /// speed glow frame again at each step instead of wrapping to frame zero.
    /// </summary>
    private SignedSixteenSixteen AccelerateDachora(
        RoomEnemyPlace place,
        DachoraEnemyState state,
        RoomLevelData level)
    {
        if (state.Speed >= DachoraMotionDefinitions.MaximumSpeed)
        {
            if (state.GlowCycleIsAtRunStart)
                _audio.QueueEnemySound(EnemySoundEffectDefinitions.DachoraSpeedBoosterSound, priority: SoundRequestPriority.Normal);

            if (state.TickGlowCycle())
            {
                ShowDachoraColours(place, DachoraColourPhase.Speed, state.GlowFrame);
                state.AdvanceSpeedGlow();
            }
        }

        // Presses the body down every running frame before changing speed.
        // The collision result is discarded; the probe keeps Dachora glued to room slopes.
        _terrain.MoveEnemyVertically(level, place, DachoraMotionDefinitions.RunningFloorPress);

        // Every run starts from rest and gains its acceleration each update, so its speed lands exactly on the cap and
        // stays there.
        if (state.Speed < DachoraMotionDefinitions.MaximumSpeed)
        {
            SignedSixteenSixteen speed = state.Speed + DachoraMotionDefinitions.Acceleration;
            state.Speed = speed;

            // Speeding up switches to the faster run at the pose the current run is showing: every run
            // plays the same six poses, so the animation keeps its place and timing.
            if (speed == DachoraMotionDefinitions.FastRunSpeed || speed == DachoraMotionDefinitions.MaximumSpeed)
                state.Playback = state.Playback with { Animation = DachoraAnimations.StartingWith(FasterDachoraRun(state.Playback.PlayingSegment)) };
        }

        return state.Speed;
    }

    /// <summary>Runs the stored-shine countdown and echo activation.</summary>
    private void RunDachoraCharging(RoomEnemyPlace place, DachoraEnemyState state)
    {
        StepDachoraShineGlow(place, state);
        if (!state.PhaseTimer.StepEnds())
            return;

        // The charge animation holds its crouch until the launch, which starts the right-facing echo animation the
        // trails also show.
        StartDachoraAnimation(place, state, DachoraSegment.EchoRight);
        state.Action = DachoraAction.Shinesparking;

        IReadOnlyList<DachoraEchoEntity> echoes = state.Echoes.Joined;
        state.TrailTimer = UpdateCountdown.Ended();
        foreach (DachoraEchoEntity echo in echoes)
            echo.State.Lifetime = UpdateCountdown.Ended();
        state.VerticalAcceleration = SignedSixteenSixteen.Zero;
        place.Position = place.Position.WithWholeY(place.Position.Y.Whole - DachoraMotionDefinitions.ChargeCrouchDepth);
        _audio.QueueEnemySound(EnemySoundEffectDefinitions.DachoraLaunchSound, priority: SoundRequestPriority.Normal);

        DachoraSegment echoList = state.Facing == HorizontalDirection.Right
            ? DachoraSegment.EchoRight
            : DachoraSegment.EchoLeft;
        foreach (DachoraEchoEntity echo in echoes)
            echo.State.Playback = StartedDachoraAnimation(echo.Place, echoList);
    }

    /// <summary>Runs upward acceleration, trail placement, and ceiling collision.</summary>
    private void RunDachoraShinespark(
        RoomEnemyPlace place,
        DachoraEnemyState state,
        SamusState samus,
        RoomLevelData level)
    {
        StepDachoraShineGlow(place, state);
        UpdateDachoraEchoPositions(place, state);

        state.VerticalAcceleration += SamusGravity(samus);
        state.Speed += state.VerticalAcceleration;

        // A speed at or past the shinespark's fastest whole pixels is capped to them; the fraction is kept.
        SignedSixteenSixteen speed = state.Speed;
        SignedSixteenSixteen capped = speed.WholePixels >= DachoraMotionDefinitions.ShinesparkMaximumWholeSpeed
            ? speed.WithWholePixels(DachoraMotionDefinitions.ShinesparkMaximumWholeSpeed)
            : speed;
        SignedSixteenSixteen upwardDisplacement = -capped;
        if (!_terrain.MoveEnemyVertically(level, place, upwardDisplacement))
            return;

        // Ceiling impact reverses Dachora's facing before the fall, a visible quirk.
        state.Facing = state.Facing.Reversed();
        StartDachoraAnimation(place, state, state.Facing == HorizontalDirection.Right
            ? DachoraSegment.FallingRight
            : DachoraSegment.FallingLeft);
        state.Action = DachoraAction.Falling;
        state.Speed = SignedSixteenSixteen.Zero;
        state.SetGlowCycle(DachoraGlowCycleDefinitions.Cleared);
        ShowDachoraColours(place, DachoraColourPhase.Default, frame: 0);
        _audio.QueueEnemySound(EnemySoundEffectDefinitions.DachoraCeilingImpactSound, priority: SoundRequestPriority.Normal);
    }

    /// <summary>Runs the gravity-capped return to the ground.</summary>
    private void RunDachoraFalling(
        RoomEnemyPlace place,
        DachoraEnemyState state,
        SamusState samus,
        RoomLevelData level)
    {
        state.Speed += SamusGravity(samus);
        if (state.Speed >= DachoraMotionDefinitions.MaximumFallSpeed)
            state.Speed = DachoraMotionDefinitions.MaximumFallSpeed;

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
            state.Facing == HorizontalDirection.Right
                ? DachoraSegment.RunningRight
                : DachoraSegment.RunningLeft);
        state.Action = state.Facing == HorizontalDirection.Right
            ? DachoraAction.RunningRight
            : DachoraAction.RunningLeft;
        state.Speed = SignedSixteenSixteen.Zero;
    }

    /// <summary>Spaces the body's trail of echoes, each new echo taking the first free one (#2147).</summary>
    private static void UpdateDachoraEchoPositions(RoomEnemyPlace body, DachoraEnemyState state)
    {
        IReadOnlyList<DachoraEchoEntity> echoes = state.Echoes.Joined;
        if (state.TrailTimer.IsRunning)
        {
            state.TrailTimer.Count();
            return;
        }

        state.TrailTimer = UpdateCountdown.For(DachoraTimingDefinitions.EchoPlacementInterval);
        for (int trailPlace = 0; trailPlace < echoes.Count; trailPlace++)
        {
            RoomEnemyPlace echo = echoes[trailPlace].Place;
            DachoraEchoState echoState = echoes[trailPlace].State;
            if (echoState.Lifetime.IsRunning)
                continue;

            echo.Position = echo.Position.WithWhole(body.Position.Whole);
            echoState.Lifetime = UpdateCountdown.For(DachoraTimingDefinitions.EchoLifetime);
            // Neighbouring echoes flicker on alternate updates, the first on odd ones.
            echoState.ShowsOnOddFrames = trailPlace % 2 == 0;
            return;
        }
    }

    /// <summary>Runs the alternating, lifetime-owned echo visibility.</summary>
    private static void RunDachoraEcho(
        RoomEnemyPlace place,
        DachoraEchoState state,
        int frameCycle)
    {
        if (state.Lifetime.IsRunning)
        {
            state.Lifetime.Count();
            bool visible = DachoraCadenceDefinitions.EchoOddUpdate.IsInSecondHalfAt(frameCycle) == state.ShowsOnOddFrames;
            place.Presence = visible
                ? place.Presence.Without(EnemyPresence.Hidden)
                : place.Presence.With(EnemyPresence.Hidden);
            return;
        }

        place.Presence = place.Presence.With(EnemyPresence.Hidden);
    }

    /// <summary>Runs the shine glow.</summary>
    private static void StepDachoraShineGlow(RoomEnemyPlace place, DachoraEnemyState state)
    {
        ShowDachoraColours(place, DachoraColourPhase.Shine, state.GlowFrame);
        state.AdvanceShineGlow();
    }

    /// <summary>
    /// Shows Dachora in one frame of a colour phase (#1512): its normal colours, or a frame of its speed or stored-shine
    /// glow, which the import pre-coloured its pictures in. Only the Dachora shows it (#1544).
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">The phase has no such frame.</exception>
    private static void ShowDachoraColours(
        RoomEnemyPlace place, DachoraColourPhase phase, int frame)
    {
        int lastFrame = DachoraColorDefinitions.PhaseFrameCounts[phase] - 1;
        place.Shading = PictureShading.Shaded(DachoraShades.Of(phase, Ensure.BetweenInclusive(0, lastFrame, frame)));
    }

    /// <summary>Starts an authored Dachora animation on the body at its first tick.</summary>
    private static void StartDachoraAnimation(RoomEnemyPlace place, DachoraEnemyState state, DachoraSegment segment) =>
        state.Playback = StartedDachoraAnimation(place, segment);

    /// <summary>A body's or echo's authored animation started at its first tick, which steps on the place's next update.</summary>
    private static AnimationPlayback<DachoraSegment> StartedDachoraAnimation(RoomEnemyPlace place, DachoraSegment segment)
    {
        place.AnimationClock.StepOnNextUpdate();
        return new(DachoraAnimations.StartingWith(segment), AnimationPlace.StartOfAnimation);
    }

    /// <summary>
    /// The quicker run of each run that speeds up, where the speed-up lands at the same pose: every run plays its facing's one
    /// <see cref="DachoraStride"/> (#2477). The very fast runs are the fastest.
    /// </summary>
    private static readonly Dictionary<DachoraSegment, DachoraSegment> FasterRuns = new()
    {
        [DachoraSegment.RunningLeft] = DachoraSegment.RunningLeftFast,
        [DachoraSegment.RunningLeftFast] = DachoraSegment.RunningLeftVeryFast,
        [DachoraSegment.RunningRight] = DachoraSegment.RunningRightFast,
        [DachoraSegment.RunningRightFast] = DachoraSegment.RunningRightVeryFast,
    };

    /// <summary>The quicker run of a run, where the speed-up lands at the same pose.</summary>
    /// <exception cref="InvalidOperationException"><paramref name="segment"/> has no faster run.</exception>
    private static DachoraSegment FasterDachoraRun(DachoraSegment segment) =>
        FasterRuns.TryGetValue(segment, out DachoraSegment faster)
            ? faster
            : throw new InvalidOperationException($"Dachora speed-up from {segment}, which has no faster run.");

    /// <summary>Whether Samus lies less than <paramref name="distance"/> pixels from the Dachora on one axis.</summary>
    private static bool DachoraWithinAxis(int actor, int samus, int distance) =>
        Math.Abs(actor - samus) < distance;

    /// <summary>Samus's current gravity per update, which the shinespark and fall accumulate.</summary>
    private static SignedSixteenSixteen SamusGravity(SamusState samus) => samus.Kinematics.YAcceleration.AsDisplacement();

    /// <summary>Advances the Dachora body's cue-free typed animation.</summary>
    internal static void AdvanceDachoraTypedAnimation(RoomEnemyPlace place, DachoraEnemyState dachoraEnemyState) =>
        EnemyAnimationPlayback.AdvanceSilently(place, dachoraEnemyState, DachoraAnimations.Segments,
            static (DachoraEnemyState state) => state.Playback, static (state, playback) => state.Playback = playback);

    /// <summary>Advances a speed-boost echo's cue-free typed animation.</summary>
    internal static void AdvanceDachoraEchoTypedAnimation(RoomEnemyPlace place, DachoraEchoState dachoraEchoState) =>
        EnemyAnimationPlayback.AdvanceSilently(place, dachoraEchoState, DachoraAnimations.Segments,
            static (DachoraEchoState state) => state.Playback, static (state, playback) => state.Playback = playback);

    /// <summary>Runs a speed-boost echo's main AI, which needs only the frame counter's parity.</summary>
    internal static void RunDachoraEchoMainAi(RoomEnemyPlace place, DachoraEchoState dachoraEchoState, IEnemyAiFrame frame) =>
        RunDachoraEcho(place, dachoraEchoState, frame.Clock.ShortCounter);

    /// <summary>Runs the Dachora body's main AI, which reads Samus's gravity and the room's collision.</summary>
    internal void RunDachoraMainAi(RoomEnemyPlace place, DachoraEnemyState dachoraEnemyState, IEnemyAiFrame frame)
    {
        RunDachoraMain(
            place,
            dachoraEnemyState,
            frame.Samus,
            frame.Level);
    }
}
