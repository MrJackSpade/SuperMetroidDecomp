using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Samus.Movement;

namespace SuperMetroid.Core.Rendering;

/// <summary>Render-side rasterization of Mother Brain's rainbow-beam geometry, <c>$AD:DE7F-$E395</c>.</summary>
internal static class RainbowBeamWindowBuilder
{
    /// <summary>
    /// The working edge word of a line the beam misses: the rasterization packs each line's left column in the low
    /// byte and its right column in the high byte, and compares those words against its off-screen sentinels.
    /// </summary>
    private static readonly ushort MissedLineWord = SnesLittleEndian.Word(MotherBrainBeamLine.None.Left, MotherBrainBeamLine.None.Right);

    /// <summary>
    /// Returns the screen columns the beam crosses on each visible scanline. The HUD lines and every line the beam
    /// misses are <see cref="MotherBrainBeamLine.None"/>, and a null geometry misses every line.
    /// </summary>
    internal static MotherBrainBeamLine[] Build(MotherBrainRainbowBeamGeometry? geometry)
    {
        var windows = new MotherBrainBeamLine[SnesPpuLayout.ScreenHeightPixels];
        Array.Fill(windows, MotherBrainBeamLine.None);
        if (geometry is not { } beam) return windows;
        bool right = beam.Direction == MotherBrainBeamRomData.Direction.Right;
        bool up = beam.Direction == MotherBrainBeamRomData.Direction.Up;
        bool down = beam.Direction == MotherBrainBeamRomData.Direction.Down;
        if (!right && !up && !down)
            throw new ArgumentOutOfRangeException(nameof(geometry), beam.Direction,
                "Rainbow-beam geometry must select the right, up or down dispatcher.");
        int a = beam.EdgeA, b = beam.EdgeB;
        int originY = beam.MouthY;
        if (originY is <= MotherBrainBeamRomData.FirstLine or >= MotherBrainBeamRomData.EndLine)
            throw new ArgumentOutOfRangeException(nameof(geometry), originY,
                "Rainbow-beam mouth Y is outside its native scanline work area.");
        int origin = SnesEightEight.FromWhole(beam.MouthScreenX);
        var data = new ushort[MotherBrainBeamRomData.EndLine];
        Array.Fill(data, MissedLineWord);
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
                ushort word = (ushort)(SnesEightEight.WholeOf(left) | (rightEdge & MotherBrainBeamRomData.RightScreenEdge));
                // Down-left uses zero as its off-screen sentinel; the other routines
                // test FFFF. Keep that asymmetry instead of normalizing the native edges.
                bool empty = down && leftNegative && rightNegative ? word == 0 : word == ushort.MaxValue;
                data[index] = empty ? MissedLineWord : word;
            }
        }

        for (int y = MotherBrainBeamRomData.FirstLine; y < windows.Length; y++)
        {
            int row = y - MotherBrainBeamRomData.FirstLine;
            if (up && row >= count) continue;
            int index = row + MotherBrainBeamRomData.DataPrefixWords;
            if (!up && row >= MotherBrainBeamRomData.FirstRunLines)
                index = row - MotherBrainBeamRomData.FirstRunLines + (right
                    ? MotherBrainBeamRomData.RightSecondRunWord : MotherBrainBeamRomData.DownSecondRunWord);
            windows[y] = new MotherBrainBeamLine(SnesLittleEndian.LowByte(data[index]), SnesLittleEndian.HighByte(data[index]));
        }
        return windows;

        void FillRight(int index, int direction, int length, int slope)
        {
            int edge = origin;
            for (int row = 0; row < length; row++, index += direction)
            {
                edge += slope;
                if (edge > ushort.MaxValue) break;
                data[index] = (ushort)(SnesEightEight.WholeOf(edge) | MotherBrainBeamRomData.RightScreenEdge);
            }
        }
    }

    /// <summary>Compiled bank-$91 absolute tangent, with the caller's angle wrapped to a byte.</summary>
    internal static int Tangent(int angle) =>
        AbsoluteTangentDefinitions.Sample(unchecked((byte)angle));
}
