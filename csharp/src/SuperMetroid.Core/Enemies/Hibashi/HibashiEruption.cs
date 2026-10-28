namespace SuperMetroid.Core.Enemies.Hibashi;

/// <summary>A Hibashi fire pillar's eruption frames.</summary>
internal static class HibashiEruption
{
    /// <summary>The Y offset and collision half-height of eruption frame <paramref name="index"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is outside the 22 frames.</exception>
    internal static HibashiActivityDefinition ActivityFrame(int index) =>
        (uint)index < (uint)HibashiDefinitions.ActivityFrames.Length
            ? HibashiDefinitions.ActivityFrames[index]
            : throw new ArgumentOutOfRangeException(nameof(index));
}
