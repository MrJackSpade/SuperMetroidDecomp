using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Enemies.MorphBallEye;

/// <summary>The Morph Ball eye species owner: its per-room state, behaviour and the interactions it registers.</summary>
internal sealed class MorphBallEyeBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <summary>Receives the species' sound requests, each queued as the species makes it.</summary>
    private readonly EnemyAudioQueue _audio;

    /// <summary>Builds the species owner on the room's enemy audio queue.</summary>
    internal MorphBallEyeBehavior(EnemyAudioQueue audio) => _audio = audio;

    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.Eye];

    /// <inheritdoc/>
    public IEnemyEntity Create(RoomEnemySlot slot, EnemyCommonParticipation common) => new MorphBallEyeEntity(this, slot, common);

    /// <summary>The eye responds to collected Morph Ball even when it is unequipped.</summary>
    /// <summary>Strict horizontal range for starting the opening animation.</summary>
    private const int EyeActivateXDistance = 128;
    /// <summary>The place, second in its room's listing, of the eye the game's beam always follows.</summary>
    private const int BeamFollowedEyePlace = 1;
    /// <summary>Wider strict horizontal range that keeps an active beam open.</summary>
    private const int EyeDeactivateXDistance = 176;
    /// <summary>Strict vertical range for initial activation.</summary>
    private const int EyeActivateYDistance = 128;
    /// <summary>Strict vertical range retained while tracking Samus.</summary>
    private const int EyeDeactivateYDistance = 128;
    /// <summary>Countdown for both opening and closing eye transitions.</summary>
    private const int EyeTransitionDuration = 32;

    /// <summary>The room-global beam an eye body casts.</summary>
    public MorphBallEyeBeamState MorphBallEyeBeam { get; } = new();

    /// <summary>Initializes a closed body from its spawn facing, or offsets a beam mount and resets the room-global beam.</summary>
    internal void InitializeMorphBallEye(RoomEnemySlot slot)
    {
        var state = new MorphBallEyeEnemyState();
        slot.SetSpeciesState(state);

        slot.Processing = slot.Processing.With(EnemyProcessing.Animates);
        slot.Frame = EnemyFrameId.Empty;
        slot.AnimationClock.Restart();

        MorphBallEyeSpawnSettings settings = slot.SpawnSettings<MorphBallEyeSpawnSettings>();
        if (settings.Part != MorphBallEyePart.BeamMount)
        {
            // The placement's facing is fixed by its mounting, not a live facing test.
            state.Action = MorphBallEyeAction.WaitForSamus;
            state.Playback = new(MorphBallEyeAnimations.StartingWith(settings.Facing == HorizontalDirection.Right
                ? MorphBallEyeSegment.FacingRightClosed
                : MorphBallEyeSegment.FacingLeftClosed), AnimationPlace.StartOfAnimation);
            return;
        }

        var mount = MorphBallEyeGeometry.Mount(settings.MountSide);
        slot.Position += mount.Offset;
        state.Action = MorphBallEyeAction.MountNoOp;
        state.Playback = new(MorphBallEyeAnimations.StartingWith(mount.Animation), AnimationPlace.StartOfAnimation);

        // A mount clears any beam when the room loads, so a prior room's reveal never
        // shows in this one.
        MorphBallEyeBeam.Reset();
    }

    /// <summary>Runs the body state machine only after Morph Ball is collected, preserving activation and tracking independently of equipment selection.</summary>
    private void RunMorphBallEyeMain(
        RoomEnemySlot slot,
        MorphBallEyeEnemyState state,
        SamusState? samus)
    {
        // The game tests the collected items, not the equipped items. This
        // matters after the player disables Morph Ball in the equipment menu.
        if (samus is null || !samus.CollectedItems.HasAny(SamusEquipmentFlags.MorphBall))
            return;

        switch (state.Action)
        {
            case MorphBallEyeAction.MountNoOp:
                return;
            case MorphBallEyeAction.WaitForSamus:
                WaitForSamusNearMorphBallEye(slot, state, samus);
                return;
            case MorphBallEyeAction.Activating:
                AdvanceMorphBallEyeActivation(slot, state, samus);
                return;
            case MorphBallEyeAction.Active:
                TrackSamusWithMorphBallEye(slot, state, samus);
                return;
            case MorphBallEyeAction.Deactivating:
                AdvanceMorphBallEyeDeactivation(state);
                return;
            default:
                throw new InvalidDataException(
                    $"Morph-ball eye function {state.Action?.ToString() ?? "none"} is not translated.");
        }
    }

    /// <summary>Starts the 32-frame opening animation only inside strict modular X and Y activation ranges.</summary>
    private static void WaitForSamusNearMorphBallEye(
        RoomEnemySlot eye,
        MorphBallEyeEnemyState state,
        SamusState samus)
    {
        if (!EnemyMath.IsWithinStrictModularDistance(samus.Position.Y.Whole, eye.Position.Y.Whole, EyeActivateYDistance) ||
            !EnemyMath.IsWithinStrictModularDistance(samus.Position.X.Whole, eye.Position.X.Whole, EyeActivateXDistance))
        {
            return;
        }

        state.FunctionTimer = EyeTransitionDuration;
        eye.AnimationClock.StepOnNextUpdate();
        state.Playback = new(MorphBallEyeAnimations.StartingWith(eye.SpawnSettings<MorphBallEyeSpawnSettings>().Facing == HorizontalDirection.Right
            ? MorphBallEyeSegment.FacingRightActivating
            : MorphBallEyeSegment.FacingLeftActivating), AnimationPlace.StartOfAnimation);
        state.Action = MorphBallEyeAction.Activating;
    }

    /// <summary>Counts the opening down, then sounds activation, requests the beam, and captures Samus's angle.</summary>
    private void AdvanceMorphBallEyeActivation(
        RoomEnemySlot eye,
        MorphBallEyeEnemyState state,
        SamusState samus)
    {
        // The step expires on reaching zero and also on going below it, so an already-zero timer expires on its
        // step to -1. This is deliberately not a test before the step.
        if (--state.FunctionTimer > 0)
            return;

        _audio.QueueEnemySound(EnemySoundEffectDefinitions.EyeActivationSound, priority: SoundRequestPriority.Normal);
        RequestMorphBallEyeBeam(eye);
        state.Action = MorphBallEyeAction.Active;
        state.Angle = CalculateMorphBallEyeAngle(eye, samus);
    }

    /// <summary>Aims one of sixteen active frames at Samus or starts closing after the wider horizontal/vertical range fails.</summary>
    private void TrackSamusWithMorphBallEye(
        RoomEnemySlot eye,
        MorphBallEyeEnemyState state,
        SamusState samus)
    {
        bool inProximity =
            EnemyMath.IsWithinStrictModularDistance(samus.Position.Y.Whole, eye.Position.Y.Whole, EyeDeactivateYDistance) &&
            EnemyMath.IsWithinStrictModularDistance(samus.Position.X.Whole, eye.Position.X.Whole, EyeDeactivateXDistance);
        if (!inProximity)
        {
            _audio.QueueEnemySound(EnemySoundEffectDefinitions.EyeDeactivationSound, priority: SoundRequestPriority.Normal);
            state.IsActivated = false;
            state.FunctionTimer = EyeTransitionDuration;
            state.Playback = new(MorphBallEyeAnimations.StartingWith(eye.SpawnSettings<MorphBallEyeSpawnSettings>().Facing == HorizontalDirection.Right
                ? MorphBallEyeSegment.FacingRightDeactivating
                : MorphBallEyeSegment.FacingLeftDeactivating), AnimationPlace.StartOfAnimation);
            state.Action = MorphBallEyeAction.Deactivating;
            eye.AnimationClock.StepOnNextUpdate();
            return;
        }

        state.Angle = CalculateMorphBallEyeAngle(eye, samus);

        // The tracking loop resumes at the pose for the arc of the turn the angle lies in.
        state.Playback = new(MorphBallEyeAnimations.StartingWith(MorphBallEyeSegment.Active), MorphBallEyeAnimations.TrackingPlaces.At(state.Angle));
        eye.AnimationClock.StepOnNextUpdate();
    }

    /// <summary>Returns to waiting when the closing countdown reaches zero or goes below it.</summary>
    private static void AdvanceMorphBallEyeDeactivation(MorphBallEyeEnemyState state)
    {
        if (--state.FunctionTimer <= 0)
            state.Action = MorphBallEyeAction.WaitForSamus;
    }

    /// <summary>Calculates the game angle from signed wrapped eye-to-Samus displacement.</summary>
    private static RotationAngle CalculateMorphBallEyeAngle(RoomEnemySlot eye, SamusState samus) =>
        EnemyMath.CalculateBearing(
            new(CoordinateAxis.Difference(samus.Position.X.Whole, eye.Position.X.Whole), CoordinateAxis.Difference(samus.Position.Y.Whole, eye.Position.Y.Whole))).ToMirroredTableAngle().ToAngle();

    /// <summary>Requires the body to be the room's second enemy before scheduling the room-global beam to appear next frame.</summary>
    private void RequestMorphBallEyeBeam(RoomEnemySlot eye)
    {
        // The game's beam always follows the enemy its room lists second, whichever eye
        // requested it. Refusing a different layout catches a malformed or custom population
        // instead of quietly aiming the beam from an eye the game would not follow.
        if (!eye.IsListedAt(BeamFollowedEyePlace) || eye.DefinitionId != EnemyDefinitionId.Eye)
        {
            throw new InvalidDataException(
                $"Morph-ball eye beam requires the body listed second in its room, got enemy {eye.SlotIndex}.");
        }

        MorphBallEyeBeam.Reset();
        MorphBallEyeBeam.Body = eye;
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
        RoomEnemySlot body = RequireMorphBallEyeBeamBody();
        MorphBallEyeEnemyState state = body.RequireSpeciesState<MorphBallEyeEnemyState>();

        MorphBallEyeBeam.Color = MorphBallEyeBeamDefinitions.InitialColor;
        MorphBallEyeBeam.AngularHalfWidth = RotationAngle.Zero;
        MorphBallEyeBeam.AngularHalfWidthGrowth = RotationAngle.Zero;
        MorphBallEyeBeam.ColorIndex = 0;
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

        int colorIndex = MorphBallEyeBeam.ColorIndex;
        MorphBallEyeBeam.Color = MorphBallEyeGeometry.BeamColor(colorIndex);
        MorphBallEyeBeam.ColorIndex = (colorIndex + 1) % MorphBallEyeBeamDefinitions.ColorCycleLength;
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
    private RoomEnemySlot RequireMorphBallEyeBeamBody()
    {
        RoomEnemySlot body = MorphBallEyeBeam.Body
            ?? throw new InvalidDataException("Morph-ball eye beam has no eye body.");
        if (body.DefinitionId != EnemyDefinitionId.Eye ||
            body.SpawnSettings<MorphBallEyeSpawnSettings>().Part == MorphBallEyePart.BeamMount)
        {
            throw new InvalidDataException(
                $"Morph-ball eye beam lost its body in enemy slot {body.SlotIndex}.");
        }
        return body;
    }

    /// <summary>Advances MorphBallEye's typed animation.</summary>
    internal static void AdvanceMorphBallEyeTypedAnimation(RoomEnemySlot slot, EnemyAnimationCueContext _) =>
        EnemyAnimationPlayback.Advance(slot, MorphBallEyeAnimations.Segments,
            static (MorphBallEyeEnemyState state) => state.Playback, static (state, playback) => state.Playback = playback,
            static (state, cue) => throw new InvalidOperationException($"The Morph Ball eye's cue-free animation raised {cue}."));

    /// <summary>Runs MorphBallEye's main AI.</summary>
    internal void RunMorphBallEyeMainAi(RoomEnemySlot slot, IEnemyAiFrame frame)
    {
        RunMorphBallEyeMain(slot, slot.RequireSpeciesState<MorphBallEyeEnemyState>(), frame.Samus);
    }
}
