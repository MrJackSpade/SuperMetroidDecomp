using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Game;

public sealed partial class RoomEnemySystem
{
    /// <summary>Consumes death actor events exactly once, alongside the live rainbow adapter.</summary>
    private void ApplyLiveMotherBrainDeath(MotherBrainEnemyState state,
        MotherBrainRainbowBeamAttackSequence sequence, MotherBrainRainbowBeamAttackStepResult step)
    {
        foreach (var explosion in step.DeathExplosions)
        {
            SpawnMotherBrainDeathExplosion(explosion);
            state.LastSoundEffectLibrary3 = explosion.SoundEffect;
        }
        if (step.PhaseBefore == MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceDisableBrainEffects &&
            step.PhaseAfter != step.PhaseBefore)
        {
            for (int i = 0; i < MotherBrainDeathRomData.CorpseColorCount; i++)
                _cgram!.SetColor(MotherBrainDeathRomData.CorpseColors + i,
                    _cgram.Colors[MotherBrainDeathRomData.BrainColors + i]);
            state.BrainPaletteIndex = sequence.BrainPaletteIndex;
            state.BrainPaletteHandlingEnabled = false;
            state.DroolGenerationEnabled = false;
        }
        bool bodyFade = step.PhaseBefore == MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceFadeOutBody ||
            step.PhaseAfter == MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceFadeOutBody;
        if (bodyFade)
        {
            // ADF41C flickers the body and BG2 together on every call, not just palette ticks.
            state.DeathBg2Hidden = (state.Body.FrameCounter & 1) == 0;
            state.Body.Properties = state.DeathBg2Hidden
                ? state.Body.Properties.With(EnemyProperties.Invisible)
                : state.Body.Properties.Without(EnemyProperties.Invisible);
            if (step.PaletteRequested)
                LoadMotherBrainDeathBodyFade(sequence.GreyTransitionCounter - 1);
        }
        if (sequence.EnemyBg2TilemapClearRequested && !state.DeathBg2Cleared)
        {
            state.DeathBg2Cleared = true;
            state.DeathBg2Hidden = false;
            state.Body.Properties = sequence.BodyProperties;
            var words = new ushort[MotherBrainDeathRomData.Bg2LastByte / 2 + 1];
            Array.Fill(words, MotherBrainDeathRomData.EmptyBodyTile);
            for (int i = 0; i < words.Length; i++)
                WriteWord(_bus!, MotherBrainDeathRomData.Bg2WorkAddress + i * 2, words[i]);
            _vram!.ExecuteWordTransfer(words, SnesPpuLayout.GameplayBg2TilemapWord, wordIncrement: 1);
            state.EnemyBg2TilemapSize = MotherBrainDeathRomData.Bg2LastByte;
            state.EnemyBg2TilemapTransferRequested = true;
        }
        if (sequence.BrainDrawSetupRequested)
        {
            // Decapitation transfers position ownership from the articulated neck to the
            // death sequence. Otherwise the next head AI call overwrites falling Y again.
            state.BrainFunction = MotherBrainBrainFunction.SetupBrainToBeDrawn;
            state.Head!.XPosition = sequence.BrainXPosition;
            state.Head.YPosition = sequence.BrainYPosition;
        }
        if (step.PhaseBefore == MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceFadeToGrey && step.PaletteRequested)
            LoadMotherBrainDeathCorpseFade(sequence.GreyTransitionCounter - 1);
        foreach (var transfer in step.CorpseRottingVramTransfers) ApplyMotherBrainRainbowTileTransfer(transfer);
        foreach (var dust in step.CorpseDustRequests)
        {
            SpawnRoomGraphicsDustExplosion(dust.XPosition, dust.YPosition, dust.ProjectileParameter);
            // $A9:B252 queues the rot dust through QueueSound_Lib2_Max3.
            if (dust.SoundEffectQueued)
                QueueEnemySound(SoundEffectId.FromCartridge(SoundEffectLibrary.Library2, dust.SoundEffect), maximumQueued: 3);
        }
        if (step.MusicStopQueued) state.RequestMusic(MusicCommand.Stop, MusicCommandDelay.EightFrames);
        if (step.EscapeMusicQueued) state.RequestMusic(MusicCommand.LoadData(MotherBrainDeathRomData.EscapeMusicData), MusicCommandDelay.EightFrames);
        if (step.EscapeMusicTrackQueued) state.RequestMusic(MusicCommand.SelectTrack(MotherBrainDeathRomData.EscapeMusicTrack), MusicCommandDelay.EightFrames);
        foreach (var transfer in step.EscapeSequenceTileTransfers) ApplyMotherBrainRainbowTileTransfer(transfer);
        if (step.EscapeTypewriterSetupRequested)
        {
            state.EnableUnpauseHook = sequence.MotherBrainUnpauseHookEnabled;
            state.EscapeTypewriter = EscapeTypewriterPresentation is { } presentation
                ? new(presentation.Get(EscapeTypewriterProgramId.Zebes), EscapeTypewriterRomData.ZebesTileBase)
                : new(EscapeTypewriterRomData.ZebesText, EscapeTypewriterRomData.ZebesTileBase);
            for (int i = 0; i < 2; i++)
                _cgram!.SetColor(EscapeTypewriterRomData.ColorDestination + i,
                    _cgram.Colors[EscapeTypewriterRomData.ColorSource + i]);
            state.Bg2XScroll = state.Bg2YScroll = 0;
        }
        if (step.ExplodedDoorPaletteRequested)
            LoadMotherBrainDeathDoorPalette();
        if (step.EscapeDoorExplosion is { } doorDust)
        {
            SpawnRoomGraphicsDustExplosion(doorDust.XPosition, doorDust.YPosition, doorDust.ProjectileParameter);
            // $A9:B346 queues the door explosion through QueueSound_Lib2_Max3.
            QueueEnemySound(SoundEffectId.FromCartridge(SoundEffectLibrary.Library2, doorDust.SoundEffect), maximumQueued: 3);
        }
        if (step.EscapeDoorPlm is { } doorPlm)
            state.RequestPlm(doorPlm.BlockX, doorPlm.BlockY, doorPlm.PlmEntry);
        foreach (var request in step.EscapeDoorParticleSpawns) SpawnMotherBrainDoorFragment(request.Parameter);
        if (step.TimeBombSetSubtitleSpawnRequested)
        {
            var subtitle = AllocateEnemyProjectile();
            if (subtitle is not null)
            {
                InitializeEnemyProjectileFromDefinition(subtitle, RoomEnemyProjectileKind.MotherBrainEscapeSubtitle, graphicsIndex: 0);
                PinMotherBrainEscapeSubtitle(subtitle);
            }
        }
        if (step.PhaseBefore == MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceBrainFallsToGround &&
            step.PhaseAfter == MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceLoadCorpseTiles ||
            step.EscapeMusicTrackQueued || step.EarthquakeTimerRefreshed)
        {
            EarthquakeType = sequence.EarthquakeType;
            EarthquakeTimer = sequence.EarthquakeTimer;
        }
    }

