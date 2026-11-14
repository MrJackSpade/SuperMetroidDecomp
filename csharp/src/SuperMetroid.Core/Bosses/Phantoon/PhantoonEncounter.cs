using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>
/// Phantoon's fight in his room: his state once his body has initialized. His combat and his flames share it: the flames
/// circle his body and go out when he is gone.
/// </summary>
[RoomLifetime]
internal sealed class PhantoonEncounter
{
    /// <summary>Phantoon's encounter state once his body has initialized in the room.</summary>
    internal PhantoonEnemyState? Phantoon { get; set; }
}
