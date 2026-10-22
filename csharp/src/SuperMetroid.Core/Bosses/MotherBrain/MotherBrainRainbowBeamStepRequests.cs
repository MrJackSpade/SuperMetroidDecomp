using SuperMetroid.Core.Fx.PaletteFx;
using SuperMetroid.Core.Escape;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// What one Mother Brain active-rainbow body-function call asks of the room, gathered while the call runs through its
/// phases and reported as one <see cref="MotherBrainRainbowBeamAttackStepResult"/>.
/// </summary>
internal sealed class MotherBrainRainbowBeamStepRequests
{
    /// <summary>The forced Samus movement the call applied, if any.</summary>
    internal MotherBrainForcedSamusMovementResult? Movement { get; set; }

    /// <summary>Whether the beam's sound was queued.</summary>
    internal bool SoundQueued { get; set; }

    /// <summary>Whether the beam's palette was requested.</summary>
    internal bool PaletteRequested { get; set; }

    /// <summary>The beam explosion requested, if any.</summary>
    internal MotherBrainRainbowExplosionRequest? Explosion { get; set; }

    /// <summary>Whether the charge sound was queued.</summary>
    internal bool ChargeSoundQueued { get; set; }

    /// <summary>Whether the body was asked to walk.</summary>
    internal bool BodyWalkRequested { get; set; }

    /// <summary>Whether the body was asked to change posture.</summary>
    internal bool BodyPostureRequested { get; set; }

    /// <summary>The sprite tile transfer requested, if any.</summary>
    internal MotherBrainSpriteTileTransferRequest? SpriteTileTransfer { get; set; }

    /// <summary>Whether the Baby Metroid's spawn was requested.</summary>
    internal bool BabySpawnRequested { get; set; }

    /// <summary>Whether the final beam's sound was queued.</summary>
    internal bool FinalBeamSoundQueued { get; set; }

    /// <summary>The death explosions requested.</summary>
    internal List<MotherBrainDeathExplosionRequest> DeathExplosions { get; } = new(capacity: 4);

    /// <summary>The corpse-rotting uploads requested, each with the rot step it shows.</summary>
    internal List<MotherBrainCorpseRotUpload> CorpseRottingVramTransfers { get; } = new(capacity: 6);

    /// <summary>The corpse dust requested.</summary>
    internal List<MotherBrainCorpseDustRequest> CorpseDustRequests { get; } = new(capacity: 1);

    /// <summary>Whether stopping the music was queued.</summary>
    internal bool MusicStopQueued { get; set; }

    /// <summary>Whether the escape music was queued.</summary>
    internal bool EscapeMusicQueued { get; set; }

    /// <summary>The escape sequence's tile transfers requested.</summary>
    internal List<MotherBrainSpriteTileTransferRequest> EscapeSequenceTileTransfers { get; } = new(capacity: 2);

    /// <summary>Whether the exploded door's palette was requested.</summary>
    internal bool ExplodedDoorPaletteRequested { get; set; }

    /// <summary>Whether the escape music track was queued.</summary>
    internal bool EscapeMusicTrackQueued { get; set; }

    /// <summary>The escape's palette effects requested.</summary>
    internal List<PaletteFxObject> EscapePaletteFxRequests { get; } = new(capacity: 4);

    /// <summary>Whether the escape typewriter's setup was requested.</summary>
    internal bool EscapeTypewriterSetupRequested { get; set; }

    /// <summary>Whether the time-bomb-set subtitle's spawn was requested.</summary>
    internal bool TimeBombSetSubtitleSpawnRequested { get; set; }

    /// <summary>The escape door's explosion requested, if any.</summary>
    internal DustExplosionRequest? EscapeDoorExplosion { get; set; }

    /// <summary>Whether enabling timer handling was requested.</summary>
    internal bool TimerHandlingEnableRequested { get; set; }

    /// <summary>Whether starting the escape timer was requested.</summary>
    internal bool MotherBrainEscapeTimerStartRequested { get; set; }

    /// <summary>Whether Mother Brain's boss bit was requested.</summary>
    internal bool MotherBrainBossBitRequested { get; set; }

    /// <summary>Whether the Zebes time-bomb event was requested.</summary>
    internal bool ZebesTimebombEventRequested { get; set; }

    /// <summary>The escape door's particle spawns requested.</summary>
    internal List<MotherBrainEscapeDoorParticleSpawnRequest> EscapeDoorParticleSpawns { get; } = new(capacity: 8);

    /// <summary>The escape door's PLM requested, if any.</summary>
    internal MotherBrainEscapeDoorPlmRequest? EscapeDoorPlm { get; set; }

    /// <summary>Whether the earthquake timer was refreshed.</summary>
    internal bool EarthquakeTimerRefreshed { get; set; }

    /// <summary>The call's result, from the phases it started and ended in.</summary>
    internal MotherBrainRainbowBeamAttackStepResult ToResult(
        MotherBrainBodyFunction? phaseBefore, MotherBrainBodyFunction? phaseAfter, bool headInstructionListRequested) =>
        new(phaseBefore, phaseAfter, Movement, SoundQueued, PaletteRequested, Explosion, ChargeSoundQueued, BodyWalkRequested,
            headInstructionListRequested, BodyPostureRequested, SpriteTileTransfer, BabySpawnRequested, FinalBeamSoundQueued,
            DeathExplosions, CorpseRottingVramTransfers, CorpseDustRequests, MusicStopQueued, EscapeMusicQueued,
            EscapeSequenceTileTransfers, ExplodedDoorPaletteRequested, EscapeMusicTrackQueued, EscapePaletteFxRequests,
            EscapeTypewriterSetupRequested, TimeBombSetSubtitleSpawnRequested, EscapeDoorExplosion, TimerHandlingEnableRequested,
            MotherBrainEscapeTimerStartRequested, MotherBrainBossBitRequested, ZebesTimebombEventRequested, EscapeDoorParticleSpawns,
            EscapeDoorPlm, EarthquakeTimerRefreshed);
}
