namespace SuperMetroid.Core.Bosses.Botwoon;

/// <summary>Botwoon's enemy-library sound effects, queued at normal priority.</summary>
internal static class BotwoonSoundDefinitions
{
    /// <summary>The spit sound, queued as the mouth opens.</summary>
    internal static readonly SoundEffectId Spit = EnemySound.BotwoonSpit;

    /// <summary>The small explosion of a landing head or body segment and of the crumbling wall.</summary>
    internal static readonly SoundEffectId Explosion = EnemySound.SmallExplosion;
}
