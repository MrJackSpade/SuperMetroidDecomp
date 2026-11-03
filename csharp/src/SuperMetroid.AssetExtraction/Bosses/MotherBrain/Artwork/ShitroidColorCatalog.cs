using SuperMetroid.Core.Assets;

namespace SuperMetroid.AssetExtraction.Bosses.MotherBrain.Artwork;

/// <summary>Editable live-Shitroid RGB5 images; timer and fade destinations stay compiled.</summary>
public sealed class ShitroidColorCatalog
{
    /// <summary>The selected normal pulse and initialization targets.</summary>
    internal readonly ShitroidColorSelection selection;

    /// <summary>Keeps the pulse and initialization targets the import recognized.</summary>
    /// <param name="selection">The normal pulse and the targets.</param>
    internal ShitroidColorCatalog(ShitroidColorSelection selection) => this.selection = selection;

    /// <summary>Gets a packed RGB5 ink from normal pulse phase 0–7 and ink 0–3, written to colours 165–168; phase cadence and cries remain actor behavior.</summary>
    public RgbColor NormalColor(int frame, int color) => ShitroidColors.NormalColor(selection.Normal, frame, color);

    /// <summary>Gets packed RGB5 color 0–15 from a selected initialization target, including its retained transparent-slot value; the actor copies targets to its palette image and current colours during setup.</summary>
    public RgbColor TargetColor(ShitroidColorTarget target, int color) => ShitroidColors.TargetColor(this, target, color);
}
