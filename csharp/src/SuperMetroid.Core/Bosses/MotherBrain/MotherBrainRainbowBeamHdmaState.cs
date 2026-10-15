using SuperMetroid.Core.Bosses.MotherBrain.Artwork;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// Simulation-owned bank-$88 rainbow HDMA object. Geometry follows $AD:DE00-$DE5D and is published
/// as a description the renderer rasterizes; color follows $88:E767/$E7ED and must not advance when
/// the host repaints a frame.
/// </summary>
public sealed class MotherBrainRainbowBeamHdmaState
{
    /// <summary>Host-owned visual colors; the beam geometry and update cadence stay cartridge logic.</summary>
    [field: NonSerialized]
    public MotherBrainRainbowPalettePresentation? PresentationColors { get; set; }
    /// <summary>Whether the last simulation update enabled the rainbow-beam color-add layer; disabling leaves the previous color and geometry stored but prevents rendering them.</summary>
    public bool Active { get; private set; }
    /// <summary>Current packed SNES RGB5 fixed-backdrop color for windowed addition, not a CGRAM entry; initialized on activation and advanced only by subsequent active updates.</summary>
    public Bgr555 Color { get; private set; }
    /// <summary>Next native $88:E833 color-table byte offset, advancing by four rather than by one color ordinal; a signed terminator resets to zero and repeats entry zero without advancing that update.</summary>
    public int ColorCursor { get; private set; }
    /// <summary>
    /// Last geometry resolved by a right, up or down dispatcher. Null until the first such
    /// update; a leftward-straddling beam ($AD:DE5E RTS) and deactivation both keep it.
    /// </summary>
    public MotherBrainRainbowBeamGeometry? Geometry { get; private set; }
    /// <summary>Value-only render input for the current state; taking it never advances the beam.</summary>
    public MotherBrainRainbowBeamRenderSnapshot RenderSnapshot => new(Active, Color, Geometry);

    /// <summary>One emulated HDMA call, after the head's position and beam aim have been resolved.</summary>
    /// <param name="active">Enemy-owned enable state; false disables drawing without advancing color or resolving geometry, and a later activation restarts the color cycle.</param>
    /// <param name="head">Head enemy's whole-pixel room position, not camera-relative. Adding fourteen to X and retaining the low byte gives the screen-window mouth X; the mouth is five pixels below Y and must lie strictly between native work-area scanlines 32 and 232.</param>
    /// <param name="angle">Beam center aim; only its 256-unit-turn table index is used when computing the two wrapping edge angles.</param>
    /// <param name="angularWidth">Native 8.8 angular-width word; its high byte is halved with integer truncation to obtain each edge's displacement in 256-unit-turn table units.</param>
    public void Step(bool active, Position16<RoomPixels> head,
        SnesAngle angle, ushort angularWidth)
    {
        if (!active)
        {
            Active = false;
            return;
        }
        if (!Active)
        {
            Active = true;
            ColorCursor = 0;
            Color = PresentationColors?.BeamInitialColor ?? MotherBrainBeamRomData.InitialColor;
        }
        else
        {
            if (!TryReadColor(ColorCursor, out Bgr555 color))
            {
                // The native signed terminator: the reset frame repeats entry zero without incrementing.
                ColorCursor = 0;
                if (!TryReadColor(0, out color))
                    throw new InvalidDataException("Mother Brain beam color cycle has no first entry.");
            }
            else ColorCursor += MotherBrainBeamRomData.ColorStride;
            Color = color;
        }
        ResolveGeometry(head, angle, angularWidth);
    }

    /// <summary>Projects the beam's two angular edges into scanline window words using native quadrant dispatch and mouth offsets.</summary>
    private void ResolveGeometry(Position16<RoomPixels> head,
        SnesAngle angle, ushort angularWidth)
    {
        int halfWidth = (SnesLittleEndian.HighByte(angularWidth)) / 2;
        byte a = unchecked((byte)(angle.TableIndex - halfWidth));
        byte b = unchecked((byte)(angle.TableIndex + halfWidth));
        int quadrant = (a / SnesAngle.QuarterTurn.TableIndex) * 4 + b / SnesAngle.QuarterTurn.TableIndex;
        var direction = MotherBrainBeamRomData.DirectionForQuadrants(quadrant);
        // Native DE5E is an RTS: retain the previous geometry for a beam straddling left.
        if (direction == MotherBrainBeamRomData.Direction.Retain) return;
        if (direction is not (MotherBrainBeamRomData.Direction.Right
            or MotherBrainBeamRomData.Direction.Up or MotherBrainBeamRomData.Direction.Down))
            throw new InvalidDataException($"Mother Brain beam quadrant pair {quadrant} selects a null native dispatcher.");

        int originY = head.Y + MotherBrainBeamRomData.MouthYOffset;
        if (originY is <= MotherBrainBeamRomData.FirstLine or >= MotherBrainBeamRomData.EndLine)
            throw new InvalidDataException($"Mother Brain beam mouth Y={originY} is outside its native scanline work area.");
        // DE20 reads XPosition-1, then masks after adding: this is the low X byte,
        // not a camera-relative coordinate or the enemy header pointer in the C decompile.
        Geometry = new(unchecked((byte)(head.X + MotherBrainBeamRomData.MouthXOffset)), originY, a, b, direction);
    }

    /// <summary>Reads the installed color cycle and fails if rainbow presentation colors were never bound.</summary>
    private bool TryReadColor(int cursor, out Bgr555 color) =>
        (PresentationColors ?? throw new InvalidOperationException(
            "Mother Brain rainbow beam requires installed colors."))
            .TryReadBeamColor(cursor, out color);
}
