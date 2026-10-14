using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Bosses.MotherBrain.Artwork;

/// <summary>
/// Five distinct bank-$A2 OAM compositions shared by the ordinary and Mother Brain Rinka loops. Their callbacks, timing
/// and branches live in the Rinkas' typed programs.
/// </summary>
internal static class RinkaVisualDefinitions
{
    /// <summary>Native Rinka spritemap bank.</summary>
    internal const byte Bank = 0xa2;

    /// <summary>Five unique compositions cover both Rinka programs.</summary>
    internal const int FrameCount = 5;

    /// <summary>Deduplicates the poses shared by ordinary and Mother Brain Rinkas, in authored order.</summary>
    internal static EnemySpritemapDefinition[] Frames()
    {
        EnemySpritemapDefinition[] frames = [.. RinkaAnimations.Spritemaps(Enum.GetValues<RinkaProgram>()).Distinct()
            .Select(pointer => new EnemySpritemapDefinition(Bank, pointer, $"rinka_spin_{pointer:x4}"))];
        if (frames.Length != FrameCount)
            throw new InvalidDataException($"Rinka shows {frames.Length} distinct frames, expected {FrameCount}.");
        return frames;
    }
}
