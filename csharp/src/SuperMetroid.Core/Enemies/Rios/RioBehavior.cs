using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Enemies.Rios;

/// <summary>The Rio species owner: its per-room state, behaviour and the interactions it registers.</summary>
[RoomLifetime]
internal sealed class RioBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.Rio];

    /// <inheritdoc/>
    public IEnemyEntity Create(RoomEnemyPlace place, EnemyCommonParticipation common) => new RioEntity(this, place, common);

    /// <summary>Supplies the shared enemy random number consumed on every Rio AI frame.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Resolves Rio's horizontal and vertical flight against current room terrain.</summary>
    private readonly EnemyTerrain _terrain;

    /// <summary>Receives the species' sound requests, each queued as the species makes it.</summary>
    private readonly EnemyAudioQueue _audio;

    /// <summary>Binds Rio AI to shared room RNG and enemy terrain collision.</summary>
    internal RioBehavior(EnemyRoomBinding room, EnemyTerrain terrain, EnemyAudioQueue audio)
    {
        _audio = audio;
        _room = room;
        _terrain = terrain;
    }

    /// <summary>Initializes the Rio.</summary>
    internal static void InitializeRio(RoomEnemyPlace place)
    {
        var state = new RioEnemyState(RioAction.WaitingForSamus)
        {
            AnimationFinished = false,
        };
        place.SetSpeciesState(state);

        // The Rio rests in its idle animation until its first swoop starts another.
        state.Playback = EnemyAnimationPlayback.Start(place, RioAnimations.StartingWith(RioSegment.Idle));
    }

    /// <summary>Runs the Rio's main AI and its five actions.</summary>
    private void RunRioMain(
        RoomEnemyPlace place,
        RioEnemyState state,
        SamusState samus,
        RoomLevelData level,
        Position16<RoomPixels> camera)
    {
        // The Rio's main behaviour advances the global enemy RNG even though this particular family never
        // consumes the returned number. That draw changes every
        // later random enemy in the room, so it is part of Rio's observable behavior.
        _room.Random.Generate();

        switch (state.Action)
        {
            case RioAction.WaitingForSamus:
                if (!EnemyMath.IsWithinStrictModularDistance(
                        samus.Position.X.Whole,
                        place.Position.X.Whole,
                        RioLaunchDefinitions.RioTriggerDistance))
                {
                    return;
                }

                state.Velocity = new EightEightVector<RoomPixels>(
                    RioLaunchDefinitions.RioXVelocity,
                    RioLaunchDefinitions.RioYVelocity);
                if (CoordinateAxis.Difference(samus.Position.X.Whole, place.Position.X.Whole) < 0)
                    state.Velocity = state.Velocity.ReversedX();
                StartRioAnimation(
                    place,
                    state,
                    RioSegment.SwoopOpening);
                state.Action = RioAction.Diving;

                // The cry plays only when the attack starts on screen:
                // it is queued only for the visible attack start.
                if (EnemyProcessingWindow.IsWithinProcessingWindow(place, camera))
                    _audio.QueueEnemySound(EnemySoundEffectDefinitions.RioDiveSound, priority: SoundRequestPriority.Normal);
                return;

            case RioAction.WaitingForLandingAnimation:
                if (!state.AnimationFinished)
                    return;
                state.AnimationFinished = false;
                StartRioAnimation(
                    place,
                    state,
                    RioSegment.PostSwoopIdle);
                state.Action = RioAction.WaitingForSamus;
                return;

            case RioAction.Diving:
                if (_terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(
                        level,
                        place,
                        state.Velocity.X.ToSixteenSixteenDelta()))
                {
                    state.Velocity = state.Velocity.ReversedX();
                    ReverseRioVerticalVelocityAndBounce(state);
                    return;
                }
                if (_terrain.MoveEnemyVertically(
                        level,
                        place,
                        state.Velocity.Y.ToSixteenSixteenDelta()))
                {
                    ReverseRioVerticalVelocityAndBounce(state);
                    return;
                }

                state.Velocity = state.Velocity.WithY(state.Velocity.Y - RioLaunchDefinitions.RioGravity);
                if (state.Velocity.Y.IsNegative)
                {
                    state.SavedHorizontalVelocity = state.Velocity.X;
                    state.Velocity = new EightEightVector<RoomPixels>(SignedEightEight.Zero, SignedEightEight.Zero);
                    state.Action = RioAction.HoveringTowardSamus;
                }
                else if (state.AnimationFinished)
                {
                    state.AnimationFinished = false;
                    StartRioAnimation(
                        place,
                        state,
                        RioSegment.SwoopLoop);
                }
                return;

            case RioAction.BouncingBackToPerch:
                if (_terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(
                        level,
                        place,
                        state.Velocity.X.ToSixteenSixteenDelta()))
                {
                    state.Velocity = state.Velocity.ReversedX();
                }
                if (_terrain.MoveEnemyVertically(
                        level,
                        place,
                        state.Velocity.Y.ToSixteenSixteenDelta()))
                {
                    StartRioAnimation(
                        place,
                        state,
                        RioSegment.SwoopCooldown);
                    state.Action = RioAction.WaitingForLandingAnimation;
                }
                else
                {
                    state.Velocity = state.Velocity.WithY(state.Velocity.Y - RioLaunchDefinitions.RioGravity);
                }
                return;

            case RioAction.HoveringTowardSamus:
                // Once Rio reaches Samus's Y coordinate from above, it restores the saved
                // horizontal dive speed and starts the upward/bounce phase at -1/256 px.
                if (CoordinateAxis.Difference(place.Position.Y.Whole, samus.Position.Y.Whole) >= 0)
                {
                    state.Velocity = new EightEightVector<RoomPixels>(
                        state.SavedHorizontalVelocity,
                        RioLaunchDefinitions.ReturnYVelocity);
                    state.Action = RioAction.BouncingBackToPerch;
                    return;
                }

                TrigonometryAngle angle = EnemyMath.CalculateBearing(
                    new(CoordinateAxis.Difference(samus.Position.X.Whole, place.Position.X.Whole), CoordinateAxis.Difference(samus.Position.Y.Whole, place.Position.Y.Whole))).ToMirroredTrigonometryAngle();
                state.Velocity = state.Velocity.WithX(HoverVelocity(
                    angle + TrigonometryAngle.QuarterTurn));
                state.Velocity = state.Velocity.WithY(HoverVelocity(angle));
                _terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(
                    level,
                    place,
                    state.Velocity.X.ToSixteenSixteenDelta());
                _terrain.MoveEnemyVertically(
                    level,
                    place,
                    state.Velocity.Y.ToSixteenSixteenDelta());
                return;

            default:
                throw new InvalidDataException(
                    $"Rio has no behaviour for action {state.Action}.");
        }
    }

    /// <summary>Carries out one cue of the Rio's animations: telling main AI the animation has finished.</summary>
    private static void ApplyRioCue(RioEnemyState state, RioCue cue)
    {
        switch (cue.Kind)
        {
            case RioCueKind.AnimationFinished:
                state.AnimationFinished = true;
                break;
            default:
                throw new InvalidOperationException($"Undefined {nameof(RioCueKind)} {cue.Kind}.");
        }
    }

    /// <summary>Starts an animation only when it differs from the one playing (#2145).</summary>
    private static void StartRioAnimation(
        RoomEnemyPlace place,
        RioEnemyState state,
        RioSegment segment) =>
        state.Playback = EnemyAnimationPlayback.StartUnlessPlaying(place, state.Playback, RioAnimations.StartingWith(segment));

    /// <summary>Reverses vertical velocity after collision and enters the rebound phase.</summary>
    private static void ReverseRioVerticalVelocityAndBounce(RioEnemyState state)
    {
        state.Velocity = state.Velocity.ReversedY();
        state.Action = RioAction.BouncingBackToPerch;
    }

    /// <summary>
    /// The Rio's hover velocity along <paramref name="angle"/>'s sine: one pixel per update at its peak, the sine itself
    /// with no speed of its own, unlike Rinka's.
    /// </summary>
    internal static SignedEightEight HoverVelocity(TrigonometryAngle angle) =>
        EnemyMath.SineVelocity(angle, 1);

    /// <summary>Runs Rio's main AI.</summary>
    internal void RunRioMainAi(RoomEnemyPlace place, IEnemyAiFrame frame)
    {
        RunRioMain(
            place,
            place.RequireSpeciesState<RioEnemyState>(),
            frame.Samus,
            frame.Level,
            frame.Camera);
    }

    /// <summary>Advances Rio's typed animation.</summary>
    internal static void AdvanceRioTypedAnimation(RoomEnemyPlace place, EnemyAnimationCueContext _) =>
        EnemyAnimationPlayback.Advance(place, RioAnimations.Segments,
            static (RioEnemyState state) => state.Playback, static (state, playback) => state.Playback = playback,
            (state, cue) =>
            {
                ApplyRioCue(state, cue);
                return null;
            });
}
