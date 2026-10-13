using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;
using static InstalledFileSelectMapVerification;
using static SuiteTimingVerification;

/// <summary>Checks compiled file-map controller masks and direction priority against native arrow records.</summary>
internal static class MapScrollControlsVerification
{
    /// <summary>Compares every direction combination, scroll trajectory, sound boundary, and installed menu under a guarded bus.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for the native map-arrow controller masks and direction order")]
    internal static void VerifyCompiledMapScrollControls(ISnesAddressSpace bus, AreaMapPresentationCatalog catalog)
    {
        var buttons = new ushort[MapScrollControls.DirectionCount];
        for (int index = 0; index < buttons.Length; index++)
        {
            int record = FileSelectMapRomData.ScrollArrows + index * 10;
            buttons[index] = RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), record + 6);
            AssertEqual(index + 1, RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), record + 8), "compiled map direction order matches native records");
            AssertEqual(buttons[index], MapScrollControls.Buttons[index], "compiled map controller mask matches native record");
        }
        var guard = NewMapScrollControlGuard(bus);
        var system = new Bank80SystemState();
        system.MarkExploredMapTile(AreaId.Crateria, new(0, 0));
        system.MarkExploredMapTile(AreaId.Crateria, new(63, 31));
        var map = catalog.Get(AreaId.Crateria);
        for (int combination = 0; combination < (1 << buttons.Length); combination++)
        {
            ushort held = 0;
            for (int index = 0; index < buttons.Length; index++)
                if ((combination & (1 << index)) != 0) held |= buttons[index];
            var native = new FileSelectMapScroll(bus, map, system, new(240, 128));
            var compiled = new FileSelectMapScroll(guard, map, system, new(240, 128));
            // Sustained direction runs include reaching native boundaries, opposing
            // input precedence, then releasing while an eight-tick step is pending.
            for (int tick = 0; tick < 280; tick++)
            {
                ushort input = tick < 267 ? held : (ushort)0;
                AssertEqual(native.Step(input), compiled.Step(input), "compiled map scroll sound timing");
                AssertEqual((native.Scroll, native.Direction),
                    (compiled.Scroll, compiled.Direction), "compiled map scroll trajectory and direction");
            }
        }
        for (int bit = 0; bit < 16; bit++)
        {
            ushort input = (ushort)(1 << bit);
            if (buttons.Contains(input)) continue;
            var scroll = new FileSelectMapScroll(guard, map, system, new(240, 128));
            AssertTrue(!scroll.Step(input), "unrelated controller bit does not queue map sound");
            AssertEqual(MapScrollDirection.None, scroll.Direction, "unrelated controller bit cannot start map scrolling");
        }
        Suite(nameof(VerifyInstalledFileSelectMenu), () => VerifyInstalledFileSelectMenu(bus, guard, catalog, catalog));
        Console.WriteLine("Compiled map controls: four native masks/order, 16 direction combinations, trajectories/sound boundaries and guarded full menu parity pass.");
    }

    /// <summary>Forbids runtime reads of controller binding and direction operands in native arrow records.</summary>
    private static CartridgeReadGuard NewMapScrollControlGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source).Forbid(
            address => (uint)(address - FileSelectMapRomData.ScrollArrows) < MapScrollControls.DirectionCount * 10 &&
                (address - FileSelectMapRomData.ScrollArrows) % 10 >= 6,
            _ => "Map navigation read controller bindings from the visual arrow ROM records.");
}
