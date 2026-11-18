namespace SuperMetroid.Core.Enemies.Hibashi;

/// <summary>The authored definitions of Hibashi, the fire pillars.</summary>
internal static class HibashiDefinitions
{
    /// <summary>The half width, in pixels, the first rising step gives the collision part back.</summary>
    internal const int RestoredHalfWidth = 8;

    /// <summary>The fire-spit sound, queued when an eruption begins.</summary>
    internal static readonly SoundEffectId EruptionSoundEffect = EnemySound.FireSpit;
}
