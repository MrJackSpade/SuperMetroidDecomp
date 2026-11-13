using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>Kraid's head: the mouth-open attack and reaction, the eye glow and the health palettes.</summary>
[RoomLifetime]
internal sealed class KraidHead
{
    /// <summary>Room services used throughout the Kraid fight.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Owns rocks, lint, and other fight projectiles.</summary>
    private readonly EnemyProjectileEngine _projectiles;
    /// <summary>Kraid's eight parts in his room, in the order they run.</summary>
    private readonly KraidParts _parts;
    /// <summary>Kraid's state and part lookups, part animations, function timer, random numbers and the sound he last requested.</summary>
    private readonly KraidEncounterAccess _access;
    /// <summary>Kraid's room background: its fade, color transitions and the ceiling rocks.</summary>
    private readonly KraidBackground _background;

    /// <summary>Builds the component from the collaborators it uses.</summary>
    internal KraidHead(EnemyRoomBinding room, EnemyProjectileEngine projectiles, KraidParts parts, KraidEncounterAccess access, KraidBackground background)
    {
        _room = room;
        _projectiles = projectiles;
        _parts = parts;
        _access = access;
        _background = background;
    }

    /// <summary>Steps his hurt flash down and shows the colours of his health band or of the flash.</summary>
    internal void RunKraidHurtColours(RoomEnemySlot body, KraidEnemyState state)
    {
        if (body.Health == 0)
        {
            state.HurtFrame = 0;
            UpdateKraidHealthColours(body, state);
            return;
        }
        if (state.HurtFrame == 0)
            return;
        if (--state.HurtFrameTimer == 0)
        {
            state.HurtFrameTimer = 2;
            state.StepHurtFrameDown();
            UpdateKraidHealthColours(body, state);
        }
    }

    /// <summary>
    /// Shows Kraid's colours for his current health band, or the hurt flash on an odd hurt frame (#1512): his body and
    /// every sprite part take the band's shade, and his eyes the band's colours at once.
    /// </summary>
    internal void UpdateKraidHealthColours(RoomEnemySlot body, KraidEnemyState state)
    {
        KraidColourBand band = KraidCadenceDefinitions.HurtColoursShown.IsSetIn(state.HurtFrame)
            ? KraidColourBand.HurtFlash
            : KraidColourBand.Health(HealthBand(body, state));
        state.Colours.ShowBand(band, BandEyes(band), _background.EyeColours.Shaded);
        PictureShading parts = state.Colours.Parts;
        foreach (RoomEnemySlot part in _parts.All)
            part.Shading = parts;
    }

    /// <summary>The eye colours of <paramref name="band"/>: a health band's, or the hurt flash's.</summary>
    internal ReadOnlySpan<RgbColor> BandEyes(KraidColourBand band) => _background.EyeColours.Of(band);

    /// <summary>The health band his colours start in, before any update: the band of his health as he initializes.</summary>
    internal static KraidColourBand InitialBand(RoomEnemySlot body, KraidEnemyState state) => KraidColourBand.Health(HealthBand(body, state));

    /// <summary>
    /// The health band Kraid's colours show: the highest of the eight bands whose threshold his health has reached,
    /// or the lowest band.
    /// </summary>
    private static int HealthBand(RoomEnemySlot body, KraidEnemyState state)
    {
        int band = KraidHealthThresholdDefinitions.EighthCount - 1;
        while (band != 0 && body.Health < state.HealthEighthThreshold(band))
            band--;
        return band;
    }

    /// <summary>Restores the small-body thinking state after an attack.</summary>
    internal void SetupKraidFirstPhaseThinking(RoomEnemySlot body, KraidEnemyState state)
    {
        state.BodyState.Phase = KraidPhase.MainloopThinking;
        _parts.TopLint.Position = _parts.TopLint.Position.WithWrappedWholeY(body.Position.Y.Whole - 20);
        _parts.MiddleLint.Position = _parts.MiddleLint.Position.WithWrappedWholeY(body.Position.Y.Whole + 46);
        _parts.BottomLint.Position = _parts.BottomLint.Position.WithWrappedWholeY(body.Position.Y.Whole + 112);
        state.ThinkingTimer = ReadKraidThinkingTimer();
    }

