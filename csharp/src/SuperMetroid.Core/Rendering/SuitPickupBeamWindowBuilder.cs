using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Core.Rendering;

/// <summary>Render-side rasterization of the suit-pickup light beam's published shape.</summary>
internal static class SuitPickupBeamWindowBuilder
{
    /// <summary>
    /// Returns one packed endpoint word per beam scanline: low byte is the inclusive left
    /// endpoint, high byte the inclusive right endpoint, and an inverted word is empty.
    /// </summary>
    internal static ushort[] Build(SamusSuitPickupBeam beam)
    {
        int lines = SamusSpecialSequenceRomData.SuitPickup.WindowScanlineCount;
        ushort empty = SamusSpecialSequenceRomData.SuitPickup.EmptyWindowEndpoints;
        var windows = new ushort[lines];
        Array.Fill(windows, empty);
        // Window registers hold eight-bit columns; the beam's columns are always 0 through 255.
        byte left = checked((byte)beam.Left.X);
        byte right = checked((byte)beam.Right.X);
        ushort extent = SnesLittleEndian.Word(left, right);
        switch (beam.Shape)
        {
            case SamusSuitPickupBeamShape.None:
                break;
            case SamusSuitPickupBeamShape.GrowingFromEdges:
                for (int i = 0; i < Math.Min(beam.Progress, lines); i++)
                {
                    windows[i] = extent;
                    windows[lines - 1 - i] = extent;
                }
                break;
            case SamusSuitPickupBeamShape.Column:
                Array.Fill(windows, extent);
                break;
            case SamusSuitPickupBeamShape.CurvedColumn:
                FillCurvedColumn(windows, left, right);
                break;
            case SamusSuitPickupBeamShape.ClosingCurvedColumn:
                FillCurvedColumn(windows, left, right);
                // The upper wipe always clears at least the first scanline; the lower wipe
                // is one scanline shorter, leaving the center line for the final stage.
                for (int i = 0; i < Math.Min(Math.Max(1, beam.Progress), lines); i++)
                    windows[i] = empty;
                for (int i = 0; i < Math.Min(Math.Max(1, beam.Progress - 1), lines); i++)
                    windows[lines - 1 - i] = empty;
                break;
            case SamusSuitPickupBeamShape.CenterLine:
                windows[lines / 2] = extent;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(beam), beam.Shape, "Unknown suit-pickup beam shape.");
        }
        return windows;
    }

    /// <summary>
    /// Widens the column by the authored curve, read forward for the top half and backward for
    /// the bottom half. The left edge clamps at zero through a signed test and the right edge
    /// saturates at the screen edge, matching the asymmetric native clamps.
    /// </summary>
    private static void FillCurvedColumn(ushort[] windows, byte left, byte right)
    {
        for (int scanline = 0; scanline < windows.Length; scanline++)
        {
            int curveIndex = scanline < windows.Length / 2 ? scanline : windows.Length - 1 - scanline;
            byte curve = SuitPickupBeamCurveDefinitions.OffsetAt(curveIndex);
            int candidateLeft = left - curve;
            byte curvedLeft = unchecked((sbyte)candidateLeft) < 0 ? (byte)0 : unchecked((byte)candidateLeft);
            int candidateRight = right + curve;
            byte curvedRight = candidateRight > byte.MaxValue ? byte.MaxValue : unchecked((byte)candidateRight);
            windows[scanline] = unchecked(SnesLittleEndian.Word(curvedLeft, curvedRight));
        }
    }
}
