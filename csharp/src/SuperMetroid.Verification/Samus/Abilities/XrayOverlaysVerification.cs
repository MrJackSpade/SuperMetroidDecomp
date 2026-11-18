using SuperMetroid.Core.Samus.Abilities.Artwork;
using static SuperMetroid.Verification.Samus.Abilities.XraySetupVerification;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;
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

    /// <summary>
    /// Applies synthetic reveal data to confirm item order, collection gates, room overrides, and that an item other than a
    /// tank is revealed as its kind's own picture's first frame (#1538) while a tank shows its installed block.
    /// </summary>
    internal static void VerifyXrayOverlays()
    {
        var bus = new TestAddressSpace();
        var overlays = InstalledXrayOverlays.FromOverlaysForVerification(
            new Dictionary<InWorldCollectibleKind, RoomBlockAppearance>
            {
                [InWorldCollectibleKind.EnergyTank] = RoomBlockAppearance.Of(5), [InWorldCollectibleKind.MissileTank] = RoomBlockAppearance.Of(6),
                [InWorldCollectibleKind.SuperMissileTank] = RoomBlockAppearance.Of(7), [InWorldCollectibleKind.PowerBombTank] = RoomBlockAppearance.Of(8),
            },
            new XrayRoomOverlayBlocks[]
            {
                new(SpecialRoom, new XrayRoomOverlayVisual[]
                {
                    new(new(1, 1), RoomBlockAppearance.Of(9, BlockMirroring.Vertical)), new(new(17, 1), RoomBlockAppearance.Of(10)),
                    new(new(2, 1), RoomBlockAppearance.Of(3, BlockMirroring.Horizontal)),
                    new(new(3, 1), RoomBlockAppearance.Of(4, BlockMirroring.Horizontal | BlockMirroring.Vertical)),
                }),
            });
        // The terrain reveals' blocks play no part in the overlays.
        var visuals = new InstalledXrayReveals(XrayRevealFixtures.EveryDrawable(shape => XrayRevealFixtures.Corners(shape, 0, 0, 0, 0)), overlays);
        var system = new SharedGameState(new SharedRandom());
        var items = new[]
        {
            new CollectibleObjectSnapshot(new(1, 1), SuperMetroid.AssetExtraction.Progression.RoomPlacementDecoding.Item(1), InWorldCollectibleKind.Bombs),
            new CollectibleObjectSnapshot(new(1, 1), SuperMetroid.AssetExtraction.Progression.RoomPlacementDecoding.Item(2), InWorldCollectibleKind.EnergyTank),
        };
        // Each overlay places one whole block (#1431): the cell at the camera's block shows the block written last, in the
        // arrangement its mirroring selects.
        var map = new XrayRevealGrid();
        RevealedBlock Defined(int block) => new(RoomCellPicture.OfBlock(block), RevealedBlockArrangement.AsDefined);
        XrayRevealOverlays.Apply(map, items, system, null, new(16, 16), visuals);
        AssertEqual(new RevealedBlock(RoomCellPicture.Of(new CollectiblePicture(InWorldCollectibleKind.Bombs, CollectibleLook.Primary)), RevealedBlockArrangement.AsDefined), map[Position16<RoomBlocks>.Zero],
            "descending item order leaves the lower item's reveal last, and Bombs is revealed as its own picture's first frame");
        system.SetCollectedItemBit(1);
        XrayRevealOverlays.Apply(map, items, system, null, new(16, 16), visuals);
        AssertEqual(Defined(5), map[Position16<RoomBlocks>.Zero], "collected items do not obscure another live reveal, and the Energy Tank shows its installed block");
        XrayRevealOverlays.Apply(map, items, system, SpecialRoom, new(16, 16), visuals);
        AssertEqual(new RevealedBlock(RoomCellPicture.OfBlock(9), RevealedBlockArrangement.MirroredVertically), map[Position16<RoomBlocks>.Zero],
            "special-room override follows item writes, and its vertical flip mirrors the block top to bottom");
        AssertEqual(new RevealedBlock(RoomCellPicture.OfBlock(3), RevealedBlockArrangement.MirroredHorizontally), map[Position16<RoomBlocks>.Of(1, 0)],
            "an overlay's horizontal flip mirrors the block left to right");
        AssertEqual(new RevealedBlock(RoomCellPicture.OfBlock(4), RevealedBlockArrangement.MirroredBoth), map[Position16<RoomBlocks>.Of(2, 0)],
            "an overlay flipped both ways is mirrored both ways");
        map = new XrayRevealGrid();
        system.SetCollectedItemBit(2);
        XrayRevealOverlays.Apply(map, items, system, null, new(16, 16), visuals);
        AssertTrue(map.Blocks.All(block => block is null), "collected items leave the reveal unchanged");
        // The tanks' reveals are keyed by tank kind (#2115): a missing tank or another kind of item is rejected.
        AssertThrows<InvalidDataException>(() => InstalledXrayOverlays.FromOverlaysForVerification(
                new Dictionary<InWorldCollectibleKind, RoomBlockAppearance> { [InWorldCollectibleKind.EnergyTank] = RoomBlockAppearance.Of(5) }, []),
            "the tanks' reveals name every tank kind");
        AssertThrows<InvalidDataException>(() => InstalledXrayOverlays.FromOverlaysForVerification(
                InstalledXrayOverlays.TankKinds.Where(tank => tank != InWorldCollectibleKind.PowerBombTank)
                    .Append(InWorldCollectibleKind.Bombs).ToDictionary(kind => kind, _ => RoomBlockAppearance.Of(5)), []),
            "the tanks' reveals name only tank kinds");
        Console.WriteLine("  X-ray overlays: item pictures, tank blocks, collection gates, item order, room override, vertical flip and viewport bounds agree.");
    }

    /// <summary>Runs the overlay and X-ray setup buffer checks.</summary>
    [VerificationCommand("--xray-overlay-fixtures")]
    internal static int XrayOverlayFixturesCommand()
    {
     VerifyXrayOverlays(); VerifyXraySetupBuffers(); return 0; 
    }
}
