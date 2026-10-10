using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Simulation-owned bank-$88 rainbow HDMA object. Geometry follows $AD:DE00-$E395;
/// color follows $88:E767/$E7ED and must not advance when the host repaints a frame.
/// </summary>
public sealed class MotherBrainRainbowBeamHdmaState
{
    private readonly ushort[] windows = new ushort[SnesPpuLayout.ScreenHeightPixels];

    /// <summary>Host-owned visual colors; the beam window and update cadence stay cartridge logic.</summary>
    [field: NonSerialized]
    public MotherBrainRainbowPalettePresentation? PresentationColors { get; set; }
    /// <summary>Whether the last simulation update enabled the rainbow-beam color-add layer; disabling leaves the previous color and windows stored but prevents rendering them.</summary>
    public bool Active { get; private set; }
    /// <summary>Current packed SNES RGB5 fixed-backdrop color for windowed addition, not a CGRAM entry; initialized on activation and advanced only by subsequent active updates.</summary>
    public Bgr555 Color { get; private set; }
    /// <summary>Next native $88:E833 color-table byte offset, advancing by four rather than by one color ordinal; a signed terminator resets to zero and repeats entry zero without advancing that update.</summary>
    public int ColorCursor { get; private set; }
    /// <summary>Live read-only view of 224 visible scanline window words: low byte is inclusive WH0 left, high byte is inclusive WH1 right, and $00FF denotes empty; the first 32 HUD lines stay empty.</summary>
    public ReadOnlySpan<ushort> Windows => windows;

    /// <summary>Creates an inactive simulation owner with every visible window empty; the first active update initializes its fixed color and cursor before building geometry.</summary>
    public MotherBrainRainbowBeamHdmaState() => Array.Fill(windows, MotherBrainBeamRomData.EmptyWindow);

    /// <summary>One emulated HDMA call, after the head's position and beam aim have been resolved.</summary>
    /// <param name="bus">Required address-space context; geometry uses compiled tangent samples and the color cycle uses installed presentation colors, not live cartridge reads.</param>
    /// <param name="active">Enemy-owned enable state; false disables drawing without advancing color or rebuilding windows, and a later activation restarts the color cycle.</param>
    /// <param name="headX">Head enemy's whole-pixel room X position, not camera-relative; adding fourteen and retaining the low byte gives the screen-window mouth X.</param>
    /// <param name="headY">Head enemy's whole-pixel room Y position; the mouth is five pixels lower and must lie strictly between native work-area scanlines 32 and 232.</param>
    /// <param name="angle">Beam center aim; only its 256-unit-turn table index is used when computing the two wrapping edge angles.</param>
    /// <param name="angularWidth">Native 8.8 angular-width word; its high byte is halved with integer truncation to obtain each edge's displacement in 256-unit-turn table units.</param>
    public void Step(ISnesAddressSpace bus, bool active, ushort headX, ushort headY,
        SnesAngle angle, ushort angularWidth)
    {
        ArgumentNullException.ThrowIfNull(bus);
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
        BuildWindows(headX, headY, angle, angularWidth);
    }

