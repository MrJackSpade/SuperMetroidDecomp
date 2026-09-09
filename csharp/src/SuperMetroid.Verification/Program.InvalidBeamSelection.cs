using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using System.Buffers.Binary;

internal static partial class Program
{
    private static void VerifyInvalidBeamSelection()
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        for (int boot = 0; boot < 3; boot++)
        {
            var navigationSamus = new SamusState { CollectedItems = ushort.MaxValue, CollectedBeams = 0x100f,
                EquippedBeams = 4, EquippedItems = (ushort)SamusEquipmentFlags.HiJumpBoots };
            var navigation = new PauseMenuState(bus, navigationSamus, new Bank80SystemState(), AreaId.Crateria, 0, 0);
            navigation.Step((ushort)SnesButton.R, (ushort)SnesButton.R);
            for (int frame = 0; frame < 32; frame++) navigation.Step(0, 0);
            navigation.Step(0, (ushort)SnesButton.Right);
            for (int step = 0; navigation.SelectedCategory != 3 && step < 6; step++)
                navigation.Step(0, (ushort)SnesButton.Down);
            AssertEqual(3, navigation.SelectedCategory, "ordinary navigation reaches Boots from initial Beams");
            for (int step = 0; step < boot; step++) navigation.Step(0, (ushort)SnesButton.Down);
            AssertEqual(boot, navigation.SelectedItem, "each of the three Boots entries is reachable");
            navigation.Step(0, (ushort)(SnesButton.Left | SnesButton.A));
            AssertEqual(0x000c, navigationSamus.EquippedBeams, $"simultaneous selection from boot {boot}");
        }
        for (int scenario = 0; scenario < 3; scenario++)
        {
        // Begin on Boots using the production initial-selection rule. Publish the rest
        // of the fixture inventory afterward, before any tested input. This avoids a
        // private cursor setter and isolates the Boots-handler Left+A ordering.
        var samus = new SamusState { CollectedItems = (ushort)SamusEquipmentFlags.HiJumpBoots,
            EquippedItems = (ushort)SamusEquipmentFlags.HiJumpBoots };
        var pause = new PauseMenuState(bus, samus, new Bank80SystemState(), AreaId.Crateria, 0, 0);
        AssertEqual(3, pause.SelectedCategory, "fixture starts on Boots");
        samus.CollectedBeams = 0x100f;
        samus.EquippedBeams = 4;
        samus.CollectedItems = samus.EquippedItems = 0x3300;
        pause.Step((ushort)SnesButton.R, (ushort)SnesButton.R);
        for (int frame = 0; frame < 32; frame++) pause.Step(0, 0);
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
                ushort oldWord = BinaryPrimitives.ReadUInt16LittleEndian(expectedVram.AsSpan(offset));
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
        AssertTrue(expectedPixels.AsSpan().SequenceEqual(actualPixels), $"native rendered VAR/adjacent-frame label, scenario {scenario}");
        Directory.CreateDirectory("csharp/test-temp/issue-395-inventory");
        SuperMetroid.Core.Assets.PngWriter.WriteRgba($"csharp/test-temp/issue-395-inventory/scenario-{scenario}.png", 256, 224, actualPixels);
        }
        Console.WriteLine("Inventory beam glitch: simultaneous/adjacent/left-only inputs match retail bits, tile footprint and rendered labels.");
    }
}
