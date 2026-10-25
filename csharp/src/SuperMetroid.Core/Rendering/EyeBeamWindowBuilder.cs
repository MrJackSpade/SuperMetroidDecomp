using SuperMetroid.Core.Samus.Movement;

namespace SuperMetroid.Core.Rendering;

/// <summary>Bank-$91 scanline edge accumulation used by the bank-$88 scanner eye.</summary>
internal static class EyeBeamWindowBuilder
{
    /// <summary>
    /// Translates CalculateXrayHdmaTableInner and its on/off-screen branches. In
    /// particular, a zero-width diagonal is a ray, not an infinite line, and the
    /// integer endpoints are accumulated before each row is written.
    /// </summary>
    internal static ushort[] Build(Offset16<ScreenPixels> apexOnScreen, int angle, int width)
    {
        int x = apexOnScreen.X;
        int y = apexOnScreen.Y;
        var rows = new ushort[EyeWindowRenderDefinitions.TableRows];
        Array.Fill(rows, EyeWindowRenderDefinitions.EmptyWindow);
        int quarter = SnesAngle.QuarterTurn.TableIndex;
        int half = SnesAngle.HalfTurn.TableIndex;
        int threeQuarter = SnesAngle.ThreeQuarterTurn.TableIndex;
        int turn = half * 2;
        int a = angle - width;
        if (a < 0) a += turn;
        int b = angle + width;
        // The native upper edge retains the inclusive final tangent-table entry.
        if (b > turn) b -= turn;
        int ta = Tangent(a < half ? a : a - half);
        int tb = Tangent(b < half ? b : b - half);
        bool offScreen = x is < 0 or >= DisplayScreen.Width;
        int origin = unchecked((ushort)SnesEightEight.FromWhole(x));
        int apex = y - EyeWindowRenderDefinitions.ApexYOffset;
        int up = offScreen ? apex : apex - 1;

        if (width == 0 && (angle == quarter || angle == threeQuarter))
        {
            Set(rows, apex, offScreen ? EyeWindowRenderDefinitions.FullWindow :
                (ushort)(angle == quarter ? x | EyeWindowRenderDefinitions.FullRightEdge : SnesEightEight.FromWhole(x)));
        }
        else if (a < quarter && b >= quarter)
        {
            if (!offScreen) Set(rows, apex, (ushort)(x | EyeWindowRenderDefinitions.FullRightEdge));
            int left = offScreen ? origin - EyeWindowRenderDefinitions.FixedPointSpan : origin;
            Ramp(rows, up, -1, new RampEdges(left, ta, ushort.MaxValue, 1), 1);
            Ramp(rows, y, 1, new RampEdges(left, tb, ushort.MaxValue, 1), -1);
            if (offScreen) FillOffScreenMiddle(rows, up, y, EyeWindowRenderDefinitions.FixedPointSpan - origin, ta, tb);
        }
        else if (a >= half && a < threeQuarter && b >= threeQuarter)
        {
            if (!offScreen) Set(rows, apex, (ushort)SnesEightEight.FromWhole(x));
            int right = offScreen ? origin + EyeWindowRenderDefinitions.FixedPointSpan : origin;
            Ramp(rows, up, -1, new RampEdges(0, -1, right, -tb), 1);
            Ramp(rows, y, 1, new RampEdges(0, -1, right, -ta), -1);
            if (offScreen) FillOffScreenMiddle(rows, up, y, origin + 1, tb, ta);
        }
        else if (a < quarter || a >= threeQuarter)
        {
            if (!offScreen) Set(rows, apex, (ushort)(x * EyeWindowRenderDefinitions.ReplicateByte));
            bool leftPositive = a < threeQuarter;
            bool rightPositive = b < threeQuarter;
            Ramp(rows, up, -1, new RampEdges(OffsetOrigin(origin, offScreen, leftPositive), leftPositive ? ta : -ta, OffsetOrigin(origin, offScreen, rightPositive), rightPositive ? tb : -tb), rightPositive ? 0 : 1, offScreen);
            if (offScreen && !leftPositive && !rightPositive)
                PreserveOffScreenUpLeftEntry(rows, up, origin + EyeWindowRenderDefinitions.FixedPointSpan, ta, tb);
        }
        else
        {
            if (!offScreen) Set(rows, apex, (ushort)(x * EyeWindowRenderDefinitions.ReplicateByte));
            bool leftPositive = b < half;
            bool rightPositive = a < half;
            Ramp(rows, y, 1, new RampEdges(OffsetOrigin(origin, offScreen, leftPositive), leftPositive ? tb : -tb, OffsetOrigin(origin, offScreen, rightPositive), rightPositive ? ta : -ta), rightPositive ? 0 : -1, offScreen);
            if (offScreen && !leftPositive && rightPositive && tb == 0)
            {
                // Native down-split zero-gradient branch clears whole words first;
                // the increasing edge then replaces only their right byte. The
                // caller's inclusive upward clear removes the origin row as well.
                int edge = origin - EyeWindowRenderDefinitions.FixedPointSpan;
                for (int row = y; row < rows.Length; row++)
                {
                    edge += ta;
                    Set(rows, row, row == y ? EyeWindowRenderDefinitions.EmptyWindow :
                        (ushort)(Math.Clamp(edge, 0, ushort.MaxValue) & EyeWindowRenderDefinitions.FullRightEdge));
                }
            }
        }
        return rows;
    }

