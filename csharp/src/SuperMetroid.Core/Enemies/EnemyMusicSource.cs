namespace SuperMetroid.Core.Enemies;

/// <summary>
/// An encounter whose music requests join the update's music requests only when the enemy pass ends, through
/// <see cref="EnemyAudioQueue.RequestMusicAtUpdateEnd"/> or <see cref="EnemyAudioQueue.AppendMusicAtUpdateEnd"/>.
/// The room enemy system publishes the sources in one fixed order, after every enemy sound.
/// </summary>
internal enum EnemyMusicSource
{
    /// <summary>Botwoon's fight music.</summary>
    Botwoon,
    /// <summary>Bomb Torizo's fight music.</summary>
    BombTorizo,
    /// <summary>Crocomire's fight and death music.</summary>
    Crocomire,
    /// <summary>Kraid's rise and death music.</summary>
    Kraid,
    /// <summary>Draygon's death music.</summary>
    Draygon,
    /// <summary>Phantoon's opening and death music.</summary>
    Phantoon,
    /// <summary>Ridley's fight, getaway and death music.</summary>
    Ridley,
    /// <summary>The Shitroid's music.</summary>
    Shitroid,
    /// <summary>Mother Brain's music sequence.</summary>
    MotherBrain,
}
