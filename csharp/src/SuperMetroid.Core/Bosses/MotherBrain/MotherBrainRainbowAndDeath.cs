using SuperMetroid.Core.Bosses.Artwork;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Escape;
using SuperMetroid.Core.Escape.Artwork;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Mother Brain's rainbow beam, its colours, and her death.</summary>
[RoomLifetime]
internal sealed class MotherBrainRainbowAndDeath
{
    /// <summary>The enemy pass's results, which this behavior writes.</summary>
    private readonly BossFrameResultBuilder _frameResults;
    /// <summary>Provides room services, pictures, colour state, and effects used by the encounter.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Owns the bombs, glass, beams, turrets, and other encounter projectiles.</summary>
    private readonly EnemyProjectileEngine _projectiles;
    /// <summary>Publishes screen shake during the encounter's collapse and attacks.</summary>
    private readonly RoomEarthquake _earthquake;
    /// <summary>Queues the encounter's music and sound effects in enemy update order.</summary>
    private readonly EnemyAudioQueue _audio;
    /// <summary>Draws encounter artwork and applies installed colours.</summary>
    private readonly EnemyPresentation _presentation;
    /// <summary>The current enemy pass's clocks and flags.</summary>
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

    /// <summary>Carries out what one step of her death asked of the room: explosions, fades, music, the escape and its door.</summary>
    internal void ApplyLiveMotherBrainDeath(MotherBrainEnemyState state, MotherBrainRainbowBeamAttackStepResult step)
    {
        foreach (var explosion in step.DeathExplosions)
        {
            SpawnMotherBrainDeathExplosion(state, explosion);
            _motherBrainEncounter.RequestCueSound(explosion.SoundEffect);
        }
        if (step.BrainDetached)
        {
            // Her brain is detached: it keeps the colours it shows, and only the corpse's fade changes them (#1544).
            state.Colours.DetachBrain();
        }
        if (step.BodyFlickers)
        {
            // Her death flickers the body and the background together on every update, not just on colour steps.
            state.DeathHidesBodyLayer = MotherBrainCadenceDefinitions.DeathFadeHidesBodyEverySecondUpdate.IsDue(state.Body.FrameCounter);
            state.Body.Presence = state.DeathHidesBodyLayer
                ? state.Body.Presence.With(EnemyPresence.Hidden)
                : state.Body.Presence.Without(EnemyPresence.Hidden);
            if (step.DeathFadeImage is { } deathFade)
                state.Colours.ShowDeathFade(deathFade);
        }
        if (step.BodyClearRequested)
        {
            // Her faded body is gone: the flicker stops and her body stays hidden, its picture cleared from the background.
            state.DeathHidesBodyLayer = false;
            state.Body.Presence = state.Body.Presence.With(EnemyPresence.Hidden);
            _presentation.RequireBossBody().Clear();
        }
        if (step.CorpseFadeImage is { } corpseFade)
            state.Colours.ShowCorpseFade(corpseFade);
        foreach (var dust in step.CorpseDustRequests)
        {
            _projectiles.SpawnDustOrExplosion(dust.Position, dust.Animation);
            // The rot dust's sound competes at low priority.
            if (dust.SoundEffectQueued)
                _audio.QueueEnemySound(dust.SoundEffect, priority: SoundRequestPriority.Low);
        }
        if (step.MusicStopQueued) _motherBrainEncounter.RequestMusic(MusicRequest.Stop, MusicQueueDelay.Shortest);
        if (step.EscapeMusicQueued) _motherBrainEncounter.RequestMusic(MusicRequest.LoadDataSet(MusicDataSet.RidleyAndDraygonBattles), MusicQueueDelay.Shortest);
        if (step.EscapeMusicTrackQueued) _motherBrainEncounter.RequestMusic(MusicRequest.SelectTrack(MotherBrainDeathDefinitions.EscapeMusicTrack), MusicQueueDelay.Shortest);
        if (step.EscapeTypewriterSetupRequested)
        {
            state.EscapeTypewriter = new(_bossArtwork.EscapeTypewriterPresentation.Get(EscapeTypewriterMessageId.Zebes));
            // The warning shows over the background from the room's top-left corner, where her locked arena's camera stays
            // (#1802).
            state.BodyPlacement = MotherBrainBodyPlacement.AtRoomCorner;
        }
        if (step.EscapeDoorExplosion is { } doorDust)
        {
            _projectiles.SpawnDustOrExplosion(doorDust.Position, doorDust.Animation);
            // The door explosion's sound competes at low priority.
            _audio.QueueEnemySound(doorDust.SoundEffect, priority: SoundRequestPriority.Low);
        }
        if (step.EscapeDoorObject is { } doorObject)
            _access.RequestMotherBrainObject(doorObject.Block, doorObject.RoomObject);
        foreach (var request in step.EscapeDoorParticleSpawns) SpawnMotherBrainDoorFragment(request.Fragment);
        if (step.TimeBombSetSubtitleSpawnRequested)
        {
            var subtitle = _projectiles.AllocateEnemyProjectile();
            if (subtitle is not null)
            {
                EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(subtitle, RoomEnemyProjectileKind.MotherBrainEscapeSubtitle,
                    _projectileAttacks.Projectile, graphics: default);
                subtitle.StartAnimation(new MotherBrainEffectAnimation(MotherBrainEffectSegment.EscapeSubtitle));
                MotherBrainProjectiles.PinMotherBrainEscapeSubtitle(subtitle);
            }
        }
    }

