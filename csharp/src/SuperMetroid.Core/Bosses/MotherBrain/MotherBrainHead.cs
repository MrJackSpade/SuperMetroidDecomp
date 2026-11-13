using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies.Drawing;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Mother Brain's head and brain: their setup, neck, programs, cues and drawing.</summary>
[RoomLifetime]
internal sealed class MotherBrainHead
{
    /// <summary>Provides room services, tiles, palette state, and effects used by the encounter.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Owns the bombs, glass, beams, turrets, and other encounter projectiles.</summary>
    private readonly EnemyProjectileEngine _projectiles;
    /// <summary>Queues the encounter's music and sound effects in enemy update order.</summary>
    private readonly EnemyAudioQueue _audio;
    /// <summary>Draws encounter artwork and applies installed palette data.</summary>
    private readonly EnemyPresentation _presentation;
    /// <summary>Supplies Samus and camera state for the current enemy frame.</summary>
    private readonly EnemyFrameContext _frameContext;
    /// <summary>Applies the encounter's damage and death/drop rules.</summary>
    private readonly EnemyDeathsAndDrops _deathsAndDrops;
    /// <summary>Holds the shared body/head state for the current Mother Brain room.</summary>
    private readonly MotherBrainEncounter _motherBrainEncounter;
    /// <summary>The other actors in the room: the Baby, the Rinkas, the Pirate laser and the afterburn.</summary>
    private readonly IMotherBrainRoomActors _roomActors;
    /// <summary>Mother Brain's projectiles: bombs, onion rings, drool, lasers, hand beams, turret shots and the tank's glass.</summary>
    private readonly MotherBrainProjectiles _projectileAttacks;
    /// <summary>Mother Brain's state lookups, body and brain program setters, timers and room-object requests.</summary>
    private readonly MotherBrainEncounterAccess _access;


    /// <summary>Builds the component from the collaborators it uses.</summary>
    internal MotherBrainHead(MotherBrainHeadDependencies dependencies)
    {
        _room = dependencies.Room;
        _projectiles = dependencies.Projectiles;
        _audio = dependencies.Audio;
        _presentation = dependencies.Presentation;
        _frameContext = dependencies.FrameContext;
        _deathsAndDrops = dependencies.DeathsAndDrops;
        _motherBrainEncounter = dependencies.MotherBrainEncounter;
        _roomActors = dependencies.RoomActors;
        _projectileAttacks = dependencies.ProjectileAttacks;
        _access = dependencies.Access;
    }

    /// <summary>Sets Mother Brain's head up when the room spawns it.</summary>
    internal void InitializeMotherBrainHead(RoomEnemySlot head)
    {
        MotherBrainEnemyState state = _motherBrainEncounter.MotherBrain ??
            throw new InvalidDataException("Mother Brain's head appeared before her body.");

        head.Health = 3000;
        // The brain initializer gives the head enemy the dummy program for life (the encounter state's
        // head-slot cursor starts there): its dummy frame is the head's collision hitbox.
        // The visible brain runs a separate program.
        head.FrameHold = WrappingCountdown.Start(1);
        head.Contact = head.Contact.With(EnemyContact.BlocksPlasmaBeam);
        head.Presence = head.Presence.With(EnemyPresence.Hidden);
        state.Head = head;
        MotherBrainEncounterAccess.SetMotherBrainBrainProgram(state, MotherBrainHeadProgram.Initial);
    }

    /// <summary>Ports the head main/hurt entry and its choice of whether her brain and neck are drawn.</summary>
    internal void RunMotherBrainHeadMain(RoomEnemySlot head, SamusState? samus)
    {
        MotherBrainEnemyState state = _access.RequireCompleteMotherBrainState(head);

        // The head main first stops drawing her brain and neck. Only an intentionally invisible head
        // dispatches the shared brain function, which draws them again.
        state.DrawBrain = false;
        state.DrawNeck = false;
        if (!head.Presence.HasAny(EnemyPresence.Hidden))
            return;

        switch (state.BrainFunction)
        {
            case MotherBrainBrainFunction.SetupBrainToBeDrawn:
                state.DrawBrain = true;
                return;
            case MotherBrainBrainFunction.SetupBrainAndNeckToBeDrawn:
                StepMotherBrainNeck(
                    state,
                    samus ?? throw new InvalidOperationException(
                        "Mother Brain's articulated neck requires the live Samus actor."));
                Position16<RoomPixels> brainAnchor = state.NeckJoints[MotherBrainNeckJointDefinitions.BrainAnchor];
                head.Position = head.Position.WithWhole(new(brainAnchor.X, CoordinateAxis.Wrap(brainAnchor.Y - 21)));
                state.DrawBrain = true;
                state.DrawNeck = true;
                return;
            default:
                throw new InvalidDataException(
                    $"Mother Brain brain function {state.BrainFunction} is not translated.");
        }
    }

