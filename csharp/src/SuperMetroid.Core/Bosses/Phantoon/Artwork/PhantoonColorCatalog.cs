using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Bosses.Phantoon.Artwork;

/// <summary>
/// Editable RGB5 color targets for Phantoon's health, materialization and ship-power
/// transitions. Boss phase selection and interpolation arithmetic remain compiled.
/// </summary>
public sealed class PhantoonColorCatalog
{
    /// <summary>The selected health, fade-out and ship-power deviations.</summary>
    internal readonly PhantoonColorSelection selection;

    /// <summary>Retains the authored deviations from the calculated health, fade, and ship-power colors.</summary>
    /// <param name="selection">The deviations the import selected.</param>
    internal PhantoonColorCatalog(PhantoonColorSelection selection) => this.selection = selection;

    /// <summary>
    /// Phantoon's health targets, selected by the health-palette update. All healthy material
    /// shades and red-tinted bands calculate; supplied edits remain independent
    /// across every ink and band, including the healthy endpoints.
    /// </summary>
    public RgbColor ResolveHealth(int band, int color) => PhantoonColors.Health(selection.HealthEdits, band, color);
    /// <summary>
    /// The fade-out step fades every
    /// body palette component to black. Independent supplied target edits override it.
    /// </summary>
    public RgbColor ResolveFadeOut(int color) => PhantoonColors.FadeOut(selection.FadeOutEdits, color);
    /// <summary>
    /// The powered ship's target colour <paramref name="color"/> of background set <paramref name="set"/>,
    /// one of the sets the power-on fades. Every set's colours calculate; supplied colours remain independently editable.
    /// </summary>
    public RgbColor ResolvePowerOn(BackgroundColourSet set, int color) => PhantoonColors.PowerOn(selection.PowerEdits, set, color);
}
