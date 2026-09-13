using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

/// <summary>Retail setup, resident event producer, and fresh door-load state selection.</summary>
internal static class ShaktoolReentryAudit
{
    public static int Run(string rom)
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
        var runtime = FlatFloorMovementFixture.Create(bus, false);
        var entry = CartridgeDoorHeader.Load(bus, ShaktoolReentryDefinitions.FromSpringBall);
        runtime.LoadCartridgeRoomThroughDoorForVerification(entry);
        if (!runtime.Camera!.Scrolls.Storage[..4].SequenceEqual(new byte[] { 1, 0, 0, 0 }))
            throw new InvalidDataException("Shaktool setup did not install native scroll boundaries.");
        int sandBefore = runtime.LevelData!.ForegroundEntries.ToArray().Count(w => w == ShaktoolReentryDefinitions.SandWord);
        runtime.Samus!.XPosition = ShaktoolReentryDefinitions.EventBoundary;
        void StepPlms() => runtime.Plms.Step(bus, runtime.LevelData!, runtime.LevelData!.CreateBackgroundStreamer(),
            0, 0, 0, runtime.Camera!.Scrolls, 0, 0, 0);
        StepPlms();
        StepPlms();
        if (runtime.System.HasEvent(EventNumber.ShaktoolClearedPath))
            throw new InvalidDataException("Shaktool event fired at inclusive threshold instead of strictly beyond it.");
        foreach (ushort status in new ushort[] { 0, 1, 0x4000, 0x8000, 0xffff })
        {
            for (int cell = 0; cell < 4; cell++) runtime.Camera.Scrolls.SetStorage(cell, RoomScrollState.RedBoundary);
            runtime.Plms.Step(bus, runtime.LevelData!, runtime.LevelData!.CreateBackgroundStreamer(),
                0, 0, 0, runtime.Camera.Scrolls, 0, 0, 0, powerBombExplosionStatus: status);
            if (runtime.Camera.Scrolls.Storage[..4].ToArray().Any(value => value != (status == 0 ? 0 : 1)))
                throw new InvalidDataException($"Shaktool scroll callback mishandled status {status:X4}.");
        }
        runtime.Samus.InputLocked = true;
        for (int cell = 0; cell < 4; cell++) runtime.Camera.Scrolls.SetStorage(cell, RoomScrollState.RedBoundary);
        runtime.BombProjectiles.PowerBombExplosion.Arm();
        runtime.BombProjectiles.PowerBombExplosion.Spawn(runtime.Samus.XPosition, runtime.Samus.YPosition);
        runtime.StepFrame(0);
        if (runtime.Camera.Scrolls.Storage[..4].ToArray().Any(value => value != 1))
            throw new InvalidDataException("Gameplay did not hand the Power Bomb status to the room controller.");
        runtime.Samus.XPosition++;
        runtime.StepFrame(0);
        bool marked = runtime.System.HasEvent(EventNumber.ShaktoolClearedPath);
        runtime.LoadCartridgeRoomThroughDoorForVerification(CartridgeDoorHeader.Load(bus, ShaktoolReentryDefinitions.ToSpringBall));
        runtime.LoadCartridgeRoomThroughDoorForVerification(entry);
        int sandAfter = runtime.LevelData!.ForegroundEntries.ToArray().Count(w => w == ShaktoolReentryDefinitions.SandWord);
        Console.WriteLine($"Shaktool reentry: event={marked}, state={runtime.ActiveRoom!.State.Pointer:X4}, sand={sandBefore}->{sandAfter}");
        if (!marked || sandBefore != 216 || sandAfter != 0 || runtime.ActiveRoom.State.Pointer != ShaktoolReentryDefinitions.ClearedState)
            throw new InvalidDataException("Crossed Shaktool passage did not persist through retail room exit and re-entry.");
        if (runtime.Plms.HasActiveHeader(ShaktoolRoomPlmRomData.Header))
            throw new InvalidDataException("Cleared room incorrectly respawned its one-shot controller.");
        var saturated = new RoomPlmSystem();
        for (int index = 0; index < 40; index++)
            if (!saturated.TrySpawnCollisionBombBlock(runtime.LevelData, index, behavior: 0))
                throw new InvalidDataException("Could not fill the native PLM pool.");
        byte[] previousScrolls = runtime.Camera.Scrolls.Storage.ToArray();
        if (saturated.TrySpawnShaktoolRoomController(runtime.Camera.Scrolls) ||
            !previousScrolls.AsSpan().SequenceEqual(runtime.Camera.Scrolls.Storage))
            throw new InvalidDataException("Failed hardcoded allocation altered room scrolls.");
        return 0;
    }
}

internal static class ShaktoolReentryDefinitions
{
    /// <summary>$83:A7C8 Door_Springball_0 enters Shaktool's room.</summary>
    public const ushort FromSpringBall = 0xa7c8;
    /// <summary>$83:A8D0 Door_Shaktool_1 exits into Spring Ball's room.</summary>
    public const ushort ToSpringBall = 0xa8d0;
    /// <summary>$8F:D8F1 RoomState_Shaktool_1 selects the cleared level.</summary>
    public const ushort ClearedState = 0xd8f1;
    /// <summary>$84:B8C3 tests Samus X strictly above $0348.</summary>
    public const ushort EventBoundary = 0x348;
    /// <summary>Authored breakable sand in the default Shaktool level.</summary>
    public const ushort SandWord = 0xa110;
}
