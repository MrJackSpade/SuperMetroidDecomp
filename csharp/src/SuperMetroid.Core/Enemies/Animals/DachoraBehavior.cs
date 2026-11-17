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

    /// <summary>A Dachora's body, or one of the speed-boost echoes its placement names (#2283).</summary>
    public IEnemyEntity Create(RoomEnemyPlace place, EnemyCommonParticipation common) =>
        Ensure.NotNull(place).SpawnSettings<DachoraSpawnSettings>().Role == DachoraRole.SpeedBoostEcho
            ? new DachoraEchoEntity(place, common)
            : new DachoraEntity(this, place, common);

    /// <summary>Probes floors, walls, and ceiling during the run and shinespark arc.</summary>
    private readonly EnemyTerrain _terrain;
    /// <summary>The room's enemy places, where the Dachora finds its four echoes when it is placed.</summary>
    private readonly EnemyPlaces _places;
    /// <summary>The four speed-boost echoes each Dachora found when it was placed, which its room lists right after it.</summary>
    private readonly EnemyPartLinks<RoomEnemyPlace[]> _echoes = new();

    /// <summary>Receives the species' sound requests, each queued as the species makes it.</summary>
    private readonly EnemyAudioQueue _audio;

    /// <summary>
    /// Binds the terrain and the echoes' places used by Dachora's AI behaviours; its glows are shades its draws name (#1512), so
    /// it needs no colour artwork.
    /// </summary>
    internal DachoraBehavior(EnemyTerrain terrain, EnemyPlaces places, EnemyAudioQueue audio)
    {
        _audio = audio;
        _terrain = terrain;
        _places = places;
    }

    /// <summary>Initializes a speed-boost echo: it faces the way the body does; each trail placement later gives it an eight-update life.</summary>
    internal static void InitializeDachoraEcho(RoomEnemyPlace place)
    {
        DachoraSpawnSettings settings = place.SpawnSettings<DachoraSpawnSettings>();
        var echo = new DachoraEchoState();
        place.SetSpeciesState(echo);
        PrepareDachoraPlace(place);
        echo.Playback = new(DachoraAnimations.StartingWith(settings.Facing == HorizontalDirection.Right
            ? DachoraSegment.EchoRight
            : DachoraSegment.EchoLeft), AnimationPlace.StartOfAnimation);
    }

    /// <summary>Initializes the Dachora's body, which finds the four echoes its room lists right after it.</summary>
    internal void InitializeDachora(RoomEnemyPlace place)
    {
        DachoraSpawnSettings settings = place.SpawnSettings<DachoraSpawnSettings>();
        var state = new DachoraEnemyState(DachoraAction.WaitingForSamus)
        {
            Facing = settings.Facing,
        };
        place.SetSpeciesState(state);
        PrepareDachoraPlace(place);
        _echoes.Place(place, _places.ListedRunAfter(place, DachoraTimingDefinitions.EchoCount));
        state.Playback = new(DachoraAnimations.StartingWith(settings.Facing == HorizontalDirection.Right
            ? DachoraSegment.IdleRight
            : DachoraSegment.IdleLeft), AnimationPlace.StartOfAnimation);
    }

    /// <summary>
    /// Readies a Dachora body's or echo's place: the animating property both disables Samus collision and admits enemy
    /// animations, and the population's tangibility property already excludes these actors from collisions.
    /// </summary>
    private static void PrepareDachoraPlace(RoomEnemyPlace place)
    {
        place.Processing = place.Processing.With(EnemyProcessing.Animates);
        place.Frame = EnemyFrameId.Empty;
        place.AnimationClock.Restart();
    }

    /// <summary>Runs the Dachora body's current action.</summary>
    private void RunDachoraMain(
        RoomEnemyPlace place,
        DachoraEnemyState state,
        SamusState samus,
        RoomLevelData level)
    {
        switch (state.Action)
        {
            case DachoraAction.WaitingForSamus:
                RunDachoraWaiting(place, state, samus, level);
                break;
            case DachoraAction.BlinkingBeforeRun:
                RunDachoraBlinking(place, state);
                break;
            case DachoraAction.RunningLeft:
                RunDachoraLeft(place, state, level);
                break;
            case DachoraAction.RunningRight:
                RunDachoraRight(place, state, level);
                break;
            case DachoraAction.ChargingShinespark:
                RunDachoraCharging(place, state);
                break;
            case DachoraAction.Shinesparking:
                RunDachoraShinespark(place, state, samus, level);
                break;
            case DachoraAction.Falling:
                RunDachoraFalling(place, state, samus, level);
                break;
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

    /// <summary>Runs the 120-frame warning countdown.</summary>
    private static void RunDachoraBlinking(RoomEnemyPlace place, DachoraEnemyState state)
    {
        if (--state.PhaseTimer != 0)
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

        if (!hitWall && place.Position.X.Whole >= DachoraMotionDefinitions.LeftTurnX)
            return;

        StartDachoraAnimation(place, state, DachoraSegment.RunningRight);
        state.Action = DachoraAction.RunningRight;
        state.SetGlowCycle(frame: 0, ticks: 1);
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
            state.SetGlowCycle(frame: 0, ticks: 0);
            ShowDachoraColours(place, DachoraColourPhase.Default, frame: 0);
            return;
        }

        EnemyTerrain.AlignEnemyYWithNonSquareSlope(level, place);
        if (place.Position.X.Whole < DachoraMotionDefinitions.ShinesparkX)
            return;

        StartDachoraAnimation(place, state, DachoraSegment.ChargeRight);
        state.Action = DachoraAction.ChargingShinespark;
        state.StartCharging(DachoraTimingDefinitions.ChargeDuration);
        state.SetGlowCycle(frame: 0, ticks: 0);
        place.Position = place.Position.WithWholeY(place.Position.Y.Whole + DachoraMotionDefinitions.ChargeCrouchDepth);
        _audio.QueueEnemySound(EnemySoundEffectDefinitions.DachoraChargeSound, priority: SoundRequestPriority.Normal);
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
        if (state.Speed.WholePixels >= DachoraMotionDefinitions.MaximumSpeed.WholePixels)
        {
            if (state.GlowFrame == 0 && state.GlowTicks == 1)
                _audio.QueueEnemySound(EnemySoundEffectDefinitions.DachoraSpeedBoosterSound, priority: SoundRequestPriority.Normal);

            if (state.TickGlowCycle())
            {
                ShowDachoraColours(place, DachoraColourPhase.Speed, state.GlowFrame);
                state.AdvanceSpeedGlow();
            }
        }

        // Presses the body down one pixel every running frame before changing speed.
        // The collision result is discarded; the probe keeps Dachora glued to room slopes.
        _terrain.MoveEnemyVertically(level, place, SignedSixteenSixteen.FromWholePixels(1));

        // Every run starts from rest and gains a sixteenth of a pixel an update, so its speed lands exactly on the cap and
        // stays there.
        if (state.Speed < DachoraMotionDefinitions.MaximumSpeed)
        {
            SignedSixteenSixteen speed = state.Speed + DachoraMotionDefinitions.Acceleration;
            state.Speed = speed;

            // The game's speed-up jump lands at the matching pose in the faster clone of the
            // current six-pose run loop: every run plays one six-pose phase, so the animation keeps its place and timing.
            if (speed == DachoraMotionDefinitions.FastRunSpeed || speed == DachoraMotionDefinitions.MaximumSpeed)
                state.Playback = state.Playback with { Animation = DachoraAnimations.StartingWith(FasterDachoraRun(state.Playback.PlayingSegment)) };
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

        RoomEnemyPlace[] echoes = _echoes.Of(place);
        state.TrailTimer = 0;
        foreach (RoomEnemyPlace echo in echoes)
            echo.RequireSpeciesState<DachoraEchoState>().Lifetime = 0;
        state.VerticalAcceleration = SignedSixteenSixteen.Zero;
        place.Position = place.Position.WithWholeY(place.Position.Y.Whole - DachoraMotionDefinitions.ChargeCrouchDepth);
        _audio.QueueEnemySound(EnemySoundEffectDefinitions.DachoraLaunchSound, priority: SoundRequestPriority.Normal);

        DachoraSegment echoList = state.Facing == HorizontalDirection.Right
            ? DachoraSegment.EchoRight
            : DachoraSegment.EchoLeft;
        foreach (RoomEnemyPlace echo in echoes)
            echo.RequireSpeciesState<DachoraEchoState>().Playback = StartedDachoraAnimation(echo, echoList);
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

        // The cap replaces only the whole pixels, compared by which way their wrapped difference lies; the fraction is kept.
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
        state.SetGlowCycle(frame: 0, ticks: 0);
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
        if (state.Speed.WholePixels >= DachoraMotionDefinitions.MaximumFallSpeed.WholePixels)
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
    private void UpdateDachoraEchoPositions(RoomEnemyPlace body, DachoraEnemyState state)
    {
        RoomEnemyPlace[] echoes = _echoes.Of(body);
        if (state.TrailTimer != 0)
        {
            state.TrailTimer--;
            return;
        }

        state.TrailTimer = DachoraTimingDefinitions.EchoPlacementInterval;
        for (int trailPlace = 0; trailPlace < echoes.Length; trailPlace++)
        {
            RoomEnemyPlace echo = echoes[trailPlace];
            DachoraEchoState echoState = echo.RequireSpeciesState<DachoraEchoState>();
            if (echoState.Lifetime != 0)
                continue;

            echo.Position = echo.Position.WithWhole(body.Position.Whole);
            echoState.Lifetime = DachoraTimingDefinitions.EchoLifetime;
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
        if (state.Lifetime != 0)
        {
            state.Lifetime--;
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
        ShowDachoraColours(place, DachoraColourPhase.Shine, state.GlowFrame);
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

    /// <summary>Advances the Dachora body's cue-free typed animation.</summary>
    internal static void AdvanceDachoraTypedAnimation(RoomEnemyPlace place, EnemyAnimationCueContext _) =>
        EnemyAnimationPlayback.Advance(place, DachoraAnimations.Segments,
            static (DachoraEnemyState state) => state.Playback, static (state, playback) => state.Playback = playback,
            static (_, cue) => throw new InvalidOperationException($"A cue-free Dachora animation raised {cue}."));

    /// <summary>Advances a speed-boost echo's cue-free typed animation.</summary>
    internal static void AdvanceDachoraEchoTypedAnimation(RoomEnemyPlace place, EnemyAnimationCueContext _) =>
        EnemyAnimationPlayback.Advance(place, DachoraAnimations.Segments,
            static (DachoraEchoState state) => state.Playback, static (state, playback) => state.Playback = playback,
            static (_, cue) => throw new InvalidOperationException($"A cue-free Dachora echo animation raised {cue}."));

    /// <summary>Runs a speed-boost echo's main AI, which needs only the frame counter's parity.</summary>
    internal static void RunDachoraEchoMainAi(RoomEnemyPlace place, IEnemyAiFrame frame) =>
        RunDachoraEcho(place, place.RequireSpeciesState<DachoraEchoState>(), frame.Clock.ShortCounter);

    /// <summary>Runs the Dachora body's main AI, which reads Samus's gravity and the room's collision.</summary>
    internal void RunDachoraMainAi(RoomEnemyPlace place, IEnemyAiFrame frame)
    {
        RunDachoraMain(
            place,
            place.RequireSpeciesState<DachoraEnemyState>(),
            frame.Samus,
            frame.Level);
    }
}
