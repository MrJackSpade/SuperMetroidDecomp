using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies.BossBodies;

namespace SuperMetroid.AssetExtraction.Bosses;

/// <summary>One frame a boss draws on its body layer as the cartridge writes it (#1466): the tile each layer cell receives.</summary>
/// <param name="Name">The frame's name in the boss's body sheet.</param>
/// <param name="Cells">The tile written at each layer cell, by cell in reading order.</param>
internal sealed record BossBodyFrameSource(string Name, SortedDictionary<int, BackgroundTile> Cells)
{
    /// <summary>
    /// The frame each room animation whose characters the frame reads shows while the frame is shown, by animation (#1513):
    /// the frame is pictured whole over those frames' characters, as each state of Crocomire's melt image shows each band
    /// of its characters at the dissolve step that state leaves it. A cell reading the characters of an animation not named
    /// here shows them as the room loads them, as a frame shown before the animation selects any does.
    /// </summary>
    internal IReadOnlyDictionary<ArtworkAnimation, ArtworkFrame> Selected { get; init; } = new Dictionary<ArtworkAnimation, ArtworkFrame>();

    /// <summary>
    /// The frame's picture cells a column dissolve erases (#1652), in the picture's cells: Crocomire's melt images'. Empty
    /// for every other frame.
    /// </summary>
    internal IReadOnlyList<DissolvingCell> Dissolving { get; init; } = [];

    /// <summary>
    /// The pixels the frame's picture lies lower on the body's area than its cells (#1802): the rows Crocomire's charging and
    /// clawing poses sit lower against him. Zero for every other frame.
    /// </summary>
    internal int LoweredPixels { get; init; }

    /// <summary>The frame writing <paramref name="tiles"/> from layer cell <paramref name="firstCell"/> onward.</summary>
    internal static BossBodyFrameSource Written(string name, int firstCell, ReadOnlySpan<BackgroundTile> tiles)
    {
        var cells = new SortedDictionary<int, BackgroundTile>();
        for (int index = 0; index < tiles.Length; index++)
            cells[firstCell + index] = tiles[index];
        return new BossBodyFrameSource(name, cells);
    }
}
