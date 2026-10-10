using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rom;
using SuperMetroid.Desktop;

internal static partial class Program
{
    /// <summary>Compares installed reserve-tank artwork with native menu output and verifies that presentation edits leave gameplay behavior intact.</summary>
    /// <param name="bus">Retail address space used for native rendering and guarded production execution.</param>
    /// <param name="stock">Directory containing the validated stock reserve-tank presentation.</param>
    /// <param name="overrides">Directory used to write and reload the edited reserve-tank document.</param>
    /// <param name="catalog">Installed area-map presentation catalog supplying reserve-tank assets.</param>
    private static void VerifyPauseReserveTankAssets(ISnesAddressSpace bus, string stock, string overrides, AreaMapPresentationCatalog catalog)
    {
        Directory.CreateDirectory(overrides);
        string stockPath = Path.Combine(stock, PauseReserveTankDefinitions.FileName), path = Path.Combine(overrides, PauseReserveTankDefinitions.FileName);
        byte[] bytes = File.ReadAllBytes(stockPath);
        var document = JsonSerializer.Deserialize<PauseReserveTankDocument>(bytes, MapPresentationFormat.JsonOptions)!;
        var guard = new ReserveTankAssetReadGuard(bus);
        Suite(nameof(VerifyPauseReserveAnchors), () => VerifyPauseReserveAnchors(bus));
        int comparisons = 0;
        Suite(nameof(VerifyPauseReserveNativeFrames), () => VerifyPauseReserveNativeFrames(bus, catalog.PauseReserveTanks));
        foreach (ushort capacity in new ushort[] { 0, 100, 200, 300, 400 })
        {
            var nativeSamus = new SamusState { MaxReserveEnergy = capacity, ReserveTankMode = 2 };
            var installedSamus = new SamusState { MaxReserveEnergy = capacity, ReserveTankMode = 2 };
            var native = Create(bus, nativeSamus, catalog);
            var installed = Create(guard, installedSamus, catalog);
            foreach (ushort supply in ReserveSupplySamples(capacity))
            foreach (byte phase in new byte[] { 0, 4 })
            {
                nativeSamus.ReserveEnergy = installedSamus.ReserveEnergy = supply;
                native.Step(0, 0, nmiFrameCounter8: phase); installed.Step(0, 0, nmiFrameCounter8: phase);
                var expected = native.CaptureRenderSnapshot(); var actual = installed.CaptureRenderSnapshot();
                AssertTrue(expected.Memory.Oam.SequenceEqual(actual.Memory.Oam), "every legal reserve supply/flicker state matches native ordered OAM with reads forbidden");
                AssertTrue(native.Render().AsSpan().SequenceEqual(installed.Render()), "every legal reserve supply/flicker state matches native pixels");
                if (capacity == 100 && supply == 1 && phase == 4)
                {
                    using var state = new MemoryStream(); DebuggerObjectGraphSerializer.Serialize(state, installed); state.Position = 0;
                    var restored = DebuggerObjectGraphSerializer.Deserialize<PauseMenuState>(state); restored.BindMapPresentation(catalog);
                    AssertTrue(actual.Memory.Oam.SequenceEqual(restored.CaptureRenderSnapshot().Memory.Oam), "restored nonzero fill flicker phase retains exact OAM");
                    AssertTrue(restored.Render().AsSpan().SequenceEqual(restored.Render()), "repeated draw does not advance fill flicker");
                }
                comparisons++;
            }
        }
        var anchors = document.Anchors.ToArray(); anchors[0] = new(120, 140);
        var frames = new Dictionary<string, SpriteVisualPart[]>(document.Frames)
        {
            ["Full"] = [new() { OffsetX = 12, OffsetY = -4, TileColumn = 2, TileRow = 3, Size = 16,
                Palette = null, Priority = 3, FlipX = true, FlipY = true }], ["EndCap"] = [],
        };
        var custom = document with { Anchors = anchors, Palette = 5, Frames = frames };
        using (var output = File.Create(path)) PauseReserveTankPresentation.Write(output, custom);
        var edited = AreaMapPresentationCatalog.Load(stock, overrides);
        AssertTrue(catalog.ContentIdentity != edited.ContentIdentity, "reserve-only edit changes content identity");
        var samus = new SamusState { MaxReserveEnergy = 100, ReserveEnergy = 100, ReserveTankMode = 2, Health = 50, MaxHealth = 99 };
        var menu = Create(guard, samus, edited);
        var snapshot = menu.CaptureRenderSnapshot();
        int last = (snapshot.Memory.ModeledSpriteCount - 1) * 4;
        AssertEqual((byte)132, snapshot.Memory.Oam[last], "actual menu uses custom tank X");
        AssertEqual((byte)136, snapshot.Memory.Oam[last + 1], "actual menu uses custom tank Y");
        AssertEqual(SnesObjAttributeWord.Create(50, 5, 3, SnesTileFlipFlags.Horizontal | SnesTileFlipFlags.Vertical).Raw,
            (ushort)(snapshot.Memory.Oam[last + 2] | snapshot.Memory.Oam[last + 3] << 8), "actual tank tile/palette/priority/flips");
        var editedPixels = menu.Render();
        AssertTrue(editedPixels.AsSpan().SequenceEqual(RenderForComparison(snapshot)), "edited reserve reaches captured renderer");
        using (var state = new MemoryStream())
        {
            DebuggerObjectGraphSerializer.Serialize(state, new object[] { menu, samus }); state.Position = 0;
            var restored = DebuggerObjectGraphSerializer.Deserialize<object[]>(state); menu = (PauseMenuState)restored[0]; samus = (SamusState)restored[1];
        }
        menu.BindMapPresentation(catalog);
        AssertTrue(!editedPixels.AsSpan().SequenceEqual(menu.Render()), "restore binds current stock artwork instead of frozen edited content");
        menu.BindMapPresentation(edited);
        AssertTrue(editedPixels.AsSpan().SequenceEqual(menu.Render()), "restoring current edited content preserves exact pixels and phase");
        menu.Step(0, (ushort)SnesButton.Down); menu.Step(0, (ushort)SnesButton.A);
        AssertEqual((ushort)51, samus.Health, "custom reserve art cannot change transfer amount");
        AssertEqual((ushort)99, samus.ReserveEnergy, "custom reserve art cannot change reserve consumption");
        void Reject(PauseReserveTankDocument invalid) => AssertThrows<InvalidDataException>(() => PauseReserveTankPresentation.Write(new MemoryStream(), invalid), "invalid reserve presentation rejected");
        Reject(document with { Version = 2 }); Reject(document with { Palette = 8 }); Reject(document with { Anchors = [] }); Reject(document with { Frames = [] });
        var badAnchors = anchors.ToArray(); badAnchors[0] = new(256, 0); Reject(document with { Anchors = badAnchors });
        Reject(custom with { Frames = new(frames) { ["Full"] = [frames["Full"][0] with { TileColumn = 15 }] } });
        File.WriteAllText(path, "bad JSON"); AssertThrows<InvalidDataException>(() => AreaMapPresentationCatalog.Load(stock, overrides), "bad reserve override never falls back");
        File.WriteAllBytes(path, bytes);
        try
        {
            File.Delete(stockPath); AssertThrows<IOException>(() => AreaMapPresentationCatalog.Load(stock, overrides), "override cannot hide missing reserve stock");
            File.WriteAllText(stockPath, "corrupt stock"); AssertThrows<InvalidDataException>(() => AreaMapPresentationCatalog.Load(stock, overrides), "override cannot hide corrupt reserve provenance");
        }
        finally { File.WriteAllBytes(stockPath, bytes); }
        Console.WriteLine($"Reserve presentation: 180 native composition/capacity cases and {comparisons} fill-step boundary supply/flicker menu frames; edits, restore, transfer and strict failures pass.");
        // $82:B2AA draws full tanks from supply / 100, then one partial tank from the remainder's
        // 14-unit fill step, flickering when the remainder is between steps. Each tank's empty,
        // step-start, step-start+1 and step-end supplies cover every distinct drawing case.
        static IEnumerable<ushort> ReserveSupplySamples(ushort capacity)
        {
            var samples = new SortedSet<ushort>();
            for (int tank = 0; tank * PauseReserveTankRomData.EnergyPerTank <= capacity; tank++)
            {
                int baseSupply = tank * PauseReserveTankRomData.EnergyPerTank;
                samples.Add((ushort)baseSupply);
                for (int step = 0; step * PauseReserveTankRomData.EnergyPerFillStep < PauseReserveTankRomData.EnergyPerTank; step++)
                {
                    int start = step * PauseReserveTankRomData.EnergyPerFillStep;
                    int end = Math.Min(start + PauseReserveTankRomData.EnergyPerFillStep, PauseReserveTankRomData.EnergyPerTank) - 1;
                    foreach (int remainder in new[] { start, start + 1, end })
                        if (remainder is > 0 and < PauseReserveTankRomData.EnergyPerTank && baseSupply + remainder <= capacity)
                            samples.Add((ushort)(baseSupply + remainder));
                }
            }
            return samples;
        }
        static PauseMenuState Create(ISnesAddressSpace source, SamusState state, AreaMapPresentationCatalog content)
        {
            var pause = new PauseMenuState(source, state, new Bank80SystemState(), AreaId.Crateria, 0, 0, mapPresentation: content);
            EnterPauseEquipment(pause);
            return pause;
        }
    }
    /// <summary>Rejects cartridge reads from reserve-tank presentation ranges that installed assets must replace.</summary>
    private sealed class ReserveTankAssetReadGuard : ISnesAddressSpace, IImportCartridgeSource
    {
        /// <summary>Underlying address space delegated reads and writes that are not blocked as installed artwork data.</summary>
        private readonly ISnesAddressSpace source;
        /// <summary>Cartridge byte addresses belonging to reserve-tank graphics or their selector and frame definitions.</summary>
        private readonly HashSet<int> blocked = [];

