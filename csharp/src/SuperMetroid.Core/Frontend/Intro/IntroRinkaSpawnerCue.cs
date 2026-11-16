namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>What the intro Rinka spawner's animation has its owner do (#2197).</summary>
internal enum IntroRinkaSpawnerCue
{
    /// <summary>Spawns the pair of Rinkas whose first strikes Samus; the other misses.</summary>
    SpawnStrikingPair,
    /// <summary>Spawns the pair of Rinkas that both miss Samus.</summary>
    SpawnMissingPair,
}
