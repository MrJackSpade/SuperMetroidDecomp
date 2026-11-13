using SuperMetroid.Core.Bosses.MotherBrain.Artwork;

using SuperMetroid.Core.Assets;
namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// Mother Brain's rainbow beam as a screen effect: whether it shows, the colour it adds and the geometry of the cone it
/// adds that colour across. The geometry is published as a description the renderer
/// rasterizes; the colour advances only with the simulation, never when a frame is repainted.
/// </summary>
public sealed class MotherBrainRainbowBeamEffect
{
    /// <summary>Host-owned visual colors; the beam geometry and update cadence stay logic.</summary>
    public MotherBrainRainbowPalettePresentation? PresentationColors { get; set; }
    /// <summary>Whether the last simulation update showed the beam; hiding it keeps the previous colour and geometry stored but stops them being drawn.</summary>
    public bool Active { get; private set; }
    /// <summary>The colour the beam adds inside its cone; set when the beam appears and advanced only by later updates that show it.</summary>
    public RgbColor Color { get; private set; }
    /// <summary>The next colour of the beam's cycle, advancing by one each update the beam shows; the cycle's end resets it to zero and repeats colour zero without advancing that update.</summary>
    public int ColorIndex { get; private set; }
    /// <summary>
    /// Last geometry resolved by a right, up or down dispatcher. Null until the first such
    /// update; a leftward-straddling beam and deactivation both keep it.
    /// </summary>
    public MotherBrainRainbowBeamGeometry? Geometry { get; private set; }
    /// <summary>Value-only render input for the current state; taking it never advances the beam.</summary>
    public MotherBrainRainbowBeamRenderSnapshot RenderSnapshot => new(Active, Color, Geometry);

    /// <summary>One update of the beam, after the head's position and beam aim have been resolved.</summary>
    /// <param name="active">Enemy-owned enable state; false disables drawing without advancing color or resolving geometry, and a later activation restarts the color cycle.</param>
    /// <param name="head">Head enemy's whole-pixel room position. The mouth lies fourteen pixels right of it and five below, and must lie strictly between work-area scanlines 32 and 232.</param>
    /// <param name="angle">Beam center aim; only its 256-unit-turn table index is used when computing the two wrapping edge angles.</param>
    /// <param name="angularWidth">Angular width; its whole steps are halved with integer truncation to obtain each edge's displacement in steps.</param>
    /// <param name="camera">The camera's whole-pixel room position, which places the mouth on the screen.</param>
    public void Step(bool active, Position16<RoomPixels> head,
        RotationAngle angle, RotationAngle angularWidth, Position16<RoomPixels> camera)
    {
        if (!active)
        {
            Active = false;
            return;
        }
        if (!Active)
        {
            Active = true;
            ColorIndex = 0;
            Color = Colours.BeamInitialColor;
        }
        else if (ColorIndex == Colours.BeamCycleLength)
        {
            // Past the cycle's last colour, the update that restarts it repeats colour zero without advancing.
            ColorIndex = 0;
            Color = Colours.BeamColor(0);
        }
        else
        {
            Color = Colours.BeamColor(ColorIndex);
            ColorIndex++;
        }
        ResolveGeometry(head, camera, angle, angularWidth);
    }

    /// <summary>Resolves the beam's cone from its two angular edges, using quadrant dispatch and mouth offsets.</summary>
    private void ResolveGeometry(Position16<RoomPixels> head, Position16<RoomPixels> camera,
        RotationAngle angle, RotationAngle angularWidth)
    {
        // The cone's edges lie half its whole-step width either side of its direction; the quarter turns the two
        // edges lie in choose how it is drawn.
        TrigonometryAngle halfWidth = angularWidth.WholeStep.DividedBy(2).ToTrigonometryAngle();
        TrigonometryAngle firstEdge = angle.ToTrigonometryAngle() - halfWidth;
        TrigonometryAngle secondEdge = angle.ToTrigonometryAngle() + halfWidth;
        int quadrant = firstEdge.ToAngle().Sector(4) * 4 + secondEdge.ToAngle().Sector(4);
        var direction = MotherBrainBeamQuadrants.DirectionForPair(quadrant);
        // The leftward-straddling case is empty: retain the previous geometry for a beam straddling left.
        if (direction == MotherBrainBeamDirection.Retain) return;
        if (direction is not (MotherBrainBeamDirection.Right
            or MotherBrainBeamDirection.Up or MotherBrainBeamDirection.Down))
            throw new InvalidDataException($"Mother Brain beam quadrant pair {quadrant} has no cone rasterizer.");

        // The mouth is placed on the screen from its room position and the camera's (#1521).
        int originY = CoordinateAxis.Difference(head.Y + MotherBrainBeamDefinitions.MouthYOffset, camera.Y);
        if (originY is <= MotherBrainBeamDefinitions.FirstLine or >= MotherBrainBeamDefinitions.EndLine)
            throw new InvalidDataException($"Mother Brain beam mouth Y={originY} is outside its scanline work area.");
        Geometry = new(CoordinateAxis.Difference(head.X + MotherBrainBeamDefinitions.MouthXOffset, camera.X), originY,
            firstEdge.RawValue, secondEdge.RawValue);
    }

    /// <summary>The installed colour cycle; fails if rainbow presentation colours were never bound.</summary>
    private MotherBrainRainbowPalettePresentation Colours => PresentationColors ?? throw new InvalidOperationException(
        "Mother Brain rainbow beam requires installed colors.");
}
