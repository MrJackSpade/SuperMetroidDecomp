using SuperMetroid.Core.Areas.Ceres;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Bosses.Ridley;
using SuperMetroid.AssetExtraction.Bosses.MotherBrain.Artwork;
using static SuperMetroid.AssetExtraction.Bosses.Ridley.Artwork.CeresBabyPaintDefinitions;

namespace SuperMetroid.AssetExtraction.Bosses.Ridley.Artwork;

/// <summary>
/// Ceres Baby/container palettes. The Baby's draw selects the
/// horizontal/round/vertical brightness with the matching sprite shape; its palette update
/// copies fifteen colors uniformly. Shared organ and fang source paints calculate
/// from existing owners. Eight selected glass/dome/outline inputs and the exact
/// clipped three-unit pose brightening specify this reviewed material composition,
/// not pixels or frame timing. Changing these choices invents different paint.
/// Supplied source and output edits remain independently observable.
/// </summary>
internal sealed class CeresBabyPaint
{
    /// <summary>Shared Ceres Ridley body paint used for Baby source colors.</summary>
    private readonly CeresRidleyFade? body;
    /// <summary>Standalone Norfair body paint when no Ceres fade owner is present.</summary>
    private readonly CeresRidleyBodyPaint? standaloneBody;
    /// <summary>Which shades this paint holds: every shade for Ceres, only the resting colours for Norfair.</summary>
    private readonly CeresBabyPaintShades shades;
    /// <summary>Independent glass-channel inputs retained from the supplied resting colours.</summary>
    private readonly int glassRed, glassBlue, glassMiddleRed, glassGreenStep;
    /// <summary>Independent dome and outline inputs retained from the supplied resting colours.</summary>
    private readonly int domeTint, domeGreen, domeShadeStep, outlineRed;
    /// <summary>Supplied colors that depart from the calculated material composition.</summary>
    internal readonly Dictionary<CeresBabyColourCell, RgbColor> edits;

    /// <summary>Keeps the material inputs, the body paint they share and the independent color edits.</summary>
    /// <param name="shades">Which shades the paint holds.</param>
    /// <param name="material">The glass, dome and outline inputs.</param>
    /// <param name="body">The Ceres body fade supplying shared body paint, or null for Norfair's standalone material.</param>
    /// <param name="standaloneBody">Norfair's standalone body paint, or null when the Ceres fade supplies it.</param>
    /// <param name="edits">The supplied colors the import found differing from the material composition.</param>
    internal CeresBabyPaint(CeresBabyPaintShades shades, CeresBabyMaterial material, CeresRidleyFade? body,
        CeresRidleyBodyPaint? standaloneBody, Dictionary<CeresBabyColourCell, RgbColor> edits)
    {
        this.shades = shades;
        glassRed = material.GlassRed; glassBlue = material.GlassBlue;
        glassMiddleRed = material.GlassMiddleRed; glassGreenStep = material.GlassGreenStep;
        domeTint = material.DomeTint; domeGreen = material.DomeGreen;
        domeShadeStep = material.DomeShadeStep; outlineRed = material.OutlineRed;
        this.body = body;
        this.standaloneBody = standaloneBody;
        this.edits = edits;
    }

    /// <summary>Returns an edited color when supplied, otherwise the calculated shade and material color.</summary>
    internal RgbColor Resolve(CeresBabyShade shade, int color)
    {
        _ = BrighteningSteps(shade);
        if (shades == CeresBabyPaintShades.RestingOnly && shade != CeresBabyShade.Resting)
            throw new ArgumentOutOfRangeException(nameof(shade), shade, "This Baby paint holds only the resting colours.");
        if ((uint)color >= CeresRidleyPaletteDefinitions.BabyColorCount) throw new ArgumentOutOfRangeException(nameof(color));
        return edits.TryGetValue(new CeresBabyColourCell(shade, color), out RgbColor edited) ? edited : Calculate(shade, color);
    }

    /// <summary>
    /// How many brightness steps <paramref name="shade"/> adds to the resting colours: the horizontal squish keeps them,
    /// and the round pose and the vertical squish each brighten once more than the shade before.
    /// </summary>
    private static int BrighteningSteps(CeresBabyShade shade) => shade switch
    {
        CeresBabyShade.Resting or CeresBabyShade.HorizontalSquish => 0,
        CeresBabyShade.Round => RoundBrighteningSteps,
        CeresBabyShade.VerticalSquish => VerticalSquishBrighteningSteps,
        _ => throw new ArgumentOutOfRangeException(nameof(shade), shade, "Undefined Ceres Baby shade."),
    };

    /// <summary>Applies clipped pose brightening with delayed red for the tooth pulse.</summary>
    private RgbColor Calculate(CeresBabyShade shade, int color)
    {
        RgbColor basis = Initial(color);
        int phase = BrighteningSteps(shade);
        int addition = BrightnessStep * phase;
        // The selected tooth pulse delays red by one pose step while G/B brighten.
        int redAddition = color is >= 9 and <= 11 ? BrightnessStep * Math.Max(0, phase - 1) : addition;
        int green = basis.Green + addition, blue = basis.Blue + addition;
        // Cyan glass stays one channel unit below the white glints at its peak.
        if (color == 12)
        {
            green = Math.Min(Maximum - 1, green);
            blue = Math.Min(Maximum - 1, blue);
        }
        return RgbColor.Saturating(basis.Red + redAddition, green, blue);
    }

    /// <summary>Constructs a base Baby/container color from glass, dome, body, or fang inputs.</summary>
    private RgbColor Initial(int color) => color switch
    {
        0 => Pack(glassRed, Maximum, glassBlue),
        >= 1 and <= 3 => Pack(domeTint, domeGreen - domeShadeStep * (color - 1), domeTint),
        >= 4 and <= 6 => BodyColor(color + 4),
        7 => Pack(BodyColor(10).Red / 2, 0, BodyColor(10).Blue / 2),
        8 => Pack(outlineRed, 0, BodyColor(10).Blue / 4),
        9 => BabyMetroidInitialPaintDefinitions.FangLight,
        10 => BabyMetroidInitialPaintDefinitions.FangLight.Zip(BabyMetroidInitialPaintDefinitions.FangDark,
            (_, light, dark) => (light + dark) / 2),
        11 => BabyMetroidInitialPaintDefinitions.FangDark,
        12 => Pack(glassMiddleRed, Maximum - glassGreenStep, Maximum - glassGreenStep),
        13 => Pack(Maximum, Maximum, Maximum),
        _ => Pack(0, Maximum - 2 * glassGreenStep, Maximum - 2 * glassGreenStep),
    };

    /// <summary>Resolves shared body paint from the Ceres fade or standalone Norfair composition.</summary>
    private RgbColor BodyColor(int color) =>
        body is not null ? body.Resolve(15, color)
        : standaloneBody is not null ? standaloneBody.Color(color)
        : throw new InvalidOperationException("Ceres Baby body paint has neither a fade nor a standalone composition.");

    /// <summary>Clips calculated material channel values into RGB5 color space.</summary>
    private static RgbColor Pack(int red, int green, int blue) => RgbColor.Saturating(red, green, blue);
}
