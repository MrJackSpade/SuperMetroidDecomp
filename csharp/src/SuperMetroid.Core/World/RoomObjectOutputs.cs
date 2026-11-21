namespace SuperMetroid.Core.World;

/// <summary>
/// Where the room objects' step sends what it makes (#2057, #2514): the update's audio requests, which their sounds and
/// music join, and the spawners of the projectiles they throw.
/// </summary>
/// <param name="AudioRequests">The update's audio requests of the runtime.</param>
/// <param name="Thrown">Where the projectiles the room objects throw spawn.</param>
internal sealed record RoomObjectOutputs(GameplayAudioRequests AudioRequests, RoomObjectThrownProjectileSpawners Thrown);