    /// <summary>
    /// Draws her brain, and her neck when it is shown, after every queued enemy has been drawn. The brain is shown by
    /// her own draw rather than the common enemy draw, although her population entry collides by its frames' hitboxes.
    /// </summary>
    internal void DrawMotherBrainBrainAndNeck(SpriteFrameBuffer sprites, Position16<RoomPixels> camera)
    {
        MotherBrainEnemyState? state = _motherBrainEncounter.MotherBrain;
        RoomEnemySlot? head = state?.Head;
        if (state?.DrawBrain != true || head is null)
            return;
        // The brain draw samples (never advances) the RNG before reading the brain frame,
        // so the small breath can spawn even on a draw that then shows no brain.
        if (state.SmallPurpleBreathGenerationEnabled && !state.SmallPurpleBreathActive &&
            BossRandomRolls.InLowerHalf(_room.Random.Read()))
            SpawnMotherBrainPurpleBreathSmall(state);
        // The brain draw advances the brain list here, after every enemy's AI, and returns out
        // of the whole draw when the list is not live.
        if (AdvanceMotherBrainBrain(state, _deathsAndDrops.SamusForEnemyDrops) is not MotherBrainHeadFrame brainFrame)
            return;

        Offset16<RoomPixels> brainShake = GetMotherBrainBrainShake(state, head);
        // The brain is published wherever it lies; the display clips it to the view (#1520).
        DrawMotherBrainWorldSpritemap(
            sprites,
            new SpriteSource(SpriteSourceKind.MotherBrainBrain),
            new MotherBrainWorldFrame(
                MotherBrainVisualDefinitions.HeadFrames.Of(brainFrame),
                head.Position.Whole + brainShake,
                // Her brain's own draw bypasses ordinary enemy colour selection. Retail
                // still selects the shared white hurt colours on odd flash-timer frames.
                MotherBrainCadenceDefinitions.HurtFlashWhite.IsSetIn(head.FlashTimer) ? EnemyShades.HurtFlashLook : state.Colours.OwnShading),
            camera);

        if (!state.DrawNeck || state.Body.Presence.HasAny(EnemyPresence.Hidden))
            return;

        // The neck draw emits the joint nearest the brain first and the body-side joint
        // last. Equal-priority overlap therefore follows the same drawing precedence.
        int shakeIndex = MotherBrainCadenceDefinitions.NeckShakeStep.Of(head.FlashTimer);
        Offset16<RoomPixels> neckShake = MotherBrainBehavior.MotherBrainShakeOffsets[shakeIndex];
        Position16<RoomPixels>[] segments = state.NeckJoints;
        for (int index = segments.Length - 1; index >= 0; index--)
        {
            Position16<RoomPixels> segment = segments[index];
            DrawMotherBrainWorldSpritemap(
                sprites,
                new SpriteSource(SpriteSourceKind.MotherBrainNeck, segments.Length - 1 - index),
                new MotherBrainWorldFrame(MotherBrainVisualDefinitions.NeckJointFrame, segment + neckShake,
                    state.Colours.OwnShading),
                camera);
        }
    }

    /// <summary>Publishes a selected world-space Mother Brain frame from the head's sheet with palette and camera offsets.</summary>
    internal void DrawMotherBrainWorldSpritemap(
        SpriteFrameBuffer sprites,
        SpriteSource source,
        MotherBrainWorldFrame frame,
        Position16<RoomPixels> camera)
    {
        // The frame is published wherever it lies, above the screen included; the display clips it (#1587).
        Position16<ScreenPixels> screen = frame.World.ToScreen(camera);

        // The head's sheet pictures each frame as the encounter's artwork changes leave it (#1392); her corpse's frame is drawn
        // whole through her corpse's own rows as its rot leaves them (#1539).
        _presentation.PublishSheetFrame(sprites, source, new EnemySheetDraw(EnemyDefinitionId.MotherBrainHead, EnemySheetDefinitions.FrameName(frame.Frame), screen)
            { Shading = frame.Shading, RotRows = _motherBrainEncounter.ShownCorpseRows });
    }

