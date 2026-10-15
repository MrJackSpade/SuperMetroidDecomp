using SuperMetroid.Core.Rooms;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using static SuperMetroid.Verification.Frontend.Map.MapSpritesVerification;
using static SuperMetroid.Verification.Frontend.Pause.PauseEquipmentRulesVerification;
using static SuperMetroid.Verification.Harness.RetailPresentationFixtureVerification;
using SuperMetroid.Core.Frontend.FileSelect;
using SuperMetroid.Core.Frontend.Pause;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Diagnostics.SaveStates;
namespace SuperMetroid.Verification.Frontend.Pause;
using SuperMetroid.Verification.Harness;

/// <summary>Checks pause-map elevator destination labels and area headings against native OAM, pixels, and VRAM words.</summary>
internal static class PauseMapAreaLabelsVerification
{
    /// <summary>Checks five mapped areas through map acquisition, pause transitions, and debugger restore.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native CGRAM slots, native OAM tables, native VRAM words")]
    [CpuBusBoundary("exact bus test: retail ROM oracle for the pause map area-label tiles, palette and heading words")]
    internal static void VerifyPauseMapAreaLabels()
    {
        var bus = CartridgeImportAddressSpaceTooling.LoadRetailRom("Super Metroid.smc");
        var cart = CartridgeImportSource.Require(bus);
        var rawVram = new SnesVram();
        var tiles = new byte[0x2000];
        for (int i = 0; i < tiles.Length; i++) tiles[i] = bus.ReadCartridgeByte(0xb6c000 + i);
        rawVram.LoadBytes(0x4000, tiles);
        var nativeColors = new SnesCgram();
        for (int color = 0; color < 256; color++) nativeColors.SetColor(color, Bgr555.FromWord(checked((ushort)(Read(0xb6f000 + color * 2)))));
        foreach (AreaId area in new[] { AreaId.Crateria, AreaId.Brinstar, AreaId.Norfair, AreaId.WreckedShip, AreaId.Maridia })
        {
            var system = new Bank80SystemState();
            var labels = new FileSelectMapIcons(system, area);
            var presentation = RetailPresentationFixture();
            labels.BindLandmarks(presentation.Landmarks);
            labels.BindSprites(presentation.Sprites);
            var gated = new OamBuffer(); gated.BeginFrame();
            labels.DrawElevatorLabels(gated, new(0, 0));
            AssertEqual(0, gated.NextByteOffset, "undownloaded area hides destination names");
            system.SetAreaMapAcquired(area == AreaId.Crateria ? AreaId.Brinstar : AreaId.Crateria);
            labels.DrawElevatorLabels(gated, new(0, 0));
            AssertEqual(0, gated.NextByteOffset, "another area's map does not reveal these destinations");
            system.SetAreaMapAcquired(area);
            var pause = CreateRetailPauseFixture(bus, new PauseMenuEntry { Samus = new SamusState(), System = system, AreaIndex = area, RoomMapPosition = new(0, 0) });
            int list = 0x820000 | Read(0x82c74d + (int)area * 2);
            ushort x = unchecked((ushort)(Read(list) - 128)), y = unchecked((ushort)(Read(list + 2) - 100));
            pause.mapScrollPosition = new Position16<MapPixels>(x, y);
            var native = new OamBuffer(); native.BeginFrame();
            for (int entry = list; Read(entry) != 0xffff; entry += 6)
            {
                int sprite = Read(entry + 4);
                DrawImportedSpritemap(bus, native, 0x820000 | Read(0x82c569 + sprite * 2),
                    new(unchecked((ushort)(Read(entry) - x)), unchecked((ushort)(Read(entry + 2) - y))), 0);
            }
            int labelBytes = native.NextByteOffset;
            native.FinalizeFrame();
            var pixels = pause.Render();
            var oam = pause.oam;
            AssertTrue(oam.LastFinalizedSpriteCount * 4 >= labelBytes, $"{area} pause emits all native destination label parts");
            int start = oam.LastFinalizedSpriteCount * 4 - labelBytes;
            AssertTrue(oam.LowTable.Slice(start, labelBytes).SequenceEqual(native.LowTable[..labelBytes]),
                $"{area} destination spelling, order, coordinates and palette match raw ROM OAM");
            AssertLabels(start, "stable map");
            var nativePixels = SnesObjRenderer.Render(native, rawVram, nativeColors, PauseMenuLayout.ObjectSelection);
            var installedPixels = SnesObjRenderer.Render(native, pause.vram, pause.cgram, PauseMenuLayout.ObjectSelection);
            AssertTrue(nativePixels.SequenceEqual(installedPixels), $"{area} installed label artwork and colors equal raw ROM");
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
            AssertTrue(beforeRestore.SequenceEqual(pause.Render()), "restoring preserves destination-label fade priority");
            pause.Step(SnesButton.R, 0); _ = pause.Render();
            AssertLabels(pause.oam.LastFinalizedSpriteCount * 4 - labelBytes, "R input stable-map dispatch");
            pause.Step(0, 0); _ = pause.Render();
            AssertLabels(0, "map-to-equipment fade-out");
            for (int frame = 0; frame < 29; frame++) pause.Step(0, 0);
            _ = pause.Render();
            AssertEqual(1, pause.ScreenMode, "equipment page reached");
            AssertTrue(pause.oam.LastFinalizedSpriteCount * 4 < labelBytes, "equipment page emits no destination names");
            // Page input is accepted only once the equipment fade-in completes (#1266 cadence).
            for (int frame = 0; pause.IsPageTransitionActive; frame++)
            {
                AssertTrue(frame < PausePageTransitionNativeLength, "equipment fade-in completes within its native length");
                pause.Step(0, 0);
            }
            pause.Step(SnesButton.L, 0);
            for (int frame = 0; frame < 16; frame++) pause.Step(0, 0);
            _ = pause.Render();
            AssertEqual(0, pause.ScreenMode, "map fade-in reached");
            AssertLabels(pause.oam.LastFinalizedSpriteCount * 4 - labelBytes, "equipment-to-map fade-in");
            void AssertLabels(int firstByte, string phase)
            {
                var actual = pause.oam;
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
                AssertEqual(Read(title + word * 2), pause.vram.ReadWord(0x38aa + word), $"{area} heading agrees with native BG2 patch");
        }
        Console.WriteLine("Pause area labels: five outside-Tourian areas match raw ROM destination OAM/pixels and area-title words.");
        ushort Read(int address) => RomDataReader.ReadWordFixedBank(cart, address);
    }

    /// <summary>Runs the focused pause area-label comparison against the repository ROM.</summary>
    [VerificationCommand("--pause-map-area-labels")]
    internal static int PauseMapAreaLabelsCommand()
    {
        Suite(nameof(VerifyPauseMapAreaLabels), () => VerifyPauseMapAreaLabels());
        return 0;
    }
}
