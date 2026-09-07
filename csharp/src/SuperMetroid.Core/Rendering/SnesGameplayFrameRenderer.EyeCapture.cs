using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Rendering;

public static partial class SnesGameplayFrameRenderer
{
    /// <summary>Resolves the published eye cone and ROM tangent table into inclusive color windows.</summary>
    public static ScanlineColorAddRenderLayer? CaptureMorphBallEyeBeam(ISnesAddressSpace bus,
        MorphBallEyeBeamRenderSnapshot beam, ushort layer1X, ushort layer1Y)
    {
        ArgumentNullException.ThrowIfNull(bus);
        if (beam.Phase is MorphBallEyeBeamPhase.Inactive or MorphBallEyeBeamPhase.PendingInitialization)
            return null;
        int originX = unchecked((short)(beam.WorldX - layer1X));
        int originY = unchecked((short)(beam.WorldY - layer1Y)) - EyeWindowRenderDefinitions.ApexYOffset;
        int angle = beam.Angle.TableIndex;
        int angularWidth = unchecked((byte)beam.AngularWidth);
        XrayDirection a = ReadXrayDirection(bus, angle - angularWidth);
        XrayDirection b = ReadXrayDirection(bus, angle + angularWidth);
        bool horizontal = angularWidth == 0 &&
            (angle == SnesAngle.QuarterTurn.TableIndex || angle == SnesAngle.ThreeQuarterTurn.TableIndex);
        var windows = new ColorAddWindow[Height];
        Array.Fill(windows, ColorAddWindow.Empty);
        byte red = ExpandFiveBit(beam.Red), green = ExpandFiveBit(beam.Green), blue = ExpandFiveBit(beam.Blue);
        for (int y = HudHeight; y < Height; y++)
        {
            long left = 0, right = Width - 1;
            if (horizontal)
            {
                if (y != originY) continue;
                if (angle == SnesAngle.QuarterTurn.TableIndex) left = Math.Max(left, originX);
                else right = Math.Min(right, originX);
            }
            else
            {
                long dy = y - originY;
                // The existing reference evaluates two cross-product inequalities per
                // pixel. For a fixed scanline each is linear in X, so their intersection
                // is an inclusive interval. Signed floor/ceiling division retains the
                // exact subpixel tolerance without a producer-side full raster scan.
                if (!IntegerWindowIntersection.IntersectGreaterEqual(-a.Y,
                    (long)a.X * dy + (long)a.Y * originX,
                    -EyeWindowRenderDefinitions.SubpixelTolerance, ref left, ref right)) continue;
                if (!IntegerWindowIntersection.IntersectGreaterEqual(b.Y,
                    -(long)b.X * dy - (long)b.Y * originX,
                    -EyeWindowRenderDefinitions.SubpixelTolerance, ref left, ref right)) continue;
            }
            if (left <= right) windows[y] = new((byte)left, (byte)right, red, green, blue);
        }
        return new(windows);
    }
}
