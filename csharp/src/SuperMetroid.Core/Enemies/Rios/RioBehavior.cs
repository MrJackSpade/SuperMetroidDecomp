using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Enemies.Rios;

/// <summary>The Rio species owner: its per-room state, behaviour and the interactions it registers.</summary>
[RoomLifetime]
internal sealed class RioBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <inheritdoc/>
    public IReadOnlyDictionary<EnemyDefinitionId, EnemyEntityCreation> Creations => field ??= EnemyEntityCreations.Each([EnemyDefinitionId.Rio], Create);

    /// <summary>Creates the entity of a newly installed occupant of <paramref name="place"/>, with the setup it has.</summary>
    /// <param name="place">The place, holding the new identity and the entry it is spawned from.</param>
    /// <param name="common">The shared participation the entity composes.</param>
    private EnemyOccupant Create(RoomEnemyPlace place, EnemyCommonParticipation common) => EnemyOccupant.Of(new RioEntity(this, place, common));

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
        var state = new RioEnemyState(RioAction.WaitingForSamus);
        place.SetSpeciesState(state);

        // The Rio rests in its idle animation until its first swoop starts another.
        state.Playback = EnemyAnimationPlayback.Start(place, RioAnimations.StartingWith(RioSegment.TuckedWingbeat));
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
                if (Math.Abs(samus.Position.X.Whole - place.Position.X.Whole) >= RioLaunchDefinitions.RioTriggerDistance)
                {
                    return;
                }

                state.Velocity = new PixelVelocity<RoomPixels>(
                    RioLaunchDefinitions.RioXVelocity.Toward(RioLaunches.SideOfSamus(place, samus)),
                    RioLaunchDefinitions.RioYVelocity);
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
                if (!state.Finish.Take())
                    return;
                state.Playback = EnemyAnimationPlayback.StartUnlessPlaying(place, state.Playback, RioAnimations.PostSwoopHover);
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
                    state.Velocity = new PixelVelocity<RoomPixels>(SignedEightEight.Zero, SignedEightEight.Zero);
                    state.Action = RioAction.HoveringTowardSamus;
                }
                else if (state.Finish.Take())
                {
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
                // Once Rio reaches Samus's height from above, it takes back its saved sideways dive speed and starts
                // its climb back to its perch at its return speed.
                if (place.Position.Y.Whole >= samus.Position.Y.Whole)
                {
                    state.Velocity = new PixelVelocity<RoomPixels>(
                        state.SavedHorizontalVelocity,
                        RioLaunchDefinitions.ReturnYVelocity);
                    state.Action = RioAction.BouncingBackToPerch;
                    return;
                }

                TrigonometryAngle angle = EnemyMath.CalculateAimAngle(
                    new(samus.Position.X.Whole - place.Position.X.Whole, samus.Position.Y.Whole - place.Position.Y.Whole));
                state.Velocity = state.Velocity.WithX(HoverVelocity(angle));
                state.Velocity = state.Velocity.WithY(HoverVelocity(angle + TrigonometryAngle.QuarterTurn));
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
                state.Finish.Raise();
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
    private static SignedEightEight HoverVelocity(TrigonometryAngle angle) =>
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
    internal static void AdvanceRioTypedAnimation(RoomEnemyPlace place) =>
        EnemyAnimationPlayback.Advance(place, RioAnimations.Segments,
            static (RioEnemyState state) => state.Playback, static (state, playback) => state.Playback = playback,
            (state, cue) =>
            {
                ApplyRioCue(state, cue);
                return null;
            });
}
