using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

/// <summary>Cartridge identities used by the integrated Power Bomb render audit.</summary>
internal static class PowerBombRuntimeAuditDefinitions
{
    /// <summary>
    /// Alpha Power Bomb room <c>$01/$26</c> at <c>$8F:A3AE</c>, where the player recording
    /// placed two Power Bombs while validating issue #288.
    /// </summary>
    public const ushort AlphaPowerBombRoomHeader = 0xa3ae;

    /// <summary>Screen-space X center used by the synthetic active HDMA owner.</summary>
    public const ushort ScreenCenterX = 0x0080;

    /// <summary>Screen-space Y center below the 32-pixel gameplay HUD.</summary>
    public const ushort ScreenCenterY = 0x0080;
}

/// <summary>
/// ROM-backed integration regression for issue #288. The lower-level bank-$88 state and
/// color-math compositor already have exhaustive geometry tests; this audit proves that
/// the same compositor is actually present in the shared renderer used by the desktop.
/// </summary>
internal static class PowerBombRuntimeAudit
{
    public static int Run(string romPath)
    {
        SuperMetroidAddressSpace bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        CartridgeRoomHeader room = CartridgeRoomHeader.Load(
            bus,
            PowerBombRuntimeAuditDefinitions.AlphaPowerBombRoomHeader);
        if (room.Identity != new RoomIdentity(AreaId.Brinstar, 0x26))
        {
            throw new InvalidDataException(
                $"Power Bomb audit selected {room.Identity}, expected $01/$26.");
        }

        var runtime = new SuperMetroidRuntime(bus);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RunNmi(controller1Input: 0, mainLoopRequestedNmi: true);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(
            PowerBombRuntimeAuditDefinitions.AlphaPowerBombRoomHeader,
            cameraX: 0,
            cameraY: 0);
        runtime.RunNmi(controller1Input: 0, mainLoopRequestedNmi: true);

        Rgba32[] baseline = SuperMetroidRuntimeFrameRenderer.Render(runtime);
        SamusPowerBombExplosionState explosion =
            runtime.BombProjectiles.PowerBombExplosion;
        explosion.Arm();
        explosion.Spawn(
            PowerBombRuntimeAuditDefinitions.ScreenCenterX,
            PowerBombRuntimeAuditDefinitions.ScreenCenterY);
        explosion.StepFrame(bus);

        GameplayPpuRenderSnapshot ppu = runtime.DisplayedGameplayPpu;
        Rgba32[] expected = baseline.ToArray();
        SnesGameplayFrameRenderer.ApplyPowerBombColorMath(
            expected,
            bus,
            explosion,
            ppu.Layer1XPosition,
            ppu.Layer1YPosition);
        Rgba32[] actual = SuperMetroidRuntimeFrameRenderer.Render(runtime);

        int expectedChangedPixels = CountDifferences(baseline, expected);
        int actualChangedPixels = CountDifferences(baseline, actual);
        int integrationDifferences = CountDifferences(expected, actual);
        if (expectedChangedPixels == 0 ||
            actualChangedPixels != expectedChangedPixels ||
            integrationDifferences != 0)
        {
            throw new InvalidDataException(
                "Shared gameplay renderer omitted or altered the active Power Bomb " +
                $"window: expected-changed={expectedChangedPixels}, " +
                $"actual-changed={actualChangedPixels}, mismatch={integrationDifferences}.");
        }

        Console.WriteLine(
            "Power Bomb runtime audit passed: retail room $01/$26 shared rendering " +
            $"composited all {actualChangedPixels} expected first-flash pixels.");
        return 0;
    }

    private static int CountDifferences(ReadOnlySpan<Rgba32> first, ReadOnlySpan<Rgba32> second)
    {
        if (first.Length != second.Length)
            throw new ArgumentException("Power Bomb comparison frames have different lengths.");

        int count = 0;
        for (int pixel = 0; pixel < first.Length; pixel++)
            count += first[pixel] != second[pixel] ? 1 : 0;
        return count;
    }
}
