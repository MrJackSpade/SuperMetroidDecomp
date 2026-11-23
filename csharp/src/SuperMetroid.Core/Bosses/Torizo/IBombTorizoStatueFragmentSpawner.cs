namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>Spawns the fragments the Bomb Torizo statue's breaking hand throws, among the room's enemy projectiles.</summary>
internal interface IBombTorizoStatueFragmentSpawner
{
    /// <summary>Spawns the fragment <paramref name="request"/> names at the hand's block.</summary>
    void SpawnBombTorizoStatueBreakingProjectile(BombTorizoStatueProjectileRequest request);

    /// <summary>Shows the crumbling hand's debris over Bomb Torizo's sprite characters as the hand starts to crumble (#2623).</summary>
    void ShowChozoDebris();
}
