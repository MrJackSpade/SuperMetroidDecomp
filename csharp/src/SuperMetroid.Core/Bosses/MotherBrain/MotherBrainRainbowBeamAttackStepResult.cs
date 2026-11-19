using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Fx.ColourEffects;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// What one step of Mother Brain's late fight asked of the room: Samus's forced movement, the effects it started
/// (<see cref="MotherBrainStepEffect"/>), the pictures, explosions and spawns it asked for, and her colours.
/// </summary>
/// <param name="Movement">The forced Samus movement the step applied, if any.</param>
/// <param name="Effects">The effects the step started, in the order its phases asked for them.</param>
/// <param name="DrainColourImage">The image of her greying as the Baby drains her that the step shows, if it shows one.</param>
/// <param name="RevivalColourImage">The image of her revival's colours that the step shows, if it shows one.</param>
/// <param name="DeathFadeImage">The image of her body's fade to black that the step shows, if it shows one.</param>
/// <param name="CorpseFadeImage">The image of her corpse's fade to grey that the step shows, if it shows one.</param>
/// <param name="Explosion">The beam explosion asked for, if any.</param>
/// <param name="DeathExplosions">The death explosions asked for.</param>
/// <param name="CorpseDustRequests">The corpse dust asked for.</param>
/// <param name="EscapeColourEffectRequests">The escape's colour effects asked for.</param>
/// <param name="EscapeDoorExplosion">The escape door's explosion asked for, if any.</param>
/// <param name="EscapeDoorParticleSpawns">The escape door's particle spawns asked for.</param>
/// <param name="EscapeDoorObject">The escape door's room object asked for, if any.</param>
/// <param name="Earthquake">What the step last asked of the room's earthquake, if anything.</param>
/// <param name="SamusProjectileCooldown">The shared projectile cooldown, in updates, the step gave Samus, if it gave her one.</param>
public readonly record struct MotherBrainRainbowBeamAttackStepResult(
    MotherBrainForcedSamusMovementResult? Movement,
    IReadOnlyList<MotherBrainStepEffect> Effects,
    MotherBrainFadeImage? DrainColourImage,
    MotherBrainFadeImage? RevivalColourImage,
    MotherBrainFadeImage? DeathFadeImage,
    MotherBrainFadeImage? CorpseFadeImage,
    MotherBrainRainbowExplosionRequest? Explosion,
    IReadOnlyList<MotherBrainDeathExplosionRequest> DeathExplosions,
    IReadOnlyList<MotherBrainCorpseDustRequest> CorpseDustRequests,
    IReadOnlyList<ColourEffectId> EscapeColourEffectRequests,
    DustExplosionRequest? EscapeDoorExplosion,
    IReadOnlyList<MotherBrainEscapeDoorParticleSpawnRequest> EscapeDoorParticleSpawns,
    RoomObjectSpawnRequest<MotherBrainRoomMutation>? EscapeDoorObject,
    MotherBrainEarthquakeRequest? Earthquake,
    int? SamusProjectileCooldown)
{
    /// <summary>Whether the step started <paramref name="effect"/>.</summary>
    public bool Starts(MotherBrainStepEffect effect) => Effects.Contains(effect);
}
