using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Frontend;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Tooling;
using System.Buffers.Binary;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using static SuperMetroid.Verification.Frontend.Pause.PauseEquipmentRulesVerification;
using SuperMetroid.Core.Frontend.Pause;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.Core.Frontend.Pause.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.Core.Assets;
using SuperMetroid.AssetExtraction.Frontend.Map;
namespace SuperMetroid.Verification.Frontend.Pause;

/// <summary>Checks installed pause equipment base tiles through page transitions and debugger rebinding.</summary>
internal static class PauseEquipmentBaseAssetsVerification
{
    /// <summary>Compares 1,024 native cells, checks a visible edit, and preserves live cells on artwork rebind.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for the pause equipment base tilemap")]
    internal static void VerifyPauseEquipmentBaseAssets(ISnesAddressSpace bus, string stock, string overrides,
        AreaMapPresentationCatalog original)
    {
        Directory.CreateDirectory(overrides);
        string stockPath = Path.Combine(stock, PauseEquipmentBaseDefinitions.FileName);
        string path = Path.Combine(overrides, PauseEquipmentBaseDefinitions.FileName);
        byte[] stockBytes = File.ReadAllBytes(stockPath);
        var document = RequiredJson.Deserialize<PauseEquipmentBaseDocument>(stockBytes, ArtworkJson.Options);
        byte[] nativeBase = RomDataReader.ReadFixedBank(CartridgeImportSource.Require(bus), PauseMenuSourceDefinitions.EquipmentPage,
            PauseEquipmentBaseDefinitions.Cells * sizeof(ushort));
        AssertTrue(nativeBase.SequenceEqual(original.PauseEquipmentBase.CreateTilemap()),
            "all 1024 equipment base cells match native words");

        var guard = NewPauseEquipmentBaseReadGuard(bus);
        var native = new PauseMenuState(new PauseMenuEntry { Samus = Samus(), System = new Bank80SystemState(), AreaIndex = AreaId.Norfair, RoomMapPosition = new(0, 0), MapPresentation = original, SpriteSheets = MenuSheetFixtures.For(original) });
        var installed = new PauseMenuState(new PauseMenuEntry { Samus = Samus(), System = new Bank80SystemState(), AreaIndex = AreaId.Norfair, RoomMapPosition = new(0, 0), MapPresentation = original, SpriteSheets = MenuSheetFixtures.For(original) });
        int toMap = PausePageTransitionNativeLength + 1;
        for (int frame = 0; frame < toMap + PausePageTransitionNativeLength + 2; frame++)
        {
            SnesButton input = frame == 0 ? SnesButton.R : frame == toMap ? SnesButton.L : 0;
            native.Step(input, input, frameCounterByte: (byte)frame);
            installed.Step(input, input, frameCounterByte: (byte)frame);
            AssertTrue(native.ShownVram().SequenceEqual(installed.ShownVram()),
                "equipment base migration retains exact native VRAM through both page transitions");
            AssertTrue(native.Render().SequenceEqual(installed.Render()),
                "equipment base migration retains native pixels with its source forbidden");
        }
        AssertEqual(PauseScreenMode.Map, installed.ScreenMode, "equipment base fixture returns to the map page");

        const int editedCell = 194;
        AssertTrue(!PauseEquipmentBaseDefinitions.IsLiveOwnedCell(editedCell) &&
            !PauseEquipmentBaseDefinitions.IsArrowCell(editedCell), "edited control cell belongs to static base presentation");
        PauseBackdropCell[] cells = document.Cells.ToArray();
        cells[editedCell] = cells[editedCell] with
        {
            Atlas = PauseAtlas.Map, TileColumn = 4, TileRow = 0,
            Palette = 3, Priority = true, FlipX = false, FlipY = false,
        };
        using (var output = File.Create(path)) PauseEquipmentBasePresentation.Write(output, document with { Cells = cells });
        var edited = MapPresentationFiles.Load(stock, overrides);
        AssertTrue(original.ContentIdentity != edited.ContentIdentity, "equipment base override changes content identity");
        var stockPause = new PauseMenuState(new PauseMenuEntry { Samus = Samus(), System = new Bank80SystemState(), AreaIndex = AreaId.Norfair, RoomMapPosition = new(0, 0), MapPresentation = original, SpriteSheets = MenuSheetFixtures.For(original) });
        var editedPause = new PauseMenuState(new PauseMenuEntry { Samus = Samus(), System = new Bank80SystemState(), AreaIndex = AreaId.Norfair, RoomMapPosition = new(0, 0), MapPresentation = edited, SpriteSheets = MenuSheetFixtures.For(edited) });
        EnterEquipment(stockPause); EnterEquipment(editedPause);
        AssertTrue(!stockPause.Render().SequenceEqual(editedPause.Render()),
            "edited static equipment cell reaches actual menu pixels");

        byte[] rebound = Enumerable.Repeat((byte)0x5a, PauseEquipmentBaseDefinitions.Cells * 2).ToArray();
        int protectedCell = Enumerable.Range(0, PauseEquipmentBaseDefinitions.Cells)
            .First(PauseEquipmentBaseDefinitions.IsLiveOwnedCell);
        int arrowCell = Enumerable.Range(0, PauseEquipmentBaseDefinitions.Cells)
            .First(PauseEquipmentBaseDefinitions.IsArrowCell);
        ushort liveArrow = new SnesBgTilemapWord(0).WithPaletteIndex(2).Raw;
        BinaryPrimitives.WriteUInt16LittleEndian(rebound.AsSpan(arrowCell * 2), liveArrow);
        edited.PauseEquipmentBase.RebindBaseInto(rebound);
        AssertEqual((byte)0x5a, rebound[protectedCell * 2], "base rebind preserves live inventory/wireframe/reserve bytes");
        AssertEqual(2, new SnesBgTilemapWord(BinaryPrimitives.ReadUInt16LittleEndian(rebound.AsSpan(arrowCell * 2))).PaletteIndex,
            "base rebind preserves the live reserve-arrow palette");
        AssertTrue(!rebound.AsSpan(editedCell * 2, 2).SequenceEqual(new byte[] { 0x5a, 0x5a }),
            "base rebind refreshes ordinary authored cells");

        using var captured = new MemoryStream();
        SuperMetroid.Diagnostics.SaveStates.DebuggerObjectGraphSerializer.Serialize(captured, editedPause); captured.Position = 0;
        var restored = SuperMetroid.Diagnostics.SaveStates.DebuggerObjectGraphSerializer.Deserialize<PauseMenuState>(captured);
        restored.BindFixtureArtwork(original);
        var stockSnapshot = stockPause.CaptureRenderSnapshot();
        byte[] stockSnapshotVram = stockPause.ShownVram();
        var restoredSnapshot = restored.CaptureRenderSnapshot();
        byte[] restoredSnapshotVram = restored.ShownVram();
        AssertTrue(stockSnapshotVram.SequenceEqual(restoredSnapshotVram),
            $"restored equipment page adopts current base VRAM while retaining live page state; first diff={FirstDifference(stockSnapshotVram, restoredSnapshotVram):X4}");
        AssertTrue(stockSnapshot.Paint.Colors.SequenceEqual(restoredSnapshot.Paint.Colors),
            "equipment base rebind does not alter live palette state");
        AssertTrue(stockPause.Render().SequenceEqual(restored.Render()),
            "restored equipment base reproduces stock pixels at the retained phase");

        void Reject(PauseEquipmentBaseDocument invalid) => AssertThrows<InvalidDataException>(() =>
            PauseEquipmentBasePresentation.Write(new MemoryStream(), invalid), "invalid equipment base rejected");
        Reject(document with { Version = 2 });
        Reject(document with { Cells = document.Cells[..^1] });
        var invalidCells = document.Cells.ToArray();
        invalidCells[0] = invalidCells[0] with { TileRow = 8 };
        Reject(document with { Cells = invalidCells });
        File.WriteAllText(path, "broken JSON");
        AssertThrows<InvalidDataException>(() => MapPresentationFiles.Load(stock, overrides),
            "invalid equipment base override is never silently replaced");
        File.WriteAllBytes(path, stockBytes);
        try
        {
            File.Delete(stockPath);
            AssertThrows<IOException>(() => MapPresentationFiles.Load(stock, overrides),
                "equipment base override cannot conceal missing stock");
            File.WriteAllText(stockPath, "corrupt stock");
            AssertThrows<InvalidDataException>(() => MapPresentationFiles.Load(stock, overrides),
                "equipment base override cannot conceal failed provenance");
        }
        finally { File.WriteAllBytes(stockPath, stockBytes); }
        Console.WriteLine("Pause equipment base: 1024 native words, guarded transitions, visible edits, state-safe rebind and strict failures pass.");

        static SamusState Samus() => new()
        {
            CollectedBeams = (SamusBeamFlags.Charge | SamusBeamFlags.Ice | SamusBeamFlags.Wave),
            EquippedBeams = (SamusBeamFlags.Charge | SamusBeamFlags.Wave),
            CollectedItems = (SamusEquipmentFlags.VariaSuit | SamusEquipmentFlags.GravitySuit |
                SamusEquipmentFlags.MorphBall | SamusEquipmentFlags.Bombs | SamusEquipmentFlags.HiJumpBoots |
                SamusEquipmentFlags.SpeedBooster),
            EquippedItems = (SamusEquipmentFlags.VariaSuit | SamusEquipmentFlags.MorphBall |
                SamusEquipmentFlags.Bombs | SamusEquipmentFlags.HiJumpBoots),
            MaxHealth = 499, Health = 499, MaxReserveEnergy = 400, ReserveEnergy = 245, ReserveTankMode = ReserveTankMode.Auto,
        };
        static void EnterEquipment(PauseMenuState pause)
        {
            byte nmiFrame = 0; ChangePausePage(pause, SnesButton.R, input => pause.Step(input, input, frameCounterByte: nmiFrame++));
            AssertEqual(PauseScreenMode.Equipment, pause.ScreenMode, "equipment-base fixture enters equipment");
        }
        static int FirstDifference(ReadOnlySpan<byte> left, ReadOnlySpan<byte> right)
        {
            for (int index = 0; index < Math.Min(left.Length, right.Length); index++)
                if (left[index] != right[index]) return index;
            return left.Length == right.Length ? -1 : Math.Min(left.Length, right.Length);
        }
    }

    /// <summary>Blocks the native equipment base tilemap while installed pause pages render.</summary>
    private static CartridgeReadGuard NewPauseEquipmentBaseReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source).ForbidRange(PauseMenuSourceDefinitions.EquipmentPage,
            PauseEquipmentBaseDefinitions.Cells * sizeof(ushort),
            address => $"Installed pause read equipment base at {address:X6}.");
}
