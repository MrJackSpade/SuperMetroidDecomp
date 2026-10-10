using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using System.Buffers.Binary;

internal static partial class Program
{
    /// <summary>Checks Boots-to-Beams input ordering and the native Plasma label footprint for simultaneous, adjacent, and direction-only presses.</summary>
    private static void VerifyInvalidBeamSelection()
    {
        var bus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        for (int boot = 0; boot < 3; boot++)
        {
            var navigationSamus = new SamusState { CollectedItems = ushort.MaxValue, CollectedBeams = 0x100f,
                EquippedBeams = 4, EquippedItems = (ushort)SamusEquipmentFlags.HiJumpBoots };
            var navigation = CreateRetailPauseFixture(bus, navigationSamus, new Bank80SystemState(), AreaId.Crateria, 0, 0);
            EnterPauseEquipment(navigation);
            SelectPauseBoots(navigation);
            for (int step = 0; step < boot; step++) navigation.Step(0, (ushort)SnesButton.Down);
            AssertEqual(boot, navigation.SelectedItem, "each of the three Boots entries is reachable");
            navigation.Step(0, (ushort)(SnesButton.Left | SnesButton.A));
            AssertEqual(0x000c, navigationSamus.EquippedBeams, $"simultaneous selection from boot {boot}");
        }
        for (int scenario = 0; scenario < 3; scenario++)
        {
        // Entry selects the first beam ($82:AB47, #1266), so reach Boots with real input
        // before the tested press; this isolates the Boots-handler Left+A ordering.
        var samus = new SamusState { CollectedItems = 0x3300, EquippedItems = 0x3300,
            CollectedBeams = 0x100f, EquippedBeams = 4 };
        var pause = CreateRetailPauseFixture(bus, samus, new Bank80SystemState(), AreaId.Crateria, 0, 0);
        EnterPauseEquipment(pause);
        SelectPauseBoots(pause);
        AssertEqual(0, pause.SelectedItem, "fixture starts on Hi-Jump Boots");
        var before = pause.CaptureRenderSnapshot();
        pause.Step(0, (ushort)(scenario == 0 ? SnesButton.Left | SnesButton.A : SnesButton.Left));
        if (scenario == 1) pause.Step(0, (ushort)SnesButton.A);
        AssertEqual(1, pause.SelectedCategory, "same-frame Left+A moves Boots to Beams");
        AssertEqual(4, pause.SelectedItem, "same-frame Left+A selects Plasma");
        AssertEqual(scenario == 0 ? 0x000c : scenario == 1 ? 0x0008 : 0x0004, samus.EquippedBeams,
            "native beam bits distinguish simultaneous, adjacent and direction-only input");
        var after = pause.CaptureRenderSnapshot();
        byte[] expectedVram = after.Memory.Vram.ToArray();
        // Restore the entire label region to its before-input contents, then apply only
        // the independently measured CPU writes. Preserve the moving OBJ selector.
        before.Memory.Vram.Slice(0x6000, 0x800).CopyTo(expectedVram.AsSpan(0x6000));
        ushort[] nativeWords = [0x08ff, 0x08ec, 0x08ed, 0x08ee, 0x08ef, 0x08ff, 0x0900, 0x0901, 0x0902];
        int copiedWords = scenario == 0 ? 9 : scenario == 1 ? 5 : 0;
        for (int word = 0; word < copiedWords; word++)
            BinaryPrimitives.WriteUInt16LittleEndian(expectedVram.AsSpan(0x6000 + 0x508 + word * 2), nativeWords[word]);
        // Independent native $82:B20C probe confirms that the wireframe subsequently
        // replaces the ninth overrun word at $3D18 with zero for this equipment set.
        BinaryPrimitives.WriteUInt16LittleEndian(expectedVram.AsSpan(0x6518), 0);
        if (scenario == 1)
            for (int word = 0; word < 5; word++)
            {
                int offset = 0x6000 + 0x4c8 + word * 2;
                // $82:C092 selects Spazer artwork; disabling rewrites glyphs, not just palette bits.
                int source = 0x820000 | (bus.ReadByte(0x82c092) | bus.ReadByte(0x82c093) << 8);
                ushort oldWord = (ushort)(bus.ReadByte(source + word * 2) | bus.ReadByte(source + word * 2 + 1) << 8);
                BinaryPrimitives.WriteUInt16LittleEndian(expectedVram.AsSpan(offset), (ushort)((oldWord & 0xe3ff) | 0x0c00));
            }
        // The native shared tail refreshes the wireframe independently of label edits.
        // It is unchanged by beam-only toggles; initial fixture item state already uses
        // the same HiJump wireframe in all three scenarios.
        Console.WriteLine($"scenario={scenario} actual label bytes={Convert.ToHexString(after.Memory.Vram.Slice(0x6508, 18))}");
        AssertTrue(expectedVram.AsSpan(0x6508, 18).SequenceEqual(after.Memory.Vram.Slice(0x6508, 18)),
            $"native nine/five/zero-word Plasma footprint, scenario {scenario}");
        var expectedMemory = new PpuMemorySnapshot(expectedVram, after.Memory.Cgram, after.Memory.Oam, after.Memory.ModeledSpriteCount);
        var expectedPixels = SoftwareLayeredSnapshotRenderer.Render(new(expectedMemory, after.Layers, after.ObjectSelection, after.Brightness));
        var actualPixels = pause.Render();
        if (!expectedPixels.AsSpan().SequenceEqual(actualPixels))
            for (int offset = 0x6000; offset < 0x6800; offset += 2)
                if (!expectedVram.AsSpan(offset, 2).SequenceEqual(after.Memory.Vram.Slice(offset, 2)))
                    Console.WriteLine($"tile mismatch {offset:X4}: expected {Convert.ToHexString(expectedVram.AsSpan(offset, 2))}, actual {Convert.ToHexString(after.Memory.Vram.Slice(offset, 2))}");
        AssertTrue(expectedPixels.AsSpan().SequenceEqual(actualPixels), $"native rendered VAR/adjacent-frame label, scenario {scenario}");
        Directory.CreateDirectory("csharp/test-temp/issue-395-inventory");
        SuperMetroid.Core.Assets.PngWriterTooling.WriteRgba($"csharp/test-temp/issue-395-inventory/scenario-{scenario}.png", 256, 224, actualPixels);
        }
        Console.WriteLine("Inventory beam glitch: simultaneous/adjacent/left-only inputs match retail bits, tile footprint and rendered labels.");
    }
}
