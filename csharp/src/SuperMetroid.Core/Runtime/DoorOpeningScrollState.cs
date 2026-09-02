using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Runtime;

/// <summary>
/// Pure coordinate owner for <c>$80:AD30-$80:AFC0</c>'s four IRQ door trajectories.
/// </summary>
/// <remarks>
/// Keeping the arithmetic independent of room loading makes every directional trace
/// exhaustively testable. The runtime merely publishes these words to Camera, Samus, and
/// the background-scroll mirrors; it never substitutes host interpolation.
/// </remarks>
internal sealed class DoorOpeningScrollState
{
    private DoorOpeningScrollState(
        int direction,
        uint samusStep,
        int remainingFrames,
        ushort cameraX,
        ushort cameraY,
        ushort layer2X,
        ushort layer2Y,
        uint samusXFixed,
        uint samusYFixed,
        ushort finalCameraX,
        ushort finalCameraY,
        ushort finalLayer2X,
        ushort finalLayer2Y,
        uint finalSamusXFixed,
        uint finalSamusYFixed)
    {
        Direction = direction;
        SamusStep = samusStep;
        RemainingFrames = remainingFrames;
        CameraX = cameraX;
        CameraY = cameraY;
        Layer2X = layer2X;
        Layer2Y = layer2Y;
        SamusXFixed = samusXFixed;
        SamusYFixed = samusYFixed;
        FinalCameraX = finalCameraX;
        FinalCameraY = finalCameraY;
        FinalLayer2X = finalLayer2X;
        FinalLayer2Y = finalLayer2Y;
        FinalSamusXFixed = finalSamusXFixed;
        FinalSamusYFixed = finalSamusYFixed;
    }

    public int Direction { get; }
    public uint SamusStep { get; }
    public int RemainingFrames { get; private set; }
    public ushort CameraX { get; private set; }
    public ushort CameraY { get; private set; }
    public ushort Layer2X { get; private set; }
    public ushort Layer2Y { get; private set; }
    public uint SamusXFixed { get; private set; }
    public uint SamusYFixed { get; private set; }
    public ushort FinalCameraX { get; }
    public ushort FinalCameraY { get; }
    public ushort FinalLayer2X { get; }
    public ushort FinalLayer2Y { get; }
    public uint FinalSamusXFixed { get; }
    public uint FinalSamusYFixed { get; }
    public bool ShouldStreamAfterAdvance { get; private set; } = true;

    public static DoorOpeningScrollState Create(
        CartridgeDoorHeader door,
        uint sourceSamusXFixed,
        uint sourceSamusYFixed,
        ushort finalCameraX,
        ushort finalCameraY,
        ushort finalLayer2X,
        ushort finalLayer2Y,
        uint finalSamusXFixed,
        uint finalSamusYFixed)
    {
        int direction = door.Orientation & 3;
        int distance = unchecked((short)door.SamusDistance);
        if (distance < 0)
            distance = (direction & 2) != 0 ? 384 : 200;
        uint samusStep = unchecked((uint)(distance << 8));
        ushort destinationX = unchecked((ushort)(door.DestinationScreenX << 8));
        ushort destinationY = unchecked((ushort)(door.DestinationScreenY << 8));

        ushort cameraX = finalCameraX;
        ushort cameraY = finalCameraY;
        ushort layer2X = finalLayer2X;
        ushort layer2Y = finalLayer2Y;
        uint samusX = finalSamusXFixed;
        uint samusY = finalSamusYFixed;
        int remainingFrames;

        switch (direction)
        {
            case 0: // Right setup calls DoorTransition_Right once before placement.
                sourceSamusXFixed = unchecked(sourceSamusXFixed + samusStep);
                cameraX = unchecked((ushort)(destinationX - 252));
                layer2X = unchecked((ushort)(finalLayer2X - 252));
                samusX = ReplaceWholePosition(
                    unchecked((ushort)(cameraX + (byte)(sourceSamusXFixed >> 16))),
                    sourceSamusXFixed);
                remainingFrames = 63;
                break;

            case 1: // Left setup is the exact subtracting mirror.
                sourceSamusXFixed = unchecked(sourceSamusXFixed - samusStep);
                cameraX = unchecked((ushort)(destinationX + 252));
                layer2X = unchecked((ushort)(finalLayer2X + 252));
                samusX = ReplaceWholePosition(
                    unchecked((ushort)(cameraX + (byte)(sourceSamusXFixed >> 16))),
                    sourceSamusXFixed);
                remainingFrames = 63;
                break;

            case 2: // Down frame zero only stages the off-screen row.
                cameraY = unchecked((ushort)(destinationY - 224));
                layer2Y = unchecked((ushort)(finalLayer2Y - 224));
                samusY = ReplaceWholePosition(
                    unchecked((ushort)(cameraY + (byte)(sourceSamusYFixed >> 16))),
                    sourceSamusYFixed);
                remainingFrames = 56;
                break;

            case 3: // FixDoorsMovingUp leaves counter one for setup's first moving call.
                sourceSamusYFixed = unchecked(sourceSamusYFixed - samusStep);
                cameraY = unchecked((ushort)(destinationY + 251));
                layer2Y = unchecked((ushort)(finalLayer2Y + 220));
                samusY = ReplaceWholePosition(
                    unchecked((ushort)(cameraY + (byte)(sourceSamusYFixed >> 16))),
                    sourceSamusYFixed);
                remainingFrames = 55;
                break;

            default:
                throw new InvalidOperationException($"Invalid door direction {direction}.");
        }

        return new DoorOpeningScrollState(
            direction,
            samusStep,
            remainingFrames,
            cameraX,
            cameraY,
            layer2X,
            layer2Y,
            samusX,
            samusY,
            finalCameraX,
            finalCameraY,
            finalLayer2X,
            finalLayer2Y,
            finalSamusXFixed,
            finalSamusYFixed);
    }

    /// <summary>Runs one IRQ call and reports the frame that sets completion bit $8000.</summary>
    public bool Advance()
    {
        if (RemainingFrames <= 0)
            return true;

        int frameCounter = Direction == 3 ? 57 - RemainingFrames : 0;
        int cameraDelta = Direction switch
        {
            0 or 2 => 4,
            1 or 3 => -4,
            _ => throw new InvalidOperationException($"Invalid door direction {Direction}."),
        };
        if ((Direction & 2) == 0)
        {
            CameraX = unchecked((ushort)(CameraX + cameraDelta));
            Layer2X = unchecked((ushort)(Layer2X + cameraDelta));
            SamusXFixed = Direction == 0
                ? unchecked(SamusXFixed + SamusStep)
                : unchecked(SamusXFixed - SamusStep);
        }
        else
        {
            CameraY = unchecked((ushort)(CameraY + cameraDelta));
            Layer2Y = unchecked((ushort)(Layer2Y + cameraDelta));
            SamusYFixed = Direction == 2
                ? unchecked(SamusYFixed + SamusStep)
                : unchecked(SamusYFixed - SamusStep);
        }

        RemainingFrames--;
        ShouldStreamAfterAdvance = Direction != 3 || frameCounter >= 5;
        if (RemainingFrames != 0)
            return false;

        // Irq_FollowDoorTransition snaps layer one to the destination after the direction
        // function returns carry set. Layer two has already reached its authored endpoint.
        CameraX = FinalCameraX;
        CameraY = FinalCameraY;
        Layer2X = FinalLayer2X;
        Layer2Y = FinalLayer2Y;
        return true;
    }

    private static uint ReplaceWholePosition(ushort whole, uint fixedPosition) =>
        ((uint)whole << 16) | (fixedPosition & 0xffff);
}
