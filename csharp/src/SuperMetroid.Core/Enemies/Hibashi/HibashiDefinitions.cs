namespace SuperMetroid.Core.Enemies.Hibashi;

/// <summary>Compiled cartridge definitions for Hibashi/fire pillars.</summary>
internal static class HibashiDefinitions
{



    /// <summary>Sound effect $61 in library two, queued when an eruption begins.</summary>
    internal static readonly SoundEffectId EruptionSoundEffect = Library2Sound.FireSpit;

    /// <summary>
    /// $A6:8DBB/$A6:8DE7, the 22 eruption Y offsets and collision half-heights.
    /// </summary>
    internal static HibashiActivityDefinition ActivityFrame(int index)
    {
        if ((uint)index >= 22)
            throw new ArgumentOutOfRangeException(nameof(index));
        return new((ushort)((index + 1) * 5), (ushort)(24 - Math.Max(0, index - 17) * 4));
    }
}
