using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Corpses;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies.Drawing;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Mother Brain's head and brain: their setup, neck, animations, cues and drawing.</summary>
[RoomLifetime]
internal sealed class MotherBrainHead
{
    /// <summary>Provides room services, pictures, colour state, and effects used by the encounter.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>The room's enemy places, in which the head finds her body listed right before it.</summary>
    private readonly EnemyPlaces _places;
    /// <summary>Owns the bombs, glass, beams, turrets, and other encounter projectiles.</summary>
    private readonly EnemyProjectileEngine _projectiles;
    /// <summary>Queues the encounter's music and sound effects in enemy update order.</summary>
    private readonly EnemyAudioQueue _audio;
    /// <summary>Draws encounter artwork and applies installed colours.</summary>
    private readonly EnemyPresentation _presentation;
    /// <summary>The current enemy pass's clocks and flags.</summary>
    private readonly EnemyFrameContext _frameContext;
    /// <summary>The other actors in the room: the Baby, the Rinkas, the Pirate laser and the afterburn.</summary>
    private readonly IMotherBrainRoomActors _roomActors;
    /// <summary>Mother Brain's projectiles: bombs, onion rings, drool, lasers, hand beams, turret shots and the tank's glass.</summary>
    private readonly MotherBrainProjectiles _projectileAttacks;


    /// <summary>Builds the component from the collaborators it uses.</summary>
    internal MotherBrainHead(MotherBrainHeadDependencies dependencies)
    {
        _room = dependencies.Room;
        _places = dependencies.Places;
        _projectiles = dependencies.Projectiles;
        _audio = dependencies.Audio;
        _presentation = dependencies.Presentation;
        _frameContext = dependencies.FrameContext;
        _roomActors = dependencies.RoomActors;
        _projectileAttacks = dependencies.ProjectileAttacks;
    }

    /// <summary>Sets Mother Brain's head up when the room spawns it.</summary>
    internal void InitializeMotherBrainHead(RoomEnemyPlace head)
    {
        // The head joins the encounter her body, listed right before it, began; it holds it from here (#2218).
        MotherBrainEnemyState state = _places.ListedBefore(head, 1).RequireSpeciesState<MotherBrainEnemyState>();
        head.SetSpeciesState(state);

        head.Health = 3000;
        // The head shows her brain's hit area from its next animation update on (#2220); the visible brain runs a
        // separate animation.
        head.AnimationClock.StepOnNextUpdate();
        head.Contact = head.Contact.With(EnemyContact.BlocksPlasmaBeam);
        head.Presence = head.Presence.With(EnemyPresence.Hidden);
        state.Head = head;
        MotherBrainEncounterAccess.SetMotherBrainBrainAnimation(state, MotherBrainHeadSegment.Initial);
    }

    /// <summary>
    /// The head place's animation update (#2220): its first shows her brain's hit area for good, which is what Samus and
    /// her shots meet there; the visible brain runs its own animation in her brain's draw.
    /// </summary>
    internal static void AdvanceMotherBrainHeadPlace(RoomEnemyPlace head)
    {
        if (head.AnimationClock.Expire())
            ShowBrainHitArea(head);
    }

    /// <summary>
    /// Shows her brain's hit area on <paramref name="place"/> for good, as a frame whose hold is left to wrap from zero.
    /// </summary>
    internal static void ShowBrainHitArea(RoomEnemyPlace place)
    {
        place.Frame = EnemyFrameId.MotherBrainBrainHitArea;
        place.AnimationClock.Show(0);
    }

    /// <summary>Runs the head main/hurt entry and its choice of whether her brain and neck are drawn.</summary>
    internal static void RunMotherBrainHeadMain(RoomEnemyPlace head, MotherBrainEnemyState state, SamusState samus)
    {

        // The head main first stops drawing her brain and neck. Only an intentionally invisible head
        // runs the shared brain action, which draws them again.
        state.DrawBrain = false;
        state.DrawNeck = false;
        if (!head.Presence.HasAny(EnemyPresence.Hidden))
            return;

        switch (state.BrainAction)
        {
            case MotherBrainBrainAction.SetupBrainToBeDrawn:
                state.DrawBrain = true;
                return;
            case MotherBrainBrainAction.SetupBrainAndNeckToBeDrawn:
                StepMotherBrainNeck(
                    state,
                    samus);
                Position16<RoomPixels> brainAnchor = state.NeckJoints[MotherBrainNeckJointDefinitions.BrainAnchor];
                head.Position = head.Position.WithWhole(new(brainAnchor.X, CoordinateAxis.Wrap(brainAnchor.Y - 21)));
                state.DrawBrain = true;
                state.DrawNeck = true;
                return;
            default:
                throw new InvalidDataException(
                    $"Mother Brain has no behaviour for brain action {state.BrainAction}.");
        }
    }

