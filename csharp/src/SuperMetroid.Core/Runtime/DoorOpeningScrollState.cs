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
        DoorDirection direction,
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

    public DoorDirection Direction { get; }
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
        DoorDirection direction = door.Orientation.Direction;
        uint samusStep = GetSamusStep(door);
        (sourceSamusXFixed, sourceSamusYFixed) = ApplySetupMovement(
            door, sourceSamusXFixed, sourceSamusYFixed);
        ushort destinationX = unchecked((ushort)(door.DestinationScreenX << 8));
        ushort destinationY = unchecked((ushort)(door.DestinationScreenY << 8));

        var setupCamera = GetSetupCamera(door);
        ushort cameraX = setupCamera.X;
        ushort cameraY = setupCamera.Y;
        ushort layer2X = finalLayer2X;
        ushort layer2Y = finalLayer2Y;
        (uint samusX, uint samusY) = RebaseSamus(door, sourceSamusXFixed, sourceSamusYFixed);
        int remainingFrames;

        switch (direction)
        {
            case DoorDirection.Right: // Right setup calls DoorTransition_Right once before placement.
                cameraX = unchecked((ushort)(destinationX - 252));
                layer2X = unchecked((ushort)(finalLayer2X - 252));
                remainingFrames = 63;
                break;

            case DoorDirection.Left: // Left setup is the exact subtracting mirror.
                cameraX = unchecked((ushort)(destinationX + 252));
                layer2X = unchecked((ushort)(finalLayer2X + 252));
                remainingFrames = 63;
                break;

            case DoorDirection.Down: // Down frame zero only stages the off-screen row.
                cameraY = unchecked((ushort)(destinationY - 224));
                layer2Y = unchecked((ushort)(finalLayer2Y - 224));
                remainingFrames = 56;
                break;

            case DoorDirection.Up: // FixDoorsMovingUp leaves counter one for setup's first moving call.
                cameraY = unchecked((ushort)(destinationY + 251));
                layer2Y = unchecked((ushort)(finalLayer2Y + 220));
                remainingFrames = 55;
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(door), direction, "Undefined door direction.");
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

    /// <summary>Layer-one origin after the directional setup's initial call.</summary>
    internal static (ushort X, ushort Y) GetSetupCamera(CartridgeDoorHeader door)
    {
        ushort x = unchecked((ushort)(door.DestinationScreenX << 8));
        ushort y = unchecked((ushort)(door.DestinationScreenY << 8));
        return door.Orientation.Direction switch
        {
            DoorDirection.Right => (unchecked((ushort)(x - 252)), y),
            DoorDirection.Left => (unchecked((ushort)(x + 252)), y),
            DoorDirection.Down => (x, unchecked((ushort)(y - 224))),
            DoorDirection.Up => (x, unchecked((ushort)(y + 251))),
            _ => throw new ArgumentOutOfRangeException(nameof(door), door.Orientation.Direction, "Undefined door direction."),
        };
    }

    /// <summary>
    /// <c>$82:E3C0</c> replaces both whole coordinates with layer one plus the
    /// low position byte, preserving the fractions and deferring final nudges.
    /// </summary>
    internal static (uint X, uint Y) RebaseSamus(CartridgeDoorHeader door, uint setupX, uint setupY)
    {
        var camera = GetSetupCamera(door);
        return (
            ReplaceWholePosition(unchecked((ushort)(camera.X + (byte)(setupX >> 16))), setupX),
            ReplaceWholePosition(unchecked((ushort)(camera.Y + (byte)(setupY >> 16))), setupY));
    }

    /// <summary>One moving IRQ call; downward setup alone omits this displacement.</summary>
    internal static (uint X, uint Y) AdvanceSamus(CartridgeDoorHeader door, uint x, uint y)
    {
        uint step = GetSamusStep(door);
        return door.Orientation.Direction switch
        {
            DoorDirection.Right => (unchecked(x + step), y),
            DoorDirection.Left => (unchecked(x - step), y),
            DoorDirection.Down => (x, unchecked(y + step)),
            DoorDirection.Up => (x, unchecked(y - step)),
            _ => throw new ArgumentOutOfRangeException(nameof(door), door.Orientation.Direction, "Undefined door direction."),
        };
    }

    /// <summary>Samus's fixed-point displacement for one native door IRQ call.</summary>
    internal static uint GetSamusStep(CartridgeDoorHeader door)
    {
        int distance = unchecked((short)door.SamusDistance);
        if (distance < 0)
            distance = door.Orientation.IsVertical ? 384 : 200;
        return unchecked((uint)(distance << 8));
    }

    /// <summary>
    /// Applies setup's first directional call before <c>$82:E3C0</c> replaces the
    /// whole position words. Downward setup stages a row without moving Samus.
    /// </summary>
    internal static (uint X, uint Y) ApplySetupMovement(
        CartridgeDoorHeader door, uint sourceX, uint sourceY)
    {
        return door.Orientation.Direction == DoorDirection.Down ? (sourceX, sourceY) : AdvanceSamus(door, sourceX, sourceY);
    }

    /// <summary>Runs one IRQ call and reports the frame that sets completion bit $8000.</summary>
    public bool Advance()
    {
        if (RemainingFrames <= 0)
            return true;

        int frameCounter = Direction == DoorDirection.Up ? 57 - RemainingFrames : 0;
        int cameraDelta = Direction switch
        {
            DoorDirection.Right or DoorDirection.Down => 4,
            DoorDirection.Left or DoorDirection.Up => -4,
            _ => throw new InvalidOperationException($"Undefined door direction {Direction}."),
        };
        if (!Direction.IsVertical())
        {
            CameraX = unchecked((ushort)(CameraX + cameraDelta));
            Layer2X = unchecked((ushort)(Layer2X + cameraDelta));
            SamusXFixed = Direction == DoorDirection.Right
                ? unchecked(SamusXFixed + SamusStep)
                : unchecked(SamusXFixed - SamusStep);
        }
        else
        {
            CameraY = unchecked((ushort)(CameraY + cameraDelta));
            Layer2Y = unchecked((ushort)(Layer2Y + cameraDelta));
            SamusYFixed = Direction == DoorDirection.Down
                ? unchecked(SamusYFixed + SamusStep)
                : unchecked(SamusYFixed - SamusStep);
        }

        RemainingFrames--;
        ShouldStreamAfterAdvance = Direction != DoorDirection.Up || frameCounter >= 5;
        return RemainingFrames == 0;
    }

    /// <summary>
    /// Applies the door loader's <c>PlaceSamusOnElevator</c> ($A3:9612): whole X, and Y
    /// with a zero fraction. Later IRQ calls move Samus on from this position.
    /// </summary>
    public void PlaceSamusOnElevator(ushort xPosition, ushort yPosition)
    {
        SamusXFixed = ((uint)xPosition << 16) | (SamusXFixed & 0xffff);
        SamusYFixed = (uint)yPosition << 16;
    }

    /// <summary>
    /// Applies <c>Irq_FollowDoorTransition</c>'s layer-one destination snap after the
    /// directional routine has produced its final background-stream request.
    /// </summary>
    public void SnapLayerOneToDestination()
    {
        if (RemainingFrames != 0)
            throw new InvalidOperationException(
                "A door-opening trajectory cannot snap before its final IRQ call.");

        // The cartridge calls $80:A3A0 from DoorTransition_* before control returns to
        // Irq_FollowDoorTransition. Only then does the wrapper replace $0911/$0915 with
        // the exact door destination. Layer two reaches its endpoint through the final
        // directional step and is not rewritten by the wrapper.
        CameraX = FinalCameraX;
        CameraY = FinalCameraY;
    }

    private static uint ReplaceWholePosition(ushort whole, uint fixedPosition) =>
        ((uint)whole << 16) | (fixedPosition & 0xffff);
}

/// <summary>A door loader's deferred write of Samus's whole position.</summary>
internal readonly record struct LoaderSamusPlacement(int EnemySlot, ushort XPosition, ushort YPosition);
