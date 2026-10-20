using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>One <c>$86:E509</c> dust explosion requested by the Baby's death or the escape door.</summary>
/// <param name="Position">Where the explosion appears in the room.</param>
/// <param name="Animation">The dust animation it plays.</param>
/// <param name="SoundEffect">The sound it queues.</param>
public readonly record struct DustExplosionRequest(Position16<RoomPixels> Position, MiscDustAnimation Animation, SoundEffectId SoundEffect);
