using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rom;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Runtime;
using static AssertionsVerification;
using static RetailRuntimeFixtureVerification;
using static SuiteTimingVerification;

/// <summary>Represents fireflea fx verification.</summary>
internal static class FirefleaFxVerification
{
    /// <summary>Verifies fireflea fx.</summary>
    internal static void VerifyFirefleaFx(bool includeXrayCapture = true)
    {
        var memory = new TestAddressSpace();
        var bus = new MutableCartridgeReadGuard(memory);
        var fixedColor = new FixedColorMemory(bus);
        bus.Forbid(address => address is >= 0x88B058 and < 0x88B07E,
            address => $"Fireflea FX attempted migrated definition read ${address:X6}.");
        var fx = new RoomLayer3FxState();
        var vram = new SnesVram();
        ushort record = RoomFxRecordDefinitions.All.First(record => record.Type == (byte)RoomFxType.Fireflea && record.DoorPointer == 0).Pointer;
        // Independent transcription of $88:B058/$88:B070. These are deliberately not
        // generated from the production catalog or implementation.
        ushort[] flash = [0, 0x100, 0x200, 0x300, 0x400, 0x500, 0x600, 0x500, 0x400, 0x300, 0x200, 0x100];
        ushort[] darkness = [0, 0x600, 0xC00, 0x1200, 0x1800, 0x1900, 0xC208];
        var rom = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        fx.PaletteBlendColors = SuperMetroid.Core.Assets.RoomFxPaletteBlendCatalog.Load(
            new MemoryStream(SuperMetroid.AssetExtraction.RoomFxPaletteBlendExtractor.Extract(rom)));
        Suite(nameof(VerifyFirefleaFlashingAlgorithm), () => VerifyFirefleaFlashingAlgorithm(rom));
        Suite(nameof(VerifyFirefleaDarknessAlgorithm), () => VerifyFirefleaDarknessAlgorithm(rom));
        fx.Load(bus, vram, new SnesCgram(), record, 0, 0);
        AssertEqual(6, bus.ReadByte(0x1778), "Fireflea load initializes native six-frame timer");
        AssertTrue(!fx.IsRenderable, "Fireflea darkness does not invent a BG3 texture");
        for (int frame = 1; frame <= 144; frame++)
        {
            fx.Step(bus, vram, new RoomLayer3FxFrame { Camera = Position16<RoomPixels>.Zero, TimeIsFrozen = false, MainGameLoopCarry = true });
            int expectedIndex = (frame / 6) % 12;
            int shade = SnesLittleEndian.HighByte(flash[expectedIndex]);
            AssertEqual(expectedIndex, bus.ReadByte(0x177A), $"Fireflea index frame {frame}");
            AssertEqual(shade | 0x20, fixedColor.Red, $"Fireflea red frame {frame}");
            AssertEqual(shade | 0x80, fixedColor.Green, $"Fireflea second COLDATA write frame {frame}");
            AssertEqual(shade | 0x40, fixedColor.Blue, $"Fireflea third COLDATA write frame {frame}");
        }
        byte timer = bus.ReadByte(0x1778);
        byte red = fixedColor.Red;
        for (int frame = 0; frame < 100; frame++) fx.Step(bus, vram, new RoomLayer3FxFrame { Camera = Position16<RoomPixels>.Zero, TimeIsFrozen = true, MainGameLoopCarry = true });
        AssertEqual(timer, bus.ReadByte(0x1778), "X-ray frozen time retains flashing phase");
        AssertEqual(red, fixedColor.Red, "X-ray frozen time retains fixed color");
        AssertEqual(LayerBlendingConfiguration.Fireflea, fx.LayerBlendConfiguration, "frozen Fireflea still selects blending");
        for (ushort level = 0; level <= 12; level += 2)
        {
            fx.Load(bus, vram, new SnesCgram(), record, 0, 0);
            for (int frame = 1; frame <= 78; frame++)
            {
                fx.Step(bus, vram, new RoomLayer3FxFrame { Camera = Position16<RoomPixels>.Zero, TimeIsFrozen = false, MainGameLoopCarry = true, FirefleaDarknessLevel = level });
                int index = level < 10 ? frame / 6 % 12 : frame < 6 ? 0 : 6;
                byte shade = SnesLittleEndian.HighByte((ushort)(darkness[level / 2] + flash[index]));
                AssertEqual(shade | 0x20, fixedColor.Red, $"death offset {level}, frame {frame}: red");
                AssertEqual(shade | 0x80, fixedColor.Green, $"death offset {level}, frame {frame}: blue");
                AssertEqual(shade | 0x40, fixedColor.Blue, $"death offset {level}, frame {frame}: green");
                AssertEqual(level, (ushort)bus.ReadByte(0x177E), "enemy death offset reaches the native FX mirror");
            }
            byte retained = fixedColor.Red;
            fx.Step(bus, vram, new RoomLayer3FxFrame { Camera = Position16<RoomPixels>.Zero, TimeIsFrozen = true, MainGameLoopCarry = true, FirefleaDarknessLevel = level });
            AssertEqual(retained, fixedColor.Red, "frozen death level retains its exact shade");
        }
        // Removing the effect must stop its producer, not let the old flashing timer
        // keep changing a subsequent room's fixed color.
        fx.Load(bus, vram, new SnesCgram(), 0, 0, 0);
        fixedColor.Red = 0x25;
        fx.Step(bus, vram, new RoomLayer3FxFrame { Camera = Position16<RoomPixels>.Zero, TimeIsFrozen = false, MainGameLoopCarry = true });
        AssertEqual(0x25, fixedColor.Red, "non-Fireflea room does not execute the old producer");
        if (includeXrayCapture) VerifyFirefleaXrayCapture();
        Console.WriteLine("  Fireflea FX: compiled native shades, ROM-read guard, initialization, flash cycles, all seven death offsets, COLDATA order and frozen-time retention agree.");
    }

