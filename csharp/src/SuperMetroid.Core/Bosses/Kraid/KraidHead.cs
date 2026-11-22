using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>Kraid's head: the mouth-open attack and reaction, the eye glow and the health colours.</summary>
[RoomLifetime]
internal sealed partial class KraidHead
{
    /// <summary>Room services used throughout the Kraid fight.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Owns rocks, lint, and other fight projectiles.</summary>
    private readonly EnemyProjectileEngine _projectiles;
    /// <summary>Kraid's state and part lookups, part animations, phase waits, random numbers and the sound he last requested.</summary>
    private readonly KraidEncounterAccess _access;
    /// <summary>Kraid's room background: its fade, color transitions and the ceiling rocks.</summary>
    private readonly KraidBackground _background;

    /// <summary>The rocks he spits, his encounter's own (#2571).</summary>
    private readonly KraidProjectiles.RockBehavior _spitRock;

    /// <summary>Builds the component from the collaborators it uses.</summary>
    internal KraidHead(EnemyRoomBinding room, EnemyProjectileEngine projectiles, KraidEncounterAccess access, KraidBackground background,
        KraidProjectiles.RockBehavior spitRock)
    {
        _spitRock = Ensure.NotNull(spitRock);
        _room = room;
        _projectiles = projectiles;
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

    /// <summary>What <see cref="UpdateKraidHealthColours"/> does for each <see cref="KraidHurtFlashShade"/> member, each member's own arm.</summary>
    [field: NonSerialized]
    private IdentityTable<KraidHurtFlashShade, Action<RoomEnemyPlace, KraidEnemyState>> UpdateKraidHealthColoursForKraidHurtFlashShade => field ??= new(new Dictionary<KraidHurtFlashShade, Action<RoomEnemyPlace, KraidEnemyState>>
    {
        [KraidHurtFlashShade.HurtColours] = (body, state) => state.Colours.ShowHurtFlash(_background.EyeColours.Hurt, _background.EyeColours.Shaded),
        [KraidHurtFlashShade.BandColours] = (body, state) =>
        {
            KraidHealthBand band = HealthBand(body, state);
            state.Colours.ShowBand(band, BandEyes(band), _background.EyeColours.Shaded);
        },
    });

    /// <summary>
    /// Shows Kraid in <paramref name="shown"/>, his hurt colours or his current health band's (#1512): his body and every
    /// sprite part take the shade, and his eyes its colours at once.
    /// </summary>
    internal void UpdateKraidHealthColours(RoomEnemyPlace body, KraidEnemyState state, KraidHurtFlashShade shown)
    {
        UpdateKraidHealthColoursForKraidHurtFlashShade[shown](body, state);
        PictureShading parts = state.Colours.Parts;
        foreach (RoomEnemyPlace part in state.Parts.All)
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
    internal void SetupKraidFirstPhaseThinking(RoomEnemyPlace body, KraidEnemyState state, KraidStageRun.Fighting fight)
    {
        fight.Phase = KraidFightPhase.FirstPhaseThinking;
        state.Parts.PlaceLintsAtBodyHeights(body);
        state.ThinkingWait.LastFor(ChooseKraidThinkingUpdates());
    }

    /// <summary>Advances the open-mouth attack and its head animation timing.</summary>
    internal void RunKraidMouthOpenAttack(RoomEnemyPlace body, KraidEnemyState state, KraidStageRun.Fighting fight)
    {
        if (AdvanceKraidHeadAnimationToItsEnd(state, fight.Head))
        {
            SetupKraidFirstPhaseThinking(body, state, fight);
            CloseKraidMouth(state, fight);
            return;
        }

        // Kraid spits a rock at his spit cadence's place in the open mouth's hold, including the update the open mouth
        // shows and starts it.
        if (state.DisplayedHeadFrame == KraidHeadFrame.Open &&
            KraidCadenceDefinitions.MouthSpitRock.IsDue(state.HeadHold.UpdatesLeft))
        {
            SpawnKraidSpitRock(body);
            _access.RequestKraidSound(KraidSoundDefinitions.RockSpit);
        }
    }

    /// <summary>
    /// Steps the head's hold on its pose and plays the head animation on when the hold runs out on this update; a hold still
    /// running, or one that has run out before, keeps the pose.
    /// </summary>
    internal void AdvanceKraidHeadAnimation(KraidEnemyState state, KraidRisenHead head)
    {
        if (state.HeadHold.StepRunsOut() &&
            PlayKraidHeadAnimation(head) is AnimationShown<KraidHeadSegment, KraidHeadFrame>.Frame shown)
            state.ShowHeadPose(shown);
    }

    /// <summary>
    /// Advances the head as <see cref="AdvanceKraidHeadAnimation"/> does, and whether its animation reached its segment's end
    /// on this update, holding there.
    /// </summary>
    internal bool AdvanceKraidHeadAnimationToItsEnd(KraidEnemyState state, KraidRisenHead head) =>
        state.HeadHold.StepRunsOut() && RunKraidHeadAnimationStep(state, head) == KraidHeadAnimationResult.AnimationEnded;

    /// <summary>
    /// Plays Kraid's head animation to its next pose: cries on the way are announced, and the head shows the pose and holds
    /// it for its duration; at its segment's end the animation holds there.
    /// </summary>
    internal KraidHeadAnimationResult RunKraidHeadAnimationStep(KraidEnemyState state, KraidRisenHead head)
    {
        if (PlayKraidHeadAnimation(head) is not AnimationShown<KraidHeadSegment, KraidHeadFrame>.Frame shown)
            return KraidHeadAnimationResult.AnimationEnded;
        state.ShowHeadPose(shown);
        return KraidHeadAnimationResult.PoseShown;
    }

    /// <summary>
    /// Plays the head animation to its next frame, announcing its cries on the way: the frame it shows, or the hold at its
    /// segment's end. It shows nothing itself.
    /// </summary>
    private AnimationShown<KraidHeadSegment, KraidHeadFrame> PlayKraidHeadAnimation(KraidRisenHead head)
    {
        AnimationPlayback<KraidHeadSegment> playback = head.Playback;
        AnimationPlace place = playback.Place;
        AnimationShown<KraidHeadSegment, KraidHeadFrame> outcome = KraidHeadAnimations.Segments.PlayToNextFrame(
            playback.Animation, ref place, PublishKraidHeadCue);
        head.Playback = playback with { Place = place };
        return outcome switch
        {
            AnimationShown<KraidHeadSegment, KraidHeadFrame>.Held or AnimationShown<KraidHeadSegment, KraidHeadFrame>.Frame => outcome,
            AnimationShown<KraidHeadSegment, KraidHeadFrame>.Paused or AnimationShown<KraidHeadSegment, KraidHeadFrame>.Ended =>
                throw new InvalidOperationException($"Kraid's head animations never reach {outcome}."),
            _ => throw new InvalidOperationException($"Undefined animation outcome {outcome}."),
        };
    }

    /// <summary>What <see cref="PublishKraidHeadCue"/> does for each <see cref="KraidHeadCue"/> member, each member's own arm.</summary>
    [field: NonSerialized]
    private IdentityTable<KraidHeadCue, Action> PublishKraidHeadCueForKraidHeadCue => field ??= new(new Dictionary<KraidHeadCue, Action>
    {
        [KraidHeadCue.Roar] = () => _access.RequestKraidSound(KraidSoundDefinitions.HeadRoar),
        [KraidHeadCue.DyingCry] = () => _access.RequestKraidSound(KraidSoundDefinitions.HeadDyingCry),
    });

    /// <summary>Plays a head animation's cry.</summary>
    private void PublishKraidHeadCue(KraidHeadCue cue)
    {
        PublishKraidHeadCueForKraidHeadCue[cue]();
    }

    /// <summary>Updates the mouth-open reaction after a damaging shot.</summary>
    internal void RunKraidMouthOpenReaction(KraidEnemyState state, KraidStageRun.Fighting fight)
    {
        if (!AdvanceKraidHeadAnimationToItsEnd(state, fight.Head))
            return;
        fight.Phase = KraidFightPhase.FirstPhaseThinking;
        CloseKraidMouth(state, fight);
    }

    /// <summary>
    /// Ends his mouth's opening (#2557): it holds closed, and a hit on his mouth or body marks a reopen: the closing mouth
    /// then spends one pending reopen and, while any remain, waits to glow his eye again; otherwise every mark is forgotten.
    /// His open-mouth attack and his reaction to a damaging shot both close it so.
    /// </summary>
    private static void CloseKraidMouth(KraidEnemyState state, KraidStageRun.Fighting fight)
    {
        state.HoldClosedMouth();
        if (state.SpendMouthReopenAsMouthCloses())
        {
            fight.Phase = KraidFightPhase.WaitBeforeEyeGlow;
            state.BodyState.PhaseWait.LastFor(KraidArenaDefinitions.EyeGlowDelayUpdates);
        }
    }

    /// <summary>Seeds eye-glow colour state after a body hit.</summary>
    internal void InitializeKraidEyeGlow(KraidEnemyState state, KraidStageRun.Fighting fight)
    {
        fight.Phase = KraidFightPhase.GlowEye;
        fight.Head.Start(KraidHeadSegment.EyeGlow, state.HeadHold);
        GlowKraidEye(state, fight);
    }

    /// <summary>Steps the eye highlight while the body reacts to a hit.</summary>
    internal void GlowKraidEye(KraidEnemyState state, KraidStageRun.Fighting fight)
    {
        AdvanceKraidHeadAnimation(state, fight.Head);
        // The glow's peak, read back from the eyes' colours, ends the glow (#1512).
        if (state.Colours.GlowEyes())
            fight.Phase = KraidFightPhase.UnglowEye;
    }

    /// <summary>Restores the eye colours after the reaction completes.</summary>
    internal void UnglowKraidEye(RoomEnemyPlace body, KraidEnemyState state, KraidStageRun.Fighting fight)
    {
        // The eyes' colours, read back, reaching the band's ends the fade and starts the roar (#1512).
        bool unglowed = state.Colours.UnglowEyes(BandEyes(HealthBand(body, state)));
        if (unglowed)
        {
            fight.Phase = KraidFightPhase.MouthOpenReaction;
            fight.Head.Start(KraidHeadSegment.Roar, state.HeadHold);
        }
    }

    /// <summary>Emits a mouth rock when a projectile place is free.</summary>
    internal void SpawnKraidSpitRock(RoomEnemyPlace body)
    {
        EnemyProjectile? rock = _projectiles.AllocateEnemyProjectile(RoomEnemyProjectileKind.KraidSpitRock, _spitRock, graphics: SpriteGraphics.SharedIn(KraidColours.RocksShading), new KraidRockAnimation(_spitRock, KraidRockSegment.SharedPose));
        if (rock is null)
            return;

        int random = _access.ReadKraidRandomNumber();
        rock.Position = FixedPosition<RoomPixels>.FromWhole(body.Position.Whole + KraidArenaDefinitions.SpitRockOffset);
        rock.Velocity = rock.Velocity.WithX(KraidRockLaunchDefinitions.SpitLaunch.Choose(random));
        rock.Velocity = rock.Velocity.WithY(KraidArenaDefinitions.SpitRockRise);
    }

    /// <summary>Derives a thinking delay of one or more steps (<see cref="KraidArenaDefinitions.ThinkingDelayStepUpdates"/>) from the room RNG; a roll of none takes the fewest steps above one.</summary>
    internal int ChooseKraidThinkingUpdates() =>
        KraidRandomDefinitions.ThinkingDelaySteps.Choose(_room.Random.Read()) * KraidArenaDefinitions.ThinkingDelayStepUpdates;
}
