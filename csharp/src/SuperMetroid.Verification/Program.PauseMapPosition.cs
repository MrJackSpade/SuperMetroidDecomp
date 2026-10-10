using System.Reflection;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rom;

internal static partial class Program
{
    /// <summary>Checks pause-map current-position marker placement, native animation timing, and rendered pixels at two room locations.</summary>
    private static void VerifyPauseMapPosition()
    {
        var bus = CartridgeImportAddressSpaceTooling.LoadRetailRom("Super Metroid.smc");
        var cart = CartridgeImportSource.Require(bus);
        var rawVram = new SnesVram();
        rawVram.LoadBytes(0x4000, RomDataReader.ReadFixedBank(cart, 0xb6c000, 0x2000));
        var colors = new SnesCgram();
        for (int i = 0; i < 256; i++) colors.SetColor(i, Read(0xb6f000 + i * 2));
        foreach (var (area, roomX, roomY, samusX, samusY) in new[]
        {
            (AreaId.Crateria, 28, 1, 0x200, 0x300),
            (AreaId.Brinstar, 7, 10, 0x80, 0x180),
        })
        {
            var system = new Bank80SystemState();
            system.MarkExploredMapTile(area, roomX + (samusX >> 8), roomY + (samusY >> 8) + 1);
            var pause = CreateRetailPauseFixture(bus,
                new SamusState { XPosition = (ushort)samusX, YPosition = (ushort)samusY },
                system, area, (byte)roomX, (byte)roomY);
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
                ushort x = unchecked((ushort)(8 * (roomX + (samusX >> 8)) - pause.MapHorizontalScroll));
                ushort y = unchecked((ushort)(8 * (roomY + (samusY >> 8) + 1) - pause.MapVerticalScroll));
                var expected = new OamBuffer(); expected.BeginFrame();
                DrawImportedSpritemap(bus, expected, 0x820000 | Read(0x82c569 + id * 2), x, y, 0x0e00);
                int bytes = expected.NextByteOffset; expected.FinalizeFrame();
                var actual = (OamBuffer)typeof(PauseMenuState).GetField("oam", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(pause)!;
                AssertTrue(expected.LowTable[..bytes].SequenceEqual(actual.LowTable[..bytes]), "current-position marker native OAM and animation timing");
                AssertEqual(expected.HighTable[0], actual.HighTable[0], "current-position marker size/X bits");
                var nativePixels = SnesObjRenderer.Render(expected, rawVram, colors, PauseMenuLayout.ObjectSelection);
                int visible = 0;
                for (int i = 0; i < pixels.Length; i++)
                    if (nativePixels[i].A != 0)
                    {
                        visible++;
                        AssertEqual(nativePixels[i], pixels[i], $"{area} frame {frame} current-position pixel at {i % 256},{i / 256}");
                    }
                AssertTrue(visible > 0, "current-position marker visible in pause compositor");
                AssertTrue(pixels.AsSpan().SequenceEqual(pause.Render()), "repaint does not advance position marker");
            }
        }
        Console.WriteLine("Pause current-position marker: raw ROM OAM, all opaque pixels, two room positions, and two 24-frame pulse cycles agree.");
        ushort Read(int address) => RomDataReader.ReadWordFixedBank(cart, address);
    }
}
