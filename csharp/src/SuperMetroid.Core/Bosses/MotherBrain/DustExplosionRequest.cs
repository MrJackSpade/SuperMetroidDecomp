using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>One dust explosion requested by the Baby's death or the escape door.</summary>
/// <param name="Position">Where the explosion appears in the room.</param>
/// <param name="Animation">The dust animation it plays.</param>
/// <param name="Sound">The sound it queues, at the priority it queues it.</param>
public readonly partial record struct DustExplosionRequest(Position16<RoomPixels> Position, DustExplosionAnimation Animation, SoundRequest Sound);