    /// <summary>
    /// Draws her brain, and her neck when it is shown, after every queued enemy has been drawn. The brain is shown by
    /// her own draw rather than the common enemy draw, although her population entry collides by its frames' hitboxes.
    /// </summary>
    /// <param name="state">The room's Mother Brain, as her body attached her to the room (#2194), or none before she sets up.</param>
    /// <param name="sprites">The frame's sprite description.</param>
    /// <param name="camera">Camera position (viewport top-left) in whole room pixels.</param>
    /// <param name="samus">Samus, whom the brain animation's cues read.</param>
    internal void DrawMotherBrainBrainAndNeck(MotherBrainEnemyState? state, SpriteFrameBuffer sprites, Position16<RoomPixels> camera, SamusState samus)
    {
        RoomEnemyPlace? head = state?.Head;
        if (state?.DrawBrain != true || head is null)
            return;
        // The brain draw samples (never advances) the RNG before reading the brain frame,
        // so the small breath can spawn even on a draw that then shows no brain.
        if (state.SmallPurpleBreathGenerationEnabled && !state.SmallPurpleBreathActive &&
            BossRandomRolls.InLowerHalf(_room.Random.Read()))
            SpawnMotherBrainPurpleBreathSmall(state);
        // The brain draw advances the brain animation here, after every enemy's AI, and returns out
        // of the whole draw when the animation is not live.
        if (AdvanceMotherBrainBrain(state, samus) is not EnemyFrameId brainFrame)
            return;

        Offset16<RoomPixels> brainShake = GetMotherBrainBrainShake(state, head);
        // The brain is published wherever it lies; the display clips it to the view (#1520).
        DrawMotherBrainWorldFrame(
            sprites,
            new SpriteSource(SpriteSourceKind.MotherBrainBrain),
            new MotherBrainWorldFrame(
                brainFrame,
                head.Position.Whole + brainShake,
                // Her brain's own draw bypasses ordinary enemy colour selection. Retail
                // still selects the shared white hurt colours on odd flash-timer frames.
                MotherBrainCadenceDefinitions.HurtFlashWhite.IsSetIn(head.FlashTimer) ? EnemyShades.HurtFlashLook : state.Colours.OwnShading),
            camera,
            ShownCorpseRows(state));

        if (!state.DrawNeck || state.Body.Presence.HasAny(EnemyPresence.Hidden))
            return;

        // The neck draw emits the joint nearest the brain first and the body-side joint
        // last. Equal-priority overlap therefore follows the same drawing precedence.
        int shakeIndex = MotherBrainCadenceDefinitions.NeckShakeStep.Of(head.FlashTimer);
        Offset16<RoomPixels> neckShake = MotherBrainShakeDefinitions.Offsets[shakeIndex];
        Position16<RoomPixels>[] segments = state.NeckJoints;
        for (int index = segments.Length - 1; index >= 0; index--)
        {
            Position16<RoomPixels> segment = segments[index];
            DrawMotherBrainWorldFrame(
                sprites,
                new SpriteSource(SpriteSourceKind.MotherBrainNeck),
                new MotherBrainWorldFrame(MotherBrainVisualDefinitions.NeckJointFrame, segment + neckShake,
                    state.Colours.OwnShading),
                camera,
                ShownCorpseRows(state));
        }
    }

    /// <summary>
    /// The rows <paramref name="state"/>'s corpse shows as its own rot has left them, which her corpse's frame is drawn
    /// whole through (#1539); none while her rainbow beam sequence has not begun.
    /// </summary>
    private static CorpseRotRows? ShownCorpseRows(MotherBrainEnemyState state) =>
        state.RainbowBeamSequence?.ThirdPhaseDeath.CorpseRotting.Shown;

