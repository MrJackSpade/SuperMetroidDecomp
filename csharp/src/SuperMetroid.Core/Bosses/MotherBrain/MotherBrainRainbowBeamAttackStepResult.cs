using SuperMetroid.Core.Fx.PaletteFx;
using SuperMetroid.Core.Escape;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Debugger witness for one Mother Brain active-rainbow body-function call.</summary>
[IndependentFlag("independent requests and edges one attack step reports to the room")]
public readonly record struct MotherBrainRainbowBeamAttackStepResult(
    MotherBrainBodyFunction? PhaseBefore,
    MotherBrainBodyFunction? PhaseAfter,
    MotherBrainForcedSamusMovementResult? Movement,
    bool SoundQueued,
    bool PaletteRequested,
    MotherBrainRainbowExplosionRequest? Explosion,
    bool ChargeSoundQueued,
    bool BodyWalkRequested,
    bool HeadInstructionListRequested,
    bool BodyPostureRequested,
    MotherBrainSpriteTileTransferRequest? SpriteTileTransfer,
    bool BabySpawnRequested,
    bool FinalBeamSoundQueued,
    IReadOnlyList<MotherBrainDeathExplosionRequest> DeathExplosions,
    IReadOnlyList<MotherBrainSpriteTileTransferRequest> CorpseRottingVramTransfers,
    IReadOnlyList<MotherBrainCorpseDustRequest> CorpseDustRequests,
    bool MusicStopQueued,
    bool EscapeMusicQueued,
    IReadOnlyList<MotherBrainSpriteTileTransferRequest> EscapeSequenceTileTransfers,
    bool ExplodedDoorPaletteRequested,
    bool EscapeMusicTrackQueued,
    IReadOnlyList<PaletteFxObject> EscapePaletteFxRequests,
    bool EscapeTypewriterSetupRequested,
    bool TimeBombSetSubtitleSpawnRequested,
    DustExplosionRequest? EscapeDoorExplosion,
    bool TimerHandlingEnableRequested,
    bool MotherBrainEscapeTimerStartRequested,
    bool MotherBrainBossBitRequested,
    bool ZebesTimebombEventRequested,
    IReadOnlyList<MotherBrainEscapeDoorParticleSpawnRequest> EscapeDoorParticleSpawns,
    MotherBrainEscapeDoorPlmRequest? EscapeDoorPlm,
    bool EarthquakeTimerRefreshed);
