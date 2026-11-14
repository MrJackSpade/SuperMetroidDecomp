using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Fx.ColourEffects;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>What one step of Mother Brain's rainbow beam attack did: its phase before and after, Samus's forced movement and the requests it made.</summary>
[IndependentFlag("independent requests and edges one attack step reports to the room")]
public readonly record struct MotherBrainRainbowBeamAttackStepResult(
    MotherBrainBodyPhase? PhaseBefore,
    MotherBrainBodyPhase? PhaseAfter,
    MotherBrainForcedSamusMovementResult? Movement,
    bool SoundQueued,
    bool ColoursRequested,
    MotherBrainRainbowExplosionRequest? Explosion,
    bool ChargeSoundQueued,
    bool BodyWalkRequested,
    bool HeadAnimationRequested,
    bool BodyPostureRequested,
    bool BabySpawnRequested,
    bool FinalBeamSoundQueued,
    IReadOnlyList<MotherBrainDeathExplosionRequest> DeathExplosions,
    IReadOnlyList<MotherBrainCorpseDustRequest> CorpseDustRequests,
    bool MusicStopQueued,
    bool EscapeMusicQueued,
    bool EscapeMusicTrackQueued,
    IReadOnlyList<ColourEffectId> EscapeColourEffectRequests,
    bool EscapeTypewriterSetupRequested,
    bool TimeBombSetSubtitleSpawnRequested,
    DustExplosionRequest? EscapeDoorExplosion,
    bool TimerHandlingEnableRequested,
    bool MotherBrainEscapeTimerStartRequested,
    bool MotherBrainDefeatRecordRequested,
    bool ZebesTimebombEventRequested,
    IReadOnlyList<MotherBrainEscapeDoorParticleSpawnRequest> EscapeDoorParticleSpawns,
    RoomObjectSpawnRequest<MotherBrainRoomMutation>? EscapeDoorObject,
    MotherBrainEarthquakeRequest? Earthquake,
    int? SamusProjectileCooldown);
