using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>Kraid's head: the mouth-open attack and reaction, the eye glow and the health colours.</summary>
[RoomLifetime]
internal sealed class KraidHead
{
    /// <summary>Room services used throughout the Kraid fight.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Owns rocks, lint, and other fight projectiles.</summary>
    private readonly EnemyProjectileEngine _projectiles;
    /// <summary>Kraid's eight parts in his room, in the order they run.</summary>
    private readonly KraidParts _parts;
    /// <summary>Kraid's state and part lookups, part animations, phase waits, random numbers and the sound he last requested.</summary>
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

    /// <summary>
    /// Plays his hurt flash on one update and shows the colours it reaches (#2400). Once his health has run out the flash
    /// is cut short, and his band's colours show on every update.
    /// </summary>
    internal void RunKraidHurtColours(RoomEnemyPlace body, KraidEnemyState state)
    {
        if (body.Health == 0)
        {
            state.SettleHurtFlash();
            UpdateKraidHealthColours(body, state, KraidHurtFlashShade.BandColours);
            return;
        }
        if (state.HurtFlash.StepToNewFrame() is { } shown)
            UpdateKraidHealthColours(body, state, shown);
    }

    /// <summary>
    /// Shows Kraid in <paramref name="shown"/>, his hurt colours or his current health band's (#1512): his body and every
    /// sprite part take the shade, and his eyes its colours at once.
    /// </summary>
    internal void UpdateKraidHealthColours(RoomEnemyPlace body, KraidEnemyState state, KraidHurtFlashShade shown)
    {
        switch (shown)
        {
            case KraidHurtFlashShade.HurtColours:
                state.Colours.ShowHurtFlash(_background.EyeColours.Hurt, _background.EyeColours.Shaded);
                break;
            case KraidHurtFlashShade.BandColours:
                KraidHealthBand band = HealthBand(body, state);
                state.Colours.ShowBand(band, BandEyes(band), _background.EyeColours.Shaded);
                break;
            default:
                throw new InvalidOperationException($"Undefined {nameof(KraidHurtFlashShade)} {shown}.");
        }
        PictureShading parts = state.Colours.Parts;
        foreach (RoomEnemyPlace part in _parts.All)
            part.Shading = parts;
    }

    /// <summary>The eye colours of health band <paramref name="band"/>.</summary>
    internal ReadOnlySpan<RgbColor> BandEyes(KraidHealthBand band) => _background.EyeColours.Of(band);

    /// <summary>The health band his colours start in, before any update: the band of his health as he initializes.</summary>
    internal static KraidHealthBand InitialBand(RoomEnemyPlace body, KraidEnemyState state) => HealthBand(body, state);

    /// <summary>
    /// The health band Kraid's colours show: the highest of the eight bands whose least health, in eighths of the health he
    /// initialized with, his health has reached, or the lowest band.
    /// </summary>
    private static KraidHealthBand HealthBand(RoomEnemyPlace body, KraidEnemyState state) => KraidHealthBand.Of(body.Health, state.InitialHealth);

    /// <summary>Restores the small-body thinking state after an attack.</summary>
    internal void SetupKraidFirstPhaseThinking(RoomEnemyPlace body, KraidEnemyState state)
    {
        state.BodyState.FightPhase = KraidFightPhase.FirstPhaseThinking;
        _parts.PlaceLintsAtBodyHeights(body);
        state.ThinkingWait.LastFor(ChooseKraidThinkingUpdates());
    }

    /// <summary>Advances the open-mouth attack and its head animation timing.</summary>
    internal void RunKraidMouthOpenAttack(RoomEnemyPlace body, KraidEnemyState state)
    {
        if (AdvanceKraidHeadAnimation(state) == KraidHeadAnimationResult.AnimationEnded)
        {
            SetupKraidFirstPhaseThinking(body, state);
            state.HoldClosedMouth();

            // A hit on his mouth or body marks a reopen: the closing mouth then spends one pending reopen and, while any
            // remain, waits to glow his eye again; otherwise every mark is forgotten.
            if (state.MouthReopenPending)
            {
                state.RemoveOneMouthReopen();
                if (state.PendingMouthReopens != 0)
                {
                    state.BodyState.FightPhase = KraidFightPhase.WaitBeforeEyeGlow;
                    state.BodyState.PhaseWait.LastFor(KraidArenaDefinitions.EyeGlowDelayUpdates);
                    return;
                }
            }
            state.ClearMouthReactions();
            return;
        }

        // Kraid spits a rock on every sixteenth update left of the open mouth's hold, including the update the open mouth
        // shows and starts it.
        if (state.DisplayedHeadFrame == KraidHeadFrame.Open &&
            KraidCadenceDefinitions.SpitRockEverySixteenthMouthUpdate.IsDue(state.HeadHold.UpdatesLeft))
        {
            SpawnKraidSpitRock(body);
            _access.RequestKraidSound(KraidSoundDefinitions.RockSpit);
        }
    }