    /// <summary>Places a death explosion with the requested position and visual phase.</summary>
    internal void SpawnMotherBrainDeathExplosion(MotherBrainEnemyState state, MotherBrainDeathExplosionRequest request)
    {
        DustExplosionAnimation animation = MotherBrainDeathExplosionDefinitions.Animations[request.Animation];
        var projectile = _projectiles.AllocateEnemyProjectile();
        if (projectile is null) return; // Shared-pool exhaustion drops the spawn.
        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(projectile, RoomEnemyProjectileKind.MotherBrainDeathExplosion,
            _projectileAttacks.Projectile, graphics: default);
        projectile.StartAnimation(new MiscDustProjectileAnimation(animation));
        projectile.State = new MotherBrainBodyExplosionState(state, request.Offset);
        projectile.Velocity = FollowingExplosionLeftVelocity.Of(request.Offset);
        MotherBrainProjectiles.MotherBrainProjectileBehavior.RunMotherBrainDeathExplosion(projectile);
    }

    /// <summary>Spawns the falling door fragment <paramref name="definition"/>.</summary>
    internal void SpawnMotherBrainDoorFragment(MotherBrainDoorFragmentDefinition definition)
    {
        var fragment = _projectiles.AllocateEnemyProjectile();
        if (fragment is null) return;
        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(fragment, RoomEnemyProjectileKind.MotherBrainEscapeDoorFragment,
            _projectileAttacks.Projectile, graphics: default);
        fragment.StartAnimation(new MotherBrainEffectAnimation(MotherBrainEffectSegment.EscapeDoorParticle));
        fragment.Position = fragment.Position.WithWhole(new Position16<RoomPixels>(MotherBrainDeathDefinitions.DoorX, MotherBrainDeathDefinitions.DoorY) + definition.Offset);
        fragment.Velocity = definition.Velocity;
        fragment.State = new MotherBrainDoorFragmentState();
    }

    /// <summary>Shows a glyph the Zebes escape typewriter typed over Mother Brain's room background (#1348, #1555).</summary>
    private void WriteZebesTypewriterGlyph(EscapeTypewriterGlyph glyph) =>
        _motherBrainEncounter.ShowZebesWarning(new WrittenPicture(glyph.Picture, glyph.Corner));

    /// <summary>Runs one body-phase update through the shared sequence.</summary>
    internal void RunLiveMotherBrainRainbowBeam(
        MotherBrainEnemyState state,
        SamusState samus,
        int enemyFrameCounter,
        SamusBombProjectileSystem sharedProjectiles)
    {
        MotherBrainRainbowBeamAttackSequence sequence = state.RainbowBeamSequence ??
            throw new InvalidOperationException(
                "Mother Brain entered a rainbow phase without its live sequence owner.");

        bool typewriterFinished = false;
        if (state.Stage == MotherBrainBodyStage.Dying && state.DeathPhase is MotherBrainDeathPhase.TypeOutZebesEscapeText or
            MotherBrainDeathPhase.SpawnTimeBombSetSubtitle)
        {
            var text = state.EscapeTypewriter ?? throw new InvalidOperationException("Mother Brain escape text was not initialized.");
            typewriterFinished = text.Step(out EscapeTypewriterGlyph? glyph);
            if (glyph is { } typed)
                WriteZebesTypewriterGlyph(typed);
            if (text.ClickRequested) _motherBrainEncounter.RequestCueSound(EscapeTypewriterDefinitions.ZebesClick);
        }
        MotherBrainRainbowBeamAttackStepResult step = sequence.Step(samus, new RainbowBeamAttackFrame { EnemyFrameCounter = enemyFrameCounter, EnemyPassCount = _frameContext.RandomEnemyCounter, PowerBombActive = sharedProjectiles.PowerBombExplosion.IsActive, RandomNumberSeed = MotherBrainRandomSample.FromGenerator(_room.Random.Read()), NextRandomNumber = _room.Random.Generate, AlternateEscapeText = _room.Session.JapaneseText, TypewriterFinished = typewriterFinished, RoomEarthquakeStopped = !_earthquake.IsShaking, CameraCheckpoint = _room.Session.RainbowBeamCameraCheckpoint });
        _frameResults.MotherBrainDeathStep = step;
        ApplyLiveMotherBrainRainbowState(state, sequence, step, samus, sharedProjectiles);
    }