    /// <summary>Publishes a selected world-space Mother Brain frame from the head's sheet with colours and camera offsets.</summary>
    /// <param name="sprites">The frame's sprite description.</param>
    /// <param name="source">The part shown.</param>
    /// <param name="frame">The frame, where it lies and its shading.</param>
    /// <param name="camera">Camera position (viewport top-left) in whole room pixels.</param>
    /// <param name="corpseRows">The rows her rotting corpse shows, or none while she shows no rotting corpse.</param>
    internal void DrawMotherBrainWorldFrame(
        SpriteFrameBuffer sprites,
        SpriteSource source,
        MotherBrainWorldFrame frame,
        Position16<RoomPixels> camera,
        CorpseRotRows? corpseRows)
    {
        // The frame is published wherever it lies, above the screen included; the display clips it (#1587).
        Offset16<ScreenPixels> screen = frame.World.RelativeToCamera(camera);

        // The head's sheet pictures each frame as the encounter's artwork changes leave it (#1392); her corpse's frame is drawn
        // whole through her corpse's own rows as its rot leaves them (#1539).
        _presentation.PublishSheetFrame(sprites, source, new EnemySheetDraw(EnemyDefinitionId.MotherBrainHead, EnemySheetDefinitions.FrameName(frame.Frame), screen)
            { Shading = frame.Shading, RotRows = corpseRows });
    }

    /// <summary>
    /// The neck turn from which her drool is the dying drool: a neck turning half a step an update or faster drools it, a
    /// slower one drools the ordinary drool.
    /// </summary>
    private static readonly RotationAngle DyingDroolNeckTurn = RotationAngle.FromSteps(0, 128);

    /// <summary>Derives the head's current shake offset from the encounter's animation state.</summary>
    private static Offset16<RoomPixels> GetMotherBrainBrainShake(
        MotherBrainEnemyState state,
        RoomEnemyPlace head)
    {
        int shake;
        if (state.BrainMainShakeTimer != 0)
        {
            shake = --state.BrainMainShakeTimer;
        }
        else
        {
            shake = head.FlashTimer != 0 ? head.FlashTimer : head.ShakeTimer;
        }

        return MotherBrainShakeDefinitions.Offsets[MotherBrainCadenceDefinitions.BrainShakeStep.Of(shake)];
    }

    /// <summary>Moves neck segments toward the head and updates their attachment to Samus's position.</summary>
    private static void StepMotherBrainNeck(MotherBrainEnemyState state, SamusState samus)
    {
        if (state.NeckMovementEnabled)
        {
            RotationAngle lowerAngle = state.LowerNeckAngle;
            RotationAngle upperAngle = state.UpperNeckAngle;
            MotherBrainNeckMovement lowerMovement = state.LowerNeckMovement;
            MotherBrainNeckMovement upperMovement = state.UpperNeckMovement;
            MotherBrainNeckKinematics.StepAngles(
                ref lowerAngle,
                ref upperAngle,
                ref lowerMovement,
                ref upperMovement,
                state.NeckTurnPerUpdate,
                state.Head.Position.Y.Whole,
                samus.Position.Y.Whole);
            state.LowerNeckAngle = lowerAngle;
            state.UpperNeckAngle = upperAngle;
            state.LowerNeckMovement = lowerMovement;
            state.UpperNeckMovement = upperMovement;
        }

        MotherBrainNeckKinematics.CalculateGeometry(state.Body.Position.Whole, state.LowerNeckAngle, state.UpperNeckAngle, state.NeckJointDistances)
            .CopyTo(state.NeckJoints);
    }

