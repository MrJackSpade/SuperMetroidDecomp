using SuperMetroid.Core.Frontend.Pause;
using SuperMetroid.Core.Frontend.Pause.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Pause.Artwork;

/// <summary>
/// Keys the editable equipment label document's labels (#1502): each ordinary label by its selector control's anchor
/// name, the Hyper label by <see cref="PauseEquipmentLabelArtworkDefinitions.HyperKey"/>, and each category's strip
/// width. Only the import reads and writes the document.
/// </summary>
public static class PauseEquipmentLabelKeys
{
    /// <summary>The JSON key of a label: its selector control's anchor name, or <see cref="PauseEquipmentLabelArtworkDefinitions.HyperKey"/>.</summary>
    public static string Name(PauseEquipmentLabelId label)
    {
        if (label == PauseEquipmentLabelId.Hyper) return PauseEquipmentLabelArtworkDefinitions.HyperKey;
        return PauseEquipmentControls.Upgrades.Single(control => control.Label == label).Anchor;
    }

    /// <summary>Returns the ordinary category strip width in sixteen-bit tile words, not bytes; Hyper's stored width is a separate contract.</summary>
    /// <param name="category">Equipment category: one denotes beams; every other integer returns the equipment width without category validation.</param>
    /// <returns>Five for the beam category, otherwise nine.</returns>
    public static int WordCount(PauseEquipmentCategory category) => category == PauseEquipmentCategory.Beams
        ? PauseEquipmentLabelArtworkDefinitions.BeamWords : PauseEquipmentLabelArtworkDefinitions.EquipmentWords;
}
