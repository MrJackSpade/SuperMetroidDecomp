using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Simulation-owned bank-$88 rainbow HDMA object. Geometry follows $AD:DE00-$E395;
/// color follows $88:E767/$E7ED and must not advance when the host repaints a frame.
/// </summary>
public sealed class MotherBrainRainbowBeamHdmaState
{
    private readonly ushort[] windows = new ushort[SnesPpuLayout.ScreenHeightPixels];
    public bool Active { get; private set; }
    public ushort Color { get; private set; }
    public int ColorCursor { get; private set; }
    public ReadOnlySpan<ushort> Windows => windows;

    public MotherBrainRainbowBeamHdmaState() => Array.Fill(windows, MotherBrainBeamRomData.EmptyWindow);

    /// <summary>One emulated HDMA call, after the head's position and beam aim have been resolved.</summary>
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
            Color = MotherBrainBeamRomData.InitialColor;
        }
        else
        {
            ushort color = ReadWord(bus, MotherBrainBeamRomData.ColorTable + ColorCursor);
            if (unchecked((short)color) < 0)
            {
                // The native reset frame repeats entry zero without incrementing.
                ColorCursor = 0;
                color = ReadWord(bus, MotherBrainBeamRomData.ColorTable);
            }
            else ColorCursor += MotherBrainBeamRomData.ColorStride;
            Color = color;
        }
        BuildWindows(bus, headX, headY, angle, angularWidth);
    }

    private void BuildWindows(ISnesAddressSpace bus, ushort headX, ushort headY,
        SnesAngle angle, ushort angularWidth)
    {
        int halfWidth = (angularWidth >> 8) / 2;
        int a = unchecked((byte)(angle.TableIndex - halfWidth));
        int b = unchecked((byte)(angle.TableIndex + halfWidth));
        int quadrant = (a / SnesAngle.QuarterTurn.TableIndex) * 4 + b / SnesAngle.QuarterTurn.TableIndex;
        var direction = MotherBrainBeamRomData.QuadrantDirections[quadrant];
        // Native DE5E is an RTS: retain the previous table for a beam straddling left.
        if (direction == MotherBrainBeamRomData.Direction.Retain) return;
        bool right = direction == MotherBrainBeamRomData.Direction.Right;
        bool up = direction == MotherBrainBeamRomData.Direction.Up;
        bool down = direction == MotherBrainBeamRomData.Direction.Down;
        if (!right && !up && !down)
            throw new InvalidDataException($"Mother Brain beam quadrant pair {quadrant} selects a null native dispatcher.");

        int originY = headY + MotherBrainBeamRomData.MouthYOffset;
        if (originY <= MotherBrainBeamRomData.FirstLine || originY >= MotherBrainBeamRomData.EndLine)
            throw new InvalidDataException($"Mother Brain beam mouth Y={originY} is outside its native scanline work area.");
        // DE20 reads XPosition-1, then masks after adding: this is the low X byte,
        // not a camera-relative coordinate or the enemy header pointer in the C decompile.
        int origin = unchecked((byte)(headX + MotherBrainBeamRomData.MouthXOffset)) << 8;
        var data = new ushort[MotherBrainBeamRomData.EndLine];
        Array.Fill(data, MotherBrainBeamRomData.EmptyWindow);
        int count = originY - MotherBrainBeamRomData.FirstLine;
        if (right)
        {
            FillRight(count + 1, -1, count, Tangent(bus, b));
            FillRight(count + 2, 1, MotherBrainBeamRomData.EndLine - originY, Tangent(bus, a));
        }
        else
        {
            int leftAngle = up ? b : a;
            int rightAngle = up ? a : b;
            bool leftNegative = leftAngle >= SnesAngle.HalfTurn.TableIndex;
            bool rightNegative = rightAngle >= SnesAngle.HalfTurn.TableIndex;
            int leftStep = Tangent(bus, leftNegative ? -leftAngle : leftAngle) * (leftNegative ? -1 : 1);
            int rightStep = Tangent(bus, rightNegative ? -rightAngle : rightAngle) * (rightNegative ? -1 : 1);
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

    private static int Tangent(ISnesAddressSpace bus, int angle) =>
        AbsoluteTangentDefinitions.Sample(unchecked((byte)angle));
    private static ushort ReadWord(ISnesAddressSpace bus, int address) =>
        (ushort)(bus.ReadByte(address) | bus.ReadByte(address + 1) << 8);
}
