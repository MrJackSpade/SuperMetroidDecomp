using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Bosses.Phantoon.Artwork;

/// <summary>
/// Editable RGB5 color targets for Phantoon's health, materialization and ship-power
/// transitions. Boss phase selection and interpolation arithmetic remain compiled.
/// </summary>
public sealed class PhantoonColorCatalog
{
    /// <summary>Canonical selected RGB5 colors and ordered rows, independent of JSON encoding.</summary>
    public string ContentIdentity => PhantoonColors.ContentIdentity(this);

    /// <summary>The selected health, fade-out and ship-power deviations.</summary>
    internal readonly PhantoonColorSelection selection;

    /// <summary>Retains only authored deviations from the calculated health, fade, and ship-power colors.</summary>
    internal PhantoonColorCatalog(RgbColor[][] healthBands, RgbColor[] fadeOut, RgbColor[] powerOn) =>
        selection = new(PhantoonColors.HealthEdits(healthBands), PhantoonColors.FadeOutEdits(fadeOut), PhantoonColors.PowerEdits(powerOn));

    /// <summary>
    /// $A7:CB41..CC40 health targets, selected by $A7:DC0F. All healthy material
    /// shades and red-tinted bands calculate; supplied edits remain independent
    /// across every ink and band, including the healthy endpoints.
    /// </summary>
    public RgbColor ResolveHealth(int band, int color) => PhantoonColors.Health(selection.HealthEdits, band, color);
    /// <summary>
    /// $A7:CA41..CA60, Palette_Phantoon_FadeOutTarget: $A7:DBB1 fades every
    /// body palette component to black. Independent supplied target edits override it.
    /// </summary>
    public RgbColor ResolveFadeOut(int color) => PhantoonColors.FadeOut(selection.FadeOutEdits, color);
    /// <summary>$A7:CA61-CB40: all seven powered ship material palettes calculate; supplied words remain independently editable.</summary>
    public RgbColor ResolvePowerOn(int color) => PhantoonColors.PowerOn(selection.PowerEdits, color);
}
