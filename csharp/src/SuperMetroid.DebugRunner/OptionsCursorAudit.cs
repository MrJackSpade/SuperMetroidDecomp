using System.Reflection;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;

/// <summary>Compares complete controller-page pixels with the cartridge cursor anchors.</summary>
internal static class OptionsCursorAudit
{
    public static int Run(string rom, string directory)
    {
        foreach (bool japanese in new[] { false, true })
            RunCase(rom, Path.Combine(directory, japanese ? "japanese" : "english"), japanese);
        return 0;
    }

    private static void RunCase(string rom, string directory, bool japanese)
    {
        Directory.CreateDirectory(directory);
        var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
        var menu = new GameOptionsMenuState(bus, japaneseText: japanese);
        ushort Word(int address) => (ushort)(bus.ReadByte(address) | bus.ReadByte(address + 1) << 8);
        int Field(string name) => (int)(typeof(GameOptionsMenuState)
            .GetField(name, BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(menu)
            ?? throw new InvalidDataException($"Missing diagnostic field {name}."));
        void Until(GameOptionsPhase phase)
        {
            for (int i = 0; i < 120 && menu.Phase != phase; i++) menu.Step(0);
            if (menu.Phase != phase) throw new InvalidDataException("Options transition timed out.");
        }
        void Press(SnesButton button) { menu.Step(0); menu.Step((ushort)button); menu.Step(0); }
        Until(GameOptionsPhase.Main);
        for (int i = 0; i < 3; i++) Press(SnesButton.Down);
        Press(SnesButton.A);
        Until(GameOptionsPhase.ControllerSettings);
        int checkedFrames = 0;
        void Check()
        {
            var packet = menu.CaptureRenderSnapshot();
            var actual = SoftwareLayeredSnapshotRenderer.Render(packet);
            if (!actual.AsSpan().SequenceEqual(menu.Render()))
                throw new InvalidDataException("Direct options rendering differs from its captured frame.");
            var reference = new OamBuffer();
            reference.BeginFrame();
            reference.AddOnScreenSpritemap(bus, OptionsCursorFixtureData.MenuBank |
                Word(OptionsCursorFixtureData.ControllerBorderList + 2),
                Word(OptionsCursorFixtureData.ControllerBorderX),
                unchecked((ushort)(Word(OptionsCursorFixtureData.BorderY) - Field("bg1VerticalScroll"))),
                Word(OptionsCursorFixtureData.Palette));
            bool scrolling = menu.Phase is GameOptionsPhase.ScrollControllerDown or GameOptionsPhase.ScrollControllerUp;
            ushort x = scrolling ? Word(OptionsCursorFixtureData.HiddenX) :
                Word(OptionsCursorFixtureData.ControllerPositions + menu.SelectedItem * 4);
            ushort y = scrolling ? Word(OptionsCursorFixtureData.HiddenY) :
                Word(OptionsCursorFixtureData.ControllerPositions + menu.SelectedItem * 4 + 2);
            // Animation phase is deliberately held equal: this test isolates placement,
            // not whether the managed missile animation matches native sequencing.
            int nativeAnimationRow = 3 - Field("missileFrame");
            reference.AddOnScreenSpritemap(bus, OptionsCursorFixtureData.MenuBank |
                Word(OptionsCursorFixtureData.MissileAnimation + nativeAnimationRow * 4 + 2),
                x, y, Word(OptionsCursorFixtureData.Palette));
            reference.FinalizeFrame();
            byte[] oam = new byte[544];
            reference.LowTable.CopyTo(oam);
            reference.HighTable.CopyTo(oam.AsSpan(512));
            var memory = new PpuMemorySnapshot(packet.Memory.Vram, packet.Memory.Cgram, oam, reference.LastFinalizedSpriteCount);
            var expected = SoftwareLayeredSnapshotRenderer.Render(new(memory, packet.Layers, packet.ObjectSelection, packet.Brightness));
            int different = actual.Zip(expected).Count(pair => pair.First != pair.Second);
            if (different != 0)
            {
                string name = $"frame-{checkedFrames}-row-{menu.SelectedItem}-{menu.Phase}";
                PngWriter.WriteRgba(Path.Combine(directory, name + "-actual.png"), 256, 224, actual);
                PngWriter.WriteRgba(Path.Combine(directory, name + "-expected.png"), 256, 224, expected);
                throw new InvalidDataException($"{name}: {different} pixels differ; native cursor origin={x},{y}.");
            }
            checkedFrames++;
        }
        void Move(SnesButton direction)
        {
            menu.Step(0); Check();
            menu.Step((ushort)direction); Check();
            for (int i = 0; i < 20 && menu.Phase != GameOptionsPhase.ControllerSettings; i++)
            { menu.Step(0); Check(); }
            if (menu.Phase != GameOptionsPhase.ControllerSettings)
                throw new InvalidDataException("Controller page failed to settle.");
        }
        Check();
        for (int i = 0; i < 9; i++) Move(SnesButton.Down);
        for (int i = 0; i < 9; i++) Move(SnesButton.Up);
        Console.WriteLine($"Controller selector (Japanese={japanese}): {checkedFrames} full-frame native-anchor comparisons pass across every row and both scroll/wrap directions.");
    }
}
