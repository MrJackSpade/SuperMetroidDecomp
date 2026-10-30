using System.Collections.Immutable;
using SuperMetroid.Core.Frontend.Pause.Artwork;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Frontend.Pause;

/// <summary>
/// What the equipment page shows (#1467), as the cartridge's writes to its tilemap last left it: each item label's state,
/// whether Hyper replaces the beam labels, the Plasma overrun's tail, the wireframe, the reserve labels and digits, and
/// the reserve arrow's colouring. The page is immutable; each write makes a new one and the menu captures the one it
/// uploads.
/// </summary>
/// <param name="Labels">Each ordinary item label, in the equipment page's category and item order.</param>
/// <param name="HyperBeam">Whether the beam labels are blanked and Hyper's label shows in their column.</param>
/// <param name="PlasmaTail">
/// The state the Plasma label's nine-cell overrun last wrote Varia's first four cells after it in, or none while no
/// overrun has been written since the page was built.
/// </param>
/// <param name="Wireframe">The Samus wireframe the page shows.</param>
/// <param name="Reserve">The reserve labels and digits, or none while Samus has no reserve capacity and the template shows.</param>
/// <param name="Arrow">How the reserve arrow's tiles are coloured.</param>
internal sealed record ShownEquipmentPage(ImmutableArray<ShownEquipmentLabel> Labels, bool HyperBeam, PauseEquipmentLabelState? PlasmaTail,
    PauseWireframeKind Wireframe, ShownReserveDisplay? Reserve, ShownReserveArrow Arrow)
{
    /// <summary>
    /// The page <c>LoadEquipmentScreen</c> builds from the template: every label blank, enabled or disabled by
    /// <paramref name="inventory"/>, no overrun tail, the wireframe of the equipped suit and boots, the reserve display
    /// when there is one, and the arrow as the template colours it.
    /// </summary>
    internal static ShownEquipmentPage Built(PauseInventory inventory, ShownReserveDisplay? reserve)
    {
        ImmutableArray<ShownEquipmentLabel> labels = [.. PauseEquipmentLabelTiles.Labels().Select(key =>
            ShownEquipmentLabel.Whole(key.Label, inventory.StateOf(PauseEquipmentRules.Mask(key.Category, key.Item))))];
        return new(labels, inventory.HyperBeam, null, PauseEquipmentRules.WireframeKind(inventory.EquippedItems), reserve, ShownReserveArrow.Unpainted);
    }

    /// <summary>How <paramref name="label"/> shows.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The label is not an ordinary item label.</exception>
    internal ShownEquipmentLabel Label(PauseEquipmentLabelId label)
    {
        foreach (ShownEquipmentLabel shown in Labels)
        {
            if (shown.Label == label)
                return shown;
        }
        throw new ArgumentOutOfRangeException(nameof(label), label, "The equipment page shows no such ordinary label.");
    }

    /// <summary>
    /// The page after one equipment button's label write: <paramref name="cells"/> cells of <paramref name="label"/> in
    /// <paramref name="state"/>. A beam column's five-cell write over a nine-cell label rewrites only its head; Plasma's
    /// nine-cell write rewrites the whole label and continues into Varia's first four cells after it.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">The write is neither a whole label, a head nor Plasma's overrun.</exception>
    /// <exception cref="InvalidDataException">A label other than Plasma would be overrun.</exception>
    internal ShownEquipmentPage WithLabelWritten(PauseEquipmentLabelId label, int cells, PauseEquipmentLabelState state)
    {
        ShownEquipmentLabel shown = Label(label);
        int labelCells = PauseEquipmentLabelTiles.StockWordCount(label);
        ShownEquipmentLabel written = cells == labelCells ? ShownEquipmentLabel.Whole(label, state)
            : cells == PauseEquipmentLabelDefinitions.BeamWords && cells < labelCells ? shown with { Head = state }
            : cells > labelCells && cells <= PauseEquipmentLabelDefinitions.EquipmentWords ? label == PauseEquipmentLabelId.Plasma && cells == PauseEquipmentLabelDefinitions.EquipmentWords
                ? ShownEquipmentLabel.Whole(label, state)
                : throw new InvalidDataException($"Pause label {label} cannot supply a {cells}-cell native overrun.")
            : throw new ArgumentOutOfRangeException(nameof(cells), cells, $"No equipment button writes {cells} cells of {label}.");
        return this with
        {
            Labels = Labels.Replace(shown, written),
            PlasmaTail = cells > labelCells ? state : PlasmaTail,
        };
    }

    /// <summary>The page with the wireframe <paramref name="wireframe"/> written over it.</summary>
    internal ShownEquipmentPage WithWireframe(PauseWireframeKind wireframe) => this with { Wireframe = wireframe };

    /// <summary>The page with its reserve labels rewritten for <paramref name="mode"/>.</summary>
    /// <exception cref="InvalidOperationException">The page shows no reserve display.</exception>
    internal ShownEquipmentPage WithReserveMode(ReserveTankMode mode) => this with { Reserve = ShownReserve with { Mode = mode } };

    /// <summary>The page with its supply digits rewritten for <paramref name="supply"/>.</summary>
    /// <exception cref="InvalidOperationException">The page shows no reserve display.</exception>
    internal ShownEquipmentPage WithReserveSupply(int supply) => this with { Reserve = ShownReserve with { Supply = supply } };

    /// <summary>The page with its reserve arrow's tiles recoloured to <paramref name="arrow"/>.</summary>
    internal ShownEquipmentPage WithArrow(ShownReserveArrow arrow) => this with { Arrow = arrow };

    /// <summary>The reserve display the page shows.</summary>
    private ShownReserveDisplay ShownReserve => Reserve ?? throw new InvalidOperationException("The equipment page shows no reserve labels or digits.");
}
