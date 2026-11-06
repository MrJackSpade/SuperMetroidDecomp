using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Fx.PaletteFx;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>What one step of Mother Brain's rainbow beam attack did: its phase before and after, Samus's forced movement and the requests it made.</summary>
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
    bool HeadAnimationScriptRequested,
    bool BodyPostureRequested,
    MotherBrainSpritePage? SpritePage,
    bool BabySpawnRequested,
    bool FinalBeamSoundQueued,
    IReadOnlyList<MotherBrainDeathExplosionRequest> DeathExplosions,
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
    PlmSpawnRequest<MotherBrainRoomMutation>? EscapeDoorPlm,
    bool EarthquakeTimerRefreshed);
