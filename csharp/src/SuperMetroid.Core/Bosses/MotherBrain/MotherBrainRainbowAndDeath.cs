using SuperMetroid.Core.Bosses.Artwork;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Escape;
using SuperMetroid.Core.Escape.Artwork;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Mother Brain's rainbow beam, its palette, and her death.</summary>
[RoomLifetime]
internal sealed class MotherBrainRainbowAndDeath
{
    /// <summary>The enemy pass's results, which this behavior writes.</summary>
    private readonly BossFrameResultBuilder _frameResults;
    /// <summary>Provides room scratch memory, tiles, palette state, and effects used by the encounter.</summary>
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
    /// <summary>Mother Brain's state lookups, body and brain program setters, timers and PLM requests.</summary>
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
            state.LastSoundEffectLibrary3 = explosion.SoundEffect;
        }
        if (step.PhaseBefore == MotherBrainBodyFunction.ThirdPhaseDeathDisableBrainEffects &&
            step.PhaseAfter != step.PhaseBefore)
        {
            _presentation.BossPalettes().MotherBrainCorpse.CopyFrom(_presentation.BossPalettes().MotherBrainBrain.Colors);
            state.BrainPalette = sequence.ThirdPhaseDeath.BrainPalette;
            state.BrainPaletteHandlingEnabled = false;
            state.DroolGenerationEnabled = false;
        }
        bool bodyFade = step.PhaseBefore == MotherBrainBodyFunction.ThirdPhaseDeathFadeOutBody ||
            step.PhaseAfter == MotherBrainBodyFunction.ThirdPhaseDeathFadeOutBody;
        if (bodyFade)
        {
            // ADF41C flickers the body and BG2 together on every call, not just palette ticks.
            state.DeathBg2Hidden = (state.Body.FrameCounter & 1) == 0;
            state.Body.Properties = state.DeathBg2Hidden
                ? state.Body.Properties.With(EnemyProperties.Invisible)
                : state.Body.Properties.Without(EnemyProperties.Invisible);
            if (step.PaletteRequested)
                LoadMotherBrainDeathBodyFade(sequence.State.GreyTransitionCounter - 1);
        }
        if (sequence.ThirdPhaseDeath.EnemyBg2TilemapClearRequested && !state.DeathBg2Cleared)
        {
            state.DeathBg2Cleared = true;
            state.DeathBg2Hidden = false;
            state.Body.Properties = sequence.ThirdPhaseDeath.BodyProperties;
            var words = new ushort[MotherBrainDeathRomData.Bg2LastByte / 2 + 1];
            Array.Fill(words, MotherBrainDeathRomData.EmptyBodyTile);
            StagingWordView bg2 = _room.Staging.EnemyBg2Tilemap;
            for (int i = 0; i < words.Length; i++)
                bg2[i] = words[i];
            _presentation.RequireBg2Image().Write(0, words);
            state.EnemyBg2TilemapSize = MotherBrainDeathRomData.Bg2LastByte;
            state.EnemyBg2TilemapTransferRequested = true;
        }
        if (sequence.ThirdPhaseDeath.BrainDrawSetupRequested)
        {
            // Decapitation transfers position ownership from the articulated neck to the
            // death sequence. Otherwise the next head AI call overwrites falling Y again.
            state.BrainFunction = MotherBrainBrainFunction.SetupBrainToBeDrawn;
            state.Head.Position = state.Head.Position.WithWhole(sequence.State.BrainPosition);
        }
        if (step.PhaseBefore == MotherBrainBodyFunction.ThirdPhaseDeathFadeToGrey && step.PaletteRequested)
            LoadMotherBrainDeathCorpseFade(sequence.State.GreyTransitionCounter - 1);
        foreach (var transfer in step.CorpseRottingVramTransfers) _motherBrainEncounter.ApplyMotherBrainRainbowTileTransfer(transfer);
        foreach (var dust in step.CorpseDustRequests)
        {
            _projectiles.SpawnRoomGraphicsDustExplosion(dust.Position, dust.Animation);
            // $A9:B252 queues the rot dust through QueueSound_Lib2_Max3.
            if (dust.SoundEffectQueued)
                _audio.QueueEnemySound(dust.SoundEffect, maximumQueued: 3);
        }
        if (step.MusicStopQueued) state.RequestMusic(MusicCommand.Stop, MusicCommandDelay.EightFrames);
        if (step.EscapeMusicQueued) state.RequestMusic(MusicCommand.LoadData(MotherBrainDeathRomData.EscapeMusicData), MusicCommandDelay.EightFrames);
        if (step.EscapeMusicTrackQueued) state.RequestMusic(MusicCommand.SelectTrack(MotherBrainDeathRomData.EscapeMusicTrack), MusicCommandDelay.EightFrames);
        foreach (var transfer in step.EscapeSequenceTileTransfers) _motherBrainEncounter.ApplyMotherBrainRainbowTileTransfer(transfer);
        if (step.EscapeTypewriterSetupRequested)
        {
            state.EnableUnpauseHook = sequence.ThirdPhaseDeath.MotherBrainUnpauseHookEnabled;
            state.EscapeTypewriter = _bossArtwork.EscapeTypewriterPresentation is { } presentation
                ? new(presentation.Get(EscapeTypewriterProgramId.Zebes), EscapeTypewriterRomData.ZebesTileBase)
                : new(EscapeTypewriterRomData.ZebesText, EscapeTypewriterRomData.ZebesTileBase);
            _presentation.Palettes.EscapeTypewriterText.CopyFrom(_presentation.Palettes.EscapeTypewriterSource.Colors);
            state.Bg2Scroll = Position16<Bg2Pixels>.Zero;
        }
        if (step.ExplodedDoorPaletteRequested)
            LoadMotherBrainDeathDoorPalette();
        if (step.EscapeDoorExplosion is { } doorDust)
        {
            _projectiles.SpawnRoomGraphicsDustExplosion(doorDust.Position, doorDust.Animation);
            // $A9:B346 queues the door explosion through QueueSound_Lib2_Max3.
            _audio.QueueEnemySound(doorDust.SoundEffect, maximumQueued: 3);
        }
        if (step.EscapeDoorPlm is { } doorPlm)
            _access.RequestMotherBrainPlm(doorPlm.Block, doorPlm.PlmEntry);
        foreach (var request in step.EscapeDoorParticleSpawns) SpawnMotherBrainDoorFragment(request.Parameter);
        if (step.TimeBombSetSubtitleSpawnRequested)
        {
            var subtitle = _projectiles.AllocateEnemyProjectile();
            if (subtitle is not null)
            {
                EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(subtitle, RoomEnemyProjectileKind.MotherBrainEscapeSubtitle, graphicsIndex: 0);
                MotherBrainProjectiles.PinMotherBrainEscapeSubtitle(subtitle);
            }
        }
        if (step.PhaseBefore == MotherBrainBodyFunction.ThirdPhaseDeathBrainFallsToGround &&
            step.PhaseAfter == MotherBrainBodyFunction.ThirdPhaseDeathLoadCorpseTiles ||
            step.EscapeMusicTrackQueued || step.EarthquakeTimerRefreshed)
        {
            _earthquake.EarthquakeType = checked((ushort)sequence.State.EarthquakeType);
            _earthquake.EarthquakeTimer = sequence.State.EarthquakeTimer;
        }
    }

    /// <summary>Applies the requested body-color row during the death fade.</summary>
    internal void LoadMotherBrainDeathBodyFade(int frame)
    {
        if (_bossArtwork.Artwork?.MotherBrainDeathColors is { } colors)
        {
            for (int color = 0; color < MotherBrainDeathRomData.BodyColorCount; color++)
            {
                Bgr555 shared = colors.BodyColor(frame, color);
                _presentation.BossPalettes().MotherBrainBody.Write(color, shared);
                _presentation.BossPalettes().MotherBrainBrain.Write(color, shared);
                _presentation.BossPalettes().MotherBrainLegs.Write(color, colors.LegColor(frame, color));
            }
            return;
        }
        throw new InvalidOperationException("Mother Brain death requires installed fade colors.");
    }

    /// <summary>Applies the requested corpse-color row during the death fade.</summary>
    internal void LoadMotherBrainDeathCorpseFade(int frame)
    {
        if (_bossArtwork.Artwork?.MotherBrainDeathColors is { } colors)
        {
            for (int color = 0; color < MotherBrainDeathRomData.CorpseColorCount; color++)
                _presentation.BossPalettes().MotherBrainCorpse.Write(color, colors.CorpseColor(frame, color));
            return;
        }
        throw new InvalidOperationException("Mother Brain corpse fade requires installed colors.");
    }

    /// <summary>Restores the room door colors required after Mother Brain dies.</summary>
    internal void LoadMotherBrainDeathDoorPalette()
    {
        if (_bossArtwork.Artwork?.MotherBrainDeathColors is { } colors)
        {
            for (int color = 0; color < MotherBrainDeathRomData.BodyColorCount; color++)
                _presentation.BossPalettes().MotherBrainBrain.Write(color, colors.ExplodedDoorColor(color));
            return;
        }
        throw new InvalidOperationException("Mother Brain exploded door requires installed colors.");
    }

    /// <summary>Places a death explosion with the requested position and visual phase.</summary>
    internal void SpawnMotherBrainDeathExplosion(MotherBrainDeathExplosionRequest request)
    {
        MiscDustProjectileProgram program = MotherBrainDeathExplosionDefinitions.Program(request.ProjectileParameter);
        var projectile = _projectiles.AllocateEnemyProjectile();
        if (projectile is null) return; // Native shared-pool exhaustion drops the spawn.
        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(projectile, RoomEnemyProjectileKind.MotherBrainDeathExplosion, graphicsIndex: 0);
        projectile.StartAnimation(new MiscDustProjectileAnimation(program));
        // The native initializer keeps the body-relative offset in the velocity words.
        projectile.Velocity = new EightEightVector<RoomPixels>(request.Offset.X, request.Offset.Y);
        _projectileAttacks.RunMotherBrainDeathExplosion(projectile);
    }

    /// <summary>Spawns a falling door fragment from the native parameter identity.</summary>
    internal void SpawnMotherBrainDoorFragment(int parameter)
    {
        MotherBrainDoorFragmentDefinition definition =
            MotherBrainDoorFragmentDefinitions.ForParameter(parameter);
        var fragment = _projectiles.AllocateEnemyProjectile();
        if (fragment is null) return;
        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(fragment, RoomEnemyProjectileKind.MotherBrainEscapeDoorFragment, graphicsIndex: 0);
        fragment.Position = fragment.Position.WithWhole(new Position16<RoomPixels>(MotherBrainDeathRomData.DoorX, MotherBrainDeathRomData.DoorY) + definition.Offset);
        fragment.Velocity = definition.Velocity;
        fragment.Variable0 = MotherBrainDeathRomData.DoorFragmentLifetime;
    }

    /// <summary>Writes a glyph the Zebes escape typewriter typed over Mother Brain's room BG2 (#1348).</summary>
    [PpuLayoutBoundary("codec: the Zebes escape typewriter program's native BG2 tilemap destinations")]
    private void WriteZebesTypewriterGlyph(EscapeTypewriterGlyph glyph) =>
        _room.Services.DrawBg2Overlay(glyph.Destination - GameplayVramLayout.Bg2TilemapWord, [glyph.Tile.Raw]);

    /// <summary>Runs one native body-function call through the shared verified sequence.</summary>
    internal void RunLiveMotherBrainRainbowBeam(
        MotherBrainEnemyState state,
        SamusState samus,
        byte enemyFrameCounter,
        SamusBombProjectileSystem? sharedProjectiles)
    {
        MotherBrainRainbowBeamAttackSequence sequence = state.RainbowBeamSequence ??
            throw new InvalidOperationException(
                "Mother Brain entered a rainbow function without its live sequence owner.");

        // Body bytecode and articulated-head AI ran through the physical room records on
        // previous frames. Import those words before dispatch so walk completion and neck
        // aiming are decided from the same state the 65816 would read from WRAM.
        SynchronizeLiveMotherBrainRainbowActor(state, sequence);

        bool typewriterFinished = false;
        if (sequence.State.Phase is MotherBrainBodyFunction.ThirdPhaseDeathTypeOutZebesEscapeText or
            MotherBrainBodyFunction.ThirdPhaseDeathSpawnTimeBombSetSubtitle)
        {
            var text = state.EscapeTypewriter ?? throw new InvalidOperationException("Mother Brain escape text was not initialized.");
            typewriterFinished = text.Step(out EscapeTypewriterGlyph? glyph);
            if (glyph is { } typed)
                WriteZebesTypewriterGlyph(typed);
            if (text.ClickRequested) state.LastSoundEffectLibrary3 = SoundEffectId.FromCartridge(SoundEffectLibrary.Library3, EscapeTypewriterRomData.ZebesClick);
        }
        MotherBrainRainbowBeamAttackStepResult step = sequence.Step(samus, new RainbowBeamAttackFrame { EnemyFrameCounter = enemyFrameCounter, MainEnemyExecutionCounter = _frameContext.RandomEnemyCounter, PowerBombActive = sharedProjectiles?.PowerBombExplosion.Status != 0, RandomNumberSeed = MotherBrainRandomSample.FromGenerator(_room.Random.Read()), NextRandomNumber = _room.Random.Generate, AlternateEscapeText = _room.Session.JapaneseText, TypewriterFinished = typewriterFinished, GlobalEarthquakeTimer = _earthquake.EarthquakeTimer });
        _frameResults.MotherBrainDeathStep = step;
        ApplyLiveMotherBrainRainbowState(state, sequence, step, samus, sharedProjectiles);
    }

    /// <summary>Copies rainbow-beam phase state into the live actor slot.</summary>
    internal static void SynchronizeLiveMotherBrainRainbowActor(
        MotherBrainEnemyState state,
        MotherBrainRainbowBeamAttackSequence sequence)
    {
        RoomEnemySlot head = state.Head;
        sequence.SynchronizeLiveActor(new MotherBrainLiveActorSnapshot { Body = state.Body.Position.Whole, BodyPose = state.Pose, Form = state.Form, BodyProperties = state.Body.Properties, BodyExtraProperties = state.Body.ExtraProperties, Brain = head.Position.Whole, BrainHealth = head.Health, BrainProperties = head.Properties, BrainExtraProperties = head.ExtraProperties, LowerNeckAngle = state.LowerNeckAngle, UpperNeckAngle = state.UpperNeckAngle, NeckMovementEnabled = state.NeckMovementEnabled, LowerNeckMovementIndex = state.LowerNeckMovementIndex, UpperNeckMovementIndex = state.UpperNeckMovementIndex, BombCounter = state.BombCounter, HitboxesEnabled = state.HitboxesEnabled != MotherBrainHitboxParts.None });
    }

    /// <summary>
    /// Publishes only values produced by the current body-AI call. In particular, cooldown
    /// and earthquake timers are event writes rather than continuously mirrored values;
    /// continuously copying the sequence's retained word would undo the global owners'
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

        // `$A9:C447` resets the physical list timer only when body AI requests a new list.
        // The dedicated latch is essential because CurrentInstruction advances away from
        // the list origin while the request itself remains unchanged.
        if (sequence.State.HeadProgram is { } headProgram &&
            (step is { HeadInstructionListRequested: true } ||
             headProgram != state.RainbowAppliedHeadProgram))
        {
            MotherBrainEncounterAccess.SetMotherBrainBrainProgram(state, headProgram);
            state.RainbowAppliedHeadProgram = headProgram;
        }

        if (step is { } result &&
            (result.BodyWalkRequested || result.BodyPostureRequested))
        {
            MotherBrainBodyProgram bodyProgram = sequence.RequestedBodyProgram ?? throw new InvalidDataException(
                "Rainbow body AI requested animation without a body program.");
            MotherBrainEncounterAccess.SetMotherBrainBodyProgram(state, bodyProgram);
        }

        state.Function = sequence.State.Phase ?? throw new InvalidDataException("The live Mother Brain rainbow sequence has no body function yet.");
        state.FunctionTimer = sequence.State.FunctionTimer;
        state.NeckAngleDelta = sequence.State.NeckAngleDelta;
        state.NeckMovementEnabled = sequence.State.NeckMovementEnabled;
        state.LowerNeckMovementIndex = sequence.State.LowerNeckMovementIndex;
        state.UpperNeckMovementIndex = sequence.State.UpperNeckMovementIndex;
        if (sequence.ThirdPhaseFight.Phase3NeckPhase == MotherBrainPhase3NeckPhase.HyperBeamRecoil &&
            sequence.ThirdPhaseFight.Phase3NeckFunctionTimer.RawValue == 10)
        {
            // `$C395` loads `$32` during the one-shot setup call, immediately before the
            // fallthrough decrements neck timer `$0B` to `$0A`. The physical draw hook owns
            // subsequent shake decrements, so publish this event only on that exact setup
            // result rather than copying the sequence's retained word every enemy frame.
            state.BrainMainShakeTimer = NativeCountdown.Start(50);
        }
        state.Form = sequence.State.Body.Form;
        state.BodySubFunctionTimer = sequence.ThirdPhaseDeath.DeathAndEscapeExplosionIndex;
        state.Body.Properties = sequence.ThirdPhaseDeath.BodyProperties;
        state.Body.ExtraProperties = sequence.ThirdPhaseDeath.BodyProperties2;
        head.Health = sequence.State.BrainHealth;
        head.Properties = sequence.ThirdPhaseDeath.BrainProperties;
        head.ExtraProperties = sequence.ThirdPhaseDeath.BrainProperties2;
        // The live word begins at two, while the reusable sequence needs only its Boolean
        // branch meaning. Preserve every nonzero cartridge value; only a translated clear
        // is allowed to collapse it to zero.
        if (!sequence.ThirdPhaseDeath.HitboxesEnabled)
            state.HitboxesEnabled = MotherBrainHitboxParts.None;
        if (sequence.State.SmallPurpleBreathGenerationWriteCount != state.RainbowAppliedSmallPurpleBreathWrites)
        {
            state.SmallPurpleBreathGenerationEnabled = sequence.State.SmallPurpleBreathGenerationEnabled;
            state.RainbowAppliedSmallPurpleBreathWrites = sequence.State.SmallPurpleBreathGenerationWriteCount;
        }
        state.DroolGenerationEnabled = sequence.State.DroolGenerationEnabled;

        // These words are the renderer-facing output of the bank-$88 HDMA object. The beam
        // renderer is deliberately fed from this single translated aim calculation.
        state.RainbowBeamHdmaActive = sequence.RainbowAttack.HdmaActive;
        state.RainbowBeamAngle = sequence.RainbowBeamAngle;
        state.RainbowBeamAngularWidth = sequence.RainbowAttack.AngularWidth;

        if (step is not { } current)
            return;

        state.RainbowBeamPaletteRequested = current.PaletteRequested;
        if (current.PhaseBefore == MotherBrainBodyFunction.SecondPhaseFinishSamusOffFireFinalBeam &&
            current.PhaseAfter == MotherBrainBodyFunction.SecondPhaseFinalRainbowBeamHolding)
            state.RainbowPaletteCursor = 0;
        ApplyMotherBrainRainbowPalette(state, current);
        // $A9:D1E4 runs after body AI. Its health handler is enabled during revival,
        // but $AD:E3D5 waits until the corpse-state word reaches final-combat state 2.
        if (sequence.State.HealthBasedPaletteHandlingEnabled && sequence.State.Phase2CorpseState == MotherBrainPhase2CorpseState.Revived)
            MotherBrainHealthPalette.Apply(_presentation.Cgram, state.Head.Health, _bossArtwork.MotherBrainHealthColors);
        ApplyLiveMotherBrainDeath(state, sequence, current);
        if (current.SoundQueued)
            // A request is not invariably a start: the painful-walk stage-six handoff
            // stops the same looping voice. Preserve the command selected by the sequence.
            state.LastSoundEffectLibrary1 = sequence.State.RainbowBeamSoundPlaying
                ? MotherBrainSoundDefinitions.StartRainbowDrain
                : MotherBrainSoundDefinitions.StopRainbowDrain;
        if (current.ChargeSoundQueued || current.FinalBeamSoundQueued)
            state.LastSoundEffect = SoundEffectLibrary2Sounds.CancelAll;
        if (current.Explosion is not null)
            state.LastSoundEffect = SoundEffectLibrary2Sounds.SmallExplosion;

        // `$BA0F` seeds the short quake, `$BA27` replaces it with the full drain duration,
        // and `$BA93` clears it when the beam contracts below its minimum width.
        if (current.PhaseBefore == MotherBrainBodyFunction.SecondPhaseRainbowOneFrameDelay &&
            current.PhaseAfter == MotherBrainBodyFunction.SecondPhaseRainbowStartDrainingSamus)
        {
            _earthquake.EarthquakeType = 8;
            _earthquake.EarthquakeTimer = NativeCountdown.Start(8);
        }
        else if (current.PhaseBefore == MotherBrainBodyFunction.SecondPhaseRainbowStartDrainingSamus)
        {
            _earthquake.EarthquakeType = 8;
            _earthquake.EarthquakeTimer = NativeCountdown.Start(299);
        }
        else if (current.PhaseBefore == MotherBrainBodyFunction.SecondPhaseRainbowFinishFiring &&
            current.PhaseAfter == MotherBrainBodyFunction.SecondPhaseRainbowLetSamusFall)
        {
            _earthquake.EarthquakeTimer = NativeCountdown.Start(0);
            state.LastSoundEffectLibrary1 = MotherBrainSoundDefinitions.StopRainbowDrain;
        }

        if (sharedProjectiles is not null)
        {
            if (current.PhaseBefore == MotherBrainBodyFunction.SecondPhaseRainbowWaitForCharge &&
                current.PhaseAfter == MotherBrainBodyFunction.SecondPhaseRainbowStartFiring)
            {
                sharedProjectiles.SetSharedCooldown(8);
            }
            else if (current.PhaseBefore == MotherBrainBodyFunction.SecondPhaseRainbowStartFiring &&
                current.PhaseAfter == MotherBrainBodyFunction.SecondPhaseRainbowMoveSamusTowardWall)
            {
                sharedProjectiles.SetSharedCooldown(0);
            }
            else if (current.PhaseBefore == MotherBrainBodyFunction.SecondPhaseRainbowFinishFiring &&
                current.PhaseAfter == MotherBrainBodyFunction.SecondPhaseRainbowLetSamusFall)
            {
                sharedProjectiles.SetSharedCooldown(8);
            }
        }

        if (current.SpriteTileTransfer is { } transfer)
            _motherBrainEncounter.ApplyMotherBrainRainbowTileTransfer(transfer);
        if (current.BabySpawnRequested)
            _roomActors.SpawnBabyMetroid(state);
        if (current.Explosion is { } explosion)
        {
            // `$BC76` runs before `$B9F1/$BA0C/$BA4F` moves Samus. The reusable sequence
            // captures that pre-move point in its typed movement result, so use it for the
            // initializer and let `$C94C` attach to the current position on the later bank-
            // $86 pass. Finish-firing has no movement and therefore uses the unchanged actor.
            Position16<RoomPixels> samusBeforeMovement = current.Movement?.Before.Whole ?? samus.Position.Whole;
            SpawnMotherBrainRainbowExplosion(samusBeforeMovement, explosion);
        }
    }

    /// <summary>Ports `$86:C92F-$C960`; the signed offsets remain in velocity words.</summary>
    internal void SpawnMotherBrainRainbowExplosion(
        Position16<RoomPixels> samusBeforeMovement,
        MotherBrainRainbowExplosionRequest request)
    {
        RoomEnemyProjectileSlot? projectile = _projectiles.AllocateEnemyProjectile();
        if (projectile is null)
            return;

        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
            projectile,
            RoomEnemyProjectileKind.MotherBrainRainbowBeamExplosion,
            graphicsIndex: 0);
        projectile.Velocity = new EightEightVector<RoomPixels>(request.Offset.X, request.Offset.Y);
        projectile.GraphicsIndex = 0;
        projectile.Position = projectile.Position.WithWhole(samusBeforeMovement + request.Offset);
    }

    /// <summary>Runs $A9:BCFD at the body-frame cadence, then $BD1D's three literal copies.</summary>
    private void ApplyMotherBrainRainbowPalette(MotherBrainEnemyState state,
        MotherBrainRainbowBeamAttackStepResult step)
    {
        bool draining = step.PhaseBefore == MotherBrainBodyFunction.SecondPhaseDrainedByBabyTransitionToGrey;
        bool reviving = step.PhaseBefore == MotherBrainBodyFunction.SecondPhaseReviveTransitionFromGrey;
        if ((draining || reviving) && step.PaletteRequested)
        {
            // The sequence increments before publishing this request. Keep the distinct
            // native copy lengths: revival intentionally preserves two brain/body colors.
            int index = MotherBrainEncounterAccess.RequireLiveMotherBrainRainbowSequence(state).State.GreyTransitionCounter - 1;
            var installed = _bossArtwork.MotherBrainRainbowColors ?? throw new InvalidDataException(
                "Mother Brain's grey transition requires installed palette artwork.");
            if (draining) installed.ApplyToGrey(_presentation.Cgram, index);
            else installed.ApplyFromGrey(_presentation.Cgram, index);
            return;
        }
        if (step.PhaseBefore == MotherBrainBodyFunction.SecondPhaseRainbowFinishFiring &&
            step.PhaseAfter == MotherBrainBodyFunction.SecondPhaseRainbowLetSamusFall)
        {
            var installed = _bossArtwork.MotherBrainRainbowColors ?? throw new InvalidDataException(
                "Mother Brain's normal palette requires installed artwork.");
            installed.ApplyNormal(_presentation.Cgram);
            return;
        }
        if (step.PhaseBefore == MotherBrainBodyFunction.SecondPhaseDrainedByBabyFiringRainbowBeam &&
            step.PhaseAfter == MotherBrainBodyFunction.SecondPhaseDrainedByBabyRainbowBeamRunOut)
        {
            var installed = _bossArtwork.MotherBrainRainbowColors ?? throw new InvalidDataException(
                "Mother Brain's drained palette requires installed artwork.");
            installed.ApplyRainbow(_presentation.Cgram, MotherBrainRainbowPaletteRomData.DrainedPointerOffset / sizeof(ushort));
            return;
        }
        bool rainbowPhase = step.PhaseBefore is
            MotherBrainBodyFunction.SecondPhaseRainbowMoveSamusTowardWall or
            MotherBrainBodyFunction.SecondPhaseRainbowOneFrameDelay or
            MotherBrainBodyFunction.SecondPhaseRainbowStartDrainingSamus or
            MotherBrainBodyFunction.SecondPhaseRainbowDrainingSamus or
            MotherBrainBodyFunction.SecondPhaseRainbowFinishFiring or
            MotherBrainBodyFunction.SecondPhaseDrainedByBabyRegainBalance or
            MotherBrainBodyFunction.SecondPhaseDrainedByBabyFiringRainbowBeam;
        if (!rainbowPhase || !step.PaletteRequested || (state.Body.FrameCounter & 2) == 0)
            return;

        var rainbow = _bossArtwork.MotherBrainRainbowColors ?? throw new InvalidDataException(
            "Mother Brain's rainbow phase requires installed palette artwork.");
        if ((state.RainbowPaletteCursor & 1) != 0 ||
            state.RainbowPaletteCursor > MotherBrainRainbowPaletteFormat.RainbowFrameCount * sizeof(ushort))
            throw new InvalidDataException($"Mother Brain rainbow cursor ${state.RainbowPaletteCursor:X4} is outside the compiled loop.");
        int frame = state.RainbowPaletteCursor / sizeof(ushort);
        if (frame == MotherBrainRainbowPaletteFormat.RainbowFrameCount)
        {
            state.RainbowPaletteCursor = 0;
            frame = 0;
        }
        state.RainbowPaletteCursor += sizeof(ushort);
        rainbow.ApplyRainbow(_presentation.Cgram, frame);
    }
}
