using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>One statue fragment the breaking Bomb Torizo hand throws from its block.</summary>
/// <param name="Fragment">The piece that falls, which selects its animation, place and fall.</param>
/// <param name="RoomObjectBlock">The block the hand occupies, which the fragment's placement is relative to.</param>
/// <param name="Spawner">The room's enemy projectiles, among which the fragment spawns.</param>
internal sealed partial record BombTorizoStatueProjectileRequest(
    BombTorizoStatueFragment Fragment,
    Position16<RoomBlocks> RoomObjectBlock,
    IBombTorizoStatueFragmentSpawner Spawner) : RoomObjectProjectileRequest(RoomObjectBlock)
{
    /// <inheritdoc/>
    internal override void Spawn() => Spawner.SpawnBombTorizoStatueBreakingProjectile(this);
}
