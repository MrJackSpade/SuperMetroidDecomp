using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Runtime;

internal static partial class Program
{
    private static void VerifyFirefleaFx(bool includeXrayCapture = true)
    {
        var memory = new TestAddressSpace();
        var bus = new FirefleaFxDefinitionReadGuard(memory);
        var fx = new RoomLayer3FxState();
        var vram = new SnesVram();
        ushort record = RoomFxRecordDefinitions.All.First(record => record.Type == (byte)RoomFxType.Fireflea && record.DoorPointer == 0).Pointer;
        // Independent transcription of $88:B058/$88:B070. These are deliberately not
        // generated from the production catalog or implementation.
        ushort[] flash = [0, 0x100, 0x200, 0x300, 0x400, 0x500, 0x600, 0x500, 0x400, 0x300, 0x200, 0x100];
        ushort[] darkness = [0, 0x600, 0xC00, 0x1200, 0x1800, 0x1900, 0xC208];
        var rom = SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        fx.PaletteBlendColors = SuperMetroid.Core.Assets.RoomFxPaletteBlendCatalog.Load(
            new MemoryStream(SuperMetroid.AssetExtraction.RoomFxPaletteBlendExtractor.Extract(rom)));
        Suite(nameof(VerifyFirefleaFlashingAlgorithm), () => VerifyFirefleaFlashingAlgorithm(rom));
        Suite(nameof(VerifyFirefleaDarknessAlgorithm), () => VerifyFirefleaDarknessAlgorithm(rom));
        fx.Load(bus, vram, new SnesCgram(), record, 0, 0);
        AssertEqual(6, bus.ReadByte(0x1778), "Fireflea load initializes native six-frame timer");
        AssertTrue(!fx.IsRenderable, "Fireflea darkness does not invent a BG3 texture");
        for (int frame = 1; frame <= 144; frame++)
        {
            fx.Step(bus, vram, 0, 0, false);
            int expectedIndex = (frame / 6) % 12;
            int shade = flash[expectedIndex] >> 8;
            AssertEqual(expectedIndex, bus.ReadByte(0x177A), $"Fireflea index frame {frame}");
            AssertEqual(shade | 0x20, bus.ReadByte(0x74), $"Fireflea red frame {frame}");
            AssertEqual(shade | 0x80, bus.ReadByte(0x75), $"Fireflea second COLDATA write frame {frame}");
            AssertEqual(shade | 0x40, bus.ReadByte(0x76), $"Fireflea third COLDATA write frame {frame}");
        }
        byte timer = bus.ReadByte(0x1778);
        byte red = bus.ReadByte(0x74);
        for (int frame = 0; frame < 100; frame++) fx.Step(bus, vram, 0, 0, true);
        AssertEqual(timer, bus.ReadByte(0x1778), "X-ray frozen time retains flashing phase");
        AssertEqual(red, bus.ReadByte(0x74), "X-ray frozen time retains fixed color");
        AssertEqual(LayerBlendingConfiguration.Fireflea, fx.LayerBlendConfiguration, "frozen Fireflea still selects blending");
        for (ushort level = 0; level <= 12; level += 2)
        {
            fx.Load(bus, vram, new SnesCgram(), record, 0, 0);
            for (int frame = 1; frame <= 78; frame++)
            {
                fx.Step(bus, vram, 0, 0, false, firefleaDarknessLevel: level);
                int index = level < 10 ? frame / 6 % 12 : frame < 6 ? 0 : 6;
                byte shade = (byte)(unchecked((ushort)(darkness[level / 2] + flash[index])) >> 8);
                AssertEqual(shade | 0x20, bus.ReadByte(0x74), $"death offset {level}, frame {frame}: red");
                AssertEqual(shade | 0x80, bus.ReadByte(0x75), $"death offset {level}, frame {frame}: blue");
                AssertEqual(shade | 0x40, bus.ReadByte(0x76), $"death offset {level}, frame {frame}: green");
                AssertEqual(level, (ushort)bus.ReadByte(0x177E), "enemy death offset reaches the native FX mirror");
            }
            byte retained = bus.ReadByte(0x74);
            fx.Step(bus, vram, 0, 0, true, firefleaDarknessLevel: level);
            AssertEqual(retained, bus.ReadByte(0x74), "frozen death level retains its exact shade");
        }
        // Removing the effect must stop its producer, not let the old flashing timer
        // keep changing a subsequent room's fixed color.
        fx.Load(bus, vram, new SnesCgram(), 0, 0, 0);
        bus.WriteByte(0x74, 0x25);
        fx.Step(bus, vram, 0, 0, false);
        AssertEqual(0x25, bus.ReadByte(0x74), "non-Fireflea room does not execute the old producer");
        if (includeXrayCapture) VerifyFirefleaXrayCapture();
        Console.WriteLine("  Fireflea FX: compiled native shades, ROM-read guard, initialization, flash cycles, all seven death offsets, COLDATA order and frozen-time retention agree.");
    }

