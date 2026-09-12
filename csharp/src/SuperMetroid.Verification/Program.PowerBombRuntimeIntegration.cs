using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

/// <summary>Retail identities and fixture coordinates for Power Bomb renderer integration.</summary>
internal static class PowerBombRuntimeVerificationDefinitions
{
    /// <summary>
    /// Alpha Power Bomb room <c>$01/$26</c> at <c>$8F:A3AE</c>, where the recorded
    /// issue-#288 playthrough demonstrated an invisible Power Bomb explosion.
    /// </summary>
    public const ushort AlphaPowerBombRoomHeader = 0xa3ae;

    /// <summary>Screen-space X center used by the controlled active HDMA owner.</summary>
    public const ushort ScreenCenterX = 0x0080;

    /// <summary>Screen-space Y center below the 32-pixel gameplay HUD.</summary>
    public const ushort ScreenCenterY = 0x0080;
}

internal static partial class Program
{
    /// <summary>
    /// Proves that the shared runtime renderer used by the desktop includes the active
    /// bank-$88 Power Bomb color-math window, rather than merely testing that compositor
    /// in isolation.
    /// </summary>
    static void VerifyPowerBombRuntimeRendererIntegration()
    {
        string romPath = Path.GetFullPath("Super Metroid.smc");
        SuperMetroidAddressSpace bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        CartridgeRoomHeader room = CartridgeRoomHeader.Load(
            bus,
            PowerBombRuntimeVerificationDefinitions.AlphaPowerBombRoomHeader);
        AssertEqual(
            new RoomIdentity(AreaId.Brinstar, 0x26),
            room.Identity,
            "Power Bomb renderer integration retail room");

        var runtime = new SuperMetroidRuntime(bus);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RunNmi(controller1Input: 0, mainLoopRequestedNmi: true);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(
            PowerBombRuntimeVerificationDefinitions.AlphaPowerBombRoomHeader,
            cameraX: 0,
            cameraY: 0);
        runtime.RunNmi(controller1Input: 0, mainLoopRequestedNmi: true);

        Rgba32[] baseline = SuperMetroidRuntimeFrameRenderer.Render(runtime);
        SamusPowerBombExplosionState explosion = runtime.BombProjectiles.PowerBombExplosion;
        explosion.Arm();
        explosion.Spawn(
            PowerBombRuntimeVerificationDefinitions.ScreenCenterX,
            PowerBombRuntimeVerificationDefinitions.ScreenCenterY);
        explosion.StepFrame(bus);
        AssertEqual(PowerBombExplosionPhase.Inactive, explosion.RenderedPhase,
            "HDMA allocation setup does not draw the first flash prematurely");
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

        int expectedChangedPixels = CountPowerBombFrameDifferences(baseline, expected);
        AssertTrue(expectedChangedPixels > 0,
            "Power Bomb fixture produces a visible first-flash window");
        AssertEqual(
            expectedChangedPixels,
            CountPowerBombFrameDifferences(baseline, actual),
            "shared runtime renderer changes every expected Power Bomb pixel");
        AssertEqual(
            0,
            CountPowerBombFrameDifferences(expected, actual),
            "shared runtime renderer matches the bank-$88 Power Bomb compositor");

        Console.WriteLine(
            "  Power Bomb runtime: shared desktop rendering includes the active bank-$88 window.");
    }

    private static int CountPowerBombFrameDifferences(
        ReadOnlySpan<Rgba32> first,
        ReadOnlySpan<Rgba32> second)
    {
        AssertEqual(first.Length, second.Length, "Power Bomb comparison frame lengths");
        int count = 0;
        for (int pixel = 0; pixel < first.Length; pixel++)
            count += first[pixel] != second[pixel] ? 1 : 0;
        return count;
    }
}
