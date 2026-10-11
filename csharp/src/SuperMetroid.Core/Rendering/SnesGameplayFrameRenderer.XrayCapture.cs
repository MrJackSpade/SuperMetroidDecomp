using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Rendering;

public static partial class SnesGameplayFrameRenderer
{
    /// <summary>Resolves the same tangent inequalities as the reference X-ray raster into immutable line intervals.</summary>
    public static XrayWindowLine[] CaptureXrayWindowLines(ISnesAddressSpace bus, SamusState samus,
        Position16<RoomPixels> layer1)
    {
        bool leftFacing = samus.IsFacingLeft(bus);
        Offset16<ScreenPixels> samusOnScreen = samus.Position.Whole.RelativeToCamera(layer1);
        int originX = samusOnScreen.X +
            (leftFacing ? -XrayWindowRenderDefinitions.EyeHorizontalOffset : XrayWindowRenderDefinitions.EyeHorizontalOffset);
        int originY = samusOnScreen.Y -
            (samus.ReadMovementType(bus) == SamusMovementType.Crouching
                ? XrayWindowRenderDefinitions.CrouchingEyeHeight : XrayWindowRenderDefinitions.StandingEyeHeight);
        int angle = samus.Xray.Angle.TableIndex;
        int width = samus.Xray.AngularWidth & 0xff;
        Offset16<ScreenPixels> a = ReadXrayDirection(angle - width), b = ReadXrayDirection(angle + width);
        bool horizontal = width == 0 && (angle == SnesAngle.QuarterTurn.TableIndex || angle == SnesAngle.ThreeQuarterTurn.TableIndex);
        var lines = new XrayWindowLine[Height];
        Array.Fill(lines, new XrayWindowLine(1, 0));
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
                if (!IntegerWindowIntersection.IntersectGreaterEqual(-a.Y, (long)a.X * dy + (long)a.Y * originX,
                        -XrayWindowRenderDefinitions.SubpixelTolerance, ref left, ref right)) continue;
                if (!IntegerWindowIntersection.IntersectGreaterEqual(b.Y, -(long)b.X * dy - (long)b.Y * originX,
                        -XrayWindowRenderDefinitions.SubpixelTolerance, ref left, ref right)) continue;
            }
            if (left <= right) lines[y] = new((byte)left, (byte)right);
        }
        return lines;
    }
}
