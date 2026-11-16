namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>Compiled sound selections owned by Phantoon's animation cues.</summary>
internal static class PhantoonSoundDefinitions
{
    /// <summary>Fight-intro starting-flame spawn: room-library blue-flame sound.</summary>
    internal static SoundEffectId StartingFlame => RoomSound.PhantoonFlame;

    /// <summary>The request priority of the sound each starting-flame spawn makes.</summary>
    internal static readonly SoundRequestPriority StartingFlamePriority = SoundRequestPriority.Normal;

    /// <summary>SpawnCasualFlame: blue flame launch sound in the room library.</summary>
    internal static SoundEffectId CasualFlame => RoomSound.PhantoonFlame;

    /// <summary>Phantoon's cry, queued when a shot hurts or kills Phantoon.</summary>
    internal static readonly SoundEffectId ShotCry = EnemySound.Shriek;

    /// <summary>The long wail, queued as the dying Phantoon turns wavy.</summary>
    internal static readonly SoundEffectId DyingCry = EnemySound.LongWail;

    /// <summary>The small explosion, queued after a big death explosion.</summary>
    internal static readonly SoundEffectId BigDeathExplosionSound = EnemySound.SmallExplosion;

    /// <summary>The rolling rumble, queued after any other death explosion.</summary>
    internal static readonly SoundEffectId DeathExplosionSound = EnemySound.RollingRumble;

    /// <summary>The casual-flame sound's request priority.</summary>
    internal static readonly SoundRequestPriority CasualFlamePriority = SoundRequestPriority.Normal;

    /// <summary>
    /// The materialization sounds in the order they are queued: each materialization queues the next, the first again after
    /// the last.
    /// </summary>
    internal static readonly SoundEffectId[] MaterializationSounds =
    [
        EnemySound.PhantoonAppears1,
        EnemySound.PhantoonAppears2,
        EnemySound.PhantoonAppears3,
    ];
}
