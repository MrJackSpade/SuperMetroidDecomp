using System.Collections.Immutable;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Enemies.BossBodies;

/// <summary>
/// A boss body layer as an accepted frame latched it (#1466): the parts the boss has drawn on it, each over cells no other shows. A part that clears its cells shows nothing over them, and a cell no
/// part shows draws nothing (#1513). The frame shows it as pictures placed where they show (<see cref="Show"/>, #1514).
/// </summary>
/// <param name="Placements">The parts showing and the cells each shows.</param>
public sealed record BossBodyLayer(ImmutableArray<BossBodyPlacement> Placements)
{
    /// <summary>
    /// The layer as the frame shows it: each part's picture for the frame its animation shows in
    /// <paramref name="selections"/>, which hold what the room's animations had selected when the frame was accepted,
    /// placed over the area its cells cover on a body covering <paramref name="area"/>. A part that clears its cells has
    /// no picture there, so the cells it shows draw nothing.
    /// </summary>
    /// <exception cref="InvalidDataException">An animated part does not picture its animation's selected frame.</exception>
    public ShownBossBody Show(ArtworkFrameSelections selections, Extent16<BackgroundPixels> area)
    {
        var pieces = ImmutableArray.CreateBuilder<ShownBossBodyPiece>();
        foreach (BossBodyPlacement placement in Placements)
        {
            if (placement.Part.Shown(selections) is { } picture)
                pieces.AddRange(ShownBossBody.PiecesOf(picture, placement.Part.Top, placement.Shows, area));
        }
        return new(pieces.ToImmutable(), area);
    }
}