    /// <summary>Derives the head's current shake offset from the encounter's animation state.</summary>
    private static Offset16<RoomPixels> GetMotherBrainBrainShake(
        MotherBrainEnemyState state,
        RoomEnemySlot head)
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

        return MotherBrainBehavior.MotherBrainShakeOffsets[MotherBrainCadenceDefinitions.BrainShakeStep.Of(shake)];
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
                state.NeckAngleDelta,
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
    /// (a sign test), the timer counting up; past it, cues run until the next frame, which
    /// resets the timer to one.
    /// </summary>
    internal MotherBrainHeadFrame? AdvanceMotherBrainBrain(MotherBrainEnemyState state, SamusState? samus)
    {
        if (state.BrainPlayback is not { } playback)
            return null;
        AnimationBeat<MotherBrainHeadFrame, MotherBrainHeadCue> beat = RestingBeat(playback);
        if (_frameContext.TimeIsFrozen)
        {
            return beat is AnimationBeat<MotherBrainHeadFrame, MotherBrainHeadCue>.Frame frozen ? frozen.Shown : throw new InvalidOperationException(
                $"Mother Brain's frozen brain rests on {beat} at {playback.Place} of its animation, not a frame.");
        }

        Animation<MotherBrainHeadProgram> animation = playback.Animation;
        AnimationPlace place = playback.Place;
        if (beat is AnimationBeat<MotherBrainHeadFrame, MotherBrainHeadCue>.Frame show)
        {
            if (state.BrainFrameTimer <= show.Duration)
            {
                state.BrainFrameTimer++;
                return show.Shown;
            }
            place = place with { Beat = place.Beat + 1 };
        }

        // No brain animation counts its plays, so nothing reads or writes this counter.
        WrappingCountdown uncounted = default;
        AnimationShown<MotherBrainHeadProgram, MotherBrainHeadFrame> advance = MotherBrainAnimations.HeadSegments.PlayToNextFrame(
            ref animation, ref place, ref uncounted, cue => ApplyMotherBrainHeadCue(state, samus, cue) is { } program
                ? new AnimationContinuation<MotherBrainHeadProgram>(MotherBrainAnimations.HeadStartingWith(program), AnimationPlace.StartOfAnimation)
                : null);
        if (advance is not AnimationShown<MotherBrainHeadProgram, MotherBrainHeadFrame>.Frame shown)
            throw new InvalidOperationException($"Mother Brain's brain animations have no {advance} beat.");
        // The place rests on the frame it shows, so it steps back onto it.
        state.BrainPlayback = new(animation, place with { Beat = place.Beat - 1 });
        state.BrainFrameTimer = 1;
        return shown.Shown;
    }

    /// <summary>The beat <paramref name="playback"/>'s place rests on: its playing phase's next beat.</summary>
    private static AnimationBeat<MotherBrainHeadFrame, MotherBrainHeadCue> RestingBeat(AnimationPlayback<MotherBrainHeadProgram> playback)
    {
        AnimationPhase<MotherBrainHeadProgram> phase = playback.Animation.Phases[playback.Place.Phase];
        return MotherBrainAnimations.HeadSegments.Beats(phase.Segment)[phase.FirstBeat + playback.Place.Beat];
    }

