using System.Collections.Immutable;
using SuperMetroid.Core.Frontend.Pause.Artwork;

namespace SuperMetroid.Core.Frontend.Pause;

/// <summary>
/// What the equipment page shows (#1467, #1515), derived each frame from what Samus has and the menu's state: each item
/// label in its item's state, whether Hyper replaces the beam labels, the wireframe of the equipped suit and boots, the
/// reserve labels and digits, and the reserve arrow's colouring.
/// </summary>
/// <param name="Labels">Each ordinary item label, in the equipment page's category and item order.</param>
/// <param name="HyperBeam">Whether the beam labels are blanked and Hyper's label shows in their column.</param>
/// <param name="Wireframe">The Samus wireframe the page shows.</param>
/// <param name="Reserve">The reserve labels and digits, or none while Samus has no reserve capacity and the template shows.</param>
/// <param name="Arrow">How the reserve arrow's pictures are coloured.</param>
internal sealed record ShownEquipmentPage(ImmutableArray<ShownEquipmentLabel> Labels, bool HyperBeam,
    PauseWireframeKind Wireframe, ShownReserveDisplay? Reserve, ShownReserveArrow Arrow)
{
    /// <summary>
    /// The page <paramref name="inventory"/> shows: every label in its item's state, the wireframe of the equipped suit and
    /// boots, <paramref name="reserve"/>'s labels and digits and the arrow coloured <paramref name="arrow"/>.
    /// </summary>
    internal static ShownEquipmentPage Of(PauseInventory inventory, ShownReserveDisplay? reserve, ShownReserveArrow arrow)
    {
        ImmutableArray<ShownEquipmentLabel> labels = [.. PauseEquipmentLabels.Labels().Select(key =>
            new ShownEquipmentLabel(key.Label, inventory.StateOf(PauseEquipmentRules.Mask(key.Category, key.Item))))];
        return new(labels, inventory.HyperBeam, PauseEquipmentRules.WireframeKind(inventory.EquippedItems), reserve, arrow);
    }
}
