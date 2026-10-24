namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>The library-two sounds Ridley queues.</summary>
internal static class RidleySoundDefinitions
{
    /// <summary>The roar $59, queued by the body program's roar cue (maximum six queued).</summary>
    internal static readonly SoundEffectId Roar = Library2Sound.RidleyRoar;
    /// <summary>The death explosion $24, queued with each of Ridley's death explosions.</summary>
    internal static readonly SoundEffectId DeathExplosion = Library2Sound.SmallExplosion;
}
