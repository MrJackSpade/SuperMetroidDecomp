using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Core.Rendering;

/// <summary>Render-side rasterization of the suit-pickup light beam's published shape.</summary>
internal static class SuitPickupBeamWindowBuilder
{
    /// <summary>
    /// Returns one packed endpoint word per beam screen line: low byte is the inclusive left
    /// endpoint, high byte the inclusive right endpoint, and an inverted word is empty.
    /// </summary>
    internal static ushort[] Build(SamusSuitPickupBeam beam)
    {
        int lines = SuitPickupBeamDefinitions.WindowScanlineCount;
        var windows = new ushort[lines];
        Array.Fill(windows, SuitPickupBeamDefinitions.EmptyWindow.Word);
        // Each edge is an eight-bit column; the beam's columns are always 0 through 255.
        SuitPickupBeamEdges column = new(checked((byte)beam.Left.X), checked((byte)beam.Right.X));
        switch (beam.Shape)
        {
            case SamusSuitPickupBeamShape.None:
                break;
            case SamusSuitPickupBeamShape.GrowingFromEdges:
                for (int i = 0; i < Math.Min(beam.Progress, lines); i++)
                {
                    windows[i] = column.Word;
                    windows[lines - 1 - i] = column.Word;
                }
                break;
            case SamusSuitPickupBeamShape.Column:
                Array.Fill(windows, column.Word);
                break;
            case SamusSuitPickupBeamShape.CurvedColumn:
                FillCurvedColumn(windows, column);
                break;
            case SamusSuitPickupBeamShape.ClosingCurvedColumn:
                FillCurvedColumn(windows, column);
                // The upper wipe always clears at least the first screen line; the lower wipe
                // is one screen line shorter, leaving the center line for the final stage.
                for (int i = 0; i < Math.Min(Math.Max(1, beam.Progress), lines); i++)
                    windows[i] = SuitPickupBeamDefinitions.EmptyWindow.Word;
                for (int i = 0; i < Math.Min(Math.Max(1, beam.Progress - 1), lines); i++)
                    windows[lines - 1 - i] = SuitPickupBeamDefinitions.EmptyWindow.Word;
                break;
            case SamusSuitPickupBeamShape.CenterLine:
                windows[lines / 2] = column.Word;
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
    private static void FillCurvedColumn(ushort[] windows, SuitPickupBeamEdges column)
    {
        for (int scanline = 0; scanline < windows.Length; scanline++)
        {
            int curveIndex = scanline < windows.Length / 2 ? scanline : windows.Length - 1 - scanline;
            int curve = SuitPickupBeamCurve.OffsetAt(curveIndex);
            int candidateLeft = column.Left - curve;
            int candidateRight = column.Right + curve;
            windows[scanline] = new SuitPickupBeamEdges(
                unchecked((sbyte)candidateLeft) < 0 ? (byte)0 : unchecked((byte)candidateLeft),
                candidateRight > byte.MaxValue ? byte.MaxValue : unchecked((byte)candidateRight)).Word;
        }
    }
}
