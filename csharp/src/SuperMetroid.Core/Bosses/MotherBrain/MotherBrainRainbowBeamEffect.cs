using SuperMetroid.Core.Bosses.MotherBrain.Artwork;

using SuperMetroid.Core.Assets;
namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// Mother Brain's rainbow beam as a screen effect: whether it shows, the colour it adds and the geometry of the cone it
/// adds that colour across. The geometry follows $AD:DE00-$DE5D and is published as a description the renderer
/// rasterizes; the colour follows $88:E767/$E7ED and advances only with the simulation, never when a frame is repainted.
/// </summary>
public sealed class MotherBrainRainbowBeamEffect
{
    /// <summary>Host-owned visual colors; the beam geometry and update cadence stay cartridge logic.</summary>
    [field: NonSerialized]
    public MotherBrainRainbowPalettePresentation? PresentationColors { get; set; }
    /// <summary>Whether the last simulation update showed the beam; hiding it keeps the previous colour and geometry stored but stops them being drawn.</summary>
    public bool Active { get; private set; }
    /// <summary>The colour the beam adds inside its cone; set when the beam appears and advanced only by later updates that show it.</summary>
    public RgbColor Color { get; private set; }
    /// <summary>The next colour of the beam's cycle, advancing by one each update the beam shows; the cycle's end resets it to zero and repeats colour zero without advancing that update.</summary>
    public int ColorIndex { get; private set; }
    /// <summary>
    /// Last geometry resolved by a right, up or down dispatcher. Null until the first such
    /// update; a leftward-straddling beam ($AD:DE5E RTS) and deactivation both keep it.
    /// </summary>
    public MotherBrainRainbowBeamGeometry? Geometry { get; private set; }
    /// <summary>Value-only render input for the current state; taking it never advances the beam.</summary>
    public MotherBrainRainbowBeamRenderSnapshot RenderSnapshot => new(Active, Color, Geometry);

    /// <summary>One update of the beam, after the head's position and beam aim have been resolved.</summary>
    /// <param name="active">Enemy-owned enable state; false disables drawing without advancing color or resolving geometry, and a later activation restarts the color cycle.</param>
    /// <param name="head">Head enemy's whole-pixel room position, not camera-relative. Adding fourteen to X and retaining the low byte gives the screen-window mouth X; the mouth is five pixels below Y and must lie strictly between native work-area scanlines 32 and 232.</param>
    /// <param name="angle">Beam center aim; only its 256-unit-turn table index is used when computing the two wrapping edge angles.</param>
    /// <param name="angularWidth">Angular width in 8.8 table units, $0200 through $0C00; its whole part is halved with integer truncation to obtain each edge's displacement in 256-unit-turn table units.</param>
    public void Step(bool active, Position16<RoomPixels> head,
        RotationAngle angle, int angularWidth)
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
            Color = PresentationColors?.BeamInitialColor ?? MotherBrainBeamDefinitions.InitialColor;
        }
        else
        {
            if (!TryReadColor(ColorIndex, out RgbColor color))
            {
                // The cycle's end: the reset frame repeats colour zero without incrementing.
                ColorIndex = 0;
                if (!TryReadColor(0, out color))
                    throw new InvalidDataException("Mother Brain beam color cycle has no first entry.");
            }
            else ColorIndex++;
            Color = color;
        }
        ResolveGeometry(head, angle, angularWidth);
    }

    /// <summary>Resolves the beam's cone from its two angular edges, using native quadrant dispatch and mouth offsets.</summary>
    private void ResolveGeometry(Position16<RoomPixels> head,
        RotationAngle angle, int angularWidth)
    {
        int halfWidth = EightEight.WholeOf(Ensure.BetweenInclusive(0, ushort.MaxValue, angularWidth)) / 2;
        int a = angle.ToByteAngle().AddTableUnits(-halfWidth).RawValue;
        int b = angle.ToByteAngle().AddTableUnits(halfWidth).RawValue;
        int quadrant = (a / RotationAngle.QuarterTurn.TableIndex) * 4 + b / RotationAngle.QuarterTurn.TableIndex;
        var direction = MotherBrainBeamQuadrants.DirectionForPair(quadrant);
        // Native DE5E is an RTS: retain the previous geometry for a beam straddling left.
        if (direction == MotherBrainBeamDirection.Retain) return;
        if (direction is not (MotherBrainBeamDirection.Right
            or MotherBrainBeamDirection.Up or MotherBrainBeamDirection.Down))
            throw new InvalidDataException($"Mother Brain beam quadrant pair {quadrant} selects a null native dispatcher.");

        int originY = head.Y + MotherBrainBeamDefinitions.MouthYOffset;
        if (originY is <= MotherBrainBeamDefinitions.FirstLine or >= MotherBrainBeamDefinitions.EndLine)
            throw new InvalidDataException($"Mother Brain beam mouth Y={originY} is outside its native scanline work area.");
        // DE20 reads XPosition-1, then masks after adding: this is the low X byte,
        // not a camera-relative coordinate or the enemy header pointer in the C decompile.
        Geometry = new(unchecked((byte)(head.X + MotherBrainBeamDefinitions.MouthXOffset)), originY, a, b, direction);
    }

    /// <summary>Reads the installed color cycle and fails if rainbow presentation colors were never bound.</summary>
    private bool TryReadColor(int index, out RgbColor color) =>
        (PresentationColors ?? throw new InvalidOperationException(
            "Mother Brain rainbow beam requires installed colors."))
            .TryReadBeamColor(index, out color);
}
