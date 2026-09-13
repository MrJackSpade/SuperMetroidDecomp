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
    private static void VerifyPauseReserveTankAssets(ISnesAddressSpace bus, string stock, string overrides, AreaMapPresentationCatalog catalog)
    {
        Directory.CreateDirectory(overrides);
        string stockPath = Path.Combine(stock, PauseReserveTankDefinitions.FileName), path = Path.Combine(overrides, PauseReserveTankDefinitions.FileName);
        byte[] bytes = File.ReadAllBytes(stockPath);
        var document = JsonSerializer.Deserialize<PauseReserveTankDocument>(bytes, MapPresentationFormat.JsonOptions)!;
        var guard = new ReserveTankAssetReadGuard(bus);
        int comparisons = 0;
        foreach (var frame in PauseReserveTankDefinitions.Frames())
        for (int index = 0; index < 6; index++)
        foreach (int occupied in new[] { 0, 127, 128 })
        {
            var expected = new OamBuffer(); var actual = new OamBuffer(); expected.BeginFrame(); actual.BeginFrame();
            for (int i = 0; i < occupied; i++) { expected.AddRawSmallSprite(12, 34, 56); actual.AddRawSmallSprite(12, 34, 56); }
            ushort x = RomDataReader.ReadWordFixedBank(bus, 0x82c1d6 + index * 2);
            ushort y = (ushort)(RomDataReader.ReadWordFixedBank(bus, 0x82c1e2) - 1);
            AssertEqual(new MapLabelPoint(x, y), catalog.PauseReserveTanks.Anchor(index), "reserve anchor matches native X and decremented Y");
            int pointer = 0x820000 | RomDataReader.ReadWordFixedBank(bus, 0x82c569 + frame.Id * 2);
            expected.AddOnScreenSpritemap(bus, pointer, x, y, 0x600);
            catalog.PauseReserveTanks.Draw(actual, frame.Id, index);
            expected.FinalizeFrame(); actual.FinalizeFrame();
            AssertTrue(expected.LowTable.SequenceEqual(actual.LowTable) && expected.HighTable.SequenceEqual(actual.HighTable), "reserve composition matches native OAM including capacity cutoff");
        }
        foreach (ushort capacity in new ushort[] { 0, 100, 200, 300, 400 })
        {
            var nativeSamus = new SamusState { MaxReserveEnergy = capacity, ReserveTankMode = 2 };
            var installedSamus = new SamusState { MaxReserveEnergy = capacity, ReserveTankMode = 2 };
            var native = Create(bus, nativeSamus, null); var installed = Create(guard, installedSamus, catalog);
            for (ushort supply = 0; supply <= capacity; supply++)
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
        AssertTrue(editedPixels.AsSpan().SequenceEqual(SoftwareLayeredSnapshotRenderer.Render(snapshot)), "edited reserve reaches captured renderer");
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
        Console.WriteLine($"Reserve presentation: 180 native composition/capacity cases and {comparisons} exhaustive supply/flicker menu frames; edits, restore, transfer and strict failures pass.");
        static PauseMenuState Create(ISnesAddressSpace source, SamusState state, AreaMapPresentationCatalog? content)
        {
            var pause = new PauseMenuState(source, state, new Bank80SystemState(), AreaId.Crateria, 0, 0, mapPresentation: content);
            pause.Step((ushort)SnesButton.R, (ushort)SnesButton.R); for (int i = 0; i < 32; i++) pause.Step(0, 0);
            return pause;
        }
    }
    private sealed class ReserveTankAssetReadGuard : ISnesAddressSpace
    {
        private readonly ISnesAddressSpace source;
        private readonly HashSet<int> blocked = [];
        public ReserveTankAssetReadGuard(ISnesAddressSpace source)
        {
            this.source = source; Add(0x82c1d6, 14); Add(0x82b3d9, 32);
            foreach (ushort id in new ushort[] { 0x1b, 0x1f, 0x20, 0x21, 0x22, 0x23, 0x24, 0x25, 0x26, 0x27 })
            {
                Add(0x82c569 + id * 2, 2);
                int pointer = 0x820000 | RomDataReader.ReadWordFixedBank(source, 0x82c569 + id * 2);
                Add(pointer, 2 + RomDataReader.ReadWordFixedBank(source, pointer) * 5);
            }
        }
        private void Add(int address, int count) { for (int i = 0; i < count; i++) blocked.Add(address + i); }
        public byte ReadByte(int address) => blocked.Contains(address) ? throw new InvalidOperationException($"Installed pause read reserve visual ROM at {address:X6}.") : source.ReadByte(address);
        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
