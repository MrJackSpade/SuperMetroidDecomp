using System.Buffers.Binary;
using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rom;

internal static partial class Program
{
    /// <summary>Verifies the installed equipment-page base against ROM and checks edits, transitions, live-state rebinding, and guarded rendering.</summary>
    /// <param name="bus">Cartridge address space supplying native tilemap words and permitted game reads.</param>
    /// <param name="stock">Directory containing the stock pause-equipment base presentation.</param>
    /// <param name="overrides">Directory used to write and reload the edited presentation override.</param>
    /// <param name="original">Loaded map presentation whose stock equipment base is checked against the cartridge.</param>
    private static void VerifyPauseEquipmentBaseAssets(ISnesAddressSpace bus, string stock, string overrides,
        AreaMapPresentationCatalog original)
    {
        Directory.CreateDirectory(overrides);
        string stockPath = Path.Combine(stock, PauseEquipmentBaseDefinitions.FileName);
        string path = Path.Combine(overrides, PauseEquipmentBaseDefinitions.FileName);
        byte[] stockBytes = File.ReadAllBytes(stockPath);
        var document = JsonSerializer.Deserialize<PauseEquipmentBaseDocument>(stockBytes, MapPresentationFormat.JsonOptions)!;
        byte[] nativeBase = RomDataReader.ReadFixedBank(CartridgeImportSource.Require(bus), PauseEquipmentBaseDefinitions.Source,
            PauseEquipmentBaseDefinitions.Cells * sizeof(ushort));
        AssertTrue(nativeBase.AsSpan().SequenceEqual(original.PauseEquipmentBase.CreateTilemap()),
            "all 1024 equipment base cells match native words");

        var guard = new PauseEquipmentBaseReadGuard(bus);
        var native = new PauseMenuState(bus, Samus(), new Bank80SystemState(),
            AreaId.Norfair, 0, 0, mapPresentation: original);
        var installed = new PauseMenuState(guard, Samus(), new Bank80SystemState(), AreaId.Norfair, 0, 0,
            mapPresentation: original);
        int toMap = PausePageTransitionNativeLength + 1;
        for (int frame = 0; frame < toMap + PausePageTransitionNativeLength + 2; frame++)
        {
            ushort input = frame == 0 ? (ushort)SnesButton.R : frame == toMap ? (ushort)SnesButton.L : (ushort)0;
            native.Step(input, input, nmiFrameCounter8: (byte)frame);
            installed.Step(input, input, nmiFrameCounter8: (byte)frame);
            AssertTrue(native.CaptureRenderSnapshot().Memory.Vram.SequenceEqual(installed.CaptureRenderSnapshot().Memory.Vram),
                "equipment base migration retains exact native VRAM through both page transitions");
            AssertTrue(native.Render().AsSpan().SequenceEqual(installed.Render()),
                "equipment base migration retains native pixels with its source forbidden");
        }
        AssertEqual(0, installed.ScreenMode, "equipment base fixture returns to the map page");

        const int editedCell = 194;
        AssertTrue(!PauseEquipmentBaseDefinitions.IsLiveOwnedCell(editedCell) &&
            !PauseEquipmentBaseDefinitions.IsArrowCell(editedCell), "edited control cell belongs to static base presentation");
        PauseBackdropCell[] cells = document.Cells.ToArray();
        cells[editedCell] = cells[editedCell] with
        {
            Atlas = PauseBackdropDefinitions.MapAtlas, TileColumn = 4, TileRow = 0,
            Palette = 3, Priority = true, FlipX = false, FlipY = false,
        };
        using (var output = File.Create(path)) PauseEquipmentBasePresentation.Write(output, document with { Cells = cells });
        var edited = AreaMapPresentationCatalog.Load(stock, overrides);
        AssertTrue(original.ContentIdentity != edited.ContentIdentity, "equipment base override changes content identity");
        var stockPause = new PauseMenuState(guard, Samus(), new Bank80SystemState(), AreaId.Norfair, 0, 0,
            mapPresentation: original);
        var editedPause = new PauseMenuState(guard, Samus(), new Bank80SystemState(), AreaId.Norfair, 0, 0,
            mapPresentation: edited);
        EnterEquipment(stockPause); EnterEquipment(editedPause);
        AssertTrue(!stockPause.Render().AsSpan().SequenceEqual(editedPause.Render()),
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
        SuperMetroid.Desktop.DebuggerObjectGraphSerializer.Serialize(captured, editedPause); captured.Position = 0;
        var restored = SuperMetroid.Desktop.DebuggerObjectGraphSerializer.Deserialize<PauseMenuState>(captured);
        restored.BindMapPresentation(original);
        var stockSnapshot = stockPause.CaptureRenderSnapshot();
        var restoredSnapshot = restored.CaptureRenderSnapshot();
        AssertTrue(stockSnapshot.Memory.Vram.SequenceEqual(restoredSnapshot.Memory.Vram),
            $"restored equipment page adopts current base VRAM while retaining live page state; first diff={FirstDifference(stockSnapshot.Memory.Vram, restoredSnapshot.Memory.Vram):X4}");
        AssertTrue(stockSnapshot.Memory.Cgram.SequenceEqual(restoredSnapshot.Memory.Cgram),
            "equipment base rebind does not alter live palette state");
        AssertTrue(stockPause.Render().AsSpan().SequenceEqual(restored.Render()),
            "restored equipment base reproduces stock pixels at the retained phase");

        void Reject(PauseEquipmentBaseDocument invalid) => AssertThrows<InvalidDataException>(() =>
            PauseEquipmentBasePresentation.Write(new MemoryStream(), invalid), "invalid equipment base rejected");
        Reject(document with { Version = 2 });
        Reject(document with { Cells = document.Cells[..^1] });
        var invalidCells = document.Cells.ToArray();
        invalidCells[0] = invalidCells[0] with { TileRow = 8 };
        Reject(document with { Cells = invalidCells });
        File.WriteAllText(path, "broken JSON");
        AssertThrows<InvalidDataException>(() => AreaMapPresentationCatalog.Load(stock, overrides),
            "invalid equipment base override is never silently replaced");
        File.WriteAllBytes(path, stockBytes);
        try
        {
            File.Delete(stockPath);
            AssertThrows<IOException>(() => AreaMapPresentationCatalog.Load(stock, overrides),
                "equipment base override cannot conceal missing stock");
            File.WriteAllText(stockPath, "corrupt stock");
            AssertThrows<InvalidDataException>(() => AreaMapPresentationCatalog.Load(stock, overrides),
                "equipment base override cannot conceal failed provenance");
        }
        finally { File.WriteAllBytes(stockPath, stockBytes); }
        Console.WriteLine("Pause equipment base: 1024 native words, guarded transitions, visible edits, state-safe rebind and strict failures pass.");

        static SamusState Samus() => new()
        {
            CollectedBeams = (ushort)(SamusBeamFlags.Charge | SamusBeamFlags.Ice | SamusBeamFlags.Wave),
            EquippedBeams = (ushort)(SamusBeamFlags.Charge | SamusBeamFlags.Wave),
            CollectedItems = (ushort)(SamusEquipmentFlags.VariaSuit | SamusEquipmentFlags.GravitySuit |
                SamusEquipmentFlags.MorphBall | SamusEquipmentFlags.Bombs | SamusEquipmentFlags.HiJumpBoots |
                SamusEquipmentFlags.SpeedBooster),
            EquippedItems = (ushort)(SamusEquipmentFlags.VariaSuit | SamusEquipmentFlags.MorphBall |
                SamusEquipmentFlags.Bombs | SamusEquipmentFlags.HiJumpBoots),
            MaxHealth = 499, Health = 499, MaxReserveEnergy = 400, ReserveEnergy = 245, ReserveTankMode = 1,
        };
        static void EnterEquipment(PauseMenuState pause)
        {
            byte nmiFrame = 0; ChangePausePage(pause, SnesButton.R, input => pause.Step(input, input, nmiFrameCounter8: nmiFrame++));
            AssertEqual(1, pause.ScreenMode, "equipment-base fixture enters equipment");
        }
        static int FirstDifference(ReadOnlySpan<byte> left, ReadOnlySpan<byte> right)
        {
            for (int index = 0; index < Math.Min(left.Length, right.Length); index++)
                if (left[index] != right[index]) return index;
            return left.Length == right.Length ? -1 : Math.Min(left.Length, right.Length);
        }
    }

    /// <summary>Address-space adapter that rejects runtime reads of the migrated pause-equipment base tilemap.</summary>
    /// <param name="source">Underlying address space used for permitted reads and all writes.</param>
    private sealed class PauseEquipmentBaseReadGuard(ISnesAddressSpace source) : ISnesAddressSpace, IImportCartridgeSource
    {
        /// <summary>Routes cartridge-byte requests through the equipment-base read guard.</summary>
        /// <param name="address">Cartridge address requested by the caller.</param>
        public byte ReadCartridgeByte(int address) => ReadByte(address);

        /// <summary>Rejects reads from the migrated tilemap range and forwards other reads to the wrapped address space.</summary>
        /// <param name="address">Address to read from the guarded address space.</param>
        /// <returns>The byte at a permitted address.</returns>
        public byte ReadByte(int address) => address >= PauseEquipmentBaseDefinitions.Source &&
            address < PauseEquipmentBaseDefinitions.Source + PauseEquipmentBaseDefinitions.Cells * sizeof(ushort)
            ? throw new InvalidOperationException($"Installed pause read equipment base at {address:X6}.") : source.ReadByte(address);
        /// <summary>Forwards a write unchanged to the wrapped address space.</summary>
        /// <param name="address">Address that receives the write.</param>
        /// <param name="value">Byte written to that address.</param>
        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
