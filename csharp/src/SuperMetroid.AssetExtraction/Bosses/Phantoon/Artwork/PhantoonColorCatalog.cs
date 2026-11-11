using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Enemies.Artwork;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Bosses.Phantoon;

namespace SuperMetroid.AssetExtraction.Bosses.Phantoon.Artwork;

/// <summary>
/// Phantoon's installed RGB5 color targets for its health, materialization and ship-power transitions, each kept as
/// installed. Boss phase selection and interpolation arithmetic remain compiled.
/// </summary>
public sealed class PhantoonColorCatalog
{
    /// <summary>The installed health palettes.</summary>
    private readonly InstalledColourRows health;
    /// <summary>The installed materialization fade-out target.</summary>
    private readonly InstalledColourRows fadeOut;
    /// <summary>Each powered part's installed target colours.</summary>
    private readonly Dictionary<WreckedShipPoweredPart, InstalledColourRows> powerOn;

    /// <summary>Keeps every complete installed colour set.</summary>
    /// <param name="health">Each health palette's <see cref="PhantoonColorFormat.HealthBandColorCount"/> colours.</param>
    /// <param name="fadeOut">The fade-out target's <see cref="PhantoonColorFormat.FadeOutCount"/> colours.</param>
    /// <param name="powerOn">Each part the power-on fades, with its <see cref="PhantoonColorFormat.PoweredPartColourCount"/> target colours.</param>
    /// <exception cref="InvalidDataException">A set, a powered part or a colour is missing.</exception>
    internal PhantoonColorCatalog(RgbColor[][] health, RgbColor[] fadeOut, IReadOnlyDictionary<WreckedShipPoweredPart, RgbColor[]> powerOn)
    {
        this.health = new InstalledColourRows(health, PhantoonColorDefinitions.HealthBandCount,
            PhantoonColorFormat.HealthBandColorCount, "Phantoon health palettes");
        this.fadeOut = new InstalledColourRows(fadeOut, PhantoonColorFormat.FadeOutCount, "Phantoon fade-out");
        if (powerOn.Count != PhantoonColorFormat.PoweredParts.Length)
            throw new InvalidDataException("The power-on targets must name exactly the parts the power-on fades.");
        this.powerOn = [];
        foreach (WreckedShipPoweredPart part in PhantoonColorFormat.PoweredParts)
            this.powerOn.Add(part, new InstalledColourRows(powerOn.TryGetValue(part, out RgbColor[]? colours)
                    ? colours
                    : throw new InvalidDataException($"The power-on targets have no colours for {part}."),
                PhantoonColorFormat.PoweredPartColourCount, $"Phantoon power-on {part}"));
    }

    /// <summary>Phantoon's installed health target <paramref name="color"/> of <paramref name="band"/>, selected by the health-palette update.</summary>
    public RgbColor ResolveHealth(int band, int color) => health.Colour(band, color);

    /// <summary>The installed fade-out target <paramref name="color"/>.</summary>
    public RgbColor ResolveFadeOut(int color) => fadeOut.Colour(color);

    /// <summary>
    /// The powered ship's installed target colour <paramref name="color"/> of <paramref name="part"/>, one of the parts the
    /// power-on fades.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">The power-on does not fade <paramref name="part"/>, or the colour is outside its group.</exception>
    public RgbColor ResolvePowerOn(WreckedShipPoweredPart part, int color) => powerOn.TryGetValue(part, out InstalledColourRows? colours)
        ? colours.Colour(color)
        : throw new ArgumentOutOfRangeException(nameof(part), part, "The Wrecked Ship's power-on does not fade this part.");
}