    /// <summary>Reads an absolute native tangent-table sample for a half-turn angle index.</summary>
    internal static int Tangent(int index)
    {
        return AbsoluteTangentDefinitions.Sample(index);
    }

    /// <summary>Shifts an off-screen ray origin by one fixed-point span toward its entering edge.</summary>
    [IndependentFlag("independent facts the ramp computes from the beam's angles: an off-screen origin, and a positively sloped edge")]
    private static int OffsetOrigin(int origin, bool offScreen, bool positive) =>
        !offScreen ? origin : origin + (positive ? -EyeWindowRenderDefinitions.FixedPointSpan : EyeWindowRenderDefinitions.FixedPointSpan);

    /// <summary>
    /// Fills scanlines between off-screen upper and lower rays as the native rectangle branch does; the rays travel
    /// <paramref name="distance"/> fixed-point units from the origin before they enter the screen.
    /// </summary>
    private static void FillOffScreenMiddle(ushort[] rows, int up, int down, int distance,
        int upperStep, int lowerStep)
    {
        int upperSteps = upperStep == 0 ? int.MaxValue : (distance + upperStep - 1) / upperStep;
        int lowerSteps = lowerStep == 0 ? int.MaxValue : (distance + lowerStep - 1) / lowerStep;
        int top = upperSteps > up + 1 ? 0 : up - upperSteps + 2;
        int bottom = lowerSteps > rows.Length - down ? rows.Length : down + lowerSteps - 2;
        // The native rectangle fill is a do/while. Even when both sloped edges
        // enter immediately, its first write occurs before comparing the limits.
        do { Set(rows, top++, EyeWindowRenderDefinitions.FullWindow); } while (top <= bottom);
    }

    /// <summary>Preserves the first entering row's native left-accumulator value in its right edge byte.</summary>
    private static void PreserveOffScreenUpLeftEntry(ushort[] rows, int start, int origin, int leftStep, int rightStep)
    {
        if (rightStep == 0) return;
        int entrySteps = (origin - EyeWindowRenderDefinitions.FixedPointSpan) / rightStep + 1;
        int entryRow = start - entrySteps + 1;
        if ((uint)entryRow >= rows.Length) return;
        int leftSteps = leftStep == 0 ? start + 1 : Math.Min(start + 1, origin / leftStep + 1);
        int finalLeft = unchecked((ushort)(origin - leftSteps * leftStep));
        // The first right-edge write in native $91:C234 reads DP_Temp23 (the
        // completed LEFT accumulator), not DP_Temp25. Subsequent rows use the
        // right accumulator. Preserve this cartridge quirk rather than correcting
        // its one-row discontinuity with mathematically ideal cone geometry.
        rows[entryRow] = (ushort)((SnesLittleEndian.LowByte(rows[entryRow])) |
            (finalLeft & EyeWindowRenderDefinitions.FullRightEdge));
    }

    /// <summary>Writes one encoded scanline window, a sixteen-bit table word, if its row falls inside the HDMA table.</summary>
    private static void Set(ushort[] rows, int y, int window)
    {
        if ((uint)y < rows.Length) rows[y] = checked((ushort)window);
    }

    /// <summary>
    /// Saturation applies to the output bytes, not to the accumulators. The native
    /// early-exit adjustment can erase the final saturated row in the side-facing
    /// branches; preserving that detail avoids a one-pixel tail on the cone.
    /// </summary>
    private static void Ramp(ushort[] rows, int y, int direction, RampEdges edges, int finishAdjustment,
        bool offScreenDiagonal = false)
    {
        int left = edges.Left, leftStep = edges.LeftStep, right = edges.Right, rightStep = edges.RightStep;
        while (direction < 0 ? y >= 0 : y < rows.Length)
        {
            left += leftStep;
            right += rightStep;
            bool leftOutside = left is < 0 or > ushort.MaxValue;
            bool rightOutside = right is < 0 or > ushort.MaxValue;
            int window = (right < 0 && rightStep >= 0) || (left > ushort.MaxValue && leftStep <= 0) ? EyeWindowRenderDefinitions.EmptyWindow :
                (ushort)(SnesEightEight.WholeOf(Math.Clamp(left, 0, ushort.MaxValue)) |
                         (Math.Clamp(right, 0, ushort.MaxValue) & EyeWindowRenderDefinitions.FullRightEdge));
            Set(rows, y, window);
            if (!offScreenDiagonal && leftOutside && rightOutside && (leftStep ^ left) >= 0 && (rightStep ^ right) >= 0)
            {
                // Other rows were initialized empty. Only this row can have been
                // written before the cartridge's adjusted empty-fill starts.
                if (finishAdjustment + direction == 0)
                    Set(rows, y, EyeWindowRenderDefinitions.EmptyWindow);
                break;
            }
            y += direction;
        }
    }

    /// <summary>A ramp's starting left and right edges and how far each moves per row (#633).</summary>
    private readonly record struct RampEdges(int Left, int LeftStep, int Right, int RightStep);
}
