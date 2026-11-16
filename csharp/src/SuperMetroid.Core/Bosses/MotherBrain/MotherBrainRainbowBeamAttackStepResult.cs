using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Fx.ColourEffects;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>What one step of Mother Brain's late fight asked of the room: Samus's forced movement, her colours and the requests it made.</summary>
[IndependentFlag("independent requests and edges one attack step reports to the room")]
public readonly record struct MotherBrainRainbowBeamAttackStepResult(
    MotherBrainForcedSamusMovementResult? Movement,
    bool SoundQueued,
    bool RainbowColours,
    bool BeamEnded,
    bool DrainedBeamRanOut,
    MotherBrainFadeImage? DrainColourImage,
    MotherBrainFadeImage? RevivalColourImage,
    bool BrainDetached,
    bool BodyFlickers,
    MotherBrainFadeImage? DeathFadeImage,
    MotherBrainFadeImage? CorpseFadeImage,
    MotherBrainRainbowExplosionRequest? Explosion,
    bool ChargeSoundQueued,
    bool BodyClearRequested,
    bool BabySpawnRequested,
    bool FinalBeamFired,
    IReadOnlyList<MotherBrainDeathExplosionRequest> DeathExplosions,
    IReadOnlyList<MotherBrainCorpseDustRequest> CorpseDustRequests,
    bool MusicStopQueued,
    bool EscapeMusicQueued,
    bool EscapeMusicTrackQueued,
    IReadOnlyList<ColourEffectId> EscapeColourEffectRequests,
    bool EscapeTypewriterSetupRequested,
    bool TimeBombSetSubtitleSpawnRequested,
    DustExplosionRequest? EscapeDoorExplosion,
    bool GetUpReleaseRequested,
    bool MotherBrainEscapeTimerStartRequested,
    bool MotherBrainDefeatRecordRequested,
    bool ZebesTimebombEventRequested,
    IReadOnlyList<MotherBrainEscapeDoorParticleSpawnRequest> EscapeDoorParticleSpawns,
    RoomObjectSpawnRequest<MotherBrainRoomMutation>? EscapeDoorObject,
    MotherBrainEarthquakeRequest? Earthquake,
    int? SamusProjectileCooldown);
