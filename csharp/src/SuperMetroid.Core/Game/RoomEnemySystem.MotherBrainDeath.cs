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
            {
                int source = DeathPaletteSource(MotherBrainDeathRomData.BodyFadeTable, sequence.GreyTransitionCounter - 1);
                _cgram!.LoadFromBus(_bus!, source, MotherBrainDeathRomData.BodyColorCount, MotherBrainDeathRomData.BodyColors);
                _cgram.LoadFromBus(_bus!, source, MotherBrainDeathRomData.BodyColorCount, MotherBrainDeathRomData.BrainColors);
                _cgram.LoadFromBus(_bus!, source + MotherBrainDeathRomData.BodyColorCount * 2,
                    MotherBrainDeathRomData.BodyColorCount, MotherBrainDeathRomData.LegColors);
            }
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
            _cgram!.LoadFromBus(_bus!, DeathPaletteSource(MotherBrainDeathRomData.CorpseFadeTable,
                sequence.GreyTransitionCounter - 1), MotherBrainDeathRomData.CorpseColorCount, MotherBrainDeathRomData.CorpseColors);
        foreach (var transfer in step.CorpseRottingVramTransfers) ApplyMotherBrainRainbowTileTransfer(transfer);
        foreach (var dust in step.CorpseDustRequests)
        {
            SpawnRoomGraphicsDustExplosion(dust.XPosition, dust.YPosition, dust.ProjectileParameter);
            if (dust.SoundEffectQueued) state.LastSoundEffectLibrary3 = dust.SoundEffect;
        }
        if (step.MusicStopQueued) state.RequestMusic(MusicCommand.Stop, MusicCommandDelay.EightFrames);
        if (step.EscapeMusicQueued) state.RequestMusic(MusicCommand.LoadData(MotherBrainDeathRomData.EscapeMusicData), MusicCommandDelay.EightFrames);
        if (step.EscapeMusicTrackQueued) state.RequestMusic(MusicCommand.SelectTrack(MotherBrainDeathRomData.EscapeMusicTrack), MusicCommandDelay.EightFrames);
        foreach (var transfer in step.EscapeSequenceTileTransfers) ApplyMotherBrainRainbowTileTransfer(transfer);
        if (step.EscapeTypewriterSetupRequested)
        {
            state.EnableUnpauseHook = sequence.MotherBrainUnpauseHookEnabled;
            state.EscapeTypewriter = new(EscapeTypewriterRomData.ZebesText, EscapeTypewriterRomData.ZebesTileBase);
            for (int i = 0; i < 2; i++)
                _cgram!.SetColor(EscapeTypewriterRomData.ColorDestination + i,
                    _cgram.Colors[EscapeTypewriterRomData.ColorSource + i]);
            state.Bg2XScroll = state.Bg2YScroll = 0;
        }
        if (step.ExplodedDoorPaletteRequested)
            _cgram!.LoadFromBus(_bus!, MotherBrainDeathRomData.DoorPalette,
                MotherBrainDeathRomData.BodyColorCount, MotherBrainDeathRomData.BrainColors);
        if (step.EscapeDoorExplosion is { } doorDust)
        {
            SpawnRoomGraphicsDustExplosion(doorDust.XPosition, doorDust.YPosition, doorDust.ProjectileParameter);
            state.LastSoundEffectLibrary3 = doorDust.SoundEffect;
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

    private int DeathPaletteSource(int table, int index)
    {
        ushort pointer = ReadWord(_bus!, table + index * 2);
        if (pointer == 0) throw new InvalidDataException("Death palette producer requested the terminating entry.");
        return MotherBrainDeathRomData.PaletteBank | pointer;
    }

    private void SpawnMotherBrainDeathExplosion(MotherBrainDeathExplosionRequest request)
    {
        var projectile = AllocateEnemyProjectile();
        if (projectile is null) return; // Native shared-pool exhaustion drops the spawn.
        InitializeEnemyProjectileFromDefinition(projectile, RoomEnemyProjectileKind.MotherBrainDeathExplosion, graphicsIndex: 0);
        projectile.InstructionPointer = ReadWord(_bus!, MotherBrainDeathRomData.ExplosionLists + request.ProjectileParameter * 2);
        projectile.InstructionTimer = 1;
        projectile.XVelocity = unchecked((ushort)request.XOffset);
        projectile.YVelocity = unchecked((ushort)request.YOffset);
        RunMotherBrainDeathExplosion(projectile);
    }

    private void RunMotherBrainDeathExplosion(RoomEnemyProjectileSlot projectile)
    {
        var body = _motherBrain?.Body ?? throw new InvalidOperationException("Body-relative death explosion has no Mother Brain owner.");
        projectile.XPosition = unchecked((ushort)(body.XPosition + projectile.XVelocity));
        projectile.YPosition = unchecked((ushort)(body.YPosition + projectile.YVelocity));
    }

    private void SpawnMotherBrainDoorFragment(ushort parameter)
    {
        if (parameter >= 8) throw new ArgumentOutOfRangeException(nameof(parameter));
        var fragment = AllocateEnemyProjectile();
        if (fragment is null) return;
        InitializeEnemyProjectileFromDefinition(fragment, RoomEnemyProjectileKind.MotherBrainEscapeDoorFragment, graphicsIndex: 0);
        int offset = parameter * 4;
        fragment.XPosition = unchecked((ushort)(MotherBrainDeathRomData.DoorX + ReadWord(_bus!, MotherBrainDeathRomData.DoorFragmentOffsets + offset)));
        fragment.YPosition = unchecked((ushort)(MotherBrainDeathRomData.DoorY + ReadWord(_bus!, MotherBrainDeathRomData.DoorFragmentOffsets + offset + 2)));
        fragment.XVelocity = ReadWord(_bus!, MotherBrainDeathRomData.DoorFragmentVelocities + offset);
        fragment.YVelocity = ReadWord(_bus!, MotherBrainDeathRomData.DoorFragmentVelocities + offset + 2);
        fragment.Variable0 = MotherBrainDeathRomData.DoorFragmentLifetime;
    }

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

    private static void PinMotherBrainEscapeSubtitle(RoomEnemyProjectileSlot subtitle)
    {
        subtitle.XVelocity = subtitle.YVelocity = 0;
        subtitle.XPosition = MotherBrainDeathRomData.SubtitleX;
        subtitle.YPosition = MotherBrainDeathRomData.SubtitleY;
    }
}
