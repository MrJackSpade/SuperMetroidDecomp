using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Samus.Abilities.Artwork;
using static SuperMetroid.Verification.Samus.Abilities.XraySetupVerification;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Rooms.Xray;
using SuperMetroid.Core.Rooms.Backgrounds;
namespace SuperMetroid.Verification.Samus.Abilities;
using SuperMetroid.Verification.Harness;

/// <summary>Checks item and room X-ray reveal layering, flips, and viewport bounds.</summary>
internal static class XrayOverlaysVerification
{
    /// <summary>The fixture's room-specific overlay.</summary>
    private static readonly XrayRoomOverlayDefinition SpecialRoom = new("fixture special room");

    /// <summary>Applies synthetic reveal data to confirm item slot order, collection gates, and room overrides.</summary>
    internal static void VerifyXrayOverlays()
    {
        var bus = new TestAddressSpace();
        var overlays = XrayOverlayVisualCatalog.FromOverlaysForVerification(
            Enumerable.Range(1, 8).Select(value => (ushort)value),
            new XrayRoomOverlayTiles[]
            {
                new(SpecialRoom, new XrayRoomOverlayVisual[]
                {
                    new(new(1, 1), new RoomLevelWord(0x0809)), new(new(17, 1), new RoomLevelWord(10)),
                    new(new(2, 1), new RoomLevelWord(0x0403)), new(new(3, 1), new RoomLevelWord(0x0c04)),
                }),
            });
        var entries = new List<XrayRevealVisualDefinition>();
        foreach (RoomCollisionType type in Enum.GetValues<RoomCollisionType>())
        for (int bts = 0; bts <= byte.MaxValue; bts++)
            if (XrayRevealTable.Find(type, (byte)bts) is { } definition &&
                XrayRevealVisualCatalog.IsDrawable(definition.Command))
                entries.Add(new(type, (byte)bts, new(definition.TopLeft, definition.TopRight,
                    definition.BottomLeft, definition.BottomRight)));
        var visuals = new XrayRevealVisualCatalog(entries, overlays);
        var level = new RoomLevelData(new Extent16<RoomBlocks>(32, 32), new RoomLevelPlanes(new ushort[1024], new byte[1024], new ushort[1024]));
        var system = new SharedGameState();
        var items = new[]
        {
            new CollectiblePlmSnapshot(PlmHeaderId.ExposedEnergyTank, 33, 1, InWorldCollectibleKind.Bombs, 1),
            new CollectiblePlmSnapshot(PlmHeaderId.ExposedEnergyTank, 33, 2, InWorldCollectibleKind.EnergyTank, -1),
        };
        // Each overlay places one whole block (#1431): the cell at the camera's block shows the block written last, in the
        // arrangement its word's flips select.
        var map = new RevealedBlock?[XrayRevealLayout.Cells];
        RevealedBlock Defined(int block) => new(block, RevealedBlockArrangement.AsDefined);
        XrayRevealOverlays.Apply(level, map, items, system, null, new(16, 16), visuals);
        AssertEqual(Defined(2), map[0], "descending item slot order leaves the lower physical slot's reveal last");
        system.SetCollectedItemBit(1);
        XrayRevealOverlays.Apply(level, map, items, system, null, new(16, 16), visuals);
        AssertEqual(Defined(5), map[0], "collected items do not obscure another live reveal");
        XrayRevealOverlays.Apply(level, map, items, system, SpecialRoom, new(16, 16), visuals);
        AssertEqual(new RevealedBlock(9, RevealedBlockArrangement.RowsSwapped), map[0],
            "special-room override follows item writes, and its vertical flip swaps the block's rows without mirroring them");
        AssertEqual(Defined(3), map[1], "an overlay's horizontal flip is not drawn, as natively");
        AssertEqual(new RevealedBlock(4, RevealedBlockArrangement.RowsSwapped), map[2],
            "an overlay flipped both ways only swaps the block's rows");
        AssertEqual(null, map[XrayRevealLayout.MetatileColumns - 1], "special-room overlay excludes the fine-scroll seventeenth column");
        Array.Clear(map);
        items[0] = items[0] with { RoomArgument = ushort.MaxValue };
        system.SetCollectedItemBit(2);
        XrayRevealOverlays.Apply(level, map, items, system, null, new(16, 16), visuals);
        AssertTrue(map.All(block => block is null), "negative item arguments and collected items leave the reveal unchanged");
        Console.WriteLine("  X-ray overlays: item graphics slots, collection gates, slot order, room override, vertical flip and viewport bounds agree.");
    }

    /// <summary>Runs the overlay and X-ray setup buffer checks.</summary>
    [VerificationCommand("--xray-overlay-fixtures")]
    internal static int XrayOverlayFixturesCommand()
    {
     VerifyXrayOverlays(); VerifyXraySetupBuffers(); return 0; 
    }
}
