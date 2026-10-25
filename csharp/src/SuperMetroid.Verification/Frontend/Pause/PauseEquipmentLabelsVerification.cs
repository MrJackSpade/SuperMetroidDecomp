using SuperMetroid.Core.Frontend;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Tooling;
using System.Buffers.Binary;
using SuperMetroid.Core.Hardware;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Input;
using static SuperMetroid.Verification.Frontend.Pause.PauseEquipmentRulesVerification;
using SuperMetroid.Core.Frontend.Pause;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.Core.Frontend.Pause.Artwork;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.Core.Assets;
using SuperMetroid.AssetExtraction.Frontend.Map;

namespace SuperMetroid.Verification.Frontend.Pause;

/// <summary>Checks installed equipment labels, editable placement, and the native Plasma overrun during rebind.</summary>
internal static class PauseEquipmentLabelsVerification
{
    /// <summary>Compares inventory tilemaps with ROM reads blocked, then checks edits and strict resource failures.</summary>
    internal static void VerifyPauseEquipmentLabelAssets(ISnesAddressSpace bus, string stock,
        string overrides, AreaMapPresentationCatalog original)
    {
        Directory.CreateDirectory(overrides);
        string stockPath = Path.Combine(stock, PauseEquipmentLabelDefinitions.FileName);
        string overridePath = Path.Combine(overrides, PauseEquipmentLabelDefinitions.FileName);
        var document = RequiredJson.Deserialize<PauseEquipmentLabelDocument>(
            File.ReadAllBytes(stockPath), ArtworkJson.Options);
        var guard = NewPauseEquipmentLabelReadGuard(bus);

        SamusState[] inventories =
        [
            new(),
            new() { CollectedBeams = (SamusBeamFlags)0x100f, EquippedBeams = (SamusBeamFlags)0x1005,
                CollectedItems = (SamusEquipmentFlags)0xf32f, EquippedItems = (SamusEquipmentFlags)0x3205, MaxReserveEnergy = 100 },
            new() { HyperBeamEnabled = true, CollectedBeams = (SamusBeamFlags)ushort.MaxValue, EquippedBeams = (SamusBeamFlags)ushort.MaxValue,
                CollectedItems = (SamusEquipmentFlags)ushort.MaxValue, EquippedItems = (SamusEquipmentFlags)ushort.MaxValue },
        ];
        foreach (SamusState inventory in inventories)
        {
            byte[] expected = EquipmentPage(new PauseMenuState(new PauseMenuEntry { Samus = Clone(inventory), System = new Bank80SystemState(), AreaIndex = AreaId.Crateria, RoomMapPosition = new(0, 0), MapPresentation = original, SpriteSheets = MenuSheetFixtures.For(original) }));
            byte[] actual = EquipmentPage(new PauseMenuState(new PauseMenuEntry { Samus = Clone(inventory), System = new Bank80SystemState(), AreaIndex = AreaId.Crateria, RoomMapPosition = new(0, 0), MapPresentation = original, SpriteSheets = MenuSheetFixtures.For(original) }));
            AssertTrue(expected.SequenceEqual(actual),
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
        AreaMapPresentationCatalog edited = MapPresentationFiles.Load(stock, overrides);

        var liveSamus = new SamusState { CollectedBeams = SamusBeamFlags.Charge,
            EquippedBeams = SamusBeamFlags.Charge };
        var live = new PauseMenuState(new PauseMenuEntry { Samus = liveSamus, System = new Bank80SystemState(), AreaIndex = AreaId.Crateria, RoomMapPosition = new(0, 0), MapPresentation = original, SpriteSheets = MenuSheetFixtures.For(original) });
        byte[] before = EquipmentPage(live);
        live.BindFixtureArtwork(edited);
        byte[] after = live.ShownVram().AsSpan(0x6000, 0x800).ToArray();
        AssertTrue(!before.SequenceEqual(after), "edited label placement/art refreshes current equipment page");
        ushort moved = BinaryPrimitives.ReadUInt16LittleEndian(after.AsSpan(
            (charge.Row * PauseEquipmentLabelDefinitions.TilemapColumns + editedColumn) * sizeof(ushort)));
        AssertEqual(chargeCells[0].FlipX, new SnesBgTilemapWord(moved).FlipHorizontally,
            "edited Charge flip reaches its edited semantic destination");

        // Exercise the retail nine-word Boots-to-Plasma copy after an asset-identity
        // change. The first four Varia words must remain the contiguous source tail.
        var glitchSamus = new SamusState { CollectedItems = (SamusEquipmentFlags)0x3300, EquippedItems = (SamusEquipmentFlags)0x3300,
            CollectedBeams = (SamusBeamFlags)0x100f, EquippedBeams = (SamusBeamFlags)4 };
        var glitch = new PauseMenuState(new PauseMenuEntry { Samus = glitchSamus, System = new Bank80SystemState(), AreaIndex = AreaId.Crateria, RoomMapPosition = new(0, 0), MapPresentation = original, SpriteSheets = MenuSheetFixtures.For(original) });
        EquipmentPage(glitch);
        SelectPauseBoots(glitch);
        glitch.Step(0, (SnesButton.Left | SnesButton.A));
        glitch.BindFixtureArtwork(edited);
        byte[] glitchPage = glitch.ShownVram().AsSpan(0x6000, 0x800).ToArray();
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
        AssertThrows<InvalidDataException>(() => MapPresentationFiles.Load(stock, overrides),
            "corrupt equipment-label override fails loudly");

        Console.WriteLine("Pause equipment labels: native inventory/Hyper parity, blocked ROM sources, editable art/placement, VAR rebind and strict failures pass.");

        static byte[] EquipmentPage(PauseMenuState pause)
        {
            EnterPauseEquipment(pause);
            return pause.ShownVram().AsSpan(0x6000, 0x800).ToArray();
        }

        static SamusState Clone(SamusState value) => new()
        {
            CollectedBeams = value.CollectedBeams,
            EquippedBeams = value.EquippedBeams,
            CollectedItems = value.CollectedItems,
            EquippedItems = value.EquippedItems,
            HyperBeamEnabled = value.HyperBeamEnabled,
            MaxReserveEnergy = value.MaxReserveEnergy,
        };
    }

    /// <summary>Blocks bank-$82 equipment-label source reads during installed pause rendering.</summary>
    private static CartridgeReadGuard NewPauseEquipmentLabelReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source)
            .Forbid(address => address is >= 0x82bf32 and < 0x82c0b2,
                address => $"Installed pause equipment label read native source ${address:X6}.");
}
