namespace SuperMetroid.Core.Enemies.Hibashi;

/// <summary>The authored definitions of Hibashi, the fire pillars.</summary>
internal static partial class HibashiDefinitions
{
    /// <summary>The half width, in pixels, the first rising step gives the collision part back.</summary>
    internal const int RestoredHalfWidth = 8;

    /// <summary>
    /// The updates a newly placed flame waits before its first eruption, the one it erupts on included: it erupts on its
    /// first update (#2536).
    /// </summary>
    internal const int FirstWait = 1;

    /// <summary>The fire-spit sound, queued when an eruption begins.</summary>
    internal static readonly SoundEffectId EruptionSoundEffect = EnemySound.FireSpit;
}
