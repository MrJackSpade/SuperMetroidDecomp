using SuperMetroid.Core;
using SuperMetroid.Tooling;
using System.Text.Json;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Frontend.InstalledFileSelectMapVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Frontend.FileSelect;
using SuperMetroid.Core.Frontend.Map;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.Diagnostics.SaveStates;

namespace SuperMetroid.Verification.Frontend.Map;

/// <summary>Checks installed file-select map arrows against native timing and OAM, including editable art and overrides.</summary>
internal static class MapArrowsVerification
{
    /// <summary>Checks installed arrow timing and OAM against native output, then exercises edited art, serialization, and override validation.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native OAM tables")]
    internal static void VerifyMapArrows(ISnesAddressSpace bus, string stock, string overrides, AreaMapPresentationCatalog original)
    {
        var guard = NewMapArrowGuard(bus);
        var native = new FileSelectMapAnimations(bus, original.Arrows);
        var installed = new FileSelectMapAnimations(guard, original.Arrows);
        for (int tick = 0; tick < 600; tick++)
        {
            bool Available(MapScrollDirection direction) => (tick / 17 + (int)direction) % 5 != 0;
            native.StepArrows(Available); installed.StepArrows(Available);
            AssertTrue(Draw(native).SequenceEqual(Draw(installed)), "all four extracted arrow sprites match native OAM");
            AssertTrue(Counters(native).SequenceEqual(Counters(installed)), "arrow phase/timer and visibility match native including hidden pauses");
        }
        Suite(nameof(VerifyInstalledFileSelectMenu), () => VerifyInstalledFileSelectMenu(bus, guard, original, original));
        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        var document = RequiredJson.Deserialize<MapArrowDocument>(File.ReadAllBytes(Path.Combine(stock, MapArrowFormat.FileName)), options);
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
        AssertTrue(!Draw(changed).SequenceEqual(Draw(control)), "edited anchor moves actual emitted arrow sprite coordinates");
        var graphics = new FileSelectRoomMapGraphics(guard, new Bank80SystemState(), AreaId.Maridia, mapPresentation: original);
        var marker = new FileSelectStationMarker(AreaId.Maridia, 0, original.SaveMarkers);
        AssertTrue(!graphics.Render(new(0, 0), marker, changed).ToArray().SequenceEqual(graphics.Render(new(0, 0), marker, control)),
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
        AssertTrue(before.SequenceEqual(Counters(installed)), "restore/rebind retains exact native phase/timer/visibility");
        AssertTrue(Draw(native).SequenceEqual(Draw(installed)), "restore/rebind retains exact arrow positions and shapes");
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
            return oam.LowTable.ConcatToArray(oam.HighTable);
        }
        static int[] Counters(FileSelectMapAnimations animation)
        {
            return [.. animation.arrows.SelectMany(arrow => new[] { arrow.Timer, arrow.Frame, arrow.Visible ? 1 : 0 })];
        }
    }

    /// <summary>Forbids the native arrow records, animation tables, and sprite programs during installed arrow playback.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle walking the native map-arrow records, animation tables and sprite programs to forbid them")]
    private static MutableCartridgeReadGuard NewMapArrowGuard(ISnesAddressSpace source)
    {
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(source);
        var forbidden = new HashSet<int>();
        for (int index = 0; index < MapArrowDefinitions.Count; index++)
        {
            int record = FileSelectMapRomData.ScrollArrows + index * 10;
            Add(record, 10);
            int animation = RomDataReader.ReadWordFixedBank(cartridge, record + 4);
            int pointer = MapAnimationRomData.SpritePrograms + (animation - 1) * 2;
            int bases = MapAnimationRomData.SpriteBases + (animation - 1) * 2;
            Add(pointer, 2); Add(bases, 2);
            Add(FileSelectMapRomData.MenuObjectBank | RomDataReader.ReadWordFixedBank(cartridge, bases), 2);
            int program = FileSelectMapRomData.MenuObjectBank | RomDataReader.ReadWordFixedBank(cartridge, pointer);
            while (CpuBusRegionRead.Read(source, program) != byte.MaxValue) { Add(program, 3); program += 3; }
            Add(program, 1);
        }
        void Add(int start, int length) { for (int offset = 0; offset < length; offset++) forbidden.Add(start + offset); }
        var guard = new MutableCartridgeReadGuard(source);
        guard.Forbid(forbidden.Contains, address => $"Installed arrows read visual metadata from ROM at {address:X6}.");
        return guard;
    }
}