    /// <summary>
    /// Returns the brain frame
    /// to draw, or null when the brain is not live. With time frozen it only reads the current
    /// frame. Otherwise a frame holds while its duration minus the timer is not negative
    /// (the timer counting up); past it, cues run until the next frame, which
    /// resets the timer to one.
    /// </summary>
    internal EnemyFrameId? AdvanceMotherBrainBrain(MotherBrainEnemyState state, SamusState samus)
    {
        if (state.BrainPlayback is not { } playback)
            return null;
        AnimationBeat<EnemyFrameId, MotherBrainHeadCue> beat = RestingBeat(playback);
        if (_frameContext.TimeIsFrozen)
        {
            return beat is AnimationBeat<EnemyFrameId, MotherBrainHeadCue>.Frame frozen ? frozen.Shown : throw new InvalidOperationException(
                $"Mother Brain's frozen brain rests on {beat} at {playback.Place} of its animation, not a frame.");
        }

        Animation<MotherBrainHeadSegment> animation = playback.Animation;
        AnimationPlace place = playback.Place;
        if (beat is AnimationBeat<EnemyFrameId, MotherBrainHeadCue>.Frame show)
        {
            if (state.BrainFrameTimer <= show.Duration)
            {
                state.BrainFrameTimer++;
                return show.Shown;
            }
            place = place with { Beat = place.Beat + 1 };
        }

        AnimationShown<MotherBrainHeadSegment, EnemyFrameId> advance = MotherBrainAnimations.HeadSegments.PlayToNextFrame(
            ref animation, ref place, cue => ApplyMotherBrainHeadCue(state, samus, cue) is { } segment
                ? new AnimationContinuation<MotherBrainHeadSegment>(MotherBrainAnimations.HeadStartingWith(segment), AnimationPlace.StartOfAnimation)
                : null);
        if (advance is not AnimationShown<MotherBrainHeadSegment, EnemyFrameId>.Frame shown)
            throw new InvalidOperationException($"Mother Brain's brain animations have no {advance} beat.");
        // The place rests on the frame it shows, so it steps back onto it.
        state.BrainPlayback = new(animation, place with { Beat = place.Beat - 1 });
        state.BrainFrameTimer = 1;
        return shown.Shown;
    }

    /// <summary>The beat <paramref name="playback"/>'s place rests on: its playing phase's next beat.</summary>
    private static AnimationBeat<EnemyFrameId, MotherBrainHeadCue> RestingBeat(AnimationPlayback<MotherBrainHeadSegment> playback)
    {
        AnimationPhase<MotherBrainHeadSegment> phase = playback.Animation.Phases[playback.Place.Phase];
        return MotherBrainAnimations.HeadSegments.Beats(phase.Segment)[phase.FirstBeat + playback.Place.Beat];
    }

