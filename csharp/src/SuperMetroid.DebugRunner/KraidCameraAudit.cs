using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;

/// <summary>Retail-room entry jump, exercising the live camera and enemy initialization.</summary>
internal static class KraidCameraAudit
{
    public static int Run(string rom)
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
        var runtime = FlatFloorMovementFixture.Create(bus, water: false);
        runtime.LoadCartridgeRoomThroughDoorForVerification(
            CartridgeDoorHeader.Load(bus, 0x91b6), 0, 256);
        runtime.InitializeDebugGroundedSamus(128, 190, 27);
        // Grounded debug placement computes its own viewport. Restore the door's actual
        // entry camera before the first emulated frame so the jump is the only stimulus.
        runtime.Camera!.SetPosition(0, 256);
        runtime.Samus!.InputLocked = false;
        byte[] initialScrolls = runtime.Camera!.Scrolls.Storage[..4].ToArray();
        ushort minCameraY = runtime.Camera.YPosition;
        ushort maxCameraY = minCameraY;
        ushort minSamusY = runtime.Samus.YPosition;
        ushort initialSamusY = minSamusY;
        for (int frame = 0; frame < 90; frame++)
        {
            runtime.StepFrame(frame < 35 ? (ushort)SnesButton.A : (ushort)0);
            minCameraY = Math.Min(minCameraY, runtime.Camera.YPosition);
            maxCameraY = Math.Max(maxCameraY, runtime.Camera.YPosition);
            minSamusY = Math.Min(minSamusY, runtime.Samus.YPosition);
        }
        Console.WriteLine($"Kraid entry jump: scrolls=[{string.Join(',', initialScrolls)}], " +
            $"camera minimum={minCameraY}, Samus {initialSamusY}->{minSamusY}->{runtime.Samus.YPosition}.");
        if (minSamusY >= initialSamusY)
            throw new InvalidDataException("Kraid entry fixture did not actually jump.");
        if (!initialScrolls.SequenceEqual(new byte[] { 0, 0, 1, 0 }) ||
            minCameraY != 256 || maxCameraY != 256 || runtime.Enemies.Kraid!.CameraDistanceIndex != 2)
            throw new InvalidDataException("Kraid entry jump escaped the cartridge first-phase camera lock.");
        return 0;
    }
}
