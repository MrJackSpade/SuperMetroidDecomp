using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Enemies.MorphBallEye;

/// <summary>The Morph Ball eye species owner: its per-room state, behaviour and the interactions it registers.</summary>
[RoomLifetime]
internal sealed class MorphBallEyeBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <summary>Receives the species' sound requests, each queued as the species makes it.</summary>
    private readonly EnemyAudioQueue _audio;

    /// <summary>The room's enemy places, where a beam mount finds the place listed right after it.</summary>
    private readonly EnemyPlaces _places;

    /// <summary>
    /// The place each beam mount's room lists right after it, recorded when the mount is placed (#1835). The room places
    /// the mount first, so that place is still empty then; the eye its beam follows is placed there next.
    /// </summary>
    private readonly EnemyPartLinks<RoomEnemyPlace> _followedPlaces = new();

    /// <summary>
    /// The eye the room's beam follows, whichever eye opens: the one listed right after the beam's mount, linked once both
    /// are placed, as that eye is placed after its mount (#1835). Null from a mount's placing until its eye is placed.
    /// </summary>
    internal RoomEnemyPlace? BeamFollowedEye { get; private set; }

    /// <summary>Builds the species owner on the room's enemy audio queue and enemy places.</summary>
    internal MorphBallEyeBehavior(EnemyAudioQueue audio, EnemyPlaces places)
    {
        _audio = audio;
        _places = Ensure.NotNull(places);
    }

    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.MorphBallEye];

    /// <inheritdoc/>
    public IEnemyEntity Create(RoomEnemyPlace place, EnemyCommonParticipation common) =>
        Ensure.NotNull(place).SpawnSettings<MorphBallEyeSpawnSettings>().Part == MorphBallEyePart.BeamMount
            ? new MorphBallEyeMountEntity(this, place, common)
            : new MorphBallEyeEntity(this, place, common);

    /// <summary>The room-global beam an eye body casts.</summary>
    public MorphBallEyeBeamState MorphBallEyeBeam { get; } = new();

    /// <summary>Initializes a closed eye body from its spawn facing.</summary>
    internal void InitializeMorphBallEyeBody(RoomEnemyPlace place)
    {
        var state = new MorphBallEyeEnemyState(MorphBallEyeAction.WaitForSamus);
        place.SetSpeciesState(state);
        ShowNothingUntilAnimated(place);

        // The placement's facing is fixed by its mounting, not a live facing test.
        state.Playback = new(MorphBallEyeAnimations.StartingWith(place.SpawnSettings<MorphBallEyeSpawnSettings>().Facing == HorizontalDirection.Right
            ? MorphBallEyeSegment.FacingRightClosed
            : MorphBallEyeSegment.FacingLeftClosed), AnimationPlace.StartOfAnimation);

        // The eye placed where a beam mount listed right before it found its followed place is the eye that
        // mount's beam follows.
        if (_places.ListedBeforeOrNone(place, 1) is { } before && IsPlacedBeamMount(before) && _followedPlaces.Of(before) == place)
            BeamFollowedEye = place;
    }

    /// <summary>Offsets a beam mount to its side of the eye, starts its pose and resets the room-global beam.</summary>
    internal void InitializeMorphBallEyeMount(RoomEnemyPlace place)
    {
        var state = new MorphBallEyeMountState();
        place.SetSpeciesState(state);
        ShowNothingUntilAnimated(place);

        var mount = MorphBallEyeGeometry.Mount(place.SpawnSettings<MorphBallEyeSpawnSettings>().MountSide);
        place.Position += mount.Offset;
        state.Playback = new(MorphBallEyeAnimations.StartingWith(mount.Animation), AnimationPlace.StartOfAnimation);

        // A mount clears any beam when the room loads, so a prior room's reveal never
        // shows in this one.
        MorphBallEyeBeam.Reset();

        // The beam the mount casts follows the eye its room lists right after the mount, which the room places next:
        // the mount records that place now, and the eye links the beam to itself when it is placed there.
        _followedPlaces.Place(place, _places.ListedAfter(place, 1));
        BeamFollowedEye = null;
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

    /// <summary>Whether <paramref name="place"/> holds a beam mount placed in this room, which recorded its followed place.</summary>
    private bool IsPlacedBeamMount(RoomEnemyPlace place) =>
        place.Entity is MorphBallEyeMountEntity && _followedPlaces.IsPlaced(place);

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

    /// <summary>Starts the 32-frame opening animation only inside strict modular X and Y activation ranges.</summary>
    private static void WaitForSamusNearMorphBallEye(
        RoomEnemyPlace eye,
        MorphBallEyeEnemyState state,
        SamusState samus)
    {
        if (!EnemyMath.IsWithinStrictModularDistance(samus.Position.Y.Whole, eye.Position.Y.Whole, MorphBallEyeWatchDefinitions.OpeningReachY) ||
            !EnemyMath.IsWithinStrictModularDistance(samus.Position.X.Whole, eye.Position.X.Whole, MorphBallEyeWatchDefinitions.OpeningReachX))
        {
            return;
        }

        state.ActionTimer = MorphBallEyeWatchDefinitions.TransitionDuration;
        eye.AnimationClock.StepOnNextUpdate();
        state.Playback = new(MorphBallEyeAnimations.StartingWith(eye.SpawnSettings<MorphBallEyeSpawnSettings>().Facing == HorizontalDirection.Right
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
        if (--state.ActionTimer != 0)
            return;

        _audio.QueueEnemySound(EnemySoundEffectDefinitions.EyeActivationSound, priority: SoundRequestPriority.Normal);
        RequestMorphBallEyeBeam();
        state.Action = MorphBallEyeAction.Active;
        state.Angle = CalculateMorphBallEyeAngle(eye, samus);
    }

    /// <summary>Aims one of sixteen active frames at Samus or starts closing after the wider horizontal/vertical range fails.</summary>
    private void TrackSamusWithMorphBallEye(
        RoomEnemyPlace eye,
        MorphBallEyeEnemyState state,
        SamusState samus)
    {
        bool inProximity =
            EnemyMath.IsWithinStrictModularDistance(samus.Position.Y.Whole, eye.Position.Y.Whole, MorphBallEyeWatchDefinitions.StayOpenReachY) &&
            EnemyMath.IsWithinStrictModularDistance(samus.Position.X.Whole, eye.Position.X.Whole, MorphBallEyeWatchDefinitions.StayOpenReachX);
        if (!inProximity)
        {
            _audio.QueueEnemySound(EnemySoundEffectDefinitions.EyeRaySoundStop, priority: SoundRequestPriority.Normal);
            state.IsActivated = false;
            state.ActionTimer = MorphBallEyeWatchDefinitions.TransitionDuration;
            state.Aim = null;
            state.Playback = new(MorphBallEyeAnimations.StartingWith(eye.SpawnSettings<MorphBallEyeSpawnSettings>().Facing == HorizontalDirection.Right
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
        if (--state.ActionTimer == 0)
            state.Action = MorphBallEyeAction.WaitForSamus;
    }

    /// <summary>Calculates the game angle from signed wrapped eye-to-Samus displacement.</summary>
    private static RotationAngle CalculateMorphBallEyeAngle(RoomEnemyPlace eye, SamusState samus) =>
        EnemyMath.CalculateBearing(
            new(CoordinateAxis.Difference(samus.Position.X.Whole, eye.Position.X.Whole), CoordinateAxis.Difference(samus.Position.Y.Whole, eye.Position.Y.Whole))).ToMirroredTrigonometryAngle().ToAngle();

    /// <summary>
    /// Schedules the room-global beam to appear next frame, following the eye its mount linked whichever eye opened.
    /// </summary>
    private void RequestMorphBallEyeBeam()
    {
        RoomEnemyPlace followed = BeamFollowedEye ?? throw new InvalidDataException(
            "A morph-ball eye opened in a room where no eye is placed right after a beam mount for the beam to follow.");
        MorphBallEyeBeam.Reset();
        MorphBallEyeBeam.Body = followed;
        MorphBallEyeBeam.Phase = MorphBallEyeBeamPhase.PendingInitialization;
    }

    /// <summary>
    /// Advances the beam before enemy AI, as the game advances it ahead of the enemies. A beam
    /// spawned by this frame's eye AI consequently initializes on the following frame.
    /// </summary>
    internal void StepMorphBallEyeBeam()
    {
        switch (MorphBallEyeBeam.Phase)
        {
            case MorphBallEyeBeamPhase.Inactive:
                return;
            case MorphBallEyeBeamPhase.PendingInitialization:
                InitializeMorphBallEyeBeamObject();
                return;
            case MorphBallEyeBeamPhase.Widening:
                WidenMorphBallEyeBeam();
                return;
            case MorphBallEyeBeamPhase.Full:
                StepFullMorphBallEyeBeam();
                return;
            case MorphBallEyeBeamPhase.Deactivating:
                FadeMorphBallEyeBeam();
                return;
            default:
                throw new InvalidDataException(
                    $"Unknown morph-ball eye beam phase {MorphBallEyeBeam.Phase}.");
        }
    }

    /// <summary>Seeds the beam's colour and zero angular width, then marks its body activated.</summary>
    private void InitializeMorphBallEyeBeamObject()
    {
        RoomEnemyPlace body = RequireMorphBallEyeBeamBody();
        MorphBallEyeEnemyState state = body.RequireSpeciesState<MorphBallEyeEnemyState>();

        MorphBallEyeBeam.Color = MorphBallEyeBeamDefinitions.InitialColor;
        MorphBallEyeBeam.AngularHalfWidth = RotationAngle.Zero;
        MorphBallEyeBeam.AngularHalfWidthGrowth = RotationAngle.Zero;
        MorphBallEyeBeam.ColourStep = 0;
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
            MorphBallEyeBeam.AngularHalfWidth = MorphBallEyeBeamDefinitions.FullHalfWidth + (width - width.WholeStep);
            MorphBallEyeBeam.Phase = MorphBallEyeBeamPhase.Full;
        }
    }

    /// <summary>Cycles the sixteen beam colors while the body stays activated, otherwise begins fading.</summary>
    internal void StepFullMorphBallEyeBeam()
    {
        MorphBallEyeEnemyState bodyState = (RequireMorphBallEyeBeamBody()).RequireSpeciesState<MorphBallEyeEnemyState>();
        if (!bodyState.IsActivated)
        {
            MorphBallEyeBeam.Phase = MorphBallEyeBeamPhase.Deactivating;
            return;
        }

        int colourStep = MorphBallEyeBeam.ColourStep;
        MorphBallEyeBeam.Color = MorphBallEyeBeamDefinitions.CycleColors[colourStep];
        MorphBallEyeBeam.ColourStep = (colourStep + 1) % MorphBallEyeBeamDefinitions.CycleColors.Count;
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

    /// <summary>Rejects a beam whose body is missing, of the wrong species, or a mount rather than the eye body.</summary>
    private RoomEnemyPlace RequireMorphBallEyeBeamBody()
    {
        RoomEnemyPlace body = MorphBallEyeBeam.Body
            ?? throw new InvalidDataException("Morph-ball eye beam has no eye body.");
        if (body.Entity is not MorphBallEyeEntity)
        {
            throw new InvalidDataException(
                $"Morph-ball eye beam lost its body in enemy place {body.Ordinal}.");
        }
        return body;
    }

    /// <summary>Advances a beam mount's typed animation: its mount pose, which raises no cue.</summary>
    internal static void AdvanceMorphBallEyeMountTypedAnimation(RoomEnemyPlace place) =>
        EnemyAnimationPlayback.Advance(place, MorphBallEyeAnimations.Segments,
            static (MorphBallEyeMountState state) => state.Playback, static (state, playback) => state.Playback = playback,
            static (state, cue) => throw new InvalidOperationException($"The Morph Ball eye mount's cue-free animation raised {cue}."));

    /// <summary>
    /// Advances the eye body's typed animation: a tracking eye shows the pose its aim chose this update for good, until the
    /// next update's tracking chooses again; otherwise its eyelid animation plays.
    /// </summary>
    internal static void AdvanceMorphBallEyeTypedAnimation(RoomEnemyPlace place, EnemyAnimationCueContext _)
    {
        if (Ensure.NotNull(place).RequireSpeciesState<MorphBallEyeEnemyState>().Aim is { } aim)
        {
            if (!place.AnimationClock.Expire())
                return;
            place.Frame = aim;
            place.AnimationClock.ShowForGood();
            return;
        }
        EnemyAnimationPlayback.Advance(place, MorphBallEyeAnimations.Segments,
            static (MorphBallEyeEnemyState state) => state.Playback, static (state, playback) => state.Playback = playback,
            static (state, cue) => throw new InvalidOperationException($"The Morph Ball eye's cue-free animation raised {cue}."));
    }

    /// <summary>Runs MorphBallEye's main AI.</summary>
    internal void RunMorphBallEyeMainAi(RoomEnemyPlace place, IEnemyAiFrame frame)
    {
        RunMorphBallEyeMain(place, place.RequireSpeciesState<MorphBallEyeEnemyState>(), frame.Samus);
    }
}
