using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Samus.Movement;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>Everything a shot's behaviour receives for one shot on one shot update (#1923).</summary>
/// <param name="Held">The shot whose behaviour runs.</param>
/// <param name="Shots">Samus's shots, whose motion, special attacks, echoes, fuse and sounds the behaviours run.</param>
/// <param name="Terrain">The room's blocks and room objects the shots act on.</param>
/// <param name="Samus">Samus, whom special attack particles and echoes follow.</param>
/// <param name="Camera">The camera of the update, in whole room pixels, which the shots test the screen against.</param>
/// <param name="SamusBombs">Samus's bombs, which carry the shared cooldown and the Power Bomb the shots read.</param>
/// <param name="Progression">
/// The progression the Spacetime shot's overwrite turns story events, bosses, items, doors, stations, maps and the saved
/// startup route on and off in (<see cref="SpacetimeShotOverwrite"/>).
/// </param>
[RoomScopeBoundary("A per-call input, built for one call and never kept.")]
internal readonly partial record struct SamusShotUpdate(
    SamusProjectileOccupant Held,
    SamusProjectileSystem Shots,
    SamusTerrain Terrain,
    SamusState Samus,
    Position16<RoomPixels> Camera,
    SamusBombProjectileSystem SamusBombs,
    SharedGameState Progression)
{
    /// <summary>The place the updated shot holds, with its position, damage and animation.</summary>
    internal SamusProjectile Shot => Held.Projectile;
}