    /// <summary>Carries out one event of a brain animation.</summary>
    /// <returns>The program whose animation a random hold or drool event continues at; otherwise null.</returns>
    /// <remarks>
    /// The brain program advances in her brain's draw, after the enemy update has collected the frame's legacy
    /// sound slots, so its sounds go straight to the enemy audio queue at their limits (#1391).
    /// </remarks>
    private MotherBrainHeadProgram? ApplyMotherBrainHeadCue(MotherBrainEnemyState state, SamusState? samus, MotherBrainHeadCue cue)
    {
        switch (cue)
        {
            case MotherBrainHeadCue.Command { Kind: MotherBrainHeadCueKind.ShakeBrain }:
                state.BrainMainShakeTimer = 50;
                return null;
            case MotherBrainHeadCue.Command { Kind: MotherBrainHeadCueKind.SpawnDrool }:
                SpawnMotherBrainDrool(state);
                return null;
            case MotherBrainHeadCue.Command { Kind: MotherBrainHeadCueKind.SpawnBigPurpleBreath }:
                SpawnMotherBrainPurpleBreathBig(state);
                return null;
            case MotherBrainHeadCue.PlaySound playSound:
                _audio.QueueEnemySound(playSound.Effect, priority: SoundRequestPriority.Normal);
                return null;
            case MotherBrainHeadCue.Command { Kind: MotherBrainHeadCueKind.DisableNeckMovement }:
                state.NeckMovementEnabled = false;
                return null;
            case MotherBrainHeadCue.Command { Kind: MotherBrainHeadCueKind.EnableNeckMovement }:
                state.NeckMovementEnabled = true;
                return null;
            case MotherBrainHeadCue.Command { Kind: MotherBrainHeadCueKind.AimOnionRingsAtSamus }:
            {
                SamusState target = samus ?? throw new InvalidOperationException(
                    "Mother Brain onion-ring aiming requires the active Samus actor.");
                AimMotherBrainOnionRingsAt(state, target.Position.Whole);
                return null;
            }
            case MotherBrainHeadCue.Command { Kind: MotherBrainHeadCueKind.AimOnionRingsAtBabyMetroid }:
            {
                BabyMetroidCutsceneState baby = state.BabyMetroid ?? throw new InvalidOperationException(
                    "Mother Brain's Baby-targeting cue ran without the Baby actor.");
                AimMotherBrainOnionRingsAt(state, baby.Position.Whole);
                return null;
            }
            case MotherBrainHeadCue.Command { Kind: MotherBrainHeadCueKind.SpawnOnionRing }:
                SpawnMotherBrainOnionRing(state, state.OnionRingsTargetAngle);
                return null;
            case MotherBrainHeadCue.SpawnBomb bomb:
                SpawnMotherBrainBomb(state, bomb.Afterburns);
                return null;
            case MotherBrainHeadCue.Command { Kind: MotherBrainHeadCueKind.SpawnLaser }:
                SpawnMotherBrainMouthLaser(state);
                return null;
            case MotherBrainHeadCue.Command { Kind: MotherBrainHeadCueKind.CountBabyMetroidAttack }:
                MotherBrainEncounterAccess.RequireLiveMotherBrainRainbowSequence(state).IncrementLiveBabyMetroidAttackCounter();
                return null;
            case MotherBrainHeadCue.Command { Kind: MotherBrainHeadCueKind.ResetBabyMetroidAttacks }:
                MotherBrainEncounterAccess.RequireLiveMotherBrainRainbowSequence(state).ResetLiveBabyMetroidAttackCounter();
                return null;
            case MotherBrainHeadCue.Command { Kind: MotherBrainHeadCueKind.QueueBabyMetroidAttackSound }:
                // The attack cry step skips the attack cry once the attack counter reaches eleven.
                if (MotherBrainEncounterAccess.RequireLiveMotherBrainRainbowSequence(state).Revival.BabyMetroidAttackCounter != 11)
                    _audio.QueueEnemySound(MotherBrainSoundDefinitions.AttackCry, priority: SoundRequestPriority.Normal);
                return null;
            case MotherBrainHeadCue.Command { Kind: MotherBrainHeadCueKind.PrepareRainbowBeamCharge }:
                state.SmallPurpleBreathGenerationEnabled = false;
                _audio.QueueEnemySound(MotherBrainSoundDefinitions.RainbowBeamCharge, priority: SoundRequestPriority.Normal);
                return null;
            case MotherBrainHeadCue.Command { Kind: MotherBrainHeadCueKind.SpawnRainbowBeamChargeParticle }:
                SpawnMotherBrainRainbowChargingProjectile();
                return null;
            // The neutral hold holds the phase-two neutral pose again on fifteen samples in sixteen.
            case MotherBrainHeadCue.Command { Kind: MotherBrainHeadCueKind.MaybeHoldNeutralPhase2 }:
                return MotherBrainRandomDefinitions.NeutralPhase2Holds.IsTakenBy(_room.Random.Read()) ? MotherBrainHeadProgram.NeutralPhase2Hold : null;
            // Retail branches unconditionally here, where the adjacent commentary might suggest a
            // conditional one: the hold's chance alone decides whether the phase-three pose holds again.
            case MotherBrainHeadCue.Command { Kind: MotherBrainHeadCueKind.MaybeHoldNeutralPhase3 }:
                return MotherBrainRandomDefinitions.NeutralPhase3Holds.IsTakenBy(_room.Random.Read()) ? MotherBrainHeadProgram.NeutralPhase3Hold : null;
            // The drool hold returns its destination: only the burst's chance bursts into more drool; every other sample
            // keeps the one-frame loop.
            case MotherBrainHeadCue.Command { Kind: MotherBrainHeadCueKind.MaybeDroolAgain }:
                return MotherBrainRandomDefinitions.DroolBursts.IsTakenBy(_room.Random.Read()) ? MotherBrainHeadProgram.DyingDroolBurst : null;
            default:
                throw new InvalidOperationException($"Undefined Mother Brain head cue {cue}.");
        }
    }

