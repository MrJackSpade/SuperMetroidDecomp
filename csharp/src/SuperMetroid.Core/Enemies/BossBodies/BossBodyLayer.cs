using System.Collections.Immutable;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Enemies.BossBodies;

/// <summary>
/// A boss body layer as an accepted frame latched it (#1466): its rows, in pages of thirty-two, and the parts showing on
/// it, each over cells no other shows. Every cell shows exactly one part.
/// </summary>
/// <param name="Rows">The layer's rows.</param>
/// <param name="Placements">The parts showing and the cells each shows.</param>
public sealed record BossBodyLayer(int Rows, ImmutableArray<BossBodyPlacement> Placements)
{
    /// <summary>
    /// The layer as the frame shows it: each part's picture for the frame its animation shows in
    /// <paramref name="selections"/>, which hold what the room's animations had selected when the frame was accepted.
    /// </summary>
    /// <exception cref="InvalidDataException">An animated part does not picture its animation's selected frame.</exception>
    public ShownBossBody Show(ArtworkFrameSelections selections) =>
        new(Rows, [.. Placements.Select(placement => new ShownBossBodyPart(placement.Part.Shown(selections), placement.Part.Top, placement.Shows))]);
}