    /// <summary>Carries out one event of a brain animation.</summary>
    /// <returns>The segment whose animation a random hold or drool event continues at; otherwise null.</returns>
    /// <remarks>
    /// The brain animation advances in her brain's draw, after the enemy update has queued the frame's enemy
    /// sounds, so its sounds go straight to the enemy audio queue at their limits (#1391).
    /// </remarks>
    private MotherBrainHeadSegment? ApplyMotherBrainHeadCue(MotherBrainEnemyState state, SamusState samus, MotherBrainHeadCue cue)
    {
        switch (cue)
        {
            case MotherBrainHeadCue.Signal { Kind: MotherBrainHeadCueKind.ShakeBrain }:
                state.BrainMainShakeTimer = 50;
                return null;
            case MotherBrainHeadCue.Signal { Kind: MotherBrainHeadCueKind.SpawnDrool }:
                SpawnMotherBrainDrool(state);
                return null;
            case MotherBrainHeadCue.Signal { Kind: MotherBrainHeadCueKind.SpawnBigPurpleBreath }:
                SpawnMotherBrainPurpleBreathBig(state);
                return null;
            case MotherBrainHeadCue.PlaySound playSound:
                _audio.QueueEnemySound(playSound.Effect, priority: SoundRequestPriority.Normal);
                return null;
            case MotherBrainHeadCue.Signal { Kind: MotherBrainHeadCueKind.DisableNeckMovement }:
                state.NeckMovementEnabled = false;
                return null;
            case MotherBrainHeadCue.Signal { Kind: MotherBrainHeadCueKind.EnableNeckMovement }:
                state.NeckMovementEnabled = true;
                return null;
            case MotherBrainHeadCue.Signal { Kind: MotherBrainHeadCueKind.AimOnionRingsAtSamus }:
            {
                AimMotherBrainOnionRingsAt(state, samus.Position.Whole);
                return null;
            }
            case MotherBrainHeadCue.Signal { Kind: MotherBrainHeadCueKind.AimOnionRingsAtBabyMetroid }:
            {
                BabyMetroidCutsceneState baby = state.BabyMetroid ?? throw new InvalidOperationException(
                    "Mother Brain's Baby-targeting cue ran without the Baby actor.");
                AimMotherBrainOnionRingsAt(state, baby.Position.Whole);
                return null;
            }
            case MotherBrainHeadCue.Signal { Kind: MotherBrainHeadCueKind.SpawnOnionRing }:
                SpawnMotherBrainOnionRing(state, state.OnionRingsTargetAngle);
                return null;
            case MotherBrainHeadCue.SpawnBomb bomb:
                SpawnMotherBrainBomb(state, bomb.Afterburns);
                return null;
            case MotherBrainHeadCue.Signal { Kind: MotherBrainHeadCueKind.SpawnLaser }:
                SpawnMotherBrainMouthLaser(state);
                return null;
            case MotherBrainHeadCue.Signal { Kind: MotherBrainHeadCueKind.CountBabyMetroidAttack }:
                MotherBrainEncounterAccess.RequireLiveMotherBrainRainbowSequence(state).IncrementLiveBabyMetroidAttackCounter();
                return null;
            case MotherBrainHeadCue.Signal { Kind: MotherBrainHeadCueKind.ResetBabyMetroidAttacks }:
                MotherBrainEncounterAccess.RequireLiveMotherBrainRainbowSequence(state).ResetLiveBabyMetroidAttackCounter();
                return null;
            case MotherBrainHeadCue.Signal { Kind: MotherBrainHeadCueKind.QueueBabyMetroidAttackSound }:
                // The attack cry step skips the attack cry once the attack counter reaches eleven.
                if (MotherBrainEncounterAccess.RequireLiveMotherBrainRainbowSequence(state).Revival.BabyMetroidAttackCounter != 11)
                    _audio.QueueEnemySound(MotherBrainSoundDefinitions.AttackCry, priority: SoundRequestPriority.Normal);
                return null;
            case MotherBrainHeadCue.Signal { Kind: MotherBrainHeadCueKind.PrepareRainbowBeamCharge }:
                state.SmallPurpleBreathGenerationEnabled = false;
                _audio.QueueEnemySound(MotherBrainSoundDefinitions.RainbowBeamCharge, priority: SoundRequestPriority.Normal);
                return null;
            case MotherBrainHeadCue.Signal { Kind: MotherBrainHeadCueKind.SpawnRainbowBeamChargeParticle }:
                SpawnMotherBrainRainbowChargingProjectile(state);
                return null;
            // The neutral hold holds the phase-two neutral pose again on fifteen samples in sixteen.
            case MotherBrainHeadCue.Signal { Kind: MotherBrainHeadCueKind.MaybeHoldNeutralPhase2 }:
                return MotherBrainRandomDefinitions.NeutralPhase2Holds.IsTakenBy(_room.Random.Read()) ? MotherBrainHeadSegment.NeutralPhase2Hold : null;
            // The hold's chance alone decides whether the phase-three pose holds again.
            case MotherBrainHeadCue.Signal { Kind: MotherBrainHeadCueKind.MaybeHoldNeutralPhase3 }:
                return MotherBrainRandomDefinitions.NeutralPhase3Holds.IsTakenBy(_room.Random.Read()) ? MotherBrainHeadSegment.NeutralPhase3Hold : null;
            // The drool hold returns its destination: only the burst's chance bursts into more drool; every other sample
            // keeps the one-frame loop.
            case MotherBrainHeadCue.Signal { Kind: MotherBrainHeadCueKind.MaybeDroolAgain }:
                return MotherBrainRandomDefinitions.DroolBursts.IsTakenBy(_room.Random.Read()) ? MotherBrainHeadSegment.DyingDroolBurst : null;
            default:
                throw new InvalidOperationException($"Undefined Mother Brain head cue {cue}.");
        }
    }

    /// <summary>Fires the mouth laser, freezing the neck.</summary>
    private void SpawnMotherBrainMouthLaser(MotherBrainEnemyState state)
    {
        RoomEnemyPlace head = state.Head;
        state.NeckMovementEnabled = false;

        // The laser leaves her mouth, sixteen pixels right of and four below her head, travelling right. It is the head's
        // shot, so it deals Mother Brain's contact damage at her laser's speed.
        EnemyProjectile? laser = _roomActors.SpawnPirateLaser(
            head,
            head.Position.Whole + new Offset16<RoomPixels>(16, 4),
            direction: HorizontalDirection.Right);
        if (laser is not null)
            _audio.QueueEnemySound(EnemySoundEffectDefinitions.PirateMotherBrainLaserSound, priority: SoundRequestPriority.Normal);
    }