    /// <summary>Advances the open-mouth attack and its head animation timing.</summary>
    internal void RunKraidMouthOpenAttack(RoomEnemySlot body, KraidEnemyState state)
    {
        if (AdvanceKraidHeadAnimation(state) == KraidHeadAnimationResult.AnimationEnded)
        {
            SetupKraidFirstPhaseThinking(body, state);
            state.BodyState.HeadTimer = 90;

            // The reopen latch and the pending reopen count are set by body/mouth shot handling. Keep
            // the game's order exactly so adding collision cannot change sequencing.
            if ((state.MouthFlags & KraidMouthFlags.ReopenPending) != KraidMouthFlags.None)
            {
                state.RemoveOneMouthReopen();
                if (state.PendingMouthReopens != 0)
                {
                    state.BodyState.Phase = KraidPhase.WaitToResumePhase;
                    state.BodyState.PhaseTimer = 64;
                    state.BodyState.PhaseToResume = KraidPhase.InitializeEyeGlow;
                    return;
                }
            }
            state.ClearMouthReactions();
            return;
        }

        // The open mouth stays shown for 64 frames. Retail emits a rock on
        // every sixteenth timer value, including the entry frame after the open-mouth setup reloads it.
        if (state.DisplayedHeadFrame is { MouthStage: KraidHeadFrame.FullyOpenMouthStage } &&
            KraidCadenceDefinitions.SpitRockEverySixteenthMouthUpdate.IsDue(state.BodyState.HeadTimer))
        {
            SpawnKraidSpitRock(body);
            _access.RequestKraidSound(KraidSoundDefinitions.RockSpit);
        }
    }

    /// <summary>
    /// Counts the displayed head frame down and runs the head animation on when it expires. A cleared timer holds the
    /// head, as does a timer still counting; a timer reaching zero gives the animation's result.
    /// </summary>
    internal KraidHeadAnimationResult AdvanceKraidHeadAnimation(KraidEnemyState state)
    {
        KraidBodyState part = state.BodyState;
        if (part.HeadTimer == 0)
            return KraidHeadAnimationResult.PoseHeld;
        return --part.HeadTimer == 0 ? RunKraidHeadAnimationStep(state) : KraidHeadAnimationResult.PoseHeld;
    }

    /// <summary>
    /// Plays Kraid's head animation to its next pose: cries on the way are announced, and a pose installs its duration as
    /// the head timer and shows its image; at its segment's end the animation holds there.
    /// </summary>
    internal KraidHeadAnimationResult RunKraidHeadAnimationStep(KraidEnemyState state)
    {
        AnimationPlayback<KraidHeadSegment> playback = state.HeadPlayback ?? throw new InvalidDataException(
            "Kraid's head animation has not been started.");
        AnimationPlace place = playback.Place;
        // The head's animations count no repeats, so nothing reads or writes this counter.
        WrappingCountdown repeats = WrappingCountdown.Start(0);
        AnimationShown<KraidHeadSegment, KraidHeadFrame> outcome = KraidHeadAnimations.Segments.PlayToNextFrame(
            playback.Animation, ref place, ref repeats, PublishKraidHeadCue);
        state.HeadPlayback = playback with { Place = place };
        switch (outcome)
        {
            case AnimationShown<KraidHeadSegment, KraidHeadFrame>.Held:
                return KraidHeadAnimationResult.AnimationEnded;

            case AnimationShown<KraidHeadSegment, KraidHeadFrame>.Frame shown:
                state.BodyState.HeadTimer = shown.Duration;
                // His body layer shows the pose over his body from here (#1515).
                state.DisplayedHeadFrame = shown.Shown;
                return KraidHeadAnimationResult.PoseShown;

            case AnimationShown<KraidHeadSegment, KraidHeadFrame>.Paused or AnimationShown<KraidHeadSegment, KraidHeadFrame>.Ended:
                throw new InvalidOperationException($"Kraid's head animations never reach {outcome}.");

            default:
                throw new InvalidOperationException($"Undefined animation outcome {outcome}.");
        }
    }

