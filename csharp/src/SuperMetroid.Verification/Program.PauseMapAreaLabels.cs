using System.Reflection;
using SuperMetroid.AssetExtraction;
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
    private static void VerifyPauseMapAreaLabels()
    {
        var bus = CartridgeImportAddressSpace.LoadRetailRom("Super Metroid.smc");
        var cart = CartridgeImportSource.Require(bus);
        var rawVram = new SnesVram();
        var tiles = new byte[0x2000];
        for (int i = 0; i < tiles.Length; i++) tiles[i] = bus.ReadCartridgeByte(0xb6c000 + i);
        rawVram.LoadBytes(0x4000, tiles);
        var nativeColors = new SnesCgram();
        for (int color = 0; color < 256; color++) nativeColors.SetColor(color, Read(0xb6f000 + color * 2));
        foreach (AreaId area in new[] { AreaId.Crateria, AreaId.Brinstar, AreaId.Norfair, AreaId.WreckedShip, AreaId.Maridia })
        {
            var system = new Bank80SystemState();
            var labels = new FileSelectMapIcons(system, area);
            var presentation = RetailPresentationFixture();
            labels.BindLandmarks(presentation.Landmarks);
            labels.BindSprites(presentation.Sprites);
            var gated = new OamBuffer(); gated.BeginFrame();
            labels.DrawElevatorLabels(gated, 0, 0);
            AssertEqual(0, gated.NextByteOffset, "undownloaded area hides destination names");
            system.SetAreaMapAcquired(area == AreaId.Crateria ? AreaId.Brinstar : AreaId.Crateria);
            labels.DrawElevatorLabels(gated, 0, 0);
            AssertEqual(0, gated.NextByteOffset, "another area's map does not reveal these destinations");
            system.SetAreaMapAcquired(area);
            var pause = CreateRetailPauseFixture(bus, new SamusState(), system, area, 0, 0);
            int list = 0x820000 | Read(0x82c74d + (int)area * 2);
            ushort x = unchecked((ushort)(Read(list) - 128)), y = unchecked((ushort)(Read(list + 2) - 100));
            Set("mapHorizontalScroll", x); Set("mapVerticalScroll", y);
            var native = new OamBuffer(); native.BeginFrame();
            for (int entry = list; Read(entry) != 0xffff; entry += 6)
            {
                int sprite = Read(entry + 4);
                DrawImportedSpritemap(bus, native, 0x820000 | Read(0x82c569 + sprite * 2),
                    unchecked((ushort)(Read(entry) - x)), unchecked((ushort)(Read(entry + 2) - y)), 0);
            }
            int labelBytes = native.NextByteOffset;
            native.FinalizeFrame();
            var pixels = pause.Render();
            var oam = Get<OamBuffer>("oam");
            AssertTrue(oam.LastFinalizedSpriteCount * 4 >= labelBytes, $"{area} pause emits all native destination label parts");
            int start = oam.LastFinalizedSpriteCount * 4 - labelBytes;
            AssertTrue(oam.LowTable.Slice(start, labelBytes).SequenceEqual(native.LowTable[..labelBytes]),
                $"{area} destination spelling, order, coordinates and palette match raw ROM OAM");
            AssertLabels(start, "stable map");
            var nativePixels = SnesObjRenderer.Render(native, rawVram, nativeColors, PauseMenuLayout.ObjectSelection);
            var installedPixels = SnesObjRenderer.Render(native, Get<SnesVram>("vram"), Get<SnesCgram>("cgram"), PauseMenuLayout.ObjectSelection);
            AssertTrue(nativePixels.AsSpan().SequenceEqual(installedPixels), $"{area} installed label artwork and colors equal raw ROM");
            // The high-priority BG2 holder covers destination names at the map edges.
            // Confirm visible lettering in the map interior, separately from complete OBJ/OAM equality above.
            int visiblePixels = 0;
            for (int py = 64; py < 176; py++)
                for (int px = 32; px < 224; px++)
                {
                    int i = py * 256 + px;
                    if (nativePixels[i].A != 0) { visiblePixels++; AssertEqual(nativePixels[i], pixels[i], $"{area} visible native label at {px},{py}"); }
                }
            AssertTrue(visiblePixels > 0, $"{area} reference label is on screen");
            pause.AdvanceAnimations(fadingOut: true); _ = pause.Render();
            AssertLabels(0, "outer unpause fade-out");
            var beforeRestore = pause.Render();
            using (var stream = new MemoryStream())
            {
                DebuggerObjectGraphSerializer.Serialize(stream, pause);
                stream.Position = 0;
                pause = DebuggerObjectGraphSerializer.Deserialize<PauseMenuState>(stream);
                pause.BindMapPresentation(RetailPresentationFixture());
            }
            AssertTrue(beforeRestore.AsSpan().SequenceEqual(pause.Render()), "restoring preserves destination-label fade priority");
            var fields = typeof(PauseMenuState).GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                .Where(field => !field.IsDefined(typeof(NonSerializedAttribute))).OrderBy(field => field.Name, StringComparer.Ordinal).ToArray();
            AssertTrue(DebuggerStateFieldMigrations.SelectSerializedFields(typeof(PauseMenuState), fields, fields.Length - 1)
                .SequenceEqual(fields.Where(field => field.Name != "mapLabelsBeforeIcons")), "prior pause layout retains serialized field mapping");
            pause.Step((ushort)SnesButton.R, 0); _ = pause.Render();
            AssertLabels(Get<OamBuffer>("oam").LastFinalizedSpriteCount * 4 - labelBytes, "R input stable-map dispatch");
            pause.Step(0, 0); _ = pause.Render();
            AssertLabels(0, "map-to-equipment fade-out");
            for (int frame = 0; frame < 29; frame++) pause.Step(0, 0);
            _ = pause.Render();
            AssertEqual(1, pause.ScreenMode, "equipment page reached");
            AssertTrue(Get<OamBuffer>("oam").LastFinalizedSpriteCount * 4 < labelBytes, "equipment page emits no destination names");
            pause.Step((ushort)SnesButton.L, 0);
            for (int frame = 0; frame < 16; frame++) pause.Step(0, 0);
            _ = pause.Render();
            AssertEqual(0, pause.ScreenMode, "map fade-in reached");
            AssertLabels(Get<OamBuffer>("oam").LastFinalizedSpriteCount * 4 - labelBytes, "equipment-to-map fade-in");
            void AssertLabels(int firstByte, string phase)
            {
                var actual = Get<OamBuffer>("oam");
                AssertTrue(actual.LowTable.Slice(firstByte, labelBytes).SequenceEqual(native.LowTable[..labelBytes]), $"{area} {phase} native label order/coordinates");
                for (int sprite = 0; sprite < labelBytes / 4; sprite++)
                {
                    int actualSprite = firstByte / 4 + sprite;
                    int expectedBits = (native.HighTable[sprite / 4] >> ((sprite % 4) * 2)) & 3;
                    int actualBits = (actual.HighTable[actualSprite / 4] >> ((actualSprite % 4) * 2)) & 3;
                    AssertEqual(expectedBits, actualBits, $"{area} {phase} label X wrapping and sprite size");
                }
            }
            // The separate current-area heading was already installed correctly.
            int title = 0x820000 | Read(0x82965f + (int)area * 2);
            for (int word = 0; word < 12; word++)
                AssertEqual(Read(title + word * 2), Get<SnesVram>("vram").ReadWord(0x38aa + word), $"{area} heading agrees with native BG2 patch");
            void Set(string name, object value) => typeof(PauseMenuState).GetField(name, BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(pause, value);
            T Get<T>(string name) => (T)typeof(PauseMenuState).GetField(name, BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(pause)!;
        }
        Console.WriteLine("Pause area labels: five outside-Tourian areas match raw ROM destination OAM/pixels and area-title words.");
        ushort Read(int address) => RomDataReader.ReadWordFixedBank(cart, address);
    }
}