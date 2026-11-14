using SuperMetroid.Core.Bosses.Artwork;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Escape;
using SuperMetroid.Core.Escape.Artwork;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Mother Brain's rainbow beam, its palette, and her death.</summary>
[RoomLifetime]
internal sealed class MotherBrainRainbowAndDeath
{
    /// <summary>The enemy pass's results, which this behavior writes.</summary>
    private readonly BossFrameResultBuilder _frameResults;
    /// <summary>Provides room services, pictures, palette state, and effects used by the encounter.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Owns the bombs, glass, beams, turrets, and other encounter projectiles.</summary>
    private readonly EnemyProjectileEngine _projectiles;
    /// <summary>Publishes screen shake during the encounter's collapse and attacks.</summary>
    private readonly RoomEarthquake _earthquake;
    /// <summary>Queues the encounter's music and sound effects in enemy update order.</summary>
    private readonly EnemyAudioQueue _audio;
    /// <summary>Draws encounter artwork and applies installed palette data.</summary>
    private readonly EnemyPresentation _presentation;
    /// <summary>Supplies Samus and camera state for the current enemy frame.</summary>
    private readonly EnemyFrameContext _frameContext;
    /// <summary>Holds the shared body/head state for the current Mother Brain room.</summary>
    private readonly MotherBrainEncounter _motherBrainEncounter;
    /// <summary>The other actors in the room: the Baby, the Rinkas, the Pirate laser and the afterburn.</summary>
    private readonly IMotherBrainRoomActors _roomActors;
    /// <summary>Mother Brain's projectiles: bombs, onion rings, drool, lasers, hand beams, turret shots and the tank's glass.</summary>
    private readonly MotherBrainProjectiles _projectileAttacks;
    /// <summary>Mother Brain's state lookups, body and brain animation setters, timers and room-object requests.</summary>
    private readonly MotherBrainEncounterAccess _access;
    /// <summary>The session's installed boss artwork.</summary>
    private readonly BossArtworkBinding _bossArtwork;


    /// <summary>Builds the component from the collaborators it uses.</summary>
    internal MotherBrainRainbowAndDeath(MotherBrainRainbowAndDeathDependencies dependencies)
    {
        _bossArtwork = dependencies.BossArtwork;
        _frameResults = dependencies.FrameResults;
        _room = dependencies.Room;
        _projectiles = dependencies.Projectiles;
        _earthquake = dependencies.Earthquake;
        _audio = dependencies.Audio;
        _presentation = dependencies.Presentation;
        _frameContext = dependencies.FrameContext;
        _motherBrainEncounter = dependencies.MotherBrainEncounter;
        _roomActors = dependencies.RoomActors;
        _projectileAttacks = dependencies.ProjectileAttacks;
        _access = dependencies.Access;
    }

    /// <summary>Consumes death actor events exactly once, alongside the live rainbow adapter.</summary>
    internal void ApplyLiveMotherBrainDeath(MotherBrainEnemyState state,
        MotherBrainRainbowBeamAttackSequence sequence, MotherBrainRainbowBeamAttackStepResult step)
    {
        foreach (var explosion in step.DeathExplosions)
        {
            SpawnMotherBrainDeathExplosion(explosion);
            _motherBrainEncounter.RequestCueSound(explosion.SoundEffect);
        }
        if (step.PhaseBefore == MotherBrainBodyPhase.ThirdPhaseDeathDisableBrainEffects &&
            step.PhaseAfter != step.PhaseBefore)
        {
            // Her brain is detached: it keeps the colours it shows, and only the corpse's fade changes them (#1544).
            state.Colours.DetachBrain();
            state.DroolGenerationEnabled = false;
        }
        bool bodyFade = step.PhaseBefore == MotherBrainBodyPhase.ThirdPhaseDeathFadeOutBody ||
            step.PhaseAfter == MotherBrainBodyPhase.ThirdPhaseDeathFadeOutBody;
        if (bodyFade)
        {
            // ADF41C flickers the body and the background together on every call, not just palette ticks.
            state.DeathHidesBodyLayer = MotherBrainCadenceDefinitions.DeathFadeHidesBodyEverySecondUpdate.IsDue(state.Body.FrameCounter);
            state.Body.Presence = state.DeathHidesBodyLayer
                ? state.Body.Presence.With(EnemyPresence.Hidden)
                : state.Body.Presence.Without(EnemyPresence.Hidden);
            if (step.ColoursRequested)
                state.Colours.ShowDeathFade(sequence.State.GreyTransitionCounter - 1);
        }
        if (sequence.ThirdPhaseDeath.BodyClearRequested && !state.DeathBodyLayerCleared)
        {
            state.DeathBodyLayerCleared = true;
            state.DeathHidesBodyLayer = false;
            state.Body.Contact = sequence.ThirdPhaseDeath.BodyContact;
            state.Body.Processing = sequence.ThirdPhaseDeath.BodyProcessing;
            state.Body.Presence = sequence.ThirdPhaseDeath.BodyPresence;
            _presentation.RequireBossBody().Clear();
        }
        if (sequence.ThirdPhaseDeath.BrainDrawSetupRequested)
        {
            // Decapitation transfers position ownership from the articulated neck to the
            // death sequence. Otherwise the next head AI call overwrites falling Y again.
            state.BrainAction = MotherBrainBrainAction.SetupBrainToBeDrawn;
            state.Head.Position = state.Head.Position.WithWhole(sequence.State.BrainPosition);
        }
        if (step.PhaseBefore == MotherBrainBodyPhase.ThirdPhaseDeathFadeToGrey && step.ColoursRequested)
            state.Colours.ShowCorpseFade(sequence.State.GreyTransitionCounter - 1);
        foreach (var dust in step.CorpseDustRequests)
        {
            _projectiles.SpawnRoomGraphicsDustExplosion(dust.Position, dust.Animation);
            // The rot dust's sound competes at low priority.
            if (dust.SoundEffectQueued)
                _audio.QueueEnemySound(dust.SoundEffect, priority: SoundRequestPriority.Low);
        }
        if (step.MusicStopQueued) _motherBrainEncounter.RequestMusic(MusicRequest.Stop, MusicQueueDelay.Shortest);
        if (step.EscapeMusicQueued) _motherBrainEncounter.RequestMusic(MusicRequest.LoadDataSet(MusicDataSet.RidleyAndDraygonBattles), MusicQueueDelay.Shortest);
        if (step.EscapeMusicTrackQueued) _motherBrainEncounter.RequestMusic(MusicRequest.SelectTrack(MotherBrainDeathDefinitions.EscapeMusicTrack), MusicQueueDelay.Shortest);
        if (step.EscapeTypewriterSetupRequested)
        {
            state.EscapeTypewriter = _bossArtwork.EscapeTypewriterPresentation is { } presentation
                ? new(presentation.Get(EscapeTypewriterMessageId.Zebes))
                : new(EscapeTypewriterMessageId.Zebes);
            state.BodyLayerPlacement = Position16<BackgroundPixels>.Zero;
        }
        if (step.EscapeDoorExplosion is { } doorDust)
        {
            _projectiles.SpawnRoomGraphicsDustExplosion(doorDust.Position, doorDust.Animation);
            // The door explosion's sound competes at low priority.
            _audio.QueueEnemySound(doorDust.SoundEffect, priority: SoundRequestPriority.Low);
        }
        if (step.EscapeDoorObject is { } doorObject)
            _access.RequestMotherBrainObject(doorObject.Block, doorObject.RoomObject);
        foreach (var request in step.EscapeDoorParticleSpawns) SpawnMotherBrainDoorFragment(request.Parameter);
        if (step.TimeBombSetSubtitleSpawnRequested)
        {
            var subtitle = _projectiles.AllocateEnemyProjectile();
            if (subtitle is not null)
            {
                MotherBrainProjectileStarts.Initialize(subtitle, RoomEnemyProjectileKind.MotherBrainEscapeSubtitle, graphics: default);
                MotherBrainProjectiles.PinMotherBrainEscapeSubtitle(subtitle);
            }
        }
    }

    /// <summary>Places a death explosion with the requested position and visual phase.</summary>
    internal void SpawnMotherBrainDeathExplosion(MotherBrainDeathExplosionRequest request)
    {
        MiscDustProjectileSegment segment = MotherBrainDeathExplosions.Animation(request.ProjectileParameter);
        var projectile = _projectiles.AllocateEnemyProjectile();
        if (projectile is null) return; // Shared-pool exhaustion drops the spawn.
        MotherBrainProjectileStarts.Initialize(projectile, RoomEnemyProjectileKind.MotherBrainDeathExplosion, graphics: default);
        projectile.StartAnimation(new MiscDustProjectileAnimation(segment));
        projectile.State = new MotherBrainFollowingExplosionState(request.Offset);
        projectile.Velocity = FollowingExplosionLeftVelocity.Of(request.Offset);
        _projectileAttacks.RunMotherBrainDeathExplosion(projectile);
    }

    /// <summary>Spawns a falling door fragment from the parameter identity.</summary>
    internal void SpawnMotherBrainDoorFragment(int parameter)
    {
        MotherBrainDoorFragmentDefinition definition =
            MotherBrainDoorFragments.ForParameter(parameter);
        var fragment = _projectiles.AllocateEnemyProjectile();
        if (fragment is null) return;
        MotherBrainProjectileStarts.Initialize(fragment, RoomEnemyProjectileKind.MotherBrainEscapeDoorFragment, graphics: default);
        fragment.Position = fragment.Position.WithWhole(new Position16<RoomPixels>(MotherBrainDeathDefinitions.DoorX, MotherBrainDeathDefinitions.DoorY) + definition.Offset);
        fragment.Velocity = definition.Velocity;
        fragment.State = new MotherBrainDoorFragmentState();
    }

    /// <summary>Shows a glyph the Zebes escape typewriter typed over Mother Brain's room background (#1348, #1555).</summary>
    private void WriteZebesTypewriterGlyph(EscapeTypewriterGlyph glyph) =>
        _motherBrainEncounter.ShowZebesWarning(new WrittenPicture(glyph.Picture, glyph.Corner));

    /// <summary>Runs one body-phase call through the shared verified sequence.</summary>
    internal void RunLiveMotherBrainRainbowBeam(
        MotherBrainEnemyState state,
        SamusState samus,
        int enemyFrameCounter,
        SamusBombProjectileSystem? sharedProjectiles)
    {
        MotherBrainRainbowBeamAttackSequence sequence = state.RainbowBeamSequence ??
            throw new InvalidOperationException(
                "Mother Brain entered a rainbow phase without its live sequence owner.");

        // The body animation and articulated-head AI ran through the physical room records on
        // previous frames. Import that state before dispatch so walk completion and neck
        // aiming are decided from the same state the game would read.
        SynchronizeLiveMotherBrainRainbowActor(state, sequence);

        bool typewriterFinished = false;
        if (sequence.State.Phase is MotherBrainBodyPhase.ThirdPhaseDeathTypeOutZebesEscapeText or
            MotherBrainBodyPhase.ThirdPhaseDeathSpawnTimeBombSetSubtitle)
        {
            var text = state.EscapeTypewriter ?? throw new InvalidOperationException("Mother Brain escape text was not initialized.");
            typewriterFinished = text.Step(out EscapeTypewriterGlyph? glyph);
            if (glyph is { } typed)
                WriteZebesTypewriterGlyph(typed);
            if (text.ClickRequested) _motherBrainEncounter.RequestCueSound(EscapeTypewriterDefinitions.ZebesClick);
        }
        MotherBrainRainbowBeamAttackStepResult step = sequence.Step(samus, new RainbowBeamAttackFrame { EnemyFrameCounter = enemyFrameCounter, MainEnemyExecutionCounter = _frameContext.RandomEnemyCounter, PowerBombActive = sharedProjectiles?.PowerBombExplosion.IsActive != false, RandomNumberSeed = MotherBrainRandomSample.FromGenerator(_room.Random.Read()), NextRandomNumber = _room.Random.Generate, AlternateEscapeText = _room.Session.JapaneseText, TypewriterFinished = typewriterFinished, RoomEarthquakeStopped = !_earthquake.IsShaking, CameraCheckpoint = _room.RainbowBeamCameraCheckpoint });
        _frameResults.MotherBrainDeathStep = step;
        ApplyLiveMotherBrainRainbowState(state, sequence, step, samus, sharedProjectiles);
    }

    /// <summary>Copies rainbow-beam phase state into the live actor slot.</summary>
    internal static void SynchronizeLiveMotherBrainRainbowActor(
        MotherBrainEnemyState state,
        MotherBrainRainbowBeamAttackSequence sequence)
    {
        RoomEnemySlot head = state.Head;
        sequence.SynchronizeLiveActor(new MotherBrainLiveActorSnapshot { Body = state.Body.Position.Whole, BodyPose = state.Pose, Form = state.Form, BodyContact = state.Body.Contact, BodyProcessing = state.Body.Processing, BodyPresence = state.Body.Presence, BodyCollidesByFrameHitboxes = state.Body.CollidesByFrameHitboxes, BodyShowsNewFrame = state.Body.AnimationClock.ShowsNewFrame, Brain = head.Position.Whole, BrainHealth = head.Health, BrainContact = head.Contact, BrainProcessing = head.Processing, BrainPresence = head.Presence, BrainCollidesByFrameHitboxes = head.CollidesByFrameHitboxes, BrainShowsNewFrame = head.AnimationClock.ShowsNewFrame, NeckMovementEnabled = state.NeckMovementEnabled, LowerNeckMovement = state.LowerNeckMovement, UpperNeckMovement = state.UpperNeckMovement, HitboxesEnabled = state.HitboxesEnabled != MotherBrainHitboxParts.None });
    }

    /// <summary>
    /// Publishes only values produced by the current body-AI call. In particular, cooldown
    /// and earthquake timers are event writes rather than continuously mirrored values;
    /// continuously copying the sequence's retained value would undo the global owners'
    /// normal per-frame decrements.
    /// </summary>
    internal void ApplyLiveMotherBrainRainbowState(
        MotherBrainEnemyState state,
        MotherBrainRainbowBeamAttackSequence sequence,
        MotherBrainRainbowBeamAttackStepResult? step,
        SamusState samus,
        SamusBombProjectileSystem? sharedProjectiles)
    {
        RoomEnemySlot head = state.Head;

        // The brain-animation request resets the physical animation's timer only when body AI requests a new animation.
        // The dedicated latch is essential because the running animation advances away from
        // the animation's start while the request itself remains unchanged.
        if (sequence.State.HeadAnimation is { } headSegment &&
            (step is { HeadAnimationRequested: true } ||
             headSegment != state.RainbowAppliedHeadAnimation))
        {
            MotherBrainEncounterAccess.SetMotherBrainBrainAnimation(state, headSegment);
            state.RainbowAppliedHeadAnimation = headSegment;
        }

        if (step is { } result &&
            (result.BodyWalkRequested || result.BodyPostureRequested))
        {
            MotherBrainBodySegment bodySegment = sequence.RequestedBodyAnimation ?? throw new InvalidDataException(
                "Rainbow body AI requested animation without a body program.");
            MotherBrainEncounterAccess.SetMotherBrainBodyAnimation(state, bodySegment);
        }

        state.Phase = sequence.State.Phase ?? throw new InvalidDataException("The live Mother Brain rainbow sequence has no body phase yet.");
        state.PhaseTimer = sequence.State.PhaseTimer;
        state.NeckAngleDelta = sequence.State.NeckAngleDelta;
        state.NeckMovementEnabled = sequence.State.NeckMovementEnabled;
        state.LowerNeckMovement = sequence.State.LowerNeckMovement;
        state.UpperNeckMovement = sequence.State.UpperNeckMovement;
        if (sequence.ThirdPhaseFight.Phase3NeckPhase == MotherBrainPhase3NeckPhase.HyperBeamRecoil &&
            sequence.ThirdPhaseFight.Phase3NeckRecoilTimer == 10)
        {
            // The rainbow setup loads fifty during the one-shot setup call, immediately before the
            // fallthrough decrements neck timer eleven to ten. Her brain's draw owns
            // subsequent shake decrements, so publish this event only on that exact setup
            // result rather than copying the sequence's retained value every enemy frame.
            state.BrainMainShakeTimer = 50;
        }
        state.Form = sequence.State.Body.Form;
        state.BodySubPhaseTimer = sequence.ThirdPhaseDeath.DeathAndEscapeExplosionIndex;
        state.Body.Contact = sequence.ThirdPhaseDeath.BodyContact;
        state.Body.Processing = sequence.ThirdPhaseDeath.BodyProcessing;
        state.Body.Presence = sequence.ThirdPhaseDeath.BodyPresence;
        state.Body.CollidesByFrameHitboxes = sequence.ThirdPhaseDeath.BodyCollidesByFrameHitboxes;
        state.Body.AnimationClock.ShowsNewFrame = sequence.ThirdPhaseDeath.BodyShowsNewFrame;
        head.Health = sequence.State.BrainHealth;
        head.Contact = sequence.ThirdPhaseDeath.BrainContact;
        head.Processing = sequence.ThirdPhaseDeath.BrainProcessing;
        head.Presence = sequence.ThirdPhaseDeath.BrainPresence;
        head.CollidesByFrameHitboxes = sequence.ThirdPhaseDeath.BrainCollidesByFrameHitboxes;
        head.AnimationClock.ShowsNewFrame = sequence.ThirdPhaseDeath.BrainShowsNewFrame;
        // The live flag begins at two, while the reusable sequence needs only its Boolean
        // branch meaning. Preserve every nonzero value; only an explicit clear
        // is allowed to collapse it to zero.
        if (!sequence.ThirdPhaseDeath.HitboxesEnabled)
            state.HitboxesEnabled = MotherBrainHitboxParts.None;
        if (sequence.State.SmallPurpleBreathGenerationWriteCount != state.RainbowAppliedSmallPurpleBreathWrites)
        {
            state.SmallPurpleBreathGenerationEnabled = sequence.State.SmallPurpleBreathGenerationEnabled;
            state.RainbowAppliedSmallPurpleBreathWrites = sequence.State.SmallPurpleBreathGenerationWriteCount;
        }
        state.DroolGenerationEnabled = sequence.State.DroolGenerationEnabled;

        // These values drive the rainbow beam effect. The beam renderer is deliberately fed
        // from this single aim calculation.
        state.RainbowBeamActive = sequence.RainbowAttack.BeamActive;
        state.RainbowBeamAngle = sequence.RainbowBeamAngle;
        state.RainbowBeamAngularWidth = sequence.RainbowAttack.AngularWidth;

        if (step is not { } current)
            return;

        if (current.PhaseBefore == MotherBrainBodyPhase.SecondPhaseFinishSamusOffFireFinalBeam &&
            current.PhaseAfter == MotherBrainBodyPhase.SecondPhaseFinalRainbowBeamHolding)
            state.NextRainbowFrame = 0;
        ApplyMotherBrainBeamColours(state, current);
        // The brain-slot handler runs after body AI. Its health handler is enabled during revival,
        // but the health palette waits until the corpse-state value reaches final-combat state 2.
        if (sequence.State.HealthColoursEnabled && sequence.State.Phase2CorpseState == MotherBrainPhase2CorpseState.Revived)
            state.Colours.ShowHealth(MotherBrainHealthPalette.Band(state.Head.Health));
        ApplyLiveMotherBrainDeath(state, sequence, current);
        if (current.SoundQueued)
            // A request is not invariably a start: the painful-walk stage-six handoff
            // stops the same looping voice. Preserve the start or stop the sequence selected.
            _motherBrainEncounter.RequestRainbowBeamSound(sequence.State.RainbowBeamSoundPlaying
                ? MotherBrainSoundDefinitions.StartRainbowDrain
                : MotherBrainSoundDefinitions.StopRainbowDrain);
        if (current.ChargeSoundQueued || current.FinalBeamSoundQueued)
            _motherBrainEncounter.RequestBodySound(EnemySound.Silence);
        if (current.Explosion is not null)
            _motherBrainEncounter.RequestBodySound(EnemySound.SmallExplosion);

        // The step's earthquake goes to the room's earthquake, its one owner: the rainbow start seeds the short quake,
        // the drain start replaces it with the full drain duration, the finish stops it when the beam contracts below
        // its minimum width, and the death sequence starts the brain's landing quake and the escape's.
        current.Earthquake?.ApplyTo(_earthquake);
        if (current.PhaseBefore == MotherBrainBodyPhase.SecondPhaseRainbowFinishFiring &&
            current.PhaseAfter == MotherBrainBodyPhase.SecondPhaseRainbowLetSamusFall)
            _motherBrainEncounter.RequestRainbowBeamSound(MotherBrainSoundDefinitions.StopRainbowDrain);

        // The attack gives Samus's shared projectile cooldown as its neck lowers, as the beam starts and as it ends;
        // her next update's firing reads it.
        if (sharedProjectiles is not null && current.SamusProjectileCooldown is int samusCooldown)
            sharedProjectiles.SetSharedCooldown(samusCooldown);

        if (current.BabySpawnRequested)
            _roomActors.SpawnBabyMetroid(state);
        if (current.Explosion is { } explosion)
        {
            // The rainbow explosion spawn runs before the rainbow's movement steps move Samus. The reusable sequence
            // captures that pre-move point in its typed movement result, so use it for the
            // initializer and let the explosion attach to the current position on the later
            // projectile pass. Finish-firing has no movement and therefore uses the unchanged actor.
            Position16<RoomPixels> samusBeforeMovement = current.Movement?.Before.Whole ?? samus.Position.Whole;
            SpawnMotherBrainRainbowExplosion(samusBeforeMovement, explosion);
        }
    }

    /// <summary>Spawns a rainbow explosion that follows Samus at the requested offset.</summary>
    internal void SpawnMotherBrainRainbowExplosion(
        Position16<RoomPixels> samusBeforeMovement,
        MotherBrainRainbowExplosionRequest request)
    {
        EnemyProjectile? projectile = _projectiles.AllocateEnemyProjectile();
        if (projectile is null)
            return;

        MotherBrainProjectileStarts.Initialize(
            projectile,
            RoomEnemyProjectileKind.MotherBrainRainbowBeamExplosion,
            graphics: default);
        projectile.State = new MotherBrainFollowingExplosionState(request.Offset);
        projectile.Velocity = FollowingExplosionLeftVelocity.Of(request.Offset);
        projectile.Position = projectile.Position.WithWhole(samusBeforeMovement + request.Offset);
    }

    /// <summary>Shows the step her beam's colours, the drain or the revival reach this update, the rainbow at the body-frame cadence.</summary>
    private static void ApplyMotherBrainBeamColours(MotherBrainEnemyState state,
        MotherBrainRainbowBeamAttackStepResult step)
    {
        bool draining = step.PhaseBefore == MotherBrainBodyPhase.SecondPhaseDrainedByBabyTransitionToGrey;
        bool reviving = step.PhaseBefore == MotherBrainBodyPhase.SecondPhaseReviveTransitionFromGrey;
        if ((draining || reviving) && step.ColoursRequested)
        {
            // The sequence increments before publishing this request. Her revival shows over the drain's last grey
            // step, which it leaves showing in two of her colours.
            int index = MotherBrainEncounterAccess.RequireLiveMotherBrainRainbowSequence(state).State.GreyTransitionCounter - 1;
            if (draining)
                state.Colours.ShowDrain(index);
            else
                state.Colours.ShowRevival(index);
            return;
        }
        if (step.PhaseBefore == MotherBrainBodyPhase.SecondPhaseRainbowFinishFiring &&
            step.PhaseAfter == MotherBrainBodyPhase.SecondPhaseRainbowLetSamusFall)
        {
            state.Colours.ShowRestored();
            return;
        }
        if (step.PhaseBefore == MotherBrainBodyPhase.SecondPhaseDrainedByBabyFiringRainbowBeam &&
            step.PhaseAfter == MotherBrainBodyPhase.SecondPhaseDrainedByBabyRainbowBeamRunOut)
        {
            state.Colours.ShowRainbow(MotherBrainRainbowPaletteDefinitions.DrainedRainbowFrame);
            return;
        }
        bool rainbowPhase = step.PhaseBefore is
            MotherBrainBodyPhase.SecondPhaseRainbowMoveSamusTowardWall or
            MotherBrainBodyPhase.SecondPhaseRainbowOneFrameDelay or
            MotherBrainBodyPhase.SecondPhaseRainbowStartDrainingSamus or
            MotherBrainBodyPhase.SecondPhaseRainbowDrainingSamus or
            MotherBrainBodyPhase.SecondPhaseRainbowFinishFiring or
            MotherBrainBodyPhase.SecondPhaseDrainedByBabyRegainBalance or
            MotherBrainBodyPhase.SecondPhaseDrainedByBabyFiringRainbowBeam;
        if (!rainbowPhase || !step.ColoursRequested || !MotherBrainCadenceDefinitions.RainbowColoursStep.IsSetIn(state.Body.FrameCounter))
            return;

        int frame = state.NextRainbowFrame;
        state.NextRainbowFrame = (frame + 1) % MotherBrainRainbowPaletteFormat.RainbowFrameCount;
        state.Colours.ShowRainbow(frame);
    }
}
