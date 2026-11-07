namespace SuperMetroid.Core.Bosses.Botwoon;

/// <summary>Botwoon's library-2 sound effects, queued with a six-entry admission limit.</summary>
internal static class BotwoonSoundDefinitions
{
    /// <summary>The spit sound, queued as the mouth opens.</summary>
    internal static readonly SoundEffectId Spit = Library2Sound.BotwoonSpit;

    /// <summary>The small explosion of a landing head or body segment and of the crumbling wall.</summary>
    internal static readonly SoundEffectId Explosion = Library2Sound.SmallExplosion;
}
