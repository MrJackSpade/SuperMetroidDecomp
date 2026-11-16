using SuperMetroid.Core.Escape;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.World;

/// <summary>An escape room's main (#2287): its random explosions, and in two rooms its shaking turning diagonal now and then.</summary>
/// <param name="site">Where the room's explosions go off.</param>
/// <param name="diagonal">The room's diagonal shaking and its countdown, or null for a room whose shaking never turns diagonal.</param>
[RoomLifetime]
internal sealed class EscapeShakingMain(EscapeExplosionSite site, EscapeDiagonalQuake? diagonal) : IRoomMain
{
    /// <inheritdoc/>
    public void Run(RoomMainUpdate update) => update.Escape.StepEscapeRoom(update.Room, site, diagonal);
}
