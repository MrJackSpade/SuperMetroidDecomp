using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Fx.ColourEffects;
using SuperMetroid.Core.Earthquakes;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// What one Mother Brain active-rainbow body-phase call asks of the room, gathered while the call runs through its
/// phases and reported as one <see cref="MotherBrainRainbowBeamAttackStepResult"/>.
/// </summary>
internal sealed class MotherBrainRainbowBeamStepRequests
{
    /// <summary>The forced Samus movement the call applied, if any.</summary>
    internal MotherBrainForcedSamusMovementResult? Movement { get; set; }

    /// <summary>Whether the beam's sound was queued.</summary>
    internal bool SoundQueued { get; set; }

    /// <summary>Whether this step shows the next step of her colours: the rainbow, the drain, the revival or the death fade.</summary>
    internal bool ColoursRequested { get; set; }

    /// <summary>The beam explosion requested, if any.</summary>
    internal MotherBrainRainbowExplosionRequest? Explosion { get; set; }

    /// <summary>Whether the charge sound was queued.</summary>
    internal bool ChargeSoundQueued { get; set; }

    /// <summary>Whether the body was asked to walk.</summary>
    internal bool BodyWalkRequested { get; set; }

    /// <summary>Whether the body was asked to change posture.</summary>
    internal bool BodyPostureRequested { get; set; }

    /// <summary>Whether the Baby Metroid's spawn was requested.</summary>
    internal bool BabySpawnRequested { get; set; }

    /// <summary>Whether the final beam's sound was queued.</summary>
    internal bool FinalBeamSoundQueued { get; set; }

    /// <summary>The death explosions requested.</summary>
    internal List<MotherBrainDeathExplosionRequest> DeathExplosions { get; } = new(capacity: 4);

    /// <summary>The corpse dust requested.</summary>
    internal List<MotherBrainCorpseDustRequest> CorpseDustRequests { get; } = new(capacity: 1);

    /// <summary>Whether stopping the music was queued.</summary>
    internal bool MusicStopQueued { get; set; }

    /// <summary>Whether the escape music was queued.</summary>
    internal bool EscapeMusicQueued { get; set; }

    /// <summary>Whether the escape music track was queued.</summary>
    internal bool EscapeMusicTrackQueued { get; set; }

    /// <summary>The escape's palette effects requested.</summary>
    internal List<ColourEffectId> EscapeColourEffectRequests { get; } = new(capacity: 4);

    /// <summary>Whether the escape typewriter's setup was requested.</summary>
    internal bool EscapeTypewriterSetupRequested { get; set; }

    /// <summary>Whether the time-bomb-set subtitle's spawn was requested.</summary>
    internal bool TimeBombSetSubtitleSpawnRequested { get; set; }

    /// <summary>The escape door's explosion requested, if any.</summary>
    internal DustExplosionRequest? EscapeDoorExplosion { get; set; }

    /// <summary>Whether ending Samus's drained get-up was requested, because the escape countdown takes its place.</summary>
    internal bool GetUpReleaseRequested { get; set; }

    /// <summary>Whether starting the escape timer was requested.</summary>
    internal bool MotherBrainEscapeTimerStartRequested { get; set; }

    /// <summary>Whether recording Mother Brain's defeat was requested.</summary>
    internal bool MotherBrainDefeatRecordRequested { get; set; }

    /// <summary>Whether the Zebes time-bomb event was requested.</summary>
    internal bool ZebesTimebombEventRequested { get; set; }

    /// <summary>The escape door's particle spawns requested.</summary>
    internal List<MotherBrainEscapeDoorParticleSpawnRequest> EscapeDoorParticleSpawns { get; } = new(capacity: 8);

    /// <summary>The escape door's room object requested, if any.</summary>
    internal RoomObjectSpawnRequest<MotherBrainRoomMutation>? EscapeDoorObject { get; set; }

    /// <summary>The shared projectile cooldown, in updates, the call gave Samus, if it gave her one.</summary>
    internal int? SamusProjectileCooldown { get; set; }

    /// <summary>What the call last asked of the room's earthquake, if anything.</summary>
    internal MotherBrainEarthquakeRequest? Earthquake { get; private set; }

    /// <summary>Starts the room's earthquake with <paramref name="shake"/> for <paramref name="frames"/> frames.</summary>
    internal void StartEarthquake(EarthquakeShake shake, int frames) => Earthquake = new(shake, frames);

    /// <summary>Runs the room's earthquake on for <paramref name="frames"/> frames, keeping any shake this call started.</summary>
    internal void RunEarthquakeOn(int frames) => Earthquake = new(Earthquake?.Shake, frames);

    /// <summary>Stops the room's earthquake, keeping any shake this call started for a later run.</summary>
    internal void StopEarthquake() => RunEarthquakeOn(0);

    /// <summary>The call's result, from the phases it started and ended in.</summary>
    internal MotherBrainRainbowBeamAttackStepResult ToResult(
        MotherBrainBodyPhase? phaseBefore, MotherBrainBodyPhase? phaseAfter, bool headAnimationScriptRequested) =>
        new(phaseBefore, phaseAfter, Movement, SoundQueued, ColoursRequested, Explosion, ChargeSoundQueued, BodyWalkRequested,
            headAnimationScriptRequested, BodyPostureRequested, BabySpawnRequested, FinalBeamSoundQueued,
            DeathExplosions, CorpseDustRequests, MusicStopQueued, EscapeMusicQueued,
            EscapeMusicTrackQueued, EscapeColourEffectRequests,
            EscapeTypewriterSetupRequested, TimeBombSetSubtitleSpawnRequested, EscapeDoorExplosion, GetUpReleaseRequested,
            MotherBrainEscapeTimerStartRequested, MotherBrainDefeatRecordRequested, ZebesTimebombEventRequested, EscapeDoorParticleSpawns,
            EscapeDoorObject, Earthquake, SamusProjectileCooldown);
}