    /// <summary>Verifies fireflea flashing algorithm.</summary>
    private static void VerifyFirefleaFlashingAlgorithm(SuperMetroidAddressSpace rom)
    {
        for (ushort i = 0; i < 12; i++)
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((FirefleaFxDefinitions.FlashReferenceAddress + 2 * i)),
                FirefleaFxDefinitions.FlashingShade(i), $"Original Fireflea flash {i}");
        AssertThrows<InvalidDataException>(() => FirefleaFxDefinitions.FlashingShade(12), "Flash upper bound");
        AssertThrows<InvalidDataException>(() => FirefleaFxDefinitions.FlashingShade(ushort.MaxValue), "Flash invalid maximum");
    }

    /// <summary>Verifies fireflea darkness algorithm.</summary>
    private static void VerifyFirefleaDarknessAlgorithm(SuperMetroidAddressSpace rom)
    {
        for (ushort offset = 0; offset <= 12; offset += 2)
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((FirefleaFxDefinitions.DarknessReferenceAddress + offset)),
                FirefleaFxDefinitions.DarknessShade(offset), $"Original Fireflea darkness including opcode alias {offset}");
        AssertThrows<InvalidDataException>(() => FirefleaFxDefinitions.DarknessShade(1), "Darkness odd offset");
        AssertThrows<InvalidDataException>(() => FirefleaFxDefinitions.DarknessShade(14), "Darkness upper bound");
        AssertThrows<InvalidDataException>(() => FirefleaFxDefinitions.DarknessShade(ushort.MaxValue), "Darkness invalid maximum");
    }

    /// <summary>Verifies fireflea xray capture.</summary>
    private static void VerifyFirefleaXrayCapture()
    {
        var bus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = CreateRetailRuntimeFixture(bus);
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(0x9C5E); // Green Brinstar Firefleas, retail room/FX population.
        AssertEqual(RoomFxType.Fireflea, runtime.RoomLayer3Fx.Type, "retail fixture selects Fireflea FX");
        var samus = runtime.Player.Samus!;
        samus.InputLocked = false;
        samus.Pose = SamusPoseId.FacingRightNormalPose;
        samus.PoseMetadata.RefreshCollisionRadii(bus);
        samus.Presentation.InitializeAnimation(bus);
        // Seed a late darkness shade through the production FX owner. X-ray freezes
        // the producer, so its display must preserve this value rather than substitute
        // ordinary X-ray's constant seven or restart the flash cycle.
        runtime.RoomLayer3Fx.Step(bus, runtime.Vram, new RoomLayer3FxFrame { Camera = Position16<RoomPixels>.Zero, TimeIsFrozen = false, MainGameLoopCarry = true, FirefleaDarknessLevel = 6 });
        byte shade = new FixedColorMemory(bus).RedIntensity;
        AssertEqual(18, shade, "retail darkness table seeds a shade above X-ray's minimum");
        var ordinaryDarkness = GameplayDisplayCapture.TryCaptureFrame(runtime)!;
        AssertTrue(ordinaryDarkness.Layers[0] is GameplayColorMathRenderLayer,
            "normal Fireflea display retains source identity for darkness subtraction");
        var darkLayer = (GameplayColorMathRenderLayer)ordinaryDarkness.Layers[0];
        AssertEqual((byte)0xA2, (byte)darkLayer.ColorMath,
            "normal Fireflea darkens BG2/backdrop, not foreground or Samus");
        AssertTrue(darkLayer.Lines.ToArray().All(line => line.Left > line.Right),
            "ordinary Fireflea darkness has no X-ray exemption window");
        AssertTrue(!darkLayer.RevealBlocks && !darkLayer.AddSubscreen,
            "ordinary Fireflea keeps both room maps and subtracts fixed color");
        AssertTrue(SoftwareLayeredSnapshotRenderer.Render(ordinaryDarkness).SequenceEqual(
            SuperMetroidRuntimeFrameRenderer.Render(runtime)), "immediate and packet Fireflea paths agree");
        AssertTrue(samus.Xray.TryBegin(bus, samus, samus.PoseMetadata.ReadMovementType(bus), new XrayBeginConditions()), "Fireflea display fixture activates X-ray");
        for (int frame = 0; frame < 90; frame++) runtime.StepFrame(runtime.Options.ControllerBindings.Dash);
        var packet = GameplayDisplayCapture.TryCaptureFrame(runtime)!;
        AssertTrue(packet.Layers[0] is GameplayColorMathRenderLayer, "Fireflea capture uses source-aware X-ray compositor");
        var layer = (GameplayColorMathRenderLayer)packet.Layers[0];
        AssertTrue(!layer.RevealBlocks && !layer.AddSubscreen, "Fireflea preserves room maps and uses fixed-color subtraction");
        AssertEqual((byte)0xB3, (byte)layer.ColorMath, "Fireflea native CGADSUB");
        AssertEqual(shade, layer.FixedRed, "Fireflea fixed red survives 90 frozen runtime frames");
        AssertEqual(shade, layer.FixedGreen, "Fireflea fixed green survives 90 frozen runtime frames");
        AssertEqual(shade, layer.FixedBlue, "Fireflea fixed blue survives 90 frozen runtime frames");
        for (int frame = 0; frame < 8; frame++) runtime.StepFrame(0);
        AssertTrue(!samus.Xray.IsActive && !runtime.Freeze.TimeIsFrozen, "Fireflea X-ray teardown releases gameplay");
        var restored = GameplayDisplayCapture.TryCaptureFrame(runtime)!;
        AssertEqual((byte)0xA2, (byte)((GameplayColorMathRenderLayer)restored.Layers[0]).ColorMath,
            "Fireflea teardown restores room subtraction rather than unblended rendering");
    }

    /// <summary>Fireflea fx command.</summary>
    [VerificationCommand("--fireflea-fx")]
    internal static int FirefleaFxCommand()
    {
        Suite(nameof(VerifyFirefleaFx), () => VerifyFirefleaFx());
        return 0;
    }
}