    /// <summary>
    /// Carries out what one step of the late fight asked of the room: colours, the beam's sound and earthquake, Samus's
    /// cooldown, the Baby's spawn, beam explosions and her death's requests. The phases wrote her own state in place.
    /// </summary>
    internal void ApplyLiveMotherBrainRainbowState(
        MotherBrainEnemyState state,
        MotherBrainRainbowBeamAttackSequence sequence,
        MotherBrainRainbowBeamAttackStepResult current,
        SamusState samus,
        SamusBombProjectileSystem sharedProjectiles)
    {
        Ensure.NotNull(state);
        Ensure.NotNull(sequence);
        Ensure.NotNull(sharedProjectiles);
        if (current.FinalBeamFired)
            state.NextRainbowImage = MotherBrainRainbowImage.First;
        ApplyMotherBrainBeamColours(state, current);
        // The brain's step runs after the body's. Its health check is enabled during revival, but the health colours wait
        // until the corpse has revived.
        if (sequence.State.HealthColoursEnabled && sequence.State.Phase2CorpseState == MotherBrainPhase2CorpseState.Revived)
            state.Colours.ShowHealth(MotherBrainHealthBand.Of(state.Head.Health));
        ApplyLiveMotherBrainDeath(state, current);
        if (current.SoundQueued)
            // A request is not invariably a start: the painful-walk handoff
            // stops the same looping sound. Preserve the start or stop the sequence selected.
            _motherBrainEncounter.RequestRainbowBeamSound(sequence.State.RainbowBeamSoundPlaying
                ? MotherBrainSoundDefinitions.StartRainbowDrain
                : MotherBrainSoundDefinitions.StopRainbowDrain);
        if (current.ChargeSoundQueued || current.FinalBeamFired)
            _motherBrainEncounter.RequestBodySound(EnemySound.Silence);
        if (current.Explosion is not null)
            _motherBrainEncounter.RequestBodySound(EnemySound.SmallExplosion);

        // The step's earthquake goes to the room's earthquake, its one owner: the rainbow start seeds the short quake,
        // the drain start replaces it with the full drain duration, the finish stops it when the beam contracts below
        // its minimum width, and the death sequence starts the brain's landing quake and the escape's.
        current.Earthquake?.ApplyTo(_earthquake);
        if (current.BeamEnded)
            _motherBrainEncounter.RequestRainbowBeamSound(MotherBrainSoundDefinitions.StopRainbowDrain);

        // The attack gives Samus's shared projectile cooldown as its neck lowers, as the beam starts and as it ends;
        // her next update's firing reads it.
        if (current.SamusProjectileCooldown is int samusCooldown)
            sharedProjectiles.SetSharedCooldown(samusCooldown);

        if (current.BabySpawnRequested)
            _roomActors.SpawnBabyMetroid(state);
        if (current.Explosion is { } explosion)
        {
            // The rainbow explosion spawn runs before the rainbow's movement steps move Samus. The reusable sequence
            // captures that pre-move point in its typed movement result, so use it for the
            // setup and let the explosion attach to the current position on the later
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

        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
            projectile,
            RoomEnemyProjectileKind.MotherBrainRainbowBeamExplosion,
            _projectileAttacks.Projectile,
            graphics: default);
        projectile.StartAnimation(new MiscDustProjectileAnimation(DustExplosionAnimation.MotherBrainRainbowBeamExplosion));
        projectile.State = new MotherBrainFollowingExplosionState(request.Offset);
        projectile.Velocity = FollowingExplosionLeftVelocity.Of(request.Offset);
        projectile.Position = projectile.Position.WithWhole(samusBeforeMovement + request.Offset);
    }

    /// <summary>Shows the step her beam's colours, the drain or the revival reach this update, the rainbow at the body-frame cadence.</summary>
    private static void ApplyMotherBrainBeamColours(MotherBrainEnemyState state,
        MotherBrainRainbowBeamAttackStepResult step)
    {
        // Her revival shows over the drain's last grey step, which it leaves showing in two of her colours.
        if (step.DrainColourImage is { } drain)
        {
            state.Colours.ShowDrain(drain);
            return;
        }
        if (step.RevivalColourImage is { } revival)
        {
            state.Colours.ShowRevival(revival);
            return;
        }
        if (step.BeamEnded)
        {
            state.Colours.ShowRestored();
            return;
        }
        if (step.DrainedBeamRanOut)
        {
            state.Colours.ShowRainbow(MotherBrainRainbowImage.Drained);
            return;
        }
        // The rainbow colours' handler steps them only on its phase of the body's update count.
        if (!step.RainbowColours || !MotherBrainCadenceDefinitions.RainbowColoursStep.IsSetIn(state.Body.FrameCounter))
            return;

        MotherBrainRainbowImage image = state.NextRainbowImage;
        state.NextRainbowImage = image.Next;
        state.Colours.ShowRainbow(image);
    }
}
