using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Bosses.MotherBrain.Artwork;

/// <summary>Editable live-Shitroid RGB5 images; native timer and fade destinations stay compiled.</summary>
public sealed class ShitroidColorCatalog
{
    /// <summary>Canonical selected RGB5 colors and ordered rows, independent of JSON encoding.</summary>
    public string ContentIdentity => ShitroidColors.ContentIdentity(this);

    /// <summary>Uses the giant Baby Metroid's selected target inks when they reproduce every pulse frame; otherwise retains the supplied frames.</summary>
    internal readonly ShitroidColorPulse normal;
    /// <summary>Live victim colors copied into the Sidehopper palette during encounter setup.</summary>
    internal readonly SidehopperInitialPalette sidehopper;
    /// <summary>Visible giant Baby Metroid inks, excluding its retained transparent slot.</summary>
    internal readonly BabyMetroidInitialPalette shitroid;
    /// <summary>$A9:F8E6 is copied by EF9F-EFA8 to target slot A0. Stock3800 is an exact
    /// transparent-slot compatibility payload: OBJ ink0 is skipped before color lookup.
    /// It has no visible hue to derive; independently supplied replacements remain exact.</summary>
    internal readonly RgbColor shitroidTransparentSlot;
    /// <summary>Victim colors selected after the giant Baby Metroid drains the Sidehopper.</summary>
    internal readonly SidehopperCorpsePalette deadSidehopper;

    /// <summary>Separates the transparent Shitroid palette word before constructing the visible-ink resolver.</summary>
    internal ShitroidColorCatalog(RgbColor[][] normal, RgbColor[] sidehopper,
        RgbColor[] shitroid, RgbColor[] deadSidehopper)
        : this(new BabyMetroidInitialPalette(shitroid.AsSpan(1).ToArray()), normal, sidehopper, shitroid[0], deadSidehopper)
    {
    }

    /// <summary>Retains the visible Shitroid inks and builds the pulse that may share them.</summary>
    private ShitroidColorCatalog(BabyMetroidInitialPalette shitroid, RgbColor[][] normal, RgbColor[] sidehopper,
        RgbColor shitroidTransparentSlot, RgbColor[] deadSidehopper) =>
        (this.shitroid, this.normal, this.sidehopper, this.shitroidTransparentSlot, this.deadSidehopper) =
            (shitroid, new ShitroidColorPulse(normal, shitroid), new SidehopperInitialPalette(sidehopper),
                shitroidTransparentSlot, new SidehopperCorpsePalette(deadSidehopper));

    /// <summary>Gets a packed RGB5 ink from normal pulse phase 0–7 and ink 0–3, corresponding to $A9:F6D1 and written to CGRAM colors 165–168; phase cadence and cries remain actor behavior.</summary>
    public RgbColor NormalColor(int frame, int color) => ShitroidColors.NormalColor(normal, frame, color);

    /// <summary>Gets packed RGB5 color 0–15 from a selected initialization target, including its retained transparent-slot word; the actor copies targets to its palette image and current CGRAM during setup.</summary>
    public RgbColor TargetColor(ShitroidColorTarget target, int color) => ShitroidColors.TargetColor(this, target, color);
}
