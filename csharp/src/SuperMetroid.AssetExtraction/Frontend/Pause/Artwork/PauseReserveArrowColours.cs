using SuperMetroid.Core;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Pause.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Pause.Artwork;

/// <summary>
/// The reserve-tank arrow's two bevel colours (#1467, #1512): solid while the transfer control is selected, pulsing while
/// AUTO is selected, kept as installed (#1615): the solid pair and every pulse phase's pair. Only the import reads them:
/// it pre-colours the arrow's pictures in the pause sheet in a shade for the solid colours and one for each pulse phase,
/// which the pause screen names.
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

    /// <summary>The installed two bevel colours of each pulse phase, in phase order.</summary>
    private readonly ReserveArrowColors[] pulse;

    /// <summary>Keeps the installed solid colours and every pulse phase's colours.</summary>
    /// <param name="solidColor6">Static arrow bevel colour for palette slot six outside pulse animation.</param>
    /// <param name="solidColor11">Static arrow bevel colour for palette slot eleven outside pulse animation.</param>
    /// <param name="arrowFrames">The two bevel colours of each of the <see cref="PauseReserveArrowDefinitions.ArrowFrames"/> pulse phases.</param>
    /// <exception cref="ArgumentNullException"><paramref name="arrowFrames"/> is null.</exception>
    /// <exception cref="InvalidDataException">The installation does not hold every pulse phase's colours.</exception>
    internal PauseReserveArrowColours(RgbColor solidColor6, RgbColor solidColor11, ReserveArrowColors[] arrowFrames)
    {
        Ensure.NotNull(arrowFrames);
        if (arrowFrames.Length != PauseReserveArrowDefinitions.ArrowFrames)
            throw new InvalidDataException($"The reserve arrow's pulse needs {PauseReserveArrowDefinitions.ArrowFrames} phases of colours, not {arrowFrames.Length}.");
        Solid = new(solidColor6, solidColor11);
        pulse = [.. arrowFrames];
    }

    /// <summary>The arrow's two bevel colours while solid.</summary>
    internal ReserveArrowColors Solid { get; }

    /// <summary>The arrow's two bevel colours at pulse phase <paramref name="frame"/>.</summary>
    /// <param name="frame">Pulse phase, masked to its low five bits; normally the accepted eight-bit frame counter.</param>
    internal ReserveArrowColors Pulse(int frame) => pulse[frame & (PauseReserveArrowDefinitions.ArrowFrames - 1)];

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
}
