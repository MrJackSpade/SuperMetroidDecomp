using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;

/// <summary>Normal page-selection inputs and independently selected native heading spritemaps.</summary>
internal static class OptionsHeadingAudit
{
    public static int Run(string rom, string directory)
    {
        Directory.CreateDirectory(directory);
        var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
        ushort Word(int address) => (ushort)(bus.ReadByte(address) | bus.ReadByte(address + 1) << 8);
        var menu = new GameOptionsMenuState(bus);
        void Until(GameOptionsPhase phase)
        {
            for (int frame = 0; frame < 120 && menu.Phase != phase; frame++) menu.Step(0);
            if (menu.Phase != phase) throw new InvalidDataException($"Menu stuck in {menu.Phase}, expected {phase}.");
        }
        void Press(SnesButton button) { menu.Step(0); menu.Step((ushort)button); menu.Step(0); }
        int checkedFrames = 0;
        void Check(string name, int nativeList, int nativeSetup, int scroll = 0)
        {
            var packet = menu.CaptureRenderSnapshot();
            var actual = SoftwareLayeredSnapshotRenderer.Render(packet);
            var border = new OamBuffer();
            border.BeginFrame();
            // Native lists contain direct spritemap pointers, independent of the
            // managed page-to-ID selection under test. Setup supplies its X anchor.
            border.AddOnScreenSpritemap(bus, 0x820000 | Word(nativeList + 2), Word(nativeSetup + 1),
                unchecked((ushort)(Word(0x82f36a) - scroll)), Word(0x82f370));
            border.FinalizeFrame();
            byte[] data = new byte[544];
            border.LowTable.CopyTo(data);
            border.HighTable.CopyTo(data.AsSpan(512));
            var referenceMemory = new PpuMemorySnapshot(packet.Memory.Vram, packet.Memory.Cgram, data, border.LastFinalizedSpriteCount);
            var expected = SoftwareLayeredSnapshotRenderer.Render(new(referenceMemory, packet.Layers, packet.ObjectSelection, packet.Brightness));
            PngWriter.WriteRgba(Path.Combine(directory, name + "-actual.png"), 256, 224, actual);
            PngWriter.WriteRgba(Path.Combine(directory, name + "-expected.png"), 256, 224, expected);
            // The cursor is below this region; compare complete heading pixels,
            // not a chosen border width or a state-only assertion.
            int mismatches = 0;
            for (int i = 0; i < 32 * 256; i++) if (actual[i] != expected[i]) mismatches++;
            if (mismatches != 0) throw new InvalidDataException($"{name}: {mismatches} heading pixels differ from native border composition.");
            if (!actual.AsSpan().SequenceEqual(menu.Render())) throw new InvalidDataException("Direct menu render disagrees with snapshot composition.");
            checkedFrames++;
        }
        Until(GameOptionsPhase.Main);
        Check("primary", 0x82f47e, 0x82f34b);
        for (int row = 0; row < 3; row++) Press(SnesButton.Down);
        Press(SnesButton.A); Until(GameOptionsPhase.ControllerSettings);
        Check("controller-entry", 0x82f48e, 0x82f353);
        for (int row = 0; row < 4; row++) Press(SnesButton.Down);
        Check("controller-item-cancel", 0x82f48e, 0x82f353);
        for (int row = 4; row < 7; row++) { Press(SnesButton.Down); Until(GameOptionsPhase.ControllerSettings); }
        Check("controller-scrolled", 0x82f48e, 0x82f353, 32);
        Press(SnesButton.A); Until(GameOptionsPhase.Main);
        for (int row = 0; row < 4; row++) Press(SnesButton.Down);
        Press(SnesButton.A); Until(GameOptionsPhase.SpecialSettings);
        Check("special", 0x82f49e, 0x82f35b);
        Console.WriteLine($"Options headings: {checkedFrames} page/selection/scroll pixel comparisons match native border lists and setup coordinates.");
        return 0;
    }
}
