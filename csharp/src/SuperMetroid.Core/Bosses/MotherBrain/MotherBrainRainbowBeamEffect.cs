using SuperMetroid.Core.Bosses.MotherBrain.Artwork;

using SuperMetroid.Core.Assets;
namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// Mother Brain's rainbow beam as a screen effect: whether it shows, the colour it adds and the cone it adds that colour
/// across, from her mouth between its two edge angles. The display draws the cone; the colour advances only with the
/// simulation, never when a frame is repainted.
/// </summary>
public sealed class MotherBrainRainbowBeamEffect
{
    /// <summary>Host-owned visual colors; the beam geometry and update cadence stay logic.</summary>
    public MotherBrainRainbowPalettePresentation? PresentationColors { get; set; }
    /// <summary>Whether the last simulation update showed the beam; a hidden beam shows nothing.</summary>
    public bool Active { get; private set; }
    /// <summary>The colour the beam adds inside its cone; set when the beam appears and advanced only by later updates that show it.</summary>
    public RgbColor Color { get; private set; }
    /// <summary>The beam cycle's next colour step, advancing by one each update the beam shows; the cycle's end resets it to the first step and repeats the first colour without advancing that update.</summary>
    public int ColourStep { get; private set; }
    /// <summary>The cone the last update that showed the beam aimed; null until the beam first shows.</summary>
    public MotherBrainRainbowBeamGeometry? Geometry { get; private set; }
    /// <summary>What the beam shows now, or null while it is hidden; taking it never advances the beam.</summary>
    public MotherBrainRainbowBeamRenderSnapshot? RenderSnapshot => Active && Geometry is { } cone ? new(Color, cone) : null;

    /// <summary>One update of the beam, after the head's position and beam aim have been resolved.</summary>
    /// <param name="active">Enemy-owned enable state; false hides the beam without advancing its colour, and a later activation restarts the color cycle.</param>
    /// <param name="head">Head enemy's whole-pixel room position. The mouth lies fourteen pixels right of it and five below.</param>
    /// <param name="angle">Beam center aim; only its whole angle units are used when computing the two wrapping edge angles.</param>
    /// <param name="angularWidth">Angular width; its whole steps are halved with integer truncation to obtain each edge's displacement in steps.</param>
    public void Step(bool active, Position16<RoomPixels> head, RotationAngle angle, RotationAngle angularWidth)
    {
        if (!active)
        {
            Active = false;
            return;
        }
        if (!Active)
        {
            Active = true;
            ColourStep = 0;
            Color = Colours.BeamInitialColor;
        }
        else if (ColourStep == Colours.BeamCycleLength)
        {
            // Past the cycle's last colour, the update that restarts it repeats colour zero without advancing.
            ColourStep = 0;
            Color = Colours.BeamColor(0);
        }
        else
        {
            Color = Colours.BeamColor(ColourStep);
            ColourStep++;
        }
        // The cone's edges lie half its whole-step width either side of its direction.
        TrigonometryAngle halfWidth = angularWidth.WholeStep.DividedBy(2).ToTrigonometryAngle();
        Geometry = new(new(CoordinateAxis.Wrap(head.X + MotherBrainBeamDefinitions.MouthXOffset),
                CoordinateAxis.Wrap(head.Y + MotherBrainBeamDefinitions.MouthYOffset)),
            angle.ToTrigonometryAngle() - halfWidth,
            angle.ToTrigonometryAngle() + halfWidth);
    }

    /// <summary>The installed colour cycle; fails if rainbow presentation colours were never bound.</summary>
    private MotherBrainRainbowPalettePresentation Colours => PresentationColors ?? throw new InvalidOperationException(
        "Mother Brain rainbow beam requires installed colors.");
}