    /// <summary>
    /// Steps the head's hold on its pose and plays the head animation on when the hold runs out on this update, giving the
    /// animation's result; a hold still running, or one that has run out before, keeps the pose.
    /// </summary>
    internal KraidHeadAnimationResult AdvanceKraidHeadAnimation(KraidEnemyState state) =>
        state.HeadHold.StepRunsOut() ? RunKraidHeadAnimationStep(state) : KraidHeadAnimationResult.PoseHeld;

    /// <summary>
    /// Plays Kraid's head animation to its next pose: cries on the way are announced, and the head shows the pose and holds
    /// it for its duration; at its segment's end the animation holds there.
    /// </summary>
    internal KraidHeadAnimationResult RunKraidHeadAnimationStep(KraidEnemyState state)
    {
        AnimationPlayback<KraidHeadSegment> playback = state.HeadPlayback ?? throw new InvalidDataException(
            "Kraid's head animation has not been started.");
        AnimationPlace place = playback.Place;
        AnimationShown<KraidHeadSegment, KraidHeadFrame> outcome = KraidHeadAnimations.Segments.PlayToNextFrame(
            playback.Animation, ref place, PublishKraidHeadCue);
        state.HeadPlayback = playback with { Place = place };
        switch (outcome)
        {
            case AnimationShown<KraidHeadSegment, KraidHeadFrame>.Held:
                return KraidHeadAnimationResult.AnimationEnded;

            case AnimationShown<KraidHeadSegment, KraidHeadFrame>.Frame shown:
                state.ShowHeadPose(shown);
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
        state.BodyState.FightPhase = KraidFightPhase.FirstPhaseThinking;
        state.HoldClosedMouth();
        if (state.MouthReopenPending)
        {
            state.RemoveOneMouthReopen();
            if (state.PendingMouthReopens != 0)
            {
                state.BodyState.FightPhase = KraidFightPhase.WaitBeforeEyeGlow;
                state.BodyState.PhaseWait.LastFor(KraidArenaDefinitions.EyeGlowDelayUpdates);
                return;
            }
        }
        state.ClearMouthReactions();
    }

    /// <summary>Seeds eye-glow colour state after a body hit.</summary>
    internal void InitializeKraidEyeGlow(KraidEnemyState state)
    {
        state.BodyState.FightPhase = KraidFightPhase.GlowEye;
        state.StartHeadAnimation(KraidHeadSegment.EyeGlow);
        GlowKraidEye(state);
    }

    /// <summary>Steps the eye highlight while the body reacts to a hit.</summary>
    internal void GlowKraidEye(KraidEnemyState state)
    {
        _ = AdvanceKraidHeadAnimation(state);
        // The glow's peak, read back from the eyes' colours, ends the glow (#1512).
        if (state.Colours.GlowEyes())
            state.BodyState.FightPhase = KraidFightPhase.UnglowEye;
    }

    /// <summary>Restores the eye colours after the reaction completes.</summary>
    internal void UnglowKraidEye(RoomEnemyPlace body, KraidEnemyState state)
    {
        // The eyes' colours, read back, reaching the band's ends the fade and starts the roar (#1512).
        bool unglowed = state.Colours.UnglowEyes(BandEyes(HealthBand(body, state)));
        if (unglowed)
        {
            state.BodyState.FightPhase = KraidFightPhase.MouthOpenReaction;
            state.StartHeadAnimation(KraidHeadSegment.Roar);
        }
    }

    /// <summary>Emits a mouth rock when a projectile place is free.</summary>
    internal void SpawnKraidSpitRock(RoomEnemyPlace body)
    {
        EnemyProjectile? rock = _projectiles.AllocateEnemyProjectile();
        if (rock is null)
            return;

        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
            rock,
            RoomEnemyProjectileKind.KraidSpitRock,
            KraidProjectiles.SpitRock,
            graphics: SpriteGraphics.SharedIn(KraidColours.RocksShading));
        rock.StartAnimation(new KraidRockAnimation(KraidRockSegment.SharedPose));
        int random = _access.ReadKraidRandomNumber();
        rock.Position = FixedPosition<RoomPixels>.FromWhole(body.Position.Whole + KraidArenaDefinitions.SpitRockOffset);
        rock.Velocity = rock.Velocity.WithX(KraidRockLaunchDefinitions.SpitLaunch.Choose(random));
        rock.Velocity = rock.Velocity.WithY(KraidArenaDefinitions.SpitRockRise);
    }

    /// <summary>Derives a thinking delay of one or more 64-update steps from the room RNG; a roll of none takes two.</summary>
    internal int ChooseKraidThinkingUpdates() =>
        KraidRandomDefinitions.ThinkingDelaySteps.Choose(_room.Random.Read()) * KraidArenaDefinitions.ThinkingDelayStepUpdates;
}