    private void BuildWindows(ushort headX, ushort headY,
        SnesAngle angle, ushort angularWidth)
    {
        int halfWidth = (angularWidth >> 8) / 2;
        int a = unchecked((byte)(angle.TableIndex - halfWidth));
        int b = unchecked((byte)(angle.TableIndex + halfWidth));
        int quadrant = (a / SnesAngle.QuarterTurn.TableIndex) * 4 + b / SnesAngle.QuarterTurn.TableIndex;
        var direction = MotherBrainBeamRomData.DirectionForQuadrants(quadrant);
        // Native DE5E is an RTS: retain the previous table for a beam straddling left.
        if (direction == MotherBrainBeamRomData.Direction.Retain) return;
        bool right = direction == MotherBrainBeamRomData.Direction.Right;
        bool up = direction == MotherBrainBeamRomData.Direction.Up;
        bool down = direction == MotherBrainBeamRomData.Direction.Down;
        if (!right && !up && !down)
            throw new InvalidDataException($"Mother Brain beam quadrant pair {quadrant} selects a null native dispatcher.");

        int originY = headY + MotherBrainBeamRomData.MouthYOffset;
        if (originY is <= MotherBrainBeamRomData.FirstLine or >= MotherBrainBeamRomData.EndLine)
            throw new InvalidDataException($"Mother Brain beam mouth Y={originY} is outside its native scanline work area.");
        // DE20 reads XPosition-1, then masks after adding: this is the low X byte,
        // not a camera-relative coordinate or the enemy header pointer in the C decompile.
        int origin = unchecked((byte)(headX + MotherBrainBeamRomData.MouthXOffset)) << 8;
        var data = new ushort[MotherBrainBeamRomData.EndLine];
        Array.Fill(data, MotherBrainBeamRomData.EmptyWindow);
        int count = originY - MotherBrainBeamRomData.FirstLine;
        if (right)
        {
            FillRight(count + 1, -1, count, Tangent(b));
            FillRight(count + 2, 1, MotherBrainBeamRomData.EndLine - originY, Tangent(a));
        }
        else
        {
            int leftAngle = up ? b : a;
            int rightAngle = up ? a : b;
            bool leftNegative = leftAngle >= SnesAngle.HalfTurn.TableIndex;
            bool rightNegative = rightAngle >= SnesAngle.HalfTurn.TableIndex;
            int leftStep = Tangent(leftNegative ? -leftAngle : leftAngle) * (leftNegative ? -1 : 1);
            int rightStep = Tangent(rightNegative ? -rightAngle : rightAngle) * (rightNegative ? -1 : 1);
            int index = up ? count + 1 : count + 2;
            int length = up ? count : MotherBrainBeamRomData.EndLine - originY;
            int left = origin, rightEdge = origin;
            for (int row = 0; row < length; row++, index += up ? -1 : 1)
            {
                left = Math.Clamp(left + leftStep, 0, ushort.MaxValue);
                rightEdge = Math.Clamp(rightEdge + rightStep, 0, ushort.MaxValue);
                ushort word = (ushort)((left >> 8) | (rightEdge & MotherBrainBeamRomData.RightScreenEdge));
                // Down-left uses zero as its off-screen sentinel; the other routines
                // test FFFF. Keep that asymmetry instead of normalizing the native edges.
                bool empty = down && leftNegative && rightNegative ? word == 0 : word == ushort.MaxValue;
                data[index] = empty ? MotherBrainBeamRomData.EmptyWindow : word;
            }
        }

        Array.Fill(windows, MotherBrainBeamRomData.EmptyWindow);
        for (int y = MotherBrainBeamRomData.FirstLine; y < windows.Length; y++)
        {
            int row = y - MotherBrainBeamRomData.FirstLine;
            if (up && row >= count) continue;
            int index = row + MotherBrainBeamRomData.DataPrefixWords;
            if (!up && row >= MotherBrainBeamRomData.FirstRunLines)
                index = row - MotherBrainBeamRomData.FirstRunLines + (right
                    ? MotherBrainBeamRomData.RightSecondRunWord : MotherBrainBeamRomData.DownSecondRunWord);
            windows[y] = data[index];
        }

        void FillRight(int index, int direction, int length, int slope)
        {
            int edge = origin;
            for (int row = 0; row < length; row++, index += direction)
            {
                edge += slope;
                if (edge > ushort.MaxValue) break;
                data[index] = (ushort)((edge >> 8) | MotherBrainBeamRomData.RightScreenEdge);
            }
        }
    }

    private static int Tangent(int angle) =>
        AbsoluteTangentDefinitions.Sample(unchecked((byte)angle));
    private bool TryReadColor(int cursor, out Bgr555 color) =>
        (PresentationColors ?? throw new InvalidOperationException(
            "Mother Brain rainbow beam requires installed colors."))
            .TryReadBeamColor(cursor, out color);
}
