using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>What Golden Torizo's egg-laying waits on (<see cref="TorizoEnemyState.EggsRemain"/>).</summary>
internal static class TorizoEggDefinitions
{
    /// <summary>
    /// The projectile kinds a laid egg's place still counts as an egg while it holds: Golden Torizo's eggs. The laying keeps
    /// each laid egg's place, not the egg, so a place another egg has since taken still counts, as the cartridge's laying
    /// reads the place's kind (#2332).
    /// </summary>
    internal static readonly RoomEnemyProjectileKind[] EggKinds = [RoomEnemyProjectileKind.GoldenTorizoEgg];
}
