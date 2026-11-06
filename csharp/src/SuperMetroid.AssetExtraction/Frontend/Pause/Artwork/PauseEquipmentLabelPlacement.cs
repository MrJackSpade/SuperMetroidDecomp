using SuperMetroid.Core;
using SuperMetroid.Core.Frontend.Pause;
using SuperMetroid.Core.Frontend.Pause.Artwork;
using SuperMetroid.Core.Spatial;
using static SuperMetroid.AssetExtraction.Frontend.Pause.Artwork.PauseEquipmentLabelArtworkDefinitions;

namespace SuperMetroid.AssetExtraction.Frontend.Pause.Artwork;

/// <summary>
/// Where the cartridge's equipment page holds each label strip and how many cells the strip spans (#1515). Only the import,
/// which pictures the page from these strips, and the checks that compare it with the cartridge read this; the game
/// places each label's installed picture whole.
/// </summary>
internal static class PauseEquipmentLabelPlacement
{
    /// <summary>The cells <paramref name="label"/>'s strip spans: five for an ordinary beam, nine for every other label, stored Hyper included.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The label has no strip.</exception>
    internal static int CellCount(PauseEquipmentLabelId label)
    {
        _ = PageCell(label);
        return PauseEquipmentLabelTiles.IsOrdinaryBeam(label) ? BeamWords : EquipmentWords;
    }

    /// <summary>
    /// The equipment-page cell <paramref name="label"/>'s strip starts at: the beams down column four from row sixteen, the
    /// suits and miscellaneous items down column twenty-one from row nine with two rows between them, the boots down column
    /// twenty-one from row nineteen, and Hyper at the beam place the cartridge's Hyper table gives it.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">The label has no strip.</exception>
    internal static int PageCell(PauseEquipmentLabelId label)
    {
        if (label == PauseEquipmentLabelId.Hyper) label = PauseEquipmentLabelTiles.Label(PauseEquipmentCategory.Beams, HyperBeamItem);
        return Ensure.Listed(pageCells.Value, label);
    }

    /// <summary>Each ordinary label's first page cell, derived once from its category's column and first row.</summary>
    private static readonly Lazy<Dictionary<PauseEquipmentLabelId, int>> pageCells = new(() =>
        PauseEquipmentLabelTiles.Labels().ToDictionary(label => label.Label, label =>
        {
            Position16<TilemapCells> first = label.Category switch
            {
                PauseEquipmentCategory.Beams => new Position16<TilemapCells>(4, 16),
                PauseEquipmentCategory.Suits => new Position16<TilemapCells>(21, 9 + (label.Item >= 2 ? 2 : 0)),
                PauseEquipmentCategory.Boots => new Position16<TilemapCells>(21, 19),
                PauseEquipmentCategory.Reserves => throw new InvalidDataException("Reserve controls have no equipment label strip."),
                _ => throw new InvalidDataException("Unknown equipment label category."),
            };
            return (first.Y + label.Item) * TilemapColumns + first.X;
        }));
}