    private static void VerifyFirefleaFlashingAlgorithm(SuperMetroidAddressSpace rom)
    {
        for (ushort i = 0; i < 12; i++)
            AssertEqual(ReadFirefleaWord(rom, FirefleaFxDefinitions.FlashReferenceAddress + 2 * i),
                FirefleaFxDefinitions.FlashingShade(i), $"Original Fireflea flash {i}");
        AssertThrows<InvalidDataException>(() => FirefleaFxDefinitions.FlashingShade(12), "Flash upper bound");
        AssertThrows<InvalidDataException>(() => FirefleaFxDefinitions.FlashingShade(ushort.MaxValue), "Flash invalid maximum");
    }

    private static void VerifyFirefleaDarknessAlgorithm(SuperMetroidAddressSpace rom)
    {
        for (ushort offset = 0; offset <= 12; offset += 2)
            AssertEqual(ReadFirefleaWord(rom, FirefleaFxDefinitions.DarknessReferenceAddress + offset),
                FirefleaFxDefinitions.DarknessShade(offset), $"Original Fireflea darkness including opcode alias {offset}");
        AssertThrows<InvalidDataException>(() => FirefleaFxDefinitions.DarknessShade(1), "Darkness odd offset");
        AssertThrows<InvalidDataException>(() => FirefleaFxDefinitions.DarknessShade(14), "Darkness upper bound");
        AssertThrows<InvalidDataException>(() => FirefleaFxDefinitions.DarknessShade(ushort.MaxValue), "Darkness invalid maximum");
    }
    private sealed class FirefleaFxDefinitionReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace, IImportCartridgeSource, ISnesMutableMemory
    {
        public byte ReadCartridgeByte(int address) => ReadByte(address);

        public byte ReadByte(int address)
        {
            if (address is >= 0x88B058 and < 0x88B07E)
                throw new InvalidOperationException($"Fireflea FX attempted migrated definition read ${address:X6}.");
            return source.ReadByte(address);
        }

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);

        public byte ReadWorkRamByte(int address) =>
            (source as ISnesMutableMemory ?? throw new InvalidOperationException(
                "Fireflea FX audit source must expose WRAM."))
                .ReadWorkRamByte(address);

        public byte ReadSaveRamByte(int address) =>
            (source as ISnesMutableMemory ?? throw new InvalidOperationException(
                "Fireflea FX audit source must expose SRAM."))
                .ReadSaveRamByte(address);
    }

    private static ushort ReadFirefleaWord(SuperMetroidAddressSpace bus, int address) =>
        (ushort)(bus.ReadByte(address) | bus.ReadByte(address + 1) << 8);

    private static void VerifyFirefleaXrayCapture()
    {
        var bus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = CreateRetailRuntimeFixture(bus);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(0x9C5E); // Green Brinstar Firefleas, retail room/FX population.
        AssertEqual(RoomFxType.Fireflea, runtime.RoomLayer3Fx.Type, "retail fixture selects Fireflea FX");
        var samus = runtime.Samus!;
        samus.InputLocked = false;
        samus.Pose = SamusPoseIds.FacingRightNormalPose;
        samus.RefreshCollisionRadii(bus);
        samus.InitializeAnimation(bus);
        // Seed a late darkness shade through the production FX owner. X-ray freezes
        // the producer, so its display must preserve this value rather than substitute
        // ordinary X-ray's constant seven or restart the flash cycle.
        runtime.RoomLayer3Fx.Step(bus, runtime.Vram, 0, 0, false, firefleaDarknessLevel: 6);
        byte shade = (byte)(bus.ReadByte(0x74) & 31);
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
        AssertTrue(samus.Xray.TryBegin(bus, samus, samus.ReadMovementType(bus)), "Fireflea display fixture activates X-ray");
        for (int frame = 0; frame < 90; frame++) runtime.StepFrame(runtime.ControllerBindings.Dash);
        var packet = GameplayDisplayCapture.TryCaptureFrame(runtime)!;
        AssertTrue(packet.Layers[0] is GameplayColorMathRenderLayer, "Fireflea capture uses source-aware X-ray compositor");
        var layer = (GameplayColorMathRenderLayer)packet.Layers[0];
        AssertTrue(!layer.RevealBlocks && !layer.AddSubscreen, "Fireflea preserves room maps and uses fixed-color subtraction");
        AssertEqual((byte)0xB3, (byte)layer.ColorMath, "Fireflea native CGADSUB");
        AssertEqual(shade, layer.FixedRed, "Fireflea fixed red survives 90 frozen runtime frames");
        AssertEqual(shade, layer.FixedGreen, "Fireflea fixed green survives 90 frozen runtime frames");
        AssertEqual(shade, layer.FixedBlue, "Fireflea fixed blue survives 90 frozen runtime frames");
        for (int frame = 0; frame < 8; frame++) runtime.StepFrame(0);
        AssertTrue(!samus.Xray.IsActive && !runtime.TimeIsFrozen, "Fireflea X-ray teardown releases gameplay");
        var restored = GameplayDisplayCapture.TryCaptureFrame(runtime)!;
        AssertEqual((byte)0xA2, (byte)((GameplayColorMathRenderLayer)restored.Layers[0]).ColorMath,
            "Fireflea teardown restores room subtraction rather than unblended rendering");
    }
}
