using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Rios.Artwork;
using System.Diagnostics.CodeAnalysis;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Enemies.Rios;

/// <summary>The Rio species owner: its per-room state, AI routines and the interactions it registers.</summary>
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
            InstalledProgram = null,
            Function = RioEnemyFunction.WaitingForSamus,
        };
        slot.SetSpeciesState(state);

        // The Rio's setup writes the list directly rather than calling the animation installer. The installed animation must
        // therefore remain none until the first behavioral transition installs a new list.
        state.Playback = new(RioAnimations.StartingWith(RioProgram.Idle), AnimationPlace.StartOfAnimation);
        slot.FrameHold = WrappingCountdown.Start(1);
        slot.Timer = WrappingCountdown.Start(0);
    }

    /// <summary>Runs the Rio's main AI and its five functions.</summary>
    private void RunRioMain(
        RoomEnemySlot slot,
        RioEnemyState state,
        SamusState? samus,
        RoomLevelData? level,
        Position16<RoomPixels> camera)
    {
        // The Rio's main behaviour advances the global enemy RNG even though this particular family never
        // consumes the returned word. Omitting that apparently-useless call changes every
        // later random enemy in the room, so it is part of Rio's observable behavior.
        _room.Random.Generate();

        switch (state.Function)
        {
            case RioEnemyFunction.WaitingForSamus:
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
                InstallRioAnimationScript(
                    slot,
                    state,
                    RioProgram.SwoopingPart1);
                state.Function = RioEnemyFunction.Diving;

                // The game's on-screen check returns zero for an on-screen origin. The
                // branch consequently queues the cry only for the visible attack start.
                if (!EnemyProcessingWindow.EnemyWithNormalSpritesIsOffScreen(slot, camera))
                    _audio.QueueEnemySound(EnemySoundEffectDefinitions.RioDiveSound, priority: SoundRequestPriority.Normal);
                return;

            case RioEnemyFunction.WaitingForLandingAnimation:
                if (!state.AnimationFinished)
                    return;
                state.AnimationFinished = false;
                InstallRioAnimationScript(
                    slot,
                    state,
                    RioProgram.PostSwoopIdle);
                state.Function = RioEnemyFunction.WaitingForSamus;
                return;

            case RioEnemyFunction.Diving:
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
                    state.Function = RioEnemyFunction.HoveringTowardSamus;
                }
                else if (state.AnimationFinished)
                {
                    state.AnimationFinished = false;
                    InstallRioAnimationScript(
                        slot,
                        state,
                        RioProgram.SwoopingPart2);
                }
                return;

            case RioEnemyFunction.BouncingBackToPerch:
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
                    InstallRioAnimationScript(
                        slot,
                        state,
                        RioProgram.SwoopCooldown);
                    state.Function = RioEnemyFunction.WaitingForLandingAnimation;
                }
                else
                {
                    state.Velocity = state.Velocity.WithY(state.Velocity.Y - RioGravityStep);
                }
                return;

            case RioEnemyFunction.HoveringTowardSamus:
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
                    state.Function = RioEnemyFunction.BouncingBackToPerch;
                    return;
                }

                TrigonometryAngle angle = EnemyMath.CalculateBearing(
                    new(CoordinateAxis.Difference(samus.Position.X.Whole, slot.Position.X.Whole), CoordinateAxis.Difference(samus.Position.Y.Whole, slot.Position.Y.Whole))).ToMirroredTableAngle();
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
                    $"Rio function {state.Function?.ToString() ?? "none"} is not translated.");
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
    private static void InstallRioAnimationScript(
        RoomEnemySlot slot,
        RioEnemyState state,
        RioProgram program) =>
        EnemyProgramAnimation.InstallOnce(slot, state.InstalledProgram, program, installed =>
        {
            state.InstalledProgram = installed;
            state.Playback = new(RioAnimations.StartingWith(installed), AnimationPlace.StartOfAnimation);
        });

    /// <summary>Reverses vertical velocity after collision and enters the rebound phase.</summary>
    private static void ReverseRioVerticalVelocityAndBounce(RioEnemyState state)
    {
        state.Velocity = state.Velocity.ReversedY();
        state.Function = RioEnemyFunction.BouncingBackToPerch;
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

    /// <summary>Ports Rio's main AI.</summary>
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
    internal static void AdvanceRioTypedAnimation(RoomEnemySlot slot, EnemyInstructionContext _) =>
        EnemyProgramAnimation.Advance(slot, RioAnimations.Segments, RioFrameDefinitions.ShownFrames.Of,
            static (RioEnemyState state) => state.Playback, static (state, playback) => state.Playback = playback,
            (state, cue) =>
            {
                ApplyRioCue(state, cue);
                return null;
            });
}
