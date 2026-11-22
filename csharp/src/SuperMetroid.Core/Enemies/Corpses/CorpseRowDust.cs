using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.Corpses;

/// <summary>
/// The dust a dead monster raises as each of its corpse's pixel rows falls out of it (dead Sidehoppers and Tourian
/// corpses), and the dust sound the family requests. Each corpse rots its own rows; this only raises their dust.
/// </summary>
internal sealed partial class CorpseRowDust
{
    /// <summary>Receives the family's dust sound, queued as the family makes it.</summary>
    private readonly EnemyAudioQueue _audio;

    /// <summary>Raises the family's dust on the room's enemy audio queue.</summary>
    internal CorpseRowDust(EnemyAudioQueue audio) => _audio = audio;

    /// <summary>
    /// Spawns the dust cloud a finished corpse row raises, spread by <paramref name="random"/> across the corpse, and
    /// queues the dust sound at its cadence (<see cref="CorpseCadenceDefinitions.DustSound"/>).
    /// </summary>
    internal void Spawn(RoomEnemyPlace place, int random, EnemyProjectileEngine projectiles, int enemyPassCount)
    {
        projectiles.SpawnDustOrExplosion(
            place.Position.Whole + CorpseDustDefinitions.CloudOrigin + new Offset16<RoomPixels>(CorpseDustSpread(random), 0),
            DustExplosionAnimation.CorpseDustCloud);
        if (CorpseCadenceDefinitions.DustSound.IsDue(enemyPassCount))
            _audio.QueueEnemySound(EnemySound.LiquidDamage, priority: SoundRequestPriority.Low);
    }

    /// <summary>How far right of its leftmost place <paramref name="random"/> spreads a corpse row's dust cloud.</summary>
    private static int CorpseDustSpread(int random) =>
        (EnemyRandomRolls.CorpseDustFineSpread.Of(random) * CorpseDustDefinitions.SpreadFineStep) +
        (EnemyRandomRolls.CorpseDustCoarseSpread.Of(random) * CorpseDustDefinitions.SpreadCoarseStep);
}
