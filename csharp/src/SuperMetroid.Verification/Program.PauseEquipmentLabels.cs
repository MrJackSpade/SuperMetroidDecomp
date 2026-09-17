using System.Buffers.Binary;
using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;

internal static partial class Program
{
    private static void VerifyPauseEquipmentLabelAssets(ISnesAddressSpace bus, string stock,
        string overrides, AreaMapPresentationCatalog original)
    {
        Directory.CreateDirectory(overrides);
        string stockPath = Path.Combine(stock, PauseEquipmentLabelDefinitions.FileName);
        string overridePath = Path.Combine(overrides, PauseEquipmentLabelDefinitions.FileName);
        var document = JsonSerializer.Deserialize<PauseEquipmentLabelDocument>(
            File.ReadAllBytes(stockPath), MapPresentationFormat.JsonOptions)!;
        var guard = new PauseEquipmentLabelReadGuard(bus);

        SamusState[] inventories =
        [
            new(),
            new() { CollectedBeams = 0x100f, EquippedBeams = 0x1005,
                CollectedItems = 0xf32f, EquippedItems = 0x3205, MaxReserveEnergy = 100 },
            new() { HyperBeam = 1, CollectedBeams = ushort.MaxValue, EquippedBeams = ushort.MaxValue,
                CollectedItems = ushort.MaxValue, EquippedItems = ushort.MaxValue },
        ];
        foreach (SamusState inventory in inventories)
        {
            byte[] expected = EquipmentPage(new PauseMenuState(bus, Clone(inventory), new Bank80SystemState(),
                AreaId.Crateria, 0, 0));
            byte[] actual = EquipmentPage(new PauseMenuState(guard, Clone(inventory), new Bank80SystemState(),
                AreaId.Crateria, 0, 0, mapPresentation: original));
            AssertTrue(expected.AsSpan().SequenceEqual(actual),
                "installed equipment labels match native tilemap with label ROM sources forbidden");
        }

        var labels = document.Labels.ToDictionary(pair => pair.Key, pair => pair.Value,
            StringComparer.Ordinal);
        PauseEquipmentLabel charge = labels["Beam.Charge"];
        PauseBackdropCell[] chargeCells = charge.Cells.ToArray();
        chargeCells[0] = chargeCells[0] with { FlipX = !chargeCells[0].FlipX };
        int editedColumn = charge.Column + 1;
        labels["Beam.Charge"] = charge with { Column = editedColumn, Cells = chargeCells };
        var editedDocument = document with { Labels = labels };
        using (var output = File.Create(overridePath))
            PauseEquipmentLabelPresentation.Write(output, editedDocument);
        AreaMapPresentationCatalog edited = AreaMapPresentationCatalog.Load(stock, overrides);

        var liveSamus = new SamusState { CollectedBeams = (ushort)SamusBeamFlags.Charge,
            EquippedBeams = (ushort)SamusBeamFlags.Charge };
        var live = new PauseMenuState(guard, liveSamus, new Bank80SystemState(), AreaId.Crateria,
            0, 0, mapPresentation: original);
        byte[] before = EquipmentPage(live);
        live.BindMapPresentation(edited);
        byte[] after = live.CaptureRenderSnapshot().Memory.Vram.Slice(0x6000, 0x800).ToArray();
        AssertTrue(!before.AsSpan().SequenceEqual(after), "edited label placement/art refreshes current equipment page");
        ushort moved = BinaryPrimitives.ReadUInt16LittleEndian(after.AsSpan(
            (charge.Row * PauseEquipmentLabelDefinitions.TilemapColumns + editedColumn) * sizeof(ushort)));
        AssertEqual(chargeCells[0].FlipX, new SnesBgTilemapWord(moved).FlipHorizontally,
            "edited Charge flip reaches its edited semantic destination");

        // Exercise the retail nine-word Boots-to-Plasma copy after an asset-identity
        // change. The first four Varia words must remain the contiguous source tail.
        var glitchSamus = new SamusState { CollectedItems = (ushort)SamusEquipmentFlags.HiJumpBoots,
            EquippedItems = (ushort)SamusEquipmentFlags.HiJumpBoots };
        var glitch = new PauseMenuState(guard, glitchSamus, new Bank80SystemState(), AreaId.Crateria,
            0, 0, mapPresentation: original);
        glitchSamus.CollectedBeams = 0x100f;
        glitchSamus.EquippedBeams = 4;
        glitchSamus.CollectedItems = glitchSamus.EquippedItems = 0x3300;
        EquipmentPage(glitch);
        glitch.Step(0, (ushort)(SnesButton.Left | SnesButton.A));
        glitch.BindMapPresentation(edited);
        byte[] glitchPage = glitch.CaptureRenderSnapshot().Memory.Vram.Slice(0x6000, 0x800).ToArray();
        int plasma = (document.Labels["Beam.Plasma"].Row * PauseEquipmentLabelDefinitions.TilemapColumns +
            document.Labels["Beam.Plasma"].Column) * sizeof(ushort);
        // The wireframe refresh owns the ninth word and replaces Varia's fourth
        // overrun word with zero, exactly as $82:B20C does after the label copy.
        ushort[] nativeTail = [0x08ff, 0x0900, 0x0901, 0x0000];
        for (int index = 0; index < nativeTail.Length; index++)
            AssertEqual(nativeTail[index], BinaryPrimitives.ReadUInt16LittleEndian(
                glitchPage.AsSpan(plasma + (PauseEquipmentLabelDefinitions.BeamWords + index) * sizeof(ushort))),
                "edited-content rebind retains the native Plasma-to-Varia overrun");

        void Reject(PauseEquipmentLabelDocument invalid, string message) =>
            AssertThrows<InvalidDataException>(() => PauseEquipmentLabelPresentation.Write(
                new MemoryStream(), invalid), message);
        Reject(document with { Version = 2 }, "unsupported equipment-label schema rejected");
        Reject(document with { DisabledPalette = 8 }, "invalid equipment-label palette rejected");
        Reject(document with { Blank = [] }, "incomplete equipment blank patch rejected");
        var missing = document.Labels.ToDictionary(pair => pair.Key, pair => pair.Value);
        missing.Remove("Beam.Ice");
        Reject(document with { Labels = missing }, "missing equipment label rejected");
        var overlap = document.Labels.ToDictionary(pair => pair.Key, pair => pair.Value);
        overlap["Beam.Ice"] = overlap["Beam.Ice"] with
        {
            Column = overlap["Beam.Charge"].Column,
            Row = overlap["Beam.Charge"].Row,
        };
        Reject(document with { Labels = overlap }, "overlapping equipment labels rejected");
        var liveOwnerOverlap = document.Labels.ToDictionary(pair => pair.Key, pair => pair.Value);
        liveOwnerOverlap["Beam.Charge"] = liveOwnerOverlap["Beam.Charge"] with { Column = 12, Row = 16 };
        Reject(document with { Labels = liveOwnerOverlap }, "equipment labels cannot overlap wireframe/reserve owners");
        File.WriteAllText(overridePath, "{ broken JSON");
        AssertThrows<InvalidDataException>(() => AreaMapPresentationCatalog.Load(stock, overrides),
            "corrupt equipment-label override fails loudly");

        Console.WriteLine("Pause equipment labels: native inventory/Hyper parity, blocked ROM sources, editable art/placement, VAR rebind and strict failures pass.");

        static byte[] EquipmentPage(PauseMenuState pause)
        {
            pause.Step((ushort)SnesButton.R, (ushort)SnesButton.R);
            for (int frame = 0; frame < 32; frame++) pause.Step(0, 0);
            return pause.CaptureRenderSnapshot().Memory.Vram.Slice(0x6000, 0x800).ToArray();
        }

        static SamusState Clone(SamusState value) => new()
        {
            CollectedBeams = value.CollectedBeams,
            EquippedBeams = value.EquippedBeams,
            CollectedItems = value.CollectedItems,
            EquippedItems = value.EquippedItems,
            HyperBeam = value.HyperBeam,
            MaxReserveEnergy = value.MaxReserveEnergy,
        };
    }

    private sealed class PauseEquipmentLabelReadGuard(ISnesAddressSpace source) : ISnesAddressSpace
    {
        public byte ReadByte(int address) => address is >= 0x82bf32 and < 0x82c0b2
            ? throw new InvalidOperationException($"Installed pause equipment label read native source ${address:X6}.")
            : source.ReadByte(address);
        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