    /// <summary>Loads one installed fade frame into the body, brain, and leg palette regions.</summary>
    /// <param name="frame">Zero-based fade-frame index selected by the death-sequence counter.</param>
    private void LoadMotherBrainDeathBodyFade(int frame)
    {
        if (TileArtwork?.MotherBrainDeathColors is { } colors)
        {
            for (int color = 0; color < MotherBrainDeathRomData.BodyColorCount; color++)
            {
                ushort shared = colors.BodyColor(frame, color);
                _cgram!.SetColor(MotherBrainDeathRomData.BodyColors + color, shared);
                _cgram.SetColor(MotherBrainDeathRomData.BrainColors + color, shared);
                _cgram.SetColor(MotherBrainDeathRomData.LegColors + color,
                    colors.LegColor(frame, color));
            }
            return;
        }
        throw new InvalidOperationException("Mother Brain death requires installed fade colors.");
    }

    /// <summary>Loads one installed fade frame into the corpse palette region.</summary>
    /// <param name="frame">Zero-based corpse-fade frame index selected by the death-sequence counter.</param>
    private void LoadMotherBrainDeathCorpseFade(int frame)
    {
        if (TileArtwork?.MotherBrainDeathColors is { } colors)
        {
            for (int color = 0; color < MotherBrainDeathRomData.CorpseColorCount; color++)
                _cgram!.SetColor(MotherBrainDeathRomData.CorpseColors + color,
                    colors.CorpseColor(frame, color));
            return;
        }
        throw new InvalidOperationException("Mother Brain corpse fade requires installed colors.");
    }

    /// <summary>Sets the brain palette entries to the installed colors for the exploded escape door.</summary>
    private void LoadMotherBrainDeathDoorPalette()
    {
        if (TileArtwork?.MotherBrainDeathColors is { } colors)
        {
            for (int color = 0; color < MotherBrainDeathRomData.BodyColorCount; color++)
                _cgram!.SetColor(MotherBrainDeathRomData.BrainColors + color,
                    colors.ExplodedDoorColor(color));
            return;
        }
        throw new InvalidOperationException("Mother Brain exploded door requires installed colors.");
    }

