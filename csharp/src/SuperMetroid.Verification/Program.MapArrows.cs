using System.Reflection;
using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;
using SuperMetroid.Desktop;

internal static partial class Program
{
    /// <summary>Compares native and installed map arrows, exercises editable positions and timing, and validates restore and failure behavior.</summary>
    /// <param name="bus">Address space used by native and guarded installed menu paths.</param>
    /// <param name="stock">Directory containing the stock map-arrow presentation document.</param>
    /// <param name="overrides">Directory where the edited arrow document is written and loaded.</param>
    /// <param name="original">Selected stock presentation catalog used for native comparisons.</param>
    private static void VerifyMapArrows(ISnesAddressSpace bus, string stock, string overrides, AreaMapPresentationCatalog original)
    {
        var guard = new MapArrowReadGuard(bus);
        var native = new FileSelectMapAnimations(bus, original.Arrows);
        var installed = new FileSelectMapAnimations(guard, original.Arrows);
        for (int tick = 0; tick < 600; tick++)
        {
            bool Available(MapScrollDirection direction) => (tick / 17 + (int)direction) % 5 != 0;
            native.StepArrows(Available); installed.StepArrows(Available);
            AssertTrue(Draw(native).AsSpan().SequenceEqual(Draw(installed)), "all four extracted arrow sprites match native OAM");
            AssertTrue(Counters(native).AsSpan().SequenceEqual(Counters(installed)), "arrow phase/timer and visibility match native including hidden pauses");
        }
        Suite(nameof(VerifyInstalledFileSelectMenu), () => VerifyInstalledFileSelectMenu(bus, guard, original, original));
        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        var document = JsonSerializer.Deserialize<MapArrowDocument>(File.ReadAllBytes(Path.Combine(stock, MapArrowFormat.FileName)), options)!;
        var entries = new Dictionary<string, MapArrowEntry>(document.Arrows);
        entries["Left"] = entries["Left"] with { X = entries["Left"].X + 8, DurationTicks = [9, 11] };
        Directory.CreateDirectory(overrides);
        string path = Path.Combine(overrides, MapArrowFormat.FileName);
        using (var output = File.Create(path)) MapArrowPresentation.Write(output, document with { Arrows = entries });
        byte[] bytes = File.ReadAllBytes(path);
        var edited = AreaMapPresentationCatalog.Load(stock, overrides);
        AssertTrue(original.ContentIdentity != edited.ContentIdentity, "arrow override affects selected content identity");
        var changed = new FileSelectMapAnimations(guard, edited.Arrows);
        var control = new FileSelectMapAnimations(bus, original.Arrows);
        changed.StepArrows(_ => true); control.StepArrows(_ => true);
        AssertTrue(!Draw(changed).AsSpan().SequenceEqual(Draw(control)), "edited anchor moves actual emitted arrow sprite coordinates");
        var graphics = new FileSelectRoomMapGraphics(guard, new Bank80SystemState(), AreaId.Maridia, mapPresentation: original);
        var marker = new FileSelectStationMarker(guard, AreaId.Maridia, 0, original.SaveMarkers);
        AssertTrue(!graphics.Render(0, 0, marker, changed).ToArray().AsSpan().SequenceEqual(graphics.Render(0, 0, marker, control)),
            "edited arrow anchor changes actual composed room-map pixels");
        AssertEqual(11, Counters(changed)[0], "first native decrement advances to authored phase one duration");
        for (int tick = 0; tick < 11; tick++) changed.StepArrows(_ => true);
        AssertEqual(9, Counters(changed)[0], "edited short cycle wraps to authored phase zero duration");
        AssertEqual(0, Counters(changed)[1], "edited short cycle wraps phase index");
        int[] before = Counters(installed);
        using var snapshot = new MemoryStream();
        DebuggerObjectGraphSerializer.Serialize(snapshot, installed); snapshot.Position = 0;
        installed = DebuggerObjectGraphSerializer.Deserialize<FileSelectMapAnimations>(snapshot);
        installed.BindPresentation(original.Arrows);
        AssertTrue(before.AsSpan().SequenceEqual(Counters(installed)), "restore/rebind retains exact native phase/timer/visibility");
        AssertTrue(Draw(native).AsSpan().SequenceEqual(Draw(installed)), "restore/rebind retains exact arrow positions and shapes");
        installed.BindPresentation(edited.Arrows);
        AssertEqual(before[0], Counters(installed)[0], "shorter replacement retains pending delay");
        AssertEqual(before[1] % 2, Counters(installed)[1], "shorter replacement normalizes phase");
        installed.StepArrows(_ => true);
        // Active gameplay no longer has a cartridge fallback for unbinding.
        var rebound = new FileSelectMapAnimations(bus, edited.Arrows);
        AssertThrows<InvalidOperationException>(() => rebound.BindPresentation(null),
            "unbinding arrows without installed presentation fails loudly");
        foreach (int duration in new[] { 0, 255, -1 })
        {
            entries["Left"] = entries["Left"] with { DurationTicks = [duration] };
            AssertThrows<InvalidDataException>(() => MapArrowPresentation.Write(new MemoryStream(), document with { Arrows = entries }), "invalid arrow duration rejected");
        }
        entries.Remove("Down");
        AssertThrows<InvalidDataException>(() => MapArrowPresentation.Write(new MemoryStream(), document with { Arrows = entries }), "missing direction rejected");
        File.WriteAllText(path, "broken arrow override");
        AssertThrows<InvalidDataException>(() => AreaMapPresentationCatalog.Load(stock, overrides), "broken arrow override fails loudly");
        AssertEqual("broken arrow override", File.ReadAllText(path), "broken override never replaced");
        File.WriteAllBytes(path, bytes);
        string stockPath = Path.Combine(stock, MapArrowFormat.FileName);
        byte[] stockBytes = File.ReadAllBytes(stockPath);
        try
        {
            File.WriteAllText(stockPath, "corrupt stock");
            AssertThrows<InvalidDataException>(() => AreaMapPresentationCatalog.Load(stock, overrides), "override does not hide stock arrow hash failure");
        }
        finally { File.WriteAllBytes(stockPath, stockBytes); }
        Console.WriteLine("Map arrows: 600 native phase/OAM ticks, guarded whole-menu transitions, editable positions/durations, restore/rebind and validation pass.");

        byte[] Draw(FileSelectMapAnimations animation)
        {
            var oam = new OamBuffer(); oam.BeginFrame();
            animation.DrawArrows(oam, original.Sprites);
            oam.FinalizeFrame();
            return oam.LowTable.ToArray().Concat(oam.HighTable.ToArray()).ToArray();
        }
        static int[] Counters(FileSelectMapAnimations animation)
        {
            var arrows = (Array)typeof(FileSelectMapAnimations).GetField("arrows", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(animation)!;
            return arrows.Cast<object>().SelectMany(arrow => new[]
            {
                (int)arrow.GetType().GetField("Timer")!.GetValue(arrow)!,
                (int)arrow.GetType().GetField("Frame")!.GetValue(arrow)!,
                (bool)arrow.GetType().GetField("Visible")!.GetValue(arrow)! ? 1 : 0
            }).ToArray();
        }
    }

    /// <summary>Rejects reads of arrow graphics metadata while forwarding permitted cartridge and mutable-memory access.</summary>
    private sealed class MapArrowReadGuard : ISnesAddressSpace, IImportCartridgeSource, ISnesMutableMemory
    {
        /// <summary>Underlying address space used for requests that are permitted by the guard.</summary>
        private readonly ISnesAddressSpace source;

        /// <summary>Bus addresses occupied by native arrow records, animation tables, and sprite instructions.</summary>
        private readonly HashSet<int> forbidden = new();

        /// <summary>Builds the blocked metadata ranges by following the retail arrow definitions and sprite programs.</summary>
        /// <param name="source">Retail-backed address space used to discover native arrow data and serve allowed reads.</param>
        public MapArrowReadGuard(ISnesAddressSpace source)
        {
            this.source = source;
            for (int index = 0; index < MapArrowDefinitions.Count; index++)
            {
                int record = FileSelectMapRomData.ScrollArrows + index * 10;
                Add(record, 10);
                int animation = RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(source), record + 4);
                int pointer = MapAnimationRomData.SpritePrograms + (animation - 1) * 2;
                int bases = MapAnimationRomData.SpriteBases + (animation - 1) * 2;
                Add(pointer, 2); Add(bases, 2);
                Add(FileSelectMapRomData.MenuObjectBank | RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(source), bases), 2);
                int program = FileSelectMapRomData.MenuObjectBank | RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(source), pointer);
                while (source.ReadByte(program) != byte.MaxValue) { Add(program, 3); program += 3; }
                Add(program, 1);
            }
            void Add(int start, int length) { for (int offset = 0; offset < length; offset++) forbidden.Add(start + offset); }
        }
        /// <summary>Throws when an installed arrow path attempts to read one of the recorded ROM metadata bytes.</summary>
        /// <param name="address">Bus address about to be accessed.</param>
        /// <exception cref="InvalidOperationException">The address belongs to native arrow presentation metadata.</exception>
        private void RejectArrowSource(int address)
        {
            if (forbidden.Contains(address))
                throw new InvalidOperationException($"Installed arrows read visual metadata from ROM at {address:X6}.");
        }
        /// <summary>Checks ordinary byte reads against the blocked metadata set before forwarding them.</summary>
        /// <param name="address">Bus address of the requested byte.</param>
        /// <returns>The forwarded byte if the address is allowed.</returns>
        public byte ReadByte(int address)
        {
            RejectArrowSource(address);
            return source.ReadByte(address);
        }
        /// <summary>Checks cartridge reads against the blocked metadata set before using the import source.</summary>
        /// <param name="address">Cartridge bus address of the requested byte.</param>
        /// <returns>The imported cartridge byte if the address is allowed.</returns>
        public byte ReadCartridgeByte(int address)
        {
            RejectArrowSource(address);
            return CartridgeImportSource.Require(source).ReadCartridgeByte(address);
        }
        /// <summary>Reads work RAM through the wrapped mutable-memory interface.</summary>
        /// <param name="address">Work-RAM bus address to read.</param>
        /// <returns>The byte stored at the requested work-RAM address.</returns>
        public byte ReadWorkRamByte(int address) =>
            (source as ISnesMutableMemory ?? throw new InvalidOperationException(
                "Map-arrow guard source does not expose WRAM.")).ReadWorkRamByte(address);

        /// <summary>Reads save RAM through the wrapped mutable-memory interface.</summary>
        /// <param name="address">Save-RAM bus address to read.</param>
        /// <returns>The byte stored at the requested save-RAM address.</returns>
        public byte ReadSaveRamByte(int address) =>
            (source as ISnesMutableMemory ?? throw new InvalidOperationException(
                "Map-arrow guard source does not expose SRAM.")).ReadSaveRamByte(address);

        /// <summary>Forwards a write unchanged to the wrapped address space.</summary>
        /// <param name="address">Bus address receiving the write.</param>
        /// <param name="value">Byte written at <paramref name="address"/>.</param>
        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
