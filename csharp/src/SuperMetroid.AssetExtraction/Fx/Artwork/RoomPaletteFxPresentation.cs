using SuperMetroid.Core.Fx.PaletteFx;

using SuperMetroid.Core.Assets;
using SuperMetroid.AssetExtraction.Fx.PaletteFx;

namespace SuperMetroid.AssetExtraction.Fx.Artwork;

/// <summary>
/// Editable colors for room and cinematic palette animations, which only the import reads (#1583): it pre-colours the
/// sheets each record shows from them. Timing, destinations, instructions, and side effects remain compiled engine mechanics.
/// </summary>
public sealed class RoomPaletteFxPresentation : IPaletteFxColorSource
{
    /// <summary>Explicit colour samples by the record color each one is.</summary>
    internal readonly Dictionary<PaletteFxColorKey, RgbColor> colors;

    /// <summary>Canonical and derived heat-palette samples shared across Samus suits.</summary>
    internal readonly HeatPaletteInputView heatInputs;
    /// <summary>Editable and derived Samus loading-palette samples.</summary>
    internal readonly LoadingPaletteInputView loadingInputs;
    /// <summary>Endpoint samples used to reconstruct the ending gunship fade.</summary>
    private readonly EndingGunshipPaletteInputView gunshipInputs;

    /// <summary>Builds calculated-color views over validated, explicitly authored samples.</summary>
    /// <param name="colors">The selected colours the import kept, by the record colour each one is.</param>
    internal RoomPaletteFxPresentation(Dictionary<PaletteFxColorKey, RgbColor> colors)
    {
        this.colors = colors;
        heatInputs = new HeatPaletteInputView(colors);
        loadingInputs = new LoadingPaletteInputView(colors);
        gunshipInputs = new EndingGunshipPaletteInputView(colors);
    }

    /// <inheritdoc />
    public bool TryReadColor(PaletteFxColorKey key, out RgbColor color) =>
        gunshipInputs.TryGetValue(key, out color) ||
        PlanetZebesTextColors.TryCalculate(key, colors, out color) ||
        MaridiaEnvironmentalColors.TryReadColor(key, colors, out color) ||
        CrateriaLightningColors.TryCalculatedColor(key, out color) ||
        CrateriaLightningColors.TryCalculatedDarkColor(key, colors, out color) ||
        TourianStatueGreyPaint.TryCalculatedColor(key, colors, out color) ||
        LoadingPaletteColors.TryReadColor(key, loadingInputs, out color) ||
        LogoGlarePaletteColors.TryCalculatedColor(key, colors, out color) ||
        EndingGunshipPaletteColors.TryCalculatedColor(key, gunshipInputs, out color) ||
        (HeatPaletteColors.TryCanonical(key, out PaletteFxColorKey canonical) &&
         (heatInputs.TryGetValue(canonical, out color) ||
          HeatPaletteColors.TryCalculatedColor(canonical, heatInputs, out color)));
}
