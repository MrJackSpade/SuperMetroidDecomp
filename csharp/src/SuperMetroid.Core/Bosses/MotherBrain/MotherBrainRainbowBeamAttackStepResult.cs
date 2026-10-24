using SuperMetroid.Core.Fx.PaletteFx;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;

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
    MotherBrainSpritePage? SpritePage,
    bool BabySpawnRequested,
    bool FinalBeamSoundQueued,
    IReadOnlyList<MotherBrainDeathExplosionRequest> DeathExplosions,
    int? CorpseRotStepShown,
    IReadOnlyList<MotherBrainCorpseDustRequest> CorpseDustRequests,
    bool MusicStopQueued,
    bool EscapeMusicQueued,
    IReadOnlyList<MotherBrainSpritePage> EscapeSequencePages,
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
