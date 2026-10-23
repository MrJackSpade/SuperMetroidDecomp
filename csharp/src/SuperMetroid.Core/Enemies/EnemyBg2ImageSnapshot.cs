using System.Collections.Immutable;

namespace SuperMetroid.Core.Enemies;

/// <summary>
/// An enemy BG2 image as an accepted NMI published it (#1353): its words in the hardware's screen order, shown on BG2
/// through the map size BG2's registers select, as the uploaded image was.
/// </summary>
/// <param name="words">The image's words, row by row, 32 per row: screen order for any 32-wide or two-screen map.</param>
public sealed class EnemyBg2ImageSnapshot(ImmutableArray<ushort> words)
{
    /// <summary>The image's words, row by row, 32 per row.</summary>
    public ImmutableArray<ushort> Words { get; } = words;
}
