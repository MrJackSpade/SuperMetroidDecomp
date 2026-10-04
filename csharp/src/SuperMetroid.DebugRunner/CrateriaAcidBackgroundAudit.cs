using System.Reflection;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Rendering;

internal static class CrateriaAcidBackgroundAudit
{
    internal static int Run(string installationRoot)
    {
        var installation = new GameInstallation(installationRoot);
        var bus = installation.OpenRuntimeAddressSpace();
        var game = new SuperMetroidGame(bus, new() { Invincibility = true }, renderGameplayFrames: false);
        InstalledInputReplay.Bind(game, installation);
        typeof(SuperMetroidGame).GetMethod("CreateGameplayRuntime", BindingFlags.Instance | BindingFlags.NonPublic)!
            .Invoke(game, [false]);
        var runtime = game.RuntimeForVerification!;
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.InitializeStartingCeresRoom(); runtime.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(0x93aa);
        var samus = runtime.Samus!;
        samus.Pose = SamusPoseIds.FacingRightNormalPose;
        samus.InitializeAnimation(bus);
        ushort[] ReadVisibleRock() => Enumerable.Range(0, 2).SelectMany(page =>
            Enumerable.Range(4 * 32, 24 * 32).Select(index => runtime.Vram.ReadWord(0x4800 + page * 0x400 + index))).ToArray();
        ushort[]? visibleRock = null;
        ushort? horizontal = null;
        int low = int.MaxValue, high = int.MinValue;
        for (int frame = 0; frame < 512; frame++)
        {
            samus.XPosition = 384; samus.YPosition = 80;
            runtime.Camera!.SetPosition(256, 0);
            runtime.StepFrame(0);
            if (frame < 2) continue;
            var layer = (OrdinaryGameplayRenderLayer)GameplayDisplayCapture.CaptureOrdinaryBase(runtime).Layers[0];
            if (!layer.HorizontalScrolls.IsEmpty)
                throw new InvalidDataException($"#1178: rock background received sky horizontal HDMA at frame {frame}; first scroll={layer.HorizontalScrolls[0]}, map={layer.Registers.Bg2WidthTiles}x{layer.Registers.Bg2HeightTiles}.");
            visibleRock ??= ReadVisibleRock();
            Require(ReadVisibleRock().SequenceEqual(visibleRock), "updater preserves visible rock tilemap rows");
            horizontal ??= layer.Registers.Bg2X;
            Require(layer.Registers.Bg2X == horizontal, "stationary background X");
            Require(layer.Registers.Bg2WidthTiles == 64 && layer.Registers.Bg2HeightTiles == 32, "ordinary rock tilemap geometry");
            low = Math.Min(low, runtime.RoomLayer3Fx.CurrentYPosition);
            high = Math.Max(high, runtime.RoomLayer3Fx.CurrentYPosition);
        }
        Require(high > low, "acid tide continues");
        Console.WriteLine($"#1178: 512 frames, stationary BG2 X={horizontal:X4}; no horizontal HDMA; acid Y={low}..{high}; rock map 64x32.");
        // Control for the separated ownership: actual sky still receives its configured effect.
        runtime.LoadCartridgeRoomForDebug(0x91f8);
        for (int frame = 0; frame < 8; frame++) runtime.StepFrame(0);
        var skyLayer = (OrdinaryGameplayRenderLayer)GameplayDisplayCapture.CaptureOrdinaryBase(runtime).Layers[0];
        Require(!skyLayer.HorizontalScrolls.IsEmpty && skyLayer.Registers.Bg2WidthTiles == 32 && skyLayer.Registers.Bg2HeightTiles == 64,
            "Landing Site retains real sky HDMA and vertical tilemap");
        return 0;
    }
    private static void Require(bool condition, string property)
    {
        if (!condition) throw new InvalidDataException("#1178 failed: " + property);
    }
}

