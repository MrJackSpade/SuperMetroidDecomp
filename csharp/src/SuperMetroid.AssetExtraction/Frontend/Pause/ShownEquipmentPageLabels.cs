using SuperMetroid.Core.Frontend.Pause;
using SuperMetroid.Core.Frontend.Pause.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Pause;

/// <summary>Finding how one label shows on an equipment page the import lays out (#1467, #1515).</summary>
internal static class ShownEquipmentPageLabels
{
    /// <summary>Reading one ordinary item label.</summary>
    extension(ShownEquipmentPage page)
    {
        /// <summary>How <paramref name="label"/> shows.</summary>
        /// <exception cref="ArgumentOutOfRangeException">The label is not an ordinary item label.</exception>
        internal ShownEquipmentLabel Label(PauseEquipmentLabelId label)
        {
            foreach (ShownEquipmentLabel shown in page.Labels)
            {
                if (shown.Label == label)
                    return shown;
            }
            throw new ArgumentOutOfRangeException(nameof(label), label, "The equipment page shows no such ordinary label.");
        }
    }
}