    /// <summary>Plays a head animation's cry.</summary>
    private void PublishKraidHeadCue(KraidHeadCue cue)
    {
        switch (cue)
        {
            case KraidHeadCue.Roar:
                _access.RequestKraidSound(KraidSoundDefinitions.HeadRoar);
                return;

            case KraidHeadCue.DyingCry:
                _access.RequestKraidSound(KraidSoundDefinitions.HeadDyingCry);
                return;

            default:
                throw new InvalidOperationException($"Undefined {nameof(KraidHeadCue)} {cue}.");
        }
    }

    /// <summary>Updates the mouth-open reaction after a damaging shot.</summary>
    internal void RunKraidMouthOpenReaction(KraidEnemyState state)
    {
        if (AdvanceKraidHeadAnimation(state) != KraidHeadAnimationResult.AnimationEnded)
            return;
        state.BodyState.Phase = KraidPhase.MainloopThinking;
        state.BodyState.HeadTimer = 90;
        if ((state.MouthFlags & KraidMouthFlags.ReopenPending) != KraidMouthFlags.None)
        {
            state.RemoveOneMouthReopen();
            if (state.PendingMouthReopens != 0)
            {
                state.BodyState.Phase = KraidPhase.WaitToResumePhase;
                state.BodyState.PhaseTimer = 64;
                state.BodyState.PhaseToResume = KraidPhase.InitializeEyeGlow;
                return;
            }
        }
        state.ClearMouthReactions();
    }

    /// <summary>Seeds eye-glow palette state after a body hit.</summary>
    internal void InitializeKraidEyeGlow(KraidEnemyState state)
    {
        state.BodyState.Phase = KraidPhase.GlowEye;
        state.HeadPlayback = KraidHeadAnimations.EyeGlowAfterEntryFrame;
        state.BodyState.HeadTimer = KraidHeadAnimations.EyeGlowEntryDuration;
        GlowKraidEye(state);
    }

    /// <summary>Steps the eye highlight while the body reacts to a hit.</summary>
    internal void GlowKraidEye(KraidEnemyState state)
    {
        _ = AdvanceKraidHeadAnimation(state);
        // The glow's peak, read back from the eyes' colours, ends the glow (#1512).
        if (state.Colours.GlowEyes())
            state.BodyState.Phase = KraidPhase.UnglowEye;
    }

    /// <summary>Restores the eye palette after the reaction completes.</summary>
    internal void UnglowKraidEye(RoomEnemySlot body, KraidEnemyState state)
    {
        // The eyes' colours, read back, reaching the band's ends the fade and starts the roar (#1512).
        bool unglowed = state.Colours.UnglowEyes(BandEyes(KraidColourBand.Health(HealthBand(body, state))));
        if (unglowed)
        {
            state.BodyState.Phase = KraidPhase.MouthOpenReaction;
            state.HeadPlayback = KraidHeadAnimations.RoarAfterEntryFrame;
            state.BodyState.HeadTimer = KraidHeadAnimations.RoarEntryDuration;
        }
    }

    /// <summary>Emits a mouth rock when a projectile slot is available.</summary>
    internal void SpawnKraidSpitRock(RoomEnemySlot body)
    {
        EnemyProjectile? rock = _projectiles.AllocateEnemyProjectile();
        if (rock is null)
            return;

        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
            rock,
            RoomEnemyProjectileKind.KraidSpitRock,
            graphics: SpriteGraphics.SharedIn(KraidColours.RocksShading));
        rock.StartAnimation(new KraidRockAnimation(KraidRockSegment.SharedPose));
        int random = _access.ReadKraidRandomNumber();
        rock.Position = FixedPosition<RoomPixels>.FromWhole(new(CoordinateAxis.Wrap(body.Position.X.Whole + 16), CoordinateAxis.Wrap(body.Position.Y.Whole - 96)));
        rock.Velocity = rock.Velocity.WithX(KraidRockLaunchDefinitions.FromRandom(random));
        rock.Velocity = rock.Velocity.WithY(SignedEightEight.FromPixels(-4));
        rock.Graphics = SpriteGraphics.SharedIn(KraidColours.RocksShading);
    }

    /// <summary>Derives a nonzero multiple-of-64 thinking delay from the room RNG.</summary>
    internal int ReadKraidThinkingTimer()
    {
        int random = _room.Random.Read();
        int value = KraidRandomDefinitions.ThinkingDelaySteps.Of(random);
        if (value == 0)
            value = 2;
        return value << 6;
    }
}