    /// <summary>
    /// The shared onion-ring aim tail. Both target selectors subtract the same mouth offsets,
    /// take the angle from those offsets, rotate by a half turn, and clamp the launch arc to
    /// <see cref="MotherBrainOnionRingDefinitions.LowestAim"/> through <see cref="MotherBrainOnionRingDefinitions.HighestAim"/>; only the source coordinate differs.
    /// </summary>
    private static void AimMotherBrainOnionRingsAt(
        MotherBrainEnemyState state,
        Position16<RoomPixels> target)
    {
        RoomEnemyPlace head = state.Head;
        Position16<RoomPixels> mouth = head.Position.Whole + new Offset16<RoomPixels>(10, 16);
        TrigonometryAngle angle = EnemyMath.CalculateBearing(target - mouth).ToTrigonometryAngle().TurnedBy(TrigonometryAngle.QuarterTurn);

        // The clamp performs two signed comparisons within one turn. The
        // wraparound half, from three quarters of a turn round past zero, maps to the lowest aim; the opposite half maps
        // to the highest. Only the aims between them survive unchanged.
        state.OnionRingsTargetAngle =
            angle >= MotherBrainOnionRingDefinitions.LowestAim && angle < MotherBrainOnionRingDefinitions.HighestAim ? angle
            : angle >= MotherBrainOnionRingDefinitions.HighestAim && angle < MotherBrainOnionRingDefinitions.ClampSplit ? MotherBrainOnionRingDefinitions.HighestAim
            : MotherBrainOnionRingDefinitions.LowestAim;
    }

