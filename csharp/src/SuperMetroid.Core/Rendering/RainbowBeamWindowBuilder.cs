using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Rendering;

/// <summary>Render-side rasterization of Mother Brain's rainbow-beam geometry, <c>$AD:DE7F-$E395</c>.</summary>
internal static class RainbowBeamWindowBuilder
{
    /// <summary>
    /// Returns one packed WH0/WH1 word per visible scanline: low byte is the inclusive left
    /// endpoint, high byte the inclusive right endpoint, and an inverted word is empty. The HUD
    /// lines stay empty, and a null geometry yields an entirely empty window.
    /// </summary>
    internal static ushort[] Build(MotherBrainRainbowBeamGeometry? geometry)
    {
        var windows = new ushort[SnesPpuLayout.ScreenHeightPixels];
        Array.Fill(windows, MotherBrainBeamRomData.EmptyWindow);
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
                ushort word = (ushort)(SnesEightEight.WholeOf(left) | (rightEdge & MotherBrainBeamRomData.RightScreenEdge));
                // Down-left uses zero as its off-screen sentinel; the other routines
                // test FFFF. Keep that asymmetry instead of normalizing the native edges.
                bool empty = down && leftNegative && rightNegative ? word == 0 : word == ushort.MaxValue;
                data[index] = empty ? MotherBrainBeamRomData.EmptyWindow : word;
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
            windows[y] = data[index];
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
