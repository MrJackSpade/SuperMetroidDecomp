using SuperMetroid.Core.Game;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    private static void VerifyXrayOverlays()
    {
        var bus = new TestAddressSpace();
        for (int i = 0; i < 8; i++)
        {
            WriteWord(bus, XrayOverlayRomData.ItemDrawPointers + i * 2, (ushort)(0x9000 + i * 4));
            WriteWord(bus, XrayOverlayRomData.ItemBank | (0x9002 + i * 4), (ushort)(i + 1));
        }
        var definitions = new byte[1024 * 8];
        for (int i = 0; i < definitions.Length / 2; i++)
        {
            definitions[i * 2] = (byte)i;
            definitions[i * 2 + 1] = (byte)(i >> 8);
        }
        var level = new RoomLevelData(32, 32, new ushort[1024], new byte[1024], new ushort[1024], definitions);
        var system = new Bank80SystemState();
        var items = new[]
        {
            new CollectiblePlmSnapshot(RoomPlmHeaders.ExposedEnergyTank, 33, 1, InWorldCollectibleKind.Bombs,
                CollectiblePresentation.ShotBlock, CollectiblePhase.ShotBlock, 1),
            new CollectiblePlmSnapshot(RoomPlmHeaders.ExposedEnergyTank, 33, 2, InWorldCollectibleKind.EnergyTank,
                CollectiblePresentation.Exposed, CollectiblePhase.Visible, -1),
        };
        var map = new ushort[XrayTilemapLayout.BufferWords];
        XrayRevealOverlays.Apply(bus, level, map, items, system, 0, 16, 16);
        AssertEqual(8, map[0], "descending item slot order leaves the lower physical slot's reveal last");
        system.SetCollectedItemBit(1);
        XrayRevealOverlays.Apply(bus, level, map, items, system, 0, 16, 16);
        AssertEqual(20, map[0], "collected items do not obscure another live reveal");
        WriteWord(bus, 0x8f9000, 0x0101);
        WriteWord(bus, 0x8f9002, 0x0809);
        WriteWord(bus, 0x8f9004, 0x0111);
        WriteWord(bus, 0x8f9006, 10);
        WriteWord(bus, 0x8f9008, 0);
        XrayRevealOverlays.Apply(bus, level, map, items, system, 0x9000, 16, 16);
        AssertEqual(38, map[0], "special-room override follows item writes and selects the flipped bottom-left tile");
        AssertEqual(39, map[1], "vertical flip preserves tile-word bits");
        AssertEqual(36, map[32], "vertical flip places original top row below");
        AssertEqual(0, map[XrayTilemapLayout.ScreenWords], "special-room overlay excludes the fine-scroll seventeenth column");
        Array.Clear(map);
        items[0] = items[0] with { RoomArgument = ushort.MaxValue };
        system.SetCollectedItemBit(2);
        XrayRevealOverlays.Apply(bus, level, map, items, system, 0, 16, 16);
        AssertTrue(map.All(word => word == 0), "negative item arguments and collected items leave the tilemap unchanged");
        Console.WriteLine("  X-ray overlays: item graphics slots, collection gates, slot order, room override, vertical flip and viewport bounds agree.");
    }
}
