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

    /// <summary>Whether the call ran her rainbow colours' handler, which steps them on its phase of the body's update count.</summary>
    internal bool RainbowColours { get; set; }

    /// <summary>Whether the beam narrowed below its narrowest and let Samus go, so her colours return and the drain's sound stops.</summary>
    internal bool BeamEnded { get; set; }

    /// <summary>Whether the drained beam ran out at the painful walk's sixth step, leaving her colours on the drained frame.</summary>
    internal bool DrainedBeamRanOut { get; set; }

    /// <summary>The step of her greying as the Baby drains her that the call shows, if it shows one.</summary>
    internal int? DrainColourStep { get; set; }

    /// <summary>The step of her revival's colours that the call shows, if it shows one.</summary>
    internal int? RevivalColourStep { get; set; }

    /// <summary>Whether disabling her brain's effects detached her brain, which keeps the colours it shows.</summary>
    internal bool BrainDetached { get; set; }

    /// <summary>Whether her body's fade ran, which flickers her body with the background on every call.</summary>
    internal bool BodyFlickers { get; set; }

    /// <summary>The step of her body's fade to black that the call shows, if it shows one.</summary>
    internal int? DeathFadeStep { get; set; }

    /// <summary>The step of her corpse's fade to grey that the call shows, if it shows one.</summary>
    internal int? CorpseFadeStep { get; set; }

    /// <summary>The beam explosion requested, if any.</summary>
    internal MotherBrainRainbowExplosionRequest? Explosion { get; set; }

    /// <summary>Whether the charge sound was queued.</summary>
    internal bool ChargeSoundQueued { get; set; }

    /// <summary>Whether her faded body has just gone, so her body's picture on the background is cleared (#1466).</summary>
    internal bool BodyClearRequested { get; set; }

    /// <summary>Whether the Baby Metroid's spawn was requested.</summary>
    internal bool BabySpawnRequested { get; set; }

    /// <summary>Whether the final beam fired: its sound is queued and her rainbow colours start again from the first.</summary>
    internal bool FinalBeamFired { get; set; }

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

    /// <summary>The escape's colour effects requested.</summary>
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

    /// <summary>Starts the room's earthquake with <paramref name="shake"/> until a producer gives it a length.</summary>
    internal void StartIndefiniteEarthquake(EarthquakeShake shake) => Earthquake = new(shake, Frames: null);

    /// <summary>
    /// Runs the room's earthquake on until a producer gives it a length, keeping any shake this call started or else the
    /// room's own.
    /// </summary>
    internal void KeepEarthquakeGoing() => Earthquake = new(Earthquake?.Shake, Frames: null);

    /// <summary>Runs the room's earthquake on for <paramref name="frames"/> frames, keeping any shake this call started.</summary>
    internal void RunEarthquakeOn(int frames) => Earthquake = new(Earthquake?.Shake, frames);

    /// <summary>Stops the room's earthquake, keeping any shake this call started for a later run.</summary>
    internal void StopEarthquake() => RunEarthquakeOn(0);

    /// <summary>The call's result.</summary>
    internal MotherBrainRainbowBeamAttackStepResult ToResult() =>
        new(Movement, SoundQueued, RainbowColours, BeamEnded, DrainedBeamRanOut, DrainColourStep, RevivalColourStep,
            BrainDetached, BodyFlickers, DeathFadeStep, CorpseFadeStep, Explosion, ChargeSoundQueued,
            BodyClearRequested, BabySpawnRequested, FinalBeamFired,
            DeathExplosions, CorpseDustRequests, MusicStopQueued, EscapeMusicQueued,
            EscapeMusicTrackQueued, EscapeColourEffectRequests,
            EscapeTypewriterSetupRequested, TimeBombSetSubtitleSpawnRequested, EscapeDoorExplosion, GetUpReleaseRequested,
            MotherBrainEscapeTimerStartRequested, MotherBrainDefeatRecordRequested, ZebesTimebombEventRequested, EscapeDoorParticleSpawns,
            EscapeDoorObject, Earthquake, SamusProjectileCooldown);
}
