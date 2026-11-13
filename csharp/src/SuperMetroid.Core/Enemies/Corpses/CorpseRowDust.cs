using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Core.Enemies.Corpses;

/// <summary>
/// The dust a dead monster raises as each of its corpse's pixel rows falls out of it (dead Sidehoppers and Tourian
/// corpses), and the dust sound the family requests. Each corpse rots its own rows; this only raises their dust.
/// </summary>
[RoomLifetime]
internal sealed class CorpseRowDust
{
    /// <summary>Receives the family's dust sound, queued as the family makes it.</summary>
    private readonly EnemyAudioQueue _audio;

    /// <summary>Raises the family's dust on the room's enemy audio queue.</summary>
    internal CorpseRowDust(EnemyAudioQueue audio) => _audio = audio;

    /// <summary>
    /// Spawns the dust cloud a finished corpse row raises, spread by <paramref name="random"/> across the corpse, and
    /// queues the dust sound on every eighth enemy frame.
    /// </summary>
    internal void Spawn(RoomEnemySlot slot, int random, EnemyProjectileEngine projectiles, int randomEnemyCounter)
    {
        projectiles.SpawnRoomGraphicsDustExplosion(
            new(CoordinateAxis.Wrap(slot.Position.X.Whole + CorpseDustSpread(random) - 14), CoordinateAxis.Wrap(slot.Position.Y.Whole + 16)),
            MiscDustAnimation.CorpseDustCloud);
        if (CorpseCadenceDefinitions.DustSoundEveryEighthEnemyPass.IsDue(randomEnemyCounter))
            _audio.QueueEnemySound(EnemySound.LiquidDamage, priority: SoundRequestPriority.Low);
    }

    /// <summary>How far right of its leftmost place <paramref name="random"/> spreads a corpse row's dust cloud.</summary>
    private static int CorpseDustSpread(int random) =>
        (CorpseDustDefinitions.SpreadFineSteps.Of(random) * CorpseDustDefinitions.SpreadFineStep) +
        (CorpseDustDefinitions.SpreadCoarseSteps.Of(random) * CorpseDustDefinitions.SpreadCoarseStep);
}
