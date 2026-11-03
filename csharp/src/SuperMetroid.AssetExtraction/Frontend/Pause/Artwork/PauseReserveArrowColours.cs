using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Pause.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Pause.Artwork;

/// <summary>
/// The reserve-tank arrow's two bevel colours (#1467, #1512): solid while the transfer control is selected, pulsing while
/// AUTO is selected. Only the import reads them: it pre-colours the arrow's pictures in the pause sheet in a shade for the
/// solid colours and one for each pulse phase, which the pause screen names.
/// </summary>
public sealed class PauseReserveArrowColours
{
    /// <summary>The background colour group the arrow's bevel colours lie in on the pause screen.</summary>
    private const int BevelGroup = 6;
    /// <summary>The colour of <see cref="BevelGroup"/> the arrow's darker bevel shows.</summary>
    private const int ShadeColour = 6;
    /// <summary>The colour of <see cref="BevelGroup"/> the arrow's lighter bevel shows.</summary>
    private const int LightColour = 11;
    /// <summary>The colours of one colour group.</summary>
    private const int GroupColours = 16;

    /// <summary>Two bevel colors shown when AUTO pulse animation is inactive.</summary>
    private readonly RgbColor solidColor6, solidColor11;
    /// <summary>Only pulse starting colors that differ from the reversed solid bevel colors.</summary>
    private readonly ReserveArrowColorEdits arrowStartEdits;
    /// <summary>Grey midpoint levels from which each mirrored pulse ramp is calculated.</summary>
    private readonly ReserveArrowGreyLevels arrowGreyLevels;
    /// <summary>Independently edited pulse colors that depart from the mirrored ramp.</summary>
    internal readonly Dictionary<ReserveArrowPulseColour, RgbColor> arrowColorEdits = [];

    /// <summary>Derives the pulse's sparse starting-colour and per-phase edits from the selected colours.</summary>
    /// <param name="solidColor6">Static arrow bevel colour for palette slot six outside pulse animation.</param>
    /// <param name="solidColor11">Static arrow bevel colour for palette slot eleven outside pulse animation.</param>
    /// <param name="arrowFrames">The two bevel colours of each of the <see cref="PauseReserveArrowDefinitions.ArrowFrames"/> pulse phases.</param>
    /// <exception cref="ArgumentNullException"><paramref name="arrowFrames"/> is null.</exception>
    internal PauseReserveArrowColours(RgbColor solidColor6, RgbColor solidColor11, ReserveArrowColors[] arrowFrames)
    {
        Ensure.NotNull(arrowFrames);
        this.solidColor6 = solidColor6;
        this.solidColor11 = solidColor11;
        arrowStartEdits = new ReserveArrowColorEdits(arrowFrames[0].Color6 == solidColor11 ? null : arrowFrames[0].Color6,
            arrowFrames[0].Color11 == solidColor6 ? null : arrowFrames[0].Color11);
        var middle = arrowFrames[PauseReserveArrowDefinitions.ArrowFrames / 2 - 1];
        arrowGreyLevels = new ReserveArrowGreyLevels(middle.Color6.Red, middle.Color11.Red);
        for (int frame = 0; frame < arrowFrames.Length; frame++)
        {
            if (arrowFrames[frame].Color6 != CalculateArrowColor(frame, ReserveArrowBevel.Shade))
                arrowColorEdits.Add(new ReserveArrowPulseColour(frame, ReserveArrowBevel.Shade), arrowFrames[frame].Color6);
            if (arrowFrames[frame].Color11 != CalculateArrowColor(frame, ReserveArrowBevel.Light))
                arrowColorEdits.Add(new ReserveArrowPulseColour(frame, ReserveArrowBevel.Light), arrowFrames[frame].Color11);
        }
    }

    /// <summary>The arrow's two bevel colours while solid.</summary>
    internal ReserveArrowColors Solid => new(solidColor6, solidColor11);

    /// <summary>The arrow's two bevel colours at pulse phase <paramref name="frame"/>.</summary>
    /// <param name="frame">Pulse phase, masked to its low five bits; normally the accepted eight-bit frame counter.</param>
    internal ReserveArrowColors Pulse(int frame) =>
        new(ArrowColor(frame, ReserveArrowBevel.Shade), ArrowColor(frame, ReserveArrowBevel.Light));

    /// <summary>
    /// The shades the import pre-colours the arrow's pictures in (#1512): the solid colours, and each pulse phase's, over
    /// the arrow's two bevel inks.
    /// </summary>
    internal IEnumerable<SheetShade> Shades()
    {
        yield return Shade(PauseReserveArrowShades.Solid, Solid);
        for (int frame = 0; frame < PauseReserveArrowDefinitions.ArrowFrames; frame++)
            yield return Shade(PauseReserveArrowShades.Pulse(frame), Pulse(frame));
    }

    /// <summary>The shade that shows the arrow's bevels in <paramref name="colours"/>.</summary>
    private static SheetShade Shade(PictureShade shade, ReserveArrowColors colours) =>
        new(shade, [BevelGroup * GroupColours + ShadeColour, BevelGroup * GroupColours + LightColour], [colours.Color6, colours.Color11]);

    /// <summary>
    /// The reserve arrow's two 16-phase RGB ramps mirror around the repeated midpoint.
    /// Starts share the reversed solid-arrow colors; grey midpoint channels share one level.
    /// Repeated normalized fade subtraction explains the below-integer blue crossings at5/10.
    /// The penultimate bright-red/dark-blue channels hold their preceding shade. Endpoint
    /// colors/levels and this exact two-channel hold are authored bevel-color/pulse content:
    /// different choices paint a different pulse. Held magnitudes derive from the prior shade;
    /// independently supplied differences stay sparse.
    /// </summary>
    private RgbColor CalculateArrowColor(int frame, ReserveArrowBevel bevel)
    {
        int last = PauseReserveArrowDefinitions.ArrowFrames - 1;
        int phase = Math.Min(frame, last - frame);
        int steps = last / 2;
        bool second = bevel == ReserveArrowBevel.Light;
        RgbColor start = second ? arrowStartEdits.Color11 ?? solidColor6 : arrowStartEdits.Color6 ?? solidColor11;
        int grey = second ? arrowGreyLevels.Color11 : arrowGreyLevels.Color6;
        if (phase == steps) return new(grey, grey, grey);
        ColorChannel held = second ? ColorChannel.Blue : ColorChannel.Red;
        return start.Map((channel, channelStart) =>
        {
            int channelPhase = phase == steps - 1 && channel == held ? phase - 1 : phase;
            float remaining = 1;
            float step = (float)(1d / steps);
            for (int tick = 0; tick < channelPhase; tick++)
                remaining = (float)((double)remaining - step);
            float amount = (float)(1d - remaining);
            // Explicit binary32 roundings separate multiplication and addition: an FMA
            // cannot remove the intermediate rounding or alter the integral crossings.
            float delta = (float)((double)(grey - channelStart) * amount);
            return (int)(float)(channelStart + (double)delta);
        });
    }

    /// <summary>Uses the supplied independent bevel shade when present; otherwise mirrors the calculated 32-phase ramp.</summary>
    private RgbColor ArrowColor(int frame, ReserveArrowBevel bevel)
    {
        frame &= PauseReserveArrowDefinitions.ArrowFrames - 1;
        return arrowColorEdits.TryGetValue(new ReserveArrowPulseColour(frame, bevel), out RgbColor supplied)
            ? supplied : CalculateArrowColor(frame, bevel);
    }
}