        /// <summary>Creates a read guard populated with the cartridge ranges that the installed reserve-tank art replaces.</summary>
        /// <param name="source">Retail address space used to resolve the original frame pointers and wrapped for later access.</param>
        public ReserveTankAssetReadGuard(ISnesAddressSpace source)
        {
            this.source = source; Add(0x82c1d6, 14); Add(0x82b3d9, 32);
            foreach (ushort id in new ushort[] { 0x1b, 0x1f, 0x20, 0x21, 0x22, 0x23, 0x24, 0x25, 0x26, 0x27 })
            {
                Add(0x82c569 + id * 2, 2);
                int pointer = 0x820000 | RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(source), 0x82c569 + id * 2);
                Add(pointer, 2 + RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(source), pointer) * 5);
            }
        }
        /// <summary>Adds a contiguous cartridge byte range to the set forbidden to installed rendering.</summary>
        /// <param name="address">First byte address in the range.</param>
        /// <param name="count">Number of consecutive bytes to block.</param>
        private void Add(int address, int count) { for (int i = 0; i < count; i++) blocked.Add(address + i); }

        /// <summary>Routes a cartridge import request through the guard's checked byte-read path.</summary>
        /// <param name="address">Cartridge address requested by the importer.</param>
        /// <returns>The underlying byte when the address is not reserved artwork data.</returns>
        public byte ReadCartridgeByte(int address) => ReadByte(address);

        /// <summary>Rejects reads from reserved artwork ranges and delegates other addresses to the wrapped source.</summary>
        /// <param name="address">Address requested from the SNES memory space.</param>
        /// <returns>The underlying byte for an allowed read.</returns>
        public byte ReadByte(int address) => blocked.Contains(address) ? throw new InvalidOperationException($"Installed pause read reserve visual ROM at {address:X6}.") : source.ReadByte(address);

        /// <summary>Forwards a write unchanged to the wrapped address space.</summary>
        /// <param name="address">Destination address.</param>
        /// <param name="value">Byte to store.</param>
        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
