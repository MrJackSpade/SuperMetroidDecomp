using SuperMetroid.Core;

namespace SuperMetroid.AssetExtraction.Frontend.Pause.Artwork;

/// <summary>
/// The pause screen's editable backdrops, wireframes, equipment-page artwork and reserve display (#1467), which the import
/// pictures into the pause sheet; the game draws that sheet's pictures and reads none of these but the reserve arrow's
/// colours, which it binds through <see cref="PauseReserveUiPresentation.ArrowColours"/>.
/// </summary>
public sealed class PauseScreenArtwork
{
    /// <summary>Keeps the pause screen's artwork.</summary>
    /// <param name="backdrops">The map and equipment backdrops and the area-name buttons.</param>
    /// <param name="wireframes">Samus's suit wireframes on the equipment page.</param>
    /// <param name="equipmentBase">The equipment page's base tilemap, before live inventory and reserve patches.</param>
    /// <param name="equipmentLabels">The equipment labels' artwork and placement.</param>
    /// <param name="reserveUi">The reserve labels, digits and arrow cells and colours.</param>
    /// <exception cref="ArgumentNullException">A part is null.</exception>
    public PauseScreenArtwork(PauseBackdropPresentation backdrops, PauseWireframePresentation wireframes,
        PauseEquipmentBasePresentation equipmentBase, PauseEquipmentLabelPresentation equipmentLabels, PauseReserveUiPresentation reserveUi)
    {
        Backdrops = Ensure.NotNull(backdrops);
        Wireframes = Ensure.NotNull(wireframes);
        EquipmentBase = Ensure.NotNull(equipmentBase);
        EquipmentLabels = Ensure.NotNull(equipmentLabels);
        ReserveUi = Ensure.NotNull(reserveUi);
    }

    /// <summary>Pause-screen backdrop tilemap presentation, independent of equipment and map state.</summary>
    public PauseBackdropPresentation Backdrops { get; }
    /// <summary>Samus suit wireframe artwork for the pause equipment screen.</summary>
    public PauseWireframePresentation Wireframes { get; }
    /// <summary>Equipment-page base tilemap artwork, before live inventory and reserve patches are applied.</summary>
    public PauseEquipmentBasePresentation EquipmentBase { get; }
    /// <summary>Equipment-page label artwork and placement, separate from the base tilemap and live inventory decisions.</summary>
    public PauseEquipmentLabelPresentation EquipmentLabels { get; }
    /// <summary>Reserve labels, digits and arrow cells, which the equipment page's reserve pictures are laid out from.</summary>
    public PauseReserveUiPresentation ReserveUi { get; }
}
