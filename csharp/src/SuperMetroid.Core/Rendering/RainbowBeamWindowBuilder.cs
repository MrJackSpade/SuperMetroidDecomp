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
    private static readonly ushort MissedLineWord = ScanlineSpanWord.Of(checked((byte)MotherBrainBeamLine.None.Left), checked((byte)MotherBrainBeamLine.None.Right));

    /// <summary>The working edge word's right column at the rightmost screen pixel, with the left column empty.</summary>
    private static readonly ushort RightScreenEdge = ScanlineSpanWord.Of(0, byte.MaxValue);

    /// <summary>The working table's two leading entries that precede the first beam line.</summary>
    private const int DataPrefixWords = 2;

    /// <summary>The beam lines the rasterization's first run covers.</summary>
    private const int FirstRunLines = 112;

    /// <summary>The working-table entry the rightward rasterization's second run starts at, skipping four entries after the first run.</summary>
    private const int RightSecondRunWord = 118;

    /// <summary>The working-table entry the downward rasterization's second run starts at, skipping one entry after the first run.</summary>
    private const int DownSecondRunWord = 115;

    /// <summary>
    /// Returns the screen columns the beam crosses on each visible screen line. The HUD lines and every line the beam
    /// misses are <see cref="MotherBrainBeamLine.None"/>, and a null geometry misses every line.
    /// </summary>
    internal static MotherBrainBeamLine[] Build(MotherBrainRainbowBeamGeometry? geometry)
    {
        var windows = new MotherBrainBeamLine[ScreenGeometry.ScreenHeightPixels];
        Array.Fill(windows, MotherBrainBeamLine.None);
        if (geometry is not { } beam) return windows;
        bool right = beam.Direction == MotherBrainBeamDirection.Right;
        bool up = beam.Direction == MotherBrainBeamDirection.Up;
        bool down = beam.Direction == MotherBrainBeamDirection.Down;
        if (!right && !up && !down)
            throw new ArgumentOutOfRangeException(nameof(geometry), beam.Direction,
                "Rainbow-beam geometry must select the right, up or down dispatcher.");
        int a = beam.EdgeA, b = beam.EdgeB;
        int originY = beam.MouthY;
        if (originY is <= MotherBrainBeamDefinitions.FirstLine or >= MotherBrainBeamDefinitions.EndLine)
            throw new ArgumentOutOfRangeException(nameof(geometry), originY,
                "Rainbow-beam mouth Y is outside the lines the beam is traced over.");
        int origin = EightEight.FromWhole(beam.MouthScreenX);
        var data = new ushort[MotherBrainBeamDefinitions.EndLine];
        Array.Fill(data, MissedLineWord);
        int count = originY - MotherBrainBeamDefinitions.FirstLine;
        if (right)
        {
            FillRight(count + 1, -1, count, Tangent(b));
            FillRight(count + 2, 1, MotherBrainBeamDefinitions.EndLine - originY, Tangent(a));
        }
        else
        {
            int leftAngle = up ? b : a;
            int rightAngle = up ? a : b;
            bool leftNegative = leftAngle >= RotationAngle.HalfTurn.TableIndex;
            bool rightNegative = rightAngle >= RotationAngle.HalfTurn.TableIndex;
            int leftStep = Tangent(leftNegative ? -leftAngle : leftAngle) * (leftNegative ? -1 : 1);
            int rightStep = Tangent(rightNegative ? -rightAngle : rightAngle) * (rightNegative ? -1 : 1);
            int index = up ? count + 1 : count + 2;
            int length = up ? count : MotherBrainBeamDefinitions.EndLine - originY;
            int left = origin, rightEdge = origin;
            for (int row = 0; row < length; row++, index += up ? -1 : 1)
            {
                left = Math.Clamp(left + leftStep, 0, ushort.MaxValue);
                rightEdge = Math.Clamp(rightEdge + rightStep, 0, ushort.MaxValue);
                int word = EightEight.WholeOf(left) | (rightEdge & RightScreenEdge);
                // Down-left uses zero as its off-screen sentinel; the other routines
                // test FFFF. Keep that asymmetry instead of normalizing the native edges.
                bool empty = down && leftNegative && rightNegative ? word == 0 : word == ushort.MaxValue;
                data[index] = empty ? MissedLineWord : checked((ushort)word);
            }
        }

        for (int y = MotherBrainBeamDefinitions.FirstLine; y < windows.Length; y++)
        {
            int row = y - MotherBrainBeamDefinitions.FirstLine;
            if (up && row >= count) continue;
            int index = row + DataPrefixWords;
            if (!up && row >= FirstRunLines)
                index = row - FirstRunLines + (right
                    ? RightSecondRunWord : DownSecondRunWord);
            windows[y] = new MotherBrainBeamLine(ScanlineSpanWord.Left(data[index]), ScanlineSpanWord.Right(data[index]));
        }
        return windows;

        void FillRight(int index, int direction, int length, int slope)
        {
            int edge = origin;
            for (int row = 0; row < length; row++, index += direction)
            {
                edge += slope;
                if (edge > ushort.MaxValue) break;
                data[index] = (ushort)(EightEight.WholeOf(edge) | RightScreenEdge);
            }
        }
    }

    /// <summary>The beam's absolute tangent, with the caller's angle wrapped to a byte.</summary>
    internal static int Tangent(int angle) =>
        AbsoluteTangents.Sample(unchecked((byte)angle));
}
