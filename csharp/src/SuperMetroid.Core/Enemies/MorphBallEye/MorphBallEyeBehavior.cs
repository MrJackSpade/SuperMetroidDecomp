using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Enemies.MorphBallEye;

/// <summary>The Morph Ball eye species owner: its per-room state, behaviour and the interactions it registers.</summary>
[RoomLifetime]
internal sealed class MorphBallEyeBehavior : IEnemySpecies, IEnemySettingsFactory<MorphBallEyeBodySpawnSettings>, IEnemySettingsFactory<MorphBallEyeMountSpawnSettings>
{
    /// <summary>Receives the species' sound requests, each queued as the species makes it.</summary>
    private readonly EnemyAudioQueue _audio;

    /// <summary>Builds the species owner on the room's enemy audio queue.</summary>
    internal MorphBallEyeBehavior(EnemyAudioQueue audio) => _audio = audio;

    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.MorphBallEye];

    /// <inheritdoc/>
    void IEnemySpeciesFactory.RegisterIn(EnemySpeciesRegistry registry)
    {
        Ensure.NotNull(registry).Add<MorphBallEyeBodySpawnSettings>(this);
        registry.Add<MorphBallEyeMountSpawnSettings>(this);
    }

    /// <summary>The eye itself, facing the way its placement names.</summary>
    public IEnemyEntity Create(RoomEnemyPlace place, EnemyCommonParticipation common, EnemySetup setup, MorphBallEyeBodySpawnSettings settings) =>
        new MorphBallEyeEntity(this, place, common, settings);

    /// <summary>The eye's mount, on the side its placement names.</summary>
    public IEnemyEntity Create(RoomEnemyPlace place, EnemyCommonParticipation common, EnemySetup setup, MorphBallEyeMountSpawnSettings settings) =>
        new MorphBallEyeMountEntity(this, place, common, settings);

    /// <summary>The room-global beam an eye body casts.</summary>
    public MorphBallEyeBeamState MorphBallEyeBeam { get; } = new();

    /// <summary>Initializes a closed eye body from its spawn facing.</summary>
    internal static MorphBallEyeEnemyState InitializeMorphBallEyeBody(RoomEnemyPlace place, MorphBallEyeBodySpawnSettings settings)
    {
        var state = new MorphBallEyeEnemyState(MorphBallEyeAction.WaitForSamus, settings);
        ShowNothingUntilAnimated(place);

        // The placement's facing is fixed by its mounting, not a live facing test.
        state.Playback = new(MorphBallEyeAnimations.StartingWith(settings.Facing == HorizontalDirection.Right
            ? MorphBallEyeSegment.FacingRightClosed
            : MorphBallEyeSegment.FacingLeftClosed), AnimationPlace.StartOfAnimation);
        return state;
    }

    /// <summary>Offsets a beam mount to its side of the eye, starts its pose and resets the room-global beam.</summary>
    internal MorphBallEyeMountState InitializeMorphBallEyeMount(RoomEnemyPlace place, MorphBallEyeMountSpawnSettings settings)
    {
        var state = new MorphBallEyeMountState();
        ShowNothingUntilAnimated(place);

        var mount = MorphBallEyeGeometry.Mount(settings.Side);
        place.Position += mount.Offset;
        state.Playback = new(MorphBallEyeAnimations.StartingWith(mount.Animation), AnimationPlace.StartOfAnimation);

        // A mount clears any beam when the room loads, so a prior room's reveal never
        // shows in this one.
        MorphBallEyeBeam.Reset();
        return state;
    }

    /// <summary>
    /// Readies a newly placed eye body or mount to animate: it shows no frame until its animation's first step, which the
    /// restarted clock takes on its first update.
    /// </summary>
    private static void ShowNothingUntilAnimated(RoomEnemyPlace place)
    {
        place.Processing = place.Processing.With(EnemyProcessing.Animates);
        place.Frame = EnemyFrameId.Empty;
        place.AnimationClock.Restart();
    }

    /// <summary>Runs the body state machine only after Morph Ball is collected, preserving activation and tracking independently of equipment selection.</summary>
    private void RunMorphBallEyeMain(
        RoomEnemyPlace place,
        MorphBallEyeEnemyState state,
        SamusState samus)
    {
        // The game tests the collected items, not the equipped items. This
        // matters after the player disables Morph Ball in the equipment menu.
        if (!samus.CollectedItems.HasAny(SamusEquipmentFlags.MorphBall))
            return;

        switch (state.Action)
        {
            case MorphBallEyeAction.WaitForSamus:
                WaitForSamusNearMorphBallEye(place, state, samus);
                return;
            case MorphBallEyeAction.Activating:
                AdvanceMorphBallEyeActivation(place, state, samus);
                return;
            case MorphBallEyeAction.Active:
                TrackSamusWithMorphBallEye(place, state, samus);
                return;
            case MorphBallEyeAction.Deactivating:
                AdvanceMorphBallEyeDeactivation(state);
                return;
            default:
                throw new InvalidDataException(
                    $"Morph-ball eye has no behaviour for action {state.Action}.");
        }
    }

    /// <summary>Starts the eye's opening once Samus is closer than its opening reach on both axes.</summary>
    private static void WaitForSamusNearMorphBallEye(
        RoomEnemyPlace eye,
        MorphBallEyeEnemyState state,
        SamusState samus)
    {
        if (Math.Abs(samus.Position.Y.Whole - eye.Position.Y.Whole) >= MorphBallEyeWatchDefinitions.OpeningReachY ||
            Math.Abs(samus.Position.X.Whole - eye.Position.X.Whole) >= MorphBallEyeWatchDefinitions.OpeningReachX)
        {
            return;
        }

        state.ActionTimer = UpdateCountdown.For(MorphBallEyeWatchDefinitions.TransitionDuration);
        eye.AnimationClock.StepOnNextUpdate();
        state.Playback = new(MorphBallEyeAnimations.StartingWith(state.Settings.Facing == HorizontalDirection.Right
            ? MorphBallEyeSegment.FacingRightActivating
            : MorphBallEyeSegment.FacingLeftActivating), AnimationPlace.StartOfAnimation);
        state.Action = MorphBallEyeAction.Activating;
    }

    /// <summary>Counts the opening down, then sounds activation, requests the beam, and captures Samus's angle.</summary>
    private void AdvanceMorphBallEyeActivation(
        RoomEnemyPlace eye,
        MorphBallEyeEnemyState state,
        SamusState samus)
    {
        // The opening lasts its whole duration: the update that takes the timer to zero opens the eye (#2340).
        if (!state.ActionTimer.StepEnds())
            return;

        _audio.QueueEnemySound(EnemySoundEffectDefinitions.EyeActivationSound, priority: SoundRequestPriority.Normal);
        RequestMorphBallEyeBeam(eye, state);
        state.Action = MorphBallEyeAction.Active;
        state.Angle = CalculateMorphBallEyeAngle(eye, samus);
    }

    /// <summary>
    /// Looks at Samus in one of its fourteen open poses (<see cref="MorphBallEyeAnimations.AimFrames"/>), or starts closing
    /// once she leaves the wider reach on either axis.
    /// </summary>
    private void TrackSamusWithMorphBallEye(
        RoomEnemyPlace eye,
        MorphBallEyeEnemyState state,
        SamusState samus)
    {
        bool inProximity =
            Math.Abs(samus.Position.Y.Whole - eye.Position.Y.Whole) < MorphBallEyeWatchDefinitions.StayOpenReachY &&
            Math.Abs(samus.Position.X.Whole - eye.Position.X.Whole) < MorphBallEyeWatchDefinitions.StayOpenReachX;
        if (!inProximity)
        {
            _audio.QueueEnemySound(EnemySoundEffectDefinitions.EyeRaySoundStop, priority: SoundRequestPriority.Normal);
            state.IsActivated = false;
            state.ActionTimer = UpdateCountdown.For(MorphBallEyeWatchDefinitions.TransitionDuration);
            state.Aim = null;
            state.Playback = new(MorphBallEyeAnimations.StartingWith(state.Settings.Facing == HorizontalDirection.Right
                ? MorphBallEyeSegment.FacingRightDeactivating
                : MorphBallEyeSegment.FacingLeftDeactivating), AnimationPlace.StartOfAnimation);
            state.Action = MorphBallEyeAction.Deactivating;
            eye.AnimationClock.StepOnNextUpdate();
            return;
        }

        state.Angle = CalculateMorphBallEyeAngle(eye, samus);

        // The eye looks along the pose for the arc of the turn the angle lies in, which its animation step shows on this
        // same update.
        state.Aim = MorphBallEyeAnimations.AimFrames.At(state.Angle);
        eye.AnimationClock.StepOnNextUpdate();
    }

    /// <summary>Returns to waiting on the update the closing's duration runs out (#2340).</summary>
    private static void AdvanceMorphBallEyeDeactivation(MorphBallEyeEnemyState state)
    {
        if (state.ActionTimer.StepEnds())
            state.Action = MorphBallEyeAction.WaitForSamus;
    }

    /// <summary>The eye's bearing to Samus, from her distance on each axis.</summary>
    private static RotationAngle CalculateMorphBallEyeAngle(RoomEnemyPlace eye, SamusState samus) =>
        // The eye's beam measures its direction clockwise from straight up, as its window cone does.
        (TrigonometryAngle.QuarterTurn - EnemyMath.CalculateAimAngle(
            new(samus.Position.X.Whole - eye.Position.X.Whole, samus.Position.Y.Whole - eye.Position.Y.Whole))).ToAngle();

    /// <summary>
    /// Schedules the room-global beam to appear next frame, following <paramref name="eye"/>, the eye that opened (#2342):
    /// every room that places the eye places one beam mount and one eye, so the eye that opens is the eye the game's beam
    /// follows.
    /// </summary>
    private void RequestMorphBallEyeBeam(RoomEnemyPlace eye, MorphBallEyeEnemyState state)
    {
        MorphBallEyeBeam.Reset();
        MorphBallEyeBeam.Caster = new MorphBallEyeBeamCaster(eye, state);
        MorphBallEyeBeam.Phase = MorphBallEyeBeamPhase.PendingInitialization;
    }

    /// <summary>
    /// Advances the beam before enemy AI, as the game advances it ahead of the enemies. A beam
    /// spawned by this frame's eye AI consequently initializes on the following frame.
    /// </summary>
    internal void StepMorphBallEyeBeam()
    {
        // No beam exists until an eye requests one, and none once its fade removes it.
        if (MorphBallEyeBeam.Caster is not { } caster)
            return;
        switch (MorphBallEyeBeam.Phase)
        {
            case MorphBallEyeBeamPhase.Inactive:
                return;
            case MorphBallEyeBeamPhase.PendingInitialization:
                StartMorphBallEyeBeam(caster.Eye);
                return;
            case MorphBallEyeBeamPhase.Widening:
                WidenMorphBallEyeBeam();
                return;
            case MorphBallEyeBeamPhase.Full:
                StepFullMorphBallEyeBeam(caster.Eye);
                return;
            case MorphBallEyeBeamPhase.Deactivating:
                FadeMorphBallEyeBeam();
                return;
            default:
                throw new InvalidDataException(
                    $"Unknown morph-ball eye beam phase {MorphBallEyeBeam.Phase}.");
        }
    }

    /// <summary>
    /// Seeds the beam's colour, then marks its body activated. The request a step earlier reset the beam to no width, no
    /// widening and its first colour (#2474), which nothing changes before this step.
    /// </summary>
    private void StartMorphBallEyeBeam(MorphBallEyeEnemyState state)
    {

        MorphBallEyeBeam.Color = MorphBallEyeBeamDefinitions.InitialColor;
        state.IsActivated = true;
        MorphBallEyeBeam.Phase = MorphBallEyeBeamPhase.Widening;
    }

    /// <summary>
    /// Accelerates the beam's angular half-width by a quarter step per update until its whole steps reach four;
    /// that update sets the whole steps to four, keeping the fraction, and the beam is full.
    /// </summary>
    private void WidenMorphBallEyeBeam()
    {
        MorphBallEyeBeam.AngularHalfWidthGrowth += MorphBallEyeBeamDefinitions.WidthGrowthStep;
        MorphBallEyeBeam.AngularHalfWidth += MorphBallEyeBeam.AngularHalfWidthGrowth;
        RotationAngle width = MorphBallEyeBeam.AngularHalfWidth;
        if (width >= MorphBallEyeBeamDefinitions.FullHalfWidth)
        {
            MorphBallEyeBeam.AngularHalfWidth = width.WithWholeStepsOf(MorphBallEyeBeamDefinitions.FullHalfWidth);
            MorphBallEyeBeam.Phase = MorphBallEyeBeamPhase.Full;
        }
    }

    /// <summary>Cycles the sixteen beam colors while the body stays activated, otherwise begins fading.</summary>
    internal void StepFullMorphBallEyeBeam(MorphBallEyeEnemyState bodyState)
    {
        if (!bodyState.IsActivated)
        {
            MorphBallEyeBeam.Phase = MorphBallEyeBeamPhase.Deactivating;
            return;
        }

        MorphBallEyeBeam.Color = MorphBallEyeBeam.NextColour.Colour;
        MorphBallEyeBeam.NextColour = MorphBallEyeBeam.NextColour.Next;
    }

    /// <summary>Darkens the beam's colour one step and removes the beam once green has faded to black.</summary>
    private void FadeMorphBallEyeBeam()
    {
        // The beam fade tests green before updating the cone or darkening any channel. Once green
        // has reached zero intensity, the beam is removed on this pass, its colour black.
        if (MorphBallEyeBeam.GreenFaded)
        {
            MorphBallEyeBeam.Reset();
            return;
        }

        MorphBallEyeBeam.FadeOneStep();
    }

    /// <summary>Advances a beam mount's typed animation: its mount pose, which raises no cue.</summary>
    internal static void AdvanceMorphBallEyeMountTypedAnimation(RoomEnemyPlace place, MorphBallEyeMountState morphBallEyeMountState) =>
        EnemyAnimationPlayback.Advance(place, morphBallEyeMountState, MorphBallEyeAnimations.Segments,
            static (MorphBallEyeMountState state) => state.Playback, static (state, playback) => state.Playback = playback,
            static (state, cue) => throw new InvalidOperationException($"The Morph Ball eye mount's cue-free animation raised {cue}."));

    /// <summary>
    /// Advances the eye body's typed animation: a tracking eye shows the pose its aim chose this update for good, until the
    /// next update's tracking chooses again; otherwise its eyelid animation plays.
    /// </summary>
    internal static void AdvanceMorphBallEyeTypedAnimation(RoomEnemyPlace place, MorphBallEyeEnemyState morphBallEyeEnemyState, EnemyAnimationCueContext _)
    {
        if (morphBallEyeEnemyState.Aim is { } aim)
        {
            if (!place.AnimationClock.Expire())
                return;
            place.Frame = aim;
            place.AnimationClock.ShowForGood();
            return;
        }
        EnemyAnimationPlayback.Advance(place, morphBallEyeEnemyState, MorphBallEyeAnimations.Segments,
            static (MorphBallEyeEnemyState state) => state.Playback, static (state, playback) => state.Playback = playback,
            static (state, cue) => throw new InvalidOperationException($"The Morph Ball eye's cue-free animation raised {cue}."));
    }

    /// <summary>Runs MorphBallEye's main AI.</summary>
    internal void RunMorphBallEyeMainAi(RoomEnemyPlace place, MorphBallEyeEnemyState state, IEnemyAiFrame frame)
    {
        RunMorphBallEyeMain(place, state, frame.Samus);
    }
}