    /// <summary>Runs the bomb's initializer.</summary>
    private void SpawnMotherBrainBomb(
        MotherBrainEnemyState state,
        int afterburnCount)
    {
        EnemyProjectile? bomb = _projectiles.AllocateEnemyProjectile();
        if (bomb is null)
            return;

        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
            bomb,
            RoomEnemyProjectileKind.MotherBrainBomb,
            _projectileAttacks.Projectile,
            graphics: SpriteGraphics.SharedIn(state.Colours.OwnShading));
        bomb.StartAnimation(new MotherBrainEffectAnimation(MotherBrainEffectSegment.Bomb));
        RoomEnemyPlace head = state.Head;
        bomb.Position = bomb.Position.WithWhole(new(CoordinateAxis.Wrap(head.Position.X.Whole + 12), CoordinateAxis.Wrap(head.Position.Y.Whole + 16)));
        bomb.Position = bomb.Position.WithFractionX(0).WithFractionY(0);
        bomb.Velocity = new(SignedEightEight.FromTwoHundredFiftySixthsOfPixel(224), SignedEightEight.FromPixels(1));
        bomb.State = new MotherBrainBombState(state, MotherBrainBombBounceDefinitions.BounceSpeed, afterburnCount);
        state.RecordBombSpawned();
    }

    /// <summary>
    /// Runs the head's onion-ring step and the ring's initializer. The shared
    /// allocator deliberately runs after the twelve room turrets and all lingering drool,
    /// so pool saturation drops the ring when the pool is full.
    /// </summary>
    internal void SpawnMotherBrainOnionRing(
        MotherBrainEnemyState state,
        TrigonometryAngle angle)
    {
        EnemyProjectile? ring = _projectiles.AllocateEnemyProjectile();
        if (ring is null)
            return;

        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
            ring,
            RoomEnemyProjectileKind.MotherBrainOnionRing,
            _projectileAttacks.OnionRing,
            graphics: SpriteGraphics.SharedIn(state.Colours.OwnShading));
        ring.StartAnimation(new MotherBrainEffectAnimation(MotherBrainEffectSegment.OnionRing));
        ring.State = new MotherBrainOnionRingState(state, MotherBrainOnionRingDefinitions.MouthDelay);
        ring.Velocity = ring.Velocity.WithX(EnemyMath.SineSpeed(MotherBrainOnionRingDefinitions.Speed, angle));
        ring.Velocity = ring.Velocity.WithY(EnemyMath.SineSpeed(
            MotherBrainOnionRingDefinitions.Speed,
            angle.TurnedBy(TrigonometryAngle.QuarterTurn)));

        // The ring's setup pins it to the mouth in the same update, before the first of its eight delayed attachment
        // frames.
        MotherBrainProjectiles.PinMotherBrainOnionRingToMouth(ring, state);
    }

    /// <summary>Runs the head's drool step and the drool's initializer.</summary>
    internal void SpawnMotherBrainDrool(MotherBrainEnemyState state)
    {
        if (!state.DroolGenerationEnabled)
            return;

        state.LastDroolMouthPoint = (state.LastDroolMouthPoint + 1) % MotherBrainBehavior.MotherBrainDroolOffsets.Length;
        Offset16<RoomPixels> mouth = MotherBrainBehavior.MotherBrainDroolOffsets[state.LastDroolMouthPoint];

        EnemyProjectile? drool = _projectiles.AllocateEnemyProjectile();
        if (drool is null)
            return;

        RoomEnemyProjectileKind kind = state.NeckTurnPerUpdate < DyingDroolNeckTurn
            ? RoomEnemyProjectileKind.MotherBrainDrool
            : RoomEnemyProjectileKind.MotherBrainDyingDrool;
        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(drool, kind, _projectileAttacks.Projectile, graphics: default);
        drool.StartAnimation(new MotherBrainEffectAnimation(MotherBrainEffectSegment.Drool));
        drool.State = new MotherBrainDroolState(state, mouth);

        // Spawning the drool attaches it to the head at once, so its position is already
        // right before the later projectile pass repeats the same attachment.
        MotherBrainProjectiles.MotherBrainProjectileBehavior.HoldMotherBrainDroolAtMouth(drool);
    }

    /// <summary>Runs the head's big purple breath step and the breath's initializer.</summary>
    private void SpawnMotherBrainPurpleBreathBig(MotherBrainEnemyState state)
    {
        EnemyProjectile? breath = _projectiles.AllocateEnemyProjectile();
        if (breath is null)
            return;

        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
            breath,
            RoomEnemyProjectileKind.MotherBrainPurpleBreathBig,
            _projectileAttacks.Projectile,
            graphics: default);
        breath.State = new MotherBrainProjectileState(state);
        breath.StartAnimation(new MotherBrainEffectAnimation(MotherBrainEffectSegment.PurpleBreathBig));
        RoomEnemyPlace head = state.Head;
        breath.Position = breath.Position.WithWhole(new(CoordinateAxis.Wrap(head.Position.X.Whole + 6), CoordinateAxis.Wrap(head.Position.Y.Whole + 16)));
    }

    /// <summary>Runs the small-breath spawn and the breath's initializer.</summary>
    private void SpawnMotherBrainPurpleBreathSmall(MotherBrainEnemyState state)
    {
        EnemyProjectile? breath = _projectiles.AllocateEnemyProjectile();
        if (breath is null)
            return;

        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
            breath,
            RoomEnemyProjectileKind.MotherBrainPurpleBreathSmall,
            _projectileAttacks.Projectile,
            graphics: default);
        breath.State = new MotherBrainProjectileState(state);
        breath.StartAnimation(new MotherBrainEffectAnimation(MotherBrainEffectSegment.PurpleBreathSmall));
        RoomEnemyPlace head = state.Head;
        breath.Position = breath.Position.WithWhole(new(CoordinateAxis.Wrap(head.Position.X.Whole + 6), CoordinateAxis.Wrap(head.Position.Y.Whole + 16)));
        state.SmallPurpleBreathActive = true;
    }

    /// <summary>Runs the rainbow charging projectile's spawn, including the initializer's same-call pin.</summary>
    internal void SpawnMotherBrainRainbowChargingProjectile(MotherBrainEnemyState state)
    {
        EnemyProjectile? projectile = _projectiles.AllocateEnemyProjectile();
        if (projectile is null)
            return;

        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
            projectile,
            RoomEnemyProjectileKind.MotherBrainRainbowBeamCharging,
            _projectileAttacks.Projectile,
            graphics: default);
        projectile.State = new MotherBrainProjectileState(state);
        projectile.StartAnimation(new MotherBrainEffectAnimation(MotherBrainEffectSegment.RainbowBeamCharging));
        projectile.Velocity = EightEightVector<RoomPixels>.Zero;
        projectile.Position = projectile.Position.WithFractionX(0).WithFractionY(0);
        MotherBrainProjectiles.MotherBrainProjectileBehavior.PinMotherBrainRainbowCharge(projectile);
    }
}
