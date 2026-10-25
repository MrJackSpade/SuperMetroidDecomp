using static SuperMetroid.Verification.Frontend.Pause.PauseHudLocationVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Frontend.Map.MapSpritesVerification;
using static SuperMetroid.Verification.Harness.RetailPresentationFixtureVerification;
using SuperMetroid.Core.Frontend.Pause;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
namespace SuperMetroid.Verification.Frontend.Pause;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Assets;

/// <summary>Checks the pause map's animated current-position marker against retail OAM and palette.</summary>
internal static class PauseMapPositionVerification
{
    /// <summary>Compares two room positions across two full 24-frame marker pulse cycles.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native CGRAM slots, native OAM tables")]
    [CpuBusBoundary("exact bus test: retail ROM oracle for the pause map position palette and spritemaps")]
    internal static void VerifyPauseMapPosition()
    {
        var bus = CartridgeImportAddressSpaceTooling.LoadRetailRom("Super Metroid.smc");
        var cart = CartridgeImportSource.Require(bus);
        // The marker's $0E00 attribute selects the last object palette, whose fifteen inks the pause screen loads from $B6:F000.
        const int markerPalette = 240;
        Bgr555[] markerColors = [.. Enumerable.Range(markerPalette + 1, 15).Select(color => Bgr555.FromWord(checked(Read(0xb6f000 + color * 2))))];
        foreach (var (area, roomX, roomY, samusX, samusY) in new[]
        {
            new VerifyPauseMapPositionCase(AreaId.Crateria, 28, 1, 0x200, 0x300),
            new VerifyPauseMapPositionCase(AreaId.Brinstar, 7, 10, 0x80, 0x180),
        })
        {
            var system = new SharedGameState();
            var samusCell = new Position16<MapTiles>(checked((ushort)(roomX + RoomScreenOf.Pixel(samusX))), checked((ushort)(roomY + RoomScreenOf.Pixel(samusY) + 1)));
            system.MarkExploredMapTile(area, samusCell);
            var pause = CreateRetailPauseFixture(new PauseMenuEntry { Samus = new SamusState { Position = new(new((ushort)samusX, 0), new((ushort)samusY, 0))}, System = system, AreaIndex = area, RoomMapPosition = new(checked((ushort)roomX), checked((ushort)roomY)) });
            int nativeFrame = 0, nativeTimer = 0;
            for (int frame = 0; frame < 48; frame++)
            {
                if (nativeTimer == 0)
                {
                    nativeFrame = (nativeFrame + 1) % 4;
                    nativeTimer = new[] { 8, 4, 8, 4 }[nativeFrame];
                }
                nativeTimer--;
                int id = new[] { 0x5f, 0x60, 0x61, 0x60 }[nativeFrame];
                pause.AdvanceAnimations();
                var pixels = pause.Render();
                Position16<ScreenPixels> marker = samusCell.ToMapPixels().ToScreen(pause.MapScroll);
                ushort x = marker.X, y = marker.Y;
                var expected = new OamBuffer(); expected.BeginFrame();
                DrawImportedSpritemap(bus, expected, 0x820000 | Read(0x82c569 + id * 2), new(x, y), 0x0e00);
                int bytes = expected.NextByteOffset; expected.FinalizeFrame();
                var actual = pause.screen.Objects;
                AssertTrue(expected.LowTable[..bytes].SequenceEqual(actual.LowTable[..bytes]), "current-position marker native OAM and animation timing");
                AssertEqual(expected.HighTable[0], actual.HighTable[0], "current-position marker size/X bits");
                for (int color = 0; color < markerColors.Length; color++)
                    AssertEqual(markerColors[color], pause.screen.Palette.Colors[markerPalette + 1 + color], $"{area} frame {frame} current-position marker ink {color + 1}");
                AssertTrue(pixels.SequenceEqual(pause.Render()), "repaint does not advance position marker");
            }
        }
        Console.WriteLine("Pause current-position marker: raw ROM OAM and palette, two room positions, and two 24-frame pulse cycles agree.");
        ushort Read(int address) => RomDataReader.ReadWordFixedBank(cart, address);
    }

    /// <summary>Runs position-marker and pause HUD location checks together.</summary>
    [VerificationCommand("--pause-map-position")]
    internal static int PauseMapPositionCommand()
    {
        Suite(nameof(VerifyPauseMapPosition), () => VerifyPauseMapPosition());
        Suite(nameof(VerifyPauseHudLocation), () => VerifyPauseHudLocation());
        return 0;
    }
}
