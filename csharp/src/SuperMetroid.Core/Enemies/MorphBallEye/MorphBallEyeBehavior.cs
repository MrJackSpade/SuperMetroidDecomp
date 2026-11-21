using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Enemies.MorphBallEye;

/// <summary>The Morph Ball eye species owner: its per-room state, behaviour and the interactions it registers.</summary>
[RoomLifetime]
internal sealed class MorphBallEyeBehavior : IEnemySpecies, IEnemySpawnFactory<MorphBallEyeSpawnSettings>
{
    /// <summary>Receives the species' sound requests, each queued as the species makes it.</summary>
    private readonly EnemyAudioQueue _audio;

    /// <summary>Builds the species owner on the room's enemy audio queue.</summary>
    internal MorphBallEyeBehavior(EnemyAudioQueue audio) => _audio = audio;

    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.MorphBallEye];

    /// <summary>The eye's mount or its body, as the placement's settings name the part it is.</summary>
    /// <exception cref="InvalidOperationException">The placement names no part of the eye.</exception>
    public EnemyOccupant Create(RoomEnemyOccupant occupant, MorphBallEyeSpawnSettings settings, RoomEnemyPlace place, EnemyCommonParticipation common) =>
        settings switch
        {
            MorphBallEyeMountSpawnSettings mount => EnemyOccupant.PlacedOnly(new MorphBallEyeMountEntity(this, occupant, place, common, mount)),
            MorphBallEyeBodySpawnSettings body => EnemyOccupant.Running(new MorphBallEyeEntity(this, occupant, place, common, body)),
            var part => throw new InvalidOperationException($"Undefined Morph Ball eye part {part}."),
        };

    /// <summary>The room-global beam an eye body casts.</summary>
    public MorphBallEyeBeamState MorphBallEyeBeam { get; } = new();

    /// <summary>Initializes a closed eye body from its spawn facing.</summary>
    /// <returns>The state the enemy holds from its setup.</returns>
    internal static MorphBallEyeEnemyState InitializeMorphBallEyeBody(MorphBallEyeBodySpawnSettings morphBallEyeBodySpawnSettings, RoomEnemyPlace place)
    {
        var state = new MorphBallEyeEnemyState(MorphBallEyeAction.WaitForSamus);
        ShowNothingUntilAnimated(place);

        // The placement's facing is fixed by its mounting, not a live facing test.
        state.Playback = new(MorphBallEyeAnimations.StartingWith(morphBallEyeBodySpawnSettings.Facing == HorizontalDirection.Right
            ? MorphBallEyeSegment.FacingRightClosed
            : MorphBallEyeSegment.FacingLeftClosed), AnimationPlace.StartOfAnimation);
        return state;
    }

    /// <summary>Offsets a beam mount to its side of the eye, starts its pose and resets the room-global beam.</summary>
    /// <returns>The state the enemy holds from its setup.</returns>
    internal MorphBallEyeMountState InitializeMorphBallEyeMount(MorphBallEyeMountSpawnSettings morphBallEyeMountSpawnSettings, RoomEnemyPlace place)
    {
        var state = new MorphBallEyeMountState();
        ShowNothingUntilAnimated(place);

        var mount = MorphBallEyeGeometry.Mount(morphBallEyeMountSpawnSettings.Side);
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
        place.Frame = EnemyFrameId.PointAtOrigin;
        place.AnimationClock.Restart();
    }

    /// <summary>What <see cref="RunMorphBallEyeMain"/> does for each <see cref="MorphBallEyeAction"/> member, each member's own arm.</summary>
    private IdentityTable<MorphBallEyeAction, Action<MorphBallEyeBodySpawnSettings, RoomEnemyPlace, MorphBallEyeEnemyState, SamusState>> RunMorphBallEyeMainForMorphBallEyeAction => field ??= new(new Dictionary<MorphBallEyeAction, Action<MorphBallEyeBodySpawnSettings, RoomEnemyPlace, MorphBallEyeEnemyState, SamusState>>
    {
        [MorphBallEyeAction.WaitForSamus] = (settings, place, state, samus) => WaitForSamusNearMorphBallEye(settings, place, state, samus),
        [MorphBallEyeAction.Activating] = (_, place, state, samus) => AdvanceMorphBallEyeActivation(place, state, samus),
        [MorphBallEyeAction.Active] = (settings, place, state, samus) => TrackSamusWithMorphBallEye(settings, place, state, samus),
        [MorphBallEyeAction.Deactivating] = (_, place, state, samus) => AdvanceMorphBallEyeDeactivation(state),
    });

    /// <summary>Runs the body state machine only after Morph Ball is collected, preserving activation and tracking independently of equipment selection.</summary>
    private void RunMorphBallEyeMain(
        MorphBallEyeBodySpawnSettings morphBallEyeBodySpawnSettings, RoomEnemyPlace place,
        MorphBallEyeEnemyState state,
        SamusState samus)
    {
        // The game tests the collected items, not the equipped items. This
        // matters after the player disables Morph Ball in the equipment menu.
        if (!samus.CollectedItems.HasAny(SamusEquipmentFlags.MorphBall))
            return;

        RunMorphBallEyeMainForMorphBallEyeAction[state.Action](morphBallEyeBodySpawnSettings, place, state, samus);
    }

    /// <summary>Starts the eye's opening once Samus is closer than its opening reach on both axes.</summary>
    private static void WaitForSamusNearMorphBallEye(
        MorphBallEyeBodySpawnSettings morphBallEyeBodySpawnSettings, RoomEnemyPlace eye,
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
        state.Playback = new(MorphBallEyeAnimations.StartingWith(morphBallEyeBodySpawnSettings.Facing == HorizontalDirection.Right
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
        MorphBallEyeBodySpawnSettings morphBallEyeBodySpawnSettings, RoomEnemyPlace eye,
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
            state.Playback = new(MorphBallEyeAnimations.StartingWith(morphBallEyeBodySpawnSettings.Facing == HorizontalDirection.Right
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
    private void RequestMorphBallEyeBeam(RoomEnemyPlace eye, MorphBallEyeEnemyState state) => MorphBallEyeBeam.Request(eye, state);

    /// <summary>
    /// Advances the beam before enemy AI, as the game advances it ahead of the enemies. A beam
    /// spawned by this frame's eye AI consequently initializes on the following frame.
    /// </summary>
    internal void StepMorphBallEyeBeam()
    {
        // No beam exists until an eye requests one.
        if (MorphBallEyeBeam.Cast is { } cast)
            cast.Stage.Step(this, cast);
    }

    /// <summary>
    /// Seeds the beam's colour, then marks its body activated. The request a step earlier reset the beam to no width, no
    /// widening and its first colour (#2474), which nothing changes before this step.
    /// </summary>
    internal void StartMorphBallEyeBeam(MorphBallEyeBeamCast cast)
    {
        MorphBallEyeBeam.Color = MorphBallEyeBeamDefinitions.InitialColor;
        cast.Eye.IsActivated = true;
        cast.Stage = MorphBallEyeBeamStage.Widening;
    }

    /// <summary>
    /// Accelerates the beam's angular half-width by <see cref="MorphBallEyeBeamDefinitions.WidthGrowthStep"/> each update until its whole steps reach
    /// <see cref="MorphBallEyeBeamDefinitions.FullHalfWidth"/>; that update sets the whole steps to it, keeping the fraction, and the beam is full.
    /// </summary>
    internal void WidenMorphBallEyeBeam(MorphBallEyeBeamCast cast)
    {
        MorphBallEyeBeam.AngularHalfWidthGrowth += MorphBallEyeBeamDefinitions.WidthGrowthStep;
        MorphBallEyeBeam.AngularHalfWidth += MorphBallEyeBeam.AngularHalfWidthGrowth;
        RotationAngle width = MorphBallEyeBeam.AngularHalfWidth;
        if (width >= MorphBallEyeBeamDefinitions.FullHalfWidth)
        {
            MorphBallEyeBeam.AngularHalfWidth = width.WithWholeStepsOf(MorphBallEyeBeamDefinitions.FullHalfWidth);
            cast.Stage = MorphBallEyeBeamStage.Full;
        }
    }

    /// <summary>Cycles the sixteen beam colors while the body stays activated, otherwise begins fading.</summary>
    internal void StepFullMorphBallEyeBeam(MorphBallEyeBeamCast cast)
    {
        if (!cast.Eye.IsActivated)
        {
            cast.Stage = MorphBallEyeBeamStage.Deactivating;
            return;
        }

        MorphBallEyeBeam.Color = MorphBallEyeBeam.NextColour.Colour;
        MorphBallEyeBeam.NextColour = MorphBallEyeBeam.NextColour.Next;
    }

    /// <summary>Darkens the beam's colour one step and removes the beam once green has faded to black.</summary>
    internal void FadeMorphBallEyeBeam()
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
        EnemyAnimationPlayback.AdvanceSilently(place, morphBallEyeMountState, MorphBallEyeAnimations.Segments,
            static (MorphBallEyeMountState state) => state.Playback, static (state, playback) => state.Playback = playback);

    /// <summary>
    /// Advances the eye body's typed animation: a tracking eye shows the pose its aim chose this update for good, until the
    /// next update's tracking chooses again; otherwise its eyelid animation plays.
    /// </summary>
    internal static void AdvanceMorphBallEyeTypedAnimation(RoomEnemyPlace place, MorphBallEyeEnemyState morphBallEyeEnemyState)
    {
        if (morphBallEyeEnemyState.Aim is { } aim)
        {
            if (!place.AnimationClock.Expire())
                return;
            place.Frame = aim;
            place.AnimationClock.ShowForGood();
            return;
        }
        EnemyAnimationPlayback.AdvanceSilently(place, morphBallEyeEnemyState, MorphBallEyeAnimations.Segments,
            static (MorphBallEyeEnemyState state) => state.Playback, static (state, playback) => state.Playback = playback);
    }

    /// <summary>Runs MorphBallEye's main AI.</summary>
    internal void RunMorphBallEyeMainAi(MorphBallEyeBodySpawnSettings morphBallEyeBodySpawnSettings, RoomEnemyPlace place, MorphBallEyeEnemyState morphBallEyeEnemyState, IEnemyAiFrame frame)
    {
        RunMorphBallEyeMain(morphBallEyeBodySpawnSettings, place, morphBallEyeEnemyState, frame.Samus);
    }
}
