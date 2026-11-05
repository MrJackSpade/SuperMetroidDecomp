using System.Collections.Immutable;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Enemies.BossBodies;

/// <summary>
/// A boss body layer as an accepted frame latched it (#1466): its rows, in pages of thirty-two, and the parts showing on
/// it, each over cells no other shows. A part that clears its cells shows nothing over them, and a cell no part shows
/// draws nothing (#1513).
/// </summary>
/// <param name="Rows">The layer's rows.</param>
/// <param name="Placements">The parts showing and the cells each shows.</param>
public sealed record BossBodyLayer(int Rows, ImmutableArray<BossBodyPlacement> Placements)
{
    /// <summary>
    /// The layer as the frame shows it: each part's picture for the frame its animation shows in
    /// <paramref name="selections"/>, which hold what the room's animations had selected when the frame was accepted. A
    /// part that clears its cells has no picture there, so the cells it shows draw nothing. Its pages cover
    /// <paramref name="area"/>.
    /// </summary>
    /// <exception cref="InvalidDataException">An animated part does not picture its animation's selected frame.</exception>
    public ShownBossBody Show(ArtworkFrameSelections selections, Extent16<BackgroundPixels> area)
    {
        var parts = ImmutableArray.CreateBuilder<ShownBossBodyPart>(Placements.Length);
        foreach (BossBodyPlacement placement in Placements)
        {
            if (placement.Part.Shown(selections) is { } picture)
                parts.Add(new ShownBossBodyPart(picture, placement.Part.Top, placement.Shows));
        }
        return new(Rows, parts.ToImmutable(), area);
    }
}
