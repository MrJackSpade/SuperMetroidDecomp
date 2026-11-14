namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>The enemy-library sounds Ridley queues.</summary>
internal static class RidleySoundDefinitions
{
    /// <summary>The roar, queued by the body animation's roar cue at normal priority.</summary>
    internal static readonly SoundEffectId Roar = EnemySound.RidleyRoar;
    /// <summary>The death explosion, queued with each of Ridley's death explosions.</summary>
    internal static readonly SoundEffectId DeathExplosion = EnemySound.SmallExplosion;
}
