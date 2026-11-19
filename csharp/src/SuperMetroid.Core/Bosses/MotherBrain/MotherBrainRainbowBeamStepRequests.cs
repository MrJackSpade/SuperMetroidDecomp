using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Fx.ColourEffects;
using SuperMetroid.Core.Earthquakes;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// What one update of Mother Brain's late fight asks of the room, gathered while the update runs through her phases and
/// reported as one <see cref="MotherBrainRainbowBeamAttackStepResult"/>.
/// </summary>
internal sealed class MotherBrainRainbowBeamStepRequests
{
    /// <summary>The forced Samus movement the update applied, if any.</summary>
    internal MotherBrainForcedSamusMovementResult? Movement { get; set; }

    /// <summary>The effects the update's phases started, in the order they asked for them.</summary>
    private readonly List<MotherBrainStepEffect> _effects = new(capacity: 4);

    /// <summary>Starts <paramref name="effect"/> on this update, after the effects started before it.</summary>
    internal void Start(MotherBrainStepEffect effect) => _effects.Add(effect);

    /// <summary>The image of her greying as the Baby drains her that the update shows, if it shows one.</summary>
    internal MotherBrainFadeImage? DrainColourImage { get; set; }

    /// <summary>The image of her revival's colours that the update shows, if it shows one.</summary>
    internal MotherBrainFadeImage? RevivalColourImage { get; set; }

    /// <summary>The image of her body's fade to black that the update shows, if it shows one.</summary>
    internal MotherBrainFadeImage? DeathFadeImage { get; set; }

    /// <summary>The image of her corpse's fade to grey that the update shows, if it shows one.</summary>
    internal MotherBrainFadeImage? CorpseFadeImage { get; set; }

    /// <summary>The beam explosion requested, if any.</summary>
    internal MotherBrainRainbowExplosionRequest? Explosion { get; set; }

    /// <summary>The death explosions requested.</summary>
    internal List<MotherBrainDeathExplosionRequest> DeathExplosions { get; } = new(capacity: 4);

    /// <summary>The corpse dust requested.</summary>
    internal List<MotherBrainCorpseDustRequest> CorpseDustRequests { get; } = new(capacity: 1);

    /// <summary>The escape's colour effects requested.</summary>
    internal List<ColourEffectId> EscapeColourEffectRequests { get; } = new(capacity: 4);

    /// <summary>The escape door's explosion requested, if any.</summary>
    internal DustExplosionRequest? EscapeDoorExplosion { get; set; }

    /// <summary>The escape door's particle spawns requested.</summary>
    internal List<MotherBrainEscapeDoorParticleSpawnRequest> EscapeDoorParticleSpawns { get; } = new(capacity: 8);

    /// <summary>The escape door's room object requested, if any.</summary>
    internal RoomObjectSpawnRequest<MotherBrainRoomMutation>? EscapeDoorObject { get; set; }

    /// <summary>The shared projectile cooldown, in updates, the update gave Samus, if it gave her one.</summary>
    internal int? SamusProjectileCooldown { get; set; }

    /// <summary>What the update last asked of the room's earthquake, if anything.</summary>
    internal MotherBrainEarthquakeRequest? Earthquake { get; private set; }

    /// <summary>Starts the room's earthquake with <paramref name="shake"/> for <paramref name="frames"/> frames.</summary>
    internal void StartEarthquake(EarthquakeShake shake, int frames) => Earthquake = new(shake, frames);

    /// <summary>Starts the room's earthquake with <paramref name="shake"/> until a producer gives it a length.</summary>
    internal void StartIndefiniteEarthquake(EarthquakeShake shake) => Earthquake = new(shake, Frames: null);

    /// <summary>
    /// Runs the room's earthquake on until a producer gives it a length, keeping any shake this update started or else the
    /// room's own.
    /// </summary>
    internal void KeepEarthquakeGoing() => Earthquake = new(Earthquake?.Shake, Frames: null);

    /// <summary>Runs the room's earthquake on for <paramref name="frames"/> frames, keeping any shake this update started.</summary>
    internal void RunEarthquakeOn(int frames) => Earthquake = new(Earthquake?.Shake, frames);

    /// <summary>Stops the room's earthquake, keeping any shake this update started for a later run.</summary>
    internal void StopEarthquake() => RunEarthquakeOn(0);

    /// <summary>The update's result.</summary>
    internal MotherBrainRainbowBeamAttackStepResult ToResult() =>
        new(Movement, [.. _effects], DrainColourImage, RevivalColourImage, DeathFadeImage, CorpseFadeImage, Explosion,
            DeathExplosions, CorpseDustRequests, EscapeColourEffectRequests, EscapeDoorExplosion, EscapeDoorParticleSpawns,
            EscapeDoorObject, Earthquake, SamusProjectileCooldown);
}