    /// <summary>Fires the mouth laser, freezing the neck.</summary>
    private void SpawnMotherBrainMouthLaser(MotherBrainEnemyState state)
    {
        RoomEnemySlot head = state.Head;
        state.NeckMovementEnabled = false;

        // The bomb spawn gives an explicit mouth coordinate as the common
        // spawn position, selects direction one (right), and invokes
        // the bomb's definition. The source definition remains the head record, so the shared
        // initializer obtains Mother Brain's authored contact damage and
        // her parameter-one speed flag without any encounter-specific substitute.
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
    /// <see cref="OnionRingLowestAim"/> through <see cref="OnionRingHighestAim"/>; only the source coordinate differs.
    /// </summary>
    private static void AimMotherBrainOnionRingsAt(
        MotherBrainEnemyState state,
        Position16<RoomPixels> target)
    {
        RoomEnemySlot head = state.Head;
        Position16<RoomPixels> mouth = head.Position.Whole + new Offset16<RoomPixels>(10, 16);
        TrigonometryAngle angle = EnemyMath.CalculateBearing(target - mouth).ToTableAngle().TurnedBy(TrigonometryAngle.QuarterTurn);

        // The clamp performs two signed comparisons within one turn. The
        // wraparound half, from three quarters of a turn round past zero, maps to the lowest aim; the opposite half maps
        // to the highest. Only the aims between them survive unchanged.
        state.OnionRingsTargetAngle =
            angle >= OnionRingLowestAim && angle < OnionRingHighestAim ? angle
            : angle >= OnionRingHighestAim && angle < OnionRingClampSplit ? OnionRingHighestAim
            : OnionRingLowestAim;
    }

    /// <summary>The speed an onion ring flies at along its aim: four pixels and 80 256ths an update.</summary>
    private static readonly SignedEightEight OnionRingSpeed = SignedEightEight.FromParts(4, 80);

    /// <summary>The lowest aim an onion ring launches at: a sixteenth of a turn.</summary>
    private static readonly TrigonometryAngle OnionRingLowestAim = new(16);

    /// <summary>The highest aim an onion ring launches at: nine thirty-seconds of a turn.</summary>
    private static readonly TrigonometryAngle OnionRingHighestAim = new(72);

    /// <summary>Where the onion-ring aim clamp stops raising aims to the highest and starts lowering them to the lowest: three quarters of a turn.</summary>
    private static readonly TrigonometryAngle OnionRingClampSplit = new(192);

    /// <summary>The sideways speed a Mother Brain bomb's floor bounce restores, without its direction.</summary>
    private static readonly SignedEightEight MotherBrainBombBounceSpeed = SignedEightEight.FromTwoHundredFiftySixthsOfPixel(112);

    /// <summary>Updates an onion ring stays at Mother Brain's mouth before it flies.</summary>
    private const int MotherBrainOnionRingAttachDelay = 8;

    /// <summary>Runs the bomb's initializer.</summary>
    private void SpawnMotherBrainBomb(
        MotherBrainEnemyState state,
        int afterburnCount)
    {
        EnemyProjectile? bomb = _projectiles.AllocateEnemyProjectile();
        if (bomb is null)
            return;

        MotherBrainProjectileStarts.Initialize(
            bomb,
            RoomEnemyProjectileKind.MotherBrainBomb,
            graphics: SpriteGraphics.SharedIn(state.Colours.OwnShading));
        RoomEnemySlot head = state.Head;
        bomb.Position = bomb.Position.WithWhole(new(CoordinateAxis.Wrap(head.Position.X.Whole + 12), CoordinateAxis.Wrap(head.Position.Y.Whole + 16)));
        bomb.Position = bomb.Position.WithFractionX(0).WithFractionY(0);
        bomb.Velocity = new(SignedEightEight.FromTwoHundredFiftySixthsOfPixel(224), SignedEightEight.FromPixels(1));
        bomb.State = new MotherBrainBombState(MotherBrainBombBounceSpeed, afterburnCount);
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

        MotherBrainProjectileStarts.Initialize(
            ring,
            RoomEnemyProjectileKind.MotherBrainOnionRing,
            graphics: SpriteGraphics.SharedIn(state.Colours.OwnShading));
        ring.State = new MotherBrainOnionRingState(MotherBrainOnionRingAttachDelay);
        ring.Velocity = ring.Velocity.WithX(EnemyMath.MultiplySineSample(OnionRingSpeed, angle));
        ring.Velocity = ring.Velocity.WithY(EnemyMath.MultiplySineSample(
            OnionRingSpeed,
            angle.TurnedBy(TrigonometryAngle.QuarterTurn)));

        // The ring's initializer falls through into its attachment step, making the mouth position observable before
        // the projectile scheduler performs the first of eight delayed attachment frames.
        MotherBrainProjectiles.PinMotherBrainOnionRingToMouth(ring, state);
    }

    /// <summary>Runs the head's drool step and the drool's initializer.</summary>
    internal void SpawnMotherBrainDrool(MotherBrainEnemyState state)
    {
        if (!state.DroolGenerationEnabled)
            return;

        int parameter = state.DroolProjectileParameter + 1;
        if (parameter >= 6)
            parameter = 0;
        state.DroolProjectileParameter = parameter;

        EnemyProjectile? drool = _projectiles.AllocateEnemyProjectile();
        if (drool is null)
            return;

        RoomEnemyProjectileKind kind = state.NeckAngleDelta < 128
            ? RoomEnemyProjectileKind.MotherBrainDrool
            : RoomEnemyProjectileKind.MotherBrainDyingDrool;
        MotherBrainProjectileStarts.Initialize(drool, kind, graphics: default);
        drool.State = new MotherBrainDroolState(parameter);

        // Spawning the drool attaches it to the head at once, so its position is already
        // right before the later projectile pass repeats the same attachment.
        _projectileAttacks.RunMotherBrainAttachedDroolFrameBehavior(drool);
    }

    /// <summary>Runs the head's big purple breath step and the breath's initializer.</summary>
    private void SpawnMotherBrainPurpleBreathBig(MotherBrainEnemyState state)
    {
        EnemyProjectile? breath = _projectiles.AllocateEnemyProjectile();
        if (breath is null)
            return;

        MotherBrainProjectileStarts.Initialize(
            breath,
            RoomEnemyProjectileKind.MotherBrainPurpleBreathBig,
            graphics: default);
        RoomEnemySlot head = state.Head;
        breath.Position = breath.Position.WithWhole(new(CoordinateAxis.Wrap(head.Position.X.Whole + 6), CoordinateAxis.Wrap(head.Position.Y.Whole + 16)));
    }

    /// <summary>Runs the small-breath spawn and the breath's initializer.</summary>
    private void SpawnMotherBrainPurpleBreathSmall(MotherBrainEnemyState state)
    {
        EnemyProjectile? breath = _projectiles.AllocateEnemyProjectile();
        if (breath is null)
            return;

        MotherBrainProjectileStarts.Initialize(
            breath,
            RoomEnemyProjectileKind.MotherBrainPurpleBreathSmall,
            graphics: default);
        RoomEnemySlot head = state.Head;
        breath.Position = breath.Position.WithWhole(new(CoordinateAxis.Wrap(head.Position.X.Whole + 6), CoordinateAxis.Wrap(head.Position.Y.Whole + 16)));
        state.SmallPurpleBreathActive = true;
    }

    /// <summary>Runs the rainbow charging projectile's spawn, including the initializer's same-call pin.</summary>
    internal void SpawnMotherBrainRainbowChargingProjectile()
    {
        EnemyProjectile? projectile = _projectiles.AllocateEnemyProjectile();
        if (projectile is null)
            return;

        MotherBrainProjectileStarts.Initialize(
            projectile,
            RoomEnemyProjectileKind.MotherBrainRainbowBeamCharging,
            graphics: default);
        projectile.Velocity = EightEightVector<RoomPixels>.Zero;
        projectile.Position = projectile.Position.WithFractionX(0).WithFractionY(0);
        _projectileAttacks.RunMotherBrainRainbowChargingFrameBehavior(projectile);
    }
}
