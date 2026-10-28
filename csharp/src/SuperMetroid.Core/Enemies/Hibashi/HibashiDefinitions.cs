namespace SuperMetroid.Core.Enemies.Hibashi;

/// <summary>Compiled cartridge definitions for Hibashi/fire pillars.</summary>
internal static class HibashiDefinitions
{



    /// <summary>Sound effect $61 in library two, queued when an eruption begins.</summary>
    internal static readonly SoundEffectId EruptionSoundEffect = Library2Sound.FireSpit;

    /// <summary>
    /// $A6:8DBB/$A6:8DE7, the 22 eruption Y offsets and collision half-heights: the pillar rises five pixels a frame,
    /// and its half-height holds at 24 until the last four frames shrink it four pixels each.
    /// </summary>
    internal static readonly HibashiActivityDefinition[] ActivityFrames =
    [
        new(5, 24), new(10, 24), new(15, 24), new(20, 24), new(25, 24), new(30, 24),
        new(35, 24), new(40, 24), new(45, 24), new(50, 24), new(55, 24), new(60, 24),
        new(65, 24), new(70, 24), new(75, 24), new(80, 24), new(85, 24), new(90, 24),
        new(95, 20), new(100, 16), new(105, 12), new(110, 8),
    ];
}
