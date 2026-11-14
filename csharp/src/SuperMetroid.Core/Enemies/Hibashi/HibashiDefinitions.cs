namespace SuperMetroid.Core.Enemies.Hibashi;

/// <summary>Compiled authored definitions for Hibashi/fire pillars.</summary>
internal static class HibashiDefinitions
{



    /// <summary>The fire-spit sound, queued when an eruption begins.</summary>
    internal static readonly SoundEffectId EruptionSoundEffect = EnemySound.FireSpit;

    /// <summary>
    /// The eruption's 22 rising steps in order: the pillar rises five pixels a step, and its half-height holds at 24 until
    /// the last four steps shrink it four pixels each. The first step also gives the collision part its width back.
    /// </summary>
    internal static readonly HibashiEruptionStep[] EruptionSteps =
    [
        new(5, 24, RestoresWidth: true), new(10, 24), new(15, 24), new(20, 24), new(25, 24), new(30, 24),
        new(35, 24), new(40, 24), new(45, 24), new(50, 24), new(55, 24), new(60, 24),
        new(65, 24), new(70, 24), new(75, 24), new(80, 24), new(85, 24), new(90, 24),
        new(95, 20), new(100, 16), new(105, 12), new(110, 8),
    ];
}
