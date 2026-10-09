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
    /// <summary>Creates the mutable trajectory from its initial registers and retained final destinations.</summary>
    /// <param name="direction">Native door orientation in the range zero through three.</param>
    /// <param name="samusStep">Unsigned 16.16 displacement applied to Samus on each moving call.</param>
    /// <param name="remainingFrames">Number of IRQ trajectory calls still required.</param>
    /// <param name="cameraX">Initial layer-one horizontal scroll register.</param>
    /// <param name="cameraY">Initial layer-one vertical scroll register.</param>
    /// <param name="layer2X">Initial layer-two horizontal scroll register.</param>
    /// <param name="layer2Y">Initial layer-two vertical scroll register.</param>
    /// <param name="samusXFixed">Initial Samus horizontal 16.16 position.</param>
    /// <param name="samusYFixed">Initial Samus vertical 16.16 position.</param>
    /// <param name="finalCameraX">Destination layer-one horizontal register after the last call.</param>
    /// <param name="finalCameraY">Destination layer-one vertical register after the last call.</param>
    /// <param name="finalLayer2X">Expected endpoint of the layer-two horizontal trajectory.</param>
    /// <param name="finalLayer2Y">Expected endpoint of the layer-two vertical trajectory.</param>
    /// <param name="finalSamusXFixed">Final Samus horizontal fixed-point position.</param>
    /// <param name="finalSamusYFixed">Final Samus vertical fixed-point position.</param>
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

    /// <summary>Native orientation selecting the horizontal or vertical IRQ branch.</summary>
    public int Direction { get; }
    /// <summary>Fixed-point Samus displacement per moving trajectory call.</summary>
    public uint SamusStep { get; }
    /// <summary>Number of trajectory calls remaining.</summary>
    public int RemainingFrames { get; private set; }
    /// <summary>Current layer-one horizontal scroll register.</summary>
    public ushort CameraX { get; private set; }
    /// <summary>Current layer-one vertical scroll register.</summary>
    public ushort CameraY { get; private set; }
    /// <summary>Current layer-two horizontal scroll register.</summary>
    public ushort Layer2X { get; private set; }
    /// <summary>Current layer-two vertical scroll register.</summary>
    public ushort Layer2Y { get; private set; }
    /// <summary>Current Samus horizontal 16.16 position.</summary>
    public uint SamusXFixed { get; private set; }
    /// <summary>Current Samus vertical 16.16 position.</summary>
    public uint SamusYFixed { get; private set; }
    /// <summary>Exact layer-one horizontal destination retained for the final snap.</summary>
    public ushort FinalCameraX { get; }
    /// <summary>Exact layer-one vertical destination retained for the final snap.</summary>
    public ushort FinalCameraY { get; }
    /// <summary>Expected horizontal endpoint of layer two.</summary>
    public ushort FinalLayer2X { get; }
    /// <summary>Expected vertical endpoint of layer two.</summary>
    public ushort FinalLayer2Y { get; }
    /// <summary>Final Samus horizontal 16.16 destination.</summary>
    public uint FinalSamusXFixed { get; }
    /// <summary>Final Samus vertical 16.16 destination.</summary>
    public uint FinalSamusYFixed { get; }
    /// <summary>Whether room background streaming should run after the latest advance.</summary>
    public bool ShouldStreamAfterAdvance { get; private set; } = true;

    /// <summary>Creates the native directional trajectory from a loaded door and its source/destination positions.</summary>
    /// <param name="door">Loaded cartridge door header selecting orientation and movement distance.</param>
    /// <param name="sourceSamusXFixed">Samus source X position in unsigned 16.16 form.</param>
    /// <param name="sourceSamusYFixed">Samus source Y position in unsigned 16.16 form.</param>
    /// <param name="finalCameraX">Destination layer-one horizontal scroll value.</param>
    /// <param name="finalCameraY">Destination layer-one vertical scroll value.</param>
    /// <param name="finalLayer2X">Layer-two horizontal trajectory endpoint.</param>
    /// <param name="finalLayer2Y">Layer-two vertical trajectory endpoint.</param>
    /// <param name="finalSamusXFixed">Destination Samus X position in unsigned 16.16 form.</param>
    /// <param name="finalSamusYFixed">Destination Samus Y position in unsigned 16.16 form.</param>
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
            case 0: // Right setup calls DoorTransition_Right once before placement.
                cameraX = unchecked((ushort)(destinationX - 252));
                layer2X = unchecked((ushort)(finalLayer2X - 252));
                remainingFrames = 63;
                break;

            case 1: // Left setup is the exact subtracting mirror.
                cameraX = unchecked((ushort)(destinationX + 252));
                layer2X = unchecked((ushort)(finalLayer2X + 252));
                remainingFrames = 63;
                break;

            case 2: // Down frame zero only stages the off-screen row.
                cameraY = unchecked((ushort)(destinationY - 224));
                layer2Y = unchecked((ushort)(finalLayer2Y - 224));
                remainingFrames = 56;
                break;

            case 3: // FixDoorsMovingUp leaves counter one for setup's first moving call.
                cameraY = unchecked((ushort)(destinationY + 251));
                layer2Y = unchecked((ushort)(finalLayer2Y + 220));
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

    /// <summary>Layer-one origin after the directional setup's initial call.</summary>
    internal static (ushort X, ushort Y) GetSetupCamera(CartridgeDoorHeader door)
    {
        ushort x = unchecked((ushort)(door.DestinationScreenX << 8));
        ushort y = unchecked((ushort)(door.DestinationScreenY << 8));
        return (door.Orientation & 3) switch
        {
            0 => (unchecked((ushort)(x - 252)), y),
            1 => (unchecked((ushort)(x + 252)), y),
            2 => (x, unchecked((ushort)(y - 224))),
            3 => (x, unchecked((ushort)(y + 251))),
            _ => throw new InvalidOperationException("Invalid door orientation."),
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
        return (door.Orientation & 3) switch
        {
            0 => (unchecked(x + step), y),
            1 => (unchecked(x - step), y),
            2 => (x, unchecked(y + step)),
            3 => (x, unchecked(y - step)),
            _ => throw new InvalidOperationException("Invalid door orientation."),
        };
    }

    /// <summary>Samus's fixed-point displacement for one native door IRQ call.</summary>
    internal static uint GetSamusStep(CartridgeDoorHeader door)
    {
        int distance = unchecked((short)door.SamusDistance);
        if (distance < 0)
            distance = (door.Orientation & 2) != 0 ? 384 : 200;
        return unchecked((uint)(distance << 8));
    }

    /// <summary>
    /// Applies setup's first directional call before <c>$82:E3C0</c> replaces the
    /// whole position words. Downward setup stages a row without moving Samus.
    /// </summary>
    internal static (uint X, uint Y) ApplySetupMovement(
        CartridgeDoorHeader door, uint sourceX, uint sourceY)
    {
        return (door.Orientation & 3) == 2 ? (sourceX, sourceY) : AdvanceSamus(door, sourceX, sourceY);
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
/// <param name="EnemySlot">Loader enemy slot whose initialization triggers the placement.</param>
/// <param name="XPosition">Whole-pixel horizontal position written by the loader.</param>
/// <param name="YPosition">Whole-pixel vertical position written by the loader.</param>
internal readonly record struct LoaderSamusPlacement(int EnemySlot, ushort XPosition, ushort YPosition);
