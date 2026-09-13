using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

internal static partial class Program
{
    private static void VerifyCompiledMapScrollControls(ISnesAddressSpace bus, AreaMapPresentationCatalog catalog)
    {
        var buttons = new ushort[MapScrollControls.DirectionCount];
        for (int index = 0; index < buttons.Length; index++)
        {
            int record = FileSelectMapRomData.ScrollArrows + index * 10;
            buttons[index] = RomDataReader.ReadWordFixedBank(bus, record + 6);
            AssertEqual(index + 1, RomDataReader.ReadWordFixedBank(bus, record + 8), "compiled map direction order matches native records");
            AssertEqual(buttons[index], MapScrollControls.Buttons[index], "compiled map controller mask matches native record");
        }
        var guard = new MapScrollControlReadGuard(bus);
        var system = new Bank80SystemState();
        system.MarkExploredMapTile(AreaId.Crateria, 0, 0);
        system.MarkExploredMapTile(AreaId.Crateria, 63, 31);
        var map = catalog.Get(AreaId.Crateria);
        for (int combination = 0; combination < (1 << buttons.Length); combination++)
        {
            ushort held = 0;
            for (int index = 0; index < buttons.Length; index++)
                if ((combination & (1 << index)) != 0) held |= buttons[index];
            var native = new FileSelectMapScroll(bus, map, system, 240, 128, buttons);
            var compiled = new FileSelectMapScroll(guard, map, system, 240, 128);
            // Sustained direction runs include reaching native boundaries, opposing
            // input precedence, then releasing while an eight-tick step is pending.
            for (int tick = 0; tick < 280; tick++)
            {
                ushort input = tick < 267 ? held : (ushort)0;
                AssertEqual(native.Step(input), compiled.Step(input), "compiled map scroll sound timing");
                AssertEqual((native.Horizontal, native.Vertical, native.Direction),
                    (compiled.Horizontal, compiled.Vertical, compiled.Direction), "compiled map scroll trajectory and direction");
            }
        }
        for (int bit = 0; bit < 16; bit++)
        {
            ushort input = (ushort)(1 << bit);
            if (buttons.Contains(input)) continue;
            var scroll = new FileSelectMapScroll(guard, map, system, 240, 128);
            AssertTrue(!scroll.Step(input), "unrelated controller bit does not queue map sound");
            AssertEqual(MapScrollDirection.None, scroll.Direction, "unrelated controller bit cannot start map scrolling");
        }
        VerifyInstalledFileSelectMenu(bus, guard, catalog, catalog);
        Console.WriteLine("Compiled map controls: four native masks/order, 16 direction combinations, trajectories/sound boundaries and guarded full menu parity pass.");
    }

    private sealed class MapScrollControlReadGuard(ISnesAddressSpace source) : ISnesAddressSpace
    {
        public byte ReadByte(int address)
        {
            int offset = address - FileSelectMapRomData.ScrollArrows;
            if ((uint)offset < MapScrollControls.DirectionCount * 10 && offset % 10 >= 6)
                throw new InvalidOperationException("Map navigation read controller bindings from the visual arrow ROM records.");
            return source.ReadByte(address);
        }
        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