    /// <summary>Allocates and initializes a body-relative death explosion from its projectile request.</summary>
    /// <param name="request">Offsets, instruction parameter, and other spawn data emitted by the death sequence.</param>
    /// <remarks>If the shared projectile pool is full, the requested explosion is dropped.</remarks>
    private void SpawnMotherBrainDeathExplosion(MotherBrainDeathExplosionRequest request)
    {
        ushort instructionList =
            MotherBrainDeathExplosionDefinitions.InstructionList(request.ProjectileParameter);
        var projectile = AllocateEnemyProjectile();
        if (projectile is null) return; // Native shared-pool exhaustion drops the spawn.
        InitializeEnemyProjectileFromDefinition(projectile, RoomEnemyProjectileKind.MotherBrainDeathExplosion, graphicsIndex: 0);
        projectile.InstructionPointer = instructionList;
        projectile.InstructionTimer = 1;
        projectile.XVelocity = unchecked((ushort)request.XOffset);
        projectile.YVelocity = unchecked((ushort)request.YOffset);
        RunMotherBrainDeathExplosion(projectile);
    }

    /// <summary>Recomputes an explosion's position from the current Mother Brain body position and stored offsets.</summary>
    /// <param name="projectile">Death-explosion slot whose velocity fields hold body-relative offsets.</param>
    /// <exception cref="InvalidOperationException">No Mother Brain body owns the relative explosion.</exception>
    private void RunMotherBrainDeathExplosion(RoomEnemyProjectileSlot projectile)
    {
        var body = _motherBrain?.Body ?? throw new InvalidOperationException("Body-relative death explosion has no Mother Brain owner.");
        projectile.XPosition = unchecked((ushort)(body.XPosition + projectile.XVelocity));
        projectile.YPosition = unchecked((ushort)(body.YPosition + projectile.YVelocity));
    }

    /// <summary>Allocates a door fragment and initializes its position, velocity, and lifetime from its variant.</summary>
    /// <param name="parameter">Fragment selector used to obtain the authored motion definition.</param>
    /// <remarks>If the shared projectile pool is full, no fragment is created.</remarks>
    private void SpawnMotherBrainDoorFragment(ushort parameter)
    {
        MotherBrainDoorFragmentDefinition definition =
            MotherBrainDoorFragmentDefinitions.ForParameter(parameter);
        var fragment = AllocateEnemyProjectile();
        if (fragment is null) return;
        InitializeEnemyProjectileFromDefinition(fragment, RoomEnemyProjectileKind.MotherBrainEscapeDoorFragment, graphicsIndex: 0);
        fragment.XPosition = unchecked((ushort)(MotherBrainDeathRomData.DoorX + definition.XOffset));
        fragment.YPosition = unchecked((ushort)(MotherBrainDeathRomData.DoorY + definition.YOffset));
        fragment.XVelocity = unchecked((ushort)definition.XVelocity);
        fragment.YVelocity = unchecked((ushort)definition.YVelocity);
        fragment.Variable0 = MotherBrainDeathRomData.DoorFragmentLifetime;
    }

    /// <summary>Applies drag and gravity to a door fragment, advances it, and emits dust when its lifetime expires.</summary>
    /// <param name="fragment">Active door-fragment projectile whose motion and lifetime are updated.</param>
    private void RunMotherBrainDoorFragment(RoomEnemyProjectileSlot fragment)
    {
        bool negative = unchecked((short)fragment.XVelocity) < 0;
        ushort magnitude = negative ? unchecked((ushort)-fragment.XVelocity) : fragment.XVelocity;
        short slowed = unchecked((short)(magnitude - MotherBrainDeathRomData.DoorFragmentDrag));
        int speed = Math.Max(0, (int)slowed);
        fragment.XVelocity = unchecked((ushort)(negative ? -speed : speed));
        fragment.YVelocity = unchecked((ushort)(fragment.YVelocity + MotherBrainDeathRomData.DoorFragmentGravity));
        (fragment.XPosition, fragment.XSubposition) = AddEightBitVelocity(fragment.XPosition, fragment.XSubposition, fragment.XVelocity);
        (fragment.YPosition, fragment.YSubposition) = AddEightBitVelocity(fragment.YPosition, fragment.YSubposition, fragment.YVelocity);
        fragment.Variable0--;
        if (unchecked((short)fragment.Variable0) >= 0) return;
        ushort x = fragment.XPosition, y = unchecked((ushort)(fragment.YPosition - MotherBrainDeathRomData.DoorDustYOffset));
        fragment.Clear();
        SpawnRoomGraphicsDustExplosion(x, y, MotherBrainDeathRomData.DoorDustParameter);
    }

    /// <summary>Stops the escape subtitle projectile and places it at its fixed screen position.</summary>
    /// <param name="subtitle">Projectile slot used to render the pinned escape subtitle.</param>
    private static void PinMotherBrainEscapeSubtitle(RoomEnemyProjectileSlot subtitle)
    {
        subtitle.XVelocity = subtitle.YVelocity = 0;
        subtitle.XPosition = MotherBrainDeathRomData.SubtitleX;
        subtitle.YPosition = MotherBrainDeathRomData.SubtitleY;
    }
}
