using SuperMetroid.Core.Animation;
using System.Diagnostics.CodeAnalysis;
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
    public IEnemyEntity Create(RoomEnemySlot slot, EnemyCommonParticipation common) => new RioEntity(this, slot, common);

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

    /// <summary>Strict horizontal Samus proximity threshold that starts a dive.</summary>
    private const int RioTriggerDistance = 160;
    /// <summary>Vertical velocity decrement applied during dive and rebound, 24 256ths of a pixel per update.</summary>
    private static readonly SignedEightEight RioGravityStep = SignedEightEight.FromParts(0, 24);

    /// <summary>Initializes the Rio.</summary>
    internal static void InitializeRio(RoomEnemySlot slot)
    {
        var state = new RioEnemyState
        {
            AnimationFinished = false,
            InstalledAnimation = null,
            Action = RioAction.WaitingForSamus,
        };
        slot.SetSpeciesState(state);

        // The Rio's setup writes the animation directly rather than calling the animation starter. The installed animation must
        // therefore remain none until the first behavioral transition starts a new animation.
        state.Playback = EnemyAnimationPlayback.Start(slot, RioAnimations.StartingWith(RioSegment.Idle));
    }

    /// <summary>Runs the Rio's main AI and its five actions.</summary>
    private void RunRioMain(
        RoomEnemySlot slot,
        RioEnemyState state,
        SamusState? samus,
        RoomLevelData? level,
        Position16<RoomPixels> camera)
    {
        // The Rio's main behaviour advances the global enemy RNG even though this particular family never
        // consumes the returned number. Omitting that apparently-useless call changes every
        // later random enemy in the room, so it is part of Rio's observable behavior.
        _room.Random.Generate();

        switch (state.Action)
        {
            case RioAction.WaitingForSamus:
                if (samus is null || !EnemyMath.IsWithinStrictModularDistance(
                        samus.Position.X.Whole,
                        slot.Position.X.Whole,
                        RioTriggerDistance))
                {
                    return;
                }

                state.Velocity = new EightEightVector<RoomPixels>(
                    RioLaunchDefinitions.RioXVelocity,
                    RioLaunchDefinitions.RioYVelocity);
                if (CoordinateAxis.Difference(samus.Position.X.Whole, slot.Position.X.Whole) < 0)
                    state.Velocity = state.Velocity.ReversedX();
                StartRioAnimation(
                    slot,
                    state,
                    RioSegment.SwoopOpening);
                state.Action = RioAction.Diving;

                // The game's on-screen check returns zero for an on-screen origin. The
                // branch consequently queues the cry only for the visible attack start.
                if (EnemyProcessingWindow.IsWithinProcessingWindow(slot, camera))
                    _audio.QueueEnemySound(EnemySoundEffectDefinitions.RioDiveSound, priority: SoundRequestPriority.Normal);
                return;

            case RioAction.WaitingForLandingAnimation:
                if (!state.AnimationFinished)
                    return;
                state.AnimationFinished = false;
                StartRioAnimation(
                    slot,
                    state,
                    RioSegment.PostSwoopIdle);
                state.Action = RioAction.WaitingForSamus;
                return;

            case RioAction.Diving:
                RequireRioLevel(level);
                if (_terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(
                        level,
                        slot,
                        state.Velocity.X.ToSixteenSixteenDelta()))
                {
                    state.Velocity = state.Velocity.ReversedX();
                    ReverseRioVerticalVelocityAndBounce(state);
                    return;
                }
                if (_terrain.MoveEnemyVertically(
                        level,
                        slot,
                        state.Velocity.Y.ToSixteenSixteenDelta()))
                {
                    ReverseRioVerticalVelocityAndBounce(state);
                    return;
                }

                state.Velocity = state.Velocity.WithY(state.Velocity.Y - RioGravityStep);
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
                        slot,
                        state,
                        RioSegment.SwoopLoop);
                }
                return;

            case RioAction.BouncingBackToPerch:
                RequireRioLevel(level);
                if (_terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(
                        level,
                        slot,
                        state.Velocity.X.ToSixteenSixteenDelta()))
                {
                    state.Velocity = state.Velocity.ReversedX();
                }
                if (_terrain.MoveEnemyVertically(
                        level,
                        slot,
                        state.Velocity.Y.ToSixteenSixteenDelta()))
                {
                    StartRioAnimation(
                        slot,
                        state,
                        RioSegment.SwoopCooldown);
                    state.Action = RioAction.WaitingForLandingAnimation;
                }
                else
                {
                    state.Velocity = state.Velocity.WithY(state.Velocity.Y - RioGravityStep);
                }
                return;

            case RioAction.HoveringTowardSamus:
                if (samus is null)
                    throw new InvalidOperationException("Rio homing requires the active Samus actor.");
                RequireRioLevel(level);

                // Once Rio reaches Samus's Y coordinate from above, it restores the saved
                // horizontal dive speed and starts the upward/bounce phase at -1/256 px.
                if (CoordinateAxis.Difference(slot.Position.Y.Whole, samus.Position.Y.Whole) >= 0)
                {
                    state.Velocity = new EightEightVector<RoomPixels>(
                        state.SavedHorizontalVelocity,
                        -SignedEightEight.FromParts(0, 1));
                    state.Action = RioAction.BouncingBackToPerch;
                    return;
                }

                TrigonometryAngle angle = EnemyMath.CalculateBearing(
                    new(CoordinateAxis.Difference(samus.Position.X.Whole, slot.Position.X.Whole), CoordinateAxis.Difference(samus.Position.Y.Whole, slot.Position.Y.Whole))).ToMirroredTrigonometryAngle();
                state.Velocity = state.Velocity.WithX(ReadRioSignedSineCosineSample(
                    angle.TurnedBySteps(EnemyTrigonometryAngleDefinitions.QuarterTurn)));
                state.Velocity = state.Velocity.WithY(ReadRioSignedSineCosineSample(angle));
                _terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(
                    level,
                    slot,
                    state.Velocity.X.ToSixteenSixteenDelta());
                _terrain.MoveEnemyVertically(
                    level,
                    slot,
                    state.Velocity.Y.ToSixteenSixteenDelta());
                return;

            default:
                throw new InvalidDataException(
                    $"Rio has no behaviour for action {state.Action?.ToString() ?? "none"}.");
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

    /// <summary>Installs an animation only when it actually changes.</summary>
    private static void StartRioAnimation(
        RoomEnemySlot slot,
        RioEnemyState state,
        RioSegment segment) =>
        EnemyAnimationPlayback.StartOnce(slot, state.InstalledAnimation, segment, installed =>
        {
            state.InstalledAnimation = installed;
            state.Playback = new(RioAnimations.StartingWith(installed), AnimationPlace.StartOfAnimation);
        });

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
    internal static SignedEightEight ReadRioSignedSineCosineSample(TrigonometryAngle angle) =>
        EnemyMath.SineVelocity(angle, 1);

    /// <summary>Rejects movement dispatch without active room collision data.</summary>
    private static void RequireRioLevel([NotNull] RoomLevelData? level)
    {
        if (level is null)
            throw new InvalidOperationException("Rio movement requires the current room level data.");
    }

    /// <summary>Runs Rio's main AI.</summary>
    internal void RunRioMainAi(RoomEnemySlot slot, IEnemyAiFrame frame)
    {
        RunRioMain(
            slot,
            slot.RequireSpeciesState<RioEnemyState>(),
            frame.Samus,
            frame.Level,
            frame.Camera);
    }

    /// <summary>Advances Rio's typed animation.</summary>
    internal static void AdvanceRioTypedAnimation(RoomEnemySlot slot, EnemyAnimationCueContext _) =>
        EnemyAnimationPlayback.Advance(slot, RioAnimations.Segments,
            static (RioEnemyState state) => state.Playback, static (state, playback) => state.Playback = playback,
            (state, cue) =>
            {
                ApplyRioCue(state, cue);
                return null;
            });
}
