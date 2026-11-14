using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Verification.Areas.Escape;
using SuperMetroid.AssetExtraction.Cartridge;
using static SuperMetroid.Verification.Rooms.RoomObjects.CollectibleVisualsVerification;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Verification.Rooms.RoomObjects;

/// <summary>
/// Checks the 24 exported collectible PLM draw records against native geometry, bits, and names, and that the installed
/// file names only the 16 editable frames, not the block-pair frames under an item's own picture (#1538).
/// </summary>
internal static class LookupCollectibleDrawsVerification
{
    /// <summary>Checks each draw record's one-cell geometry and complete pointer domain.</summary>
    internal static void VerifyCollectibleDrawGeometry(CartridgeImportAddressSpace rom) => VerifyCollectibleDrawField(rom, 0);
    /// <summary>Compares collision bits in each collectible's level word with the cartridge.</summary>
    internal static void VerifyCollectibleDrawCollision(CartridgeImportAddressSpace rom) => VerifyCollectibleDrawField(rom, 1);
    /// <summary>Compares visual tile bits in each collectible's level word with the cartridge.</summary>
    internal static void VerifyCollectibleDrawVisuals(CartridgeImportAddressSpace rom) => VerifyCollectibleDrawField(rom, 2);
    /// <summary>Checks published names, reverse lookup, and ordinal identity rejection.</summary>
    internal static void VerifyCollectibleDrawIdentity(CartridgeImportAddressSpace rom) => VerifyCollectibleDrawField(rom, 3);

    /// <summary>Checks one requested draw field across all exported pointers and unsupported inputs.</summary>
    private static void VerifyCollectibleDrawField(CartridgeImportAddressSpace rom, int field)
    {
        // Original exported pointer/name contract; physical values come from the ROM.
        LookupEscapeGateStockFrame[] expected = [new LookupEscapeGateStockFrame(0xa2b5,"empty"),new LookupEscapeGateStockFrame(0xa2c7,"chozo-orb-dim"),new LookupEscapeGateStockFrame(0xa2cd,"chozo-orb-glowing"),new LookupEscapeGateStockFrame(0xa2d3,"chozo-orb-bright"),new LookupEscapeGateStockFrame(0xa2d9,"chozo-orb-burst"),new LookupEscapeGateStockFrame(0xa2df,"energy-tank-bright"),new LookupEscapeGateStockFrame(0xa2e5,"energy-tank-dim"),new LookupEscapeGateStockFrame(0xa2eb,"missile-tank-dim"),new LookupEscapeGateStockFrame(0xa2f1,"missile-tank-bright"),new LookupEscapeGateStockFrame(0xa2f7,"super-missile-tank-dim"),new LookupEscapeGateStockFrame(0xa2fd,"super-missile-tank-bright"),new LookupEscapeGateStockFrame(0xa303,"power-bomb-tank-bright"),new LookupEscapeGateStockFrame(0xa309,"power-bomb-tank-dim"),new LookupEscapeGateStockFrame(0xa30f,"first-block-pair-first-frame"),new LookupEscapeGateStockFrame(0xa315,"first-block-pair-second-frame"),new LookupEscapeGateStockFrame(0xa31b,"second-block-pair-first-frame"),new LookupEscapeGateStockFrame(0xa321,"second-block-pair-second-frame"),new LookupEscapeGateStockFrame(0xa327,"third-block-pair-first-frame"),new LookupEscapeGateStockFrame(0xa32d,"third-block-pair-second-frame"),new LookupEscapeGateStockFrame(0xa333,"fourth-block-pair-first-frame"),new LookupEscapeGateStockFrame(0xa339,"fourth-block-pair-second-frame"),new LookupEscapeGateStockFrame(0xa3dd,"shot-block-intact"),new LookupEscapeGateStockFrame(0xa3e3,"shot-block-cracking"),new LookupEscapeGateStockFrame(0xa3e9,"shot-block-dust")];
        var exported = RoomObjectCollectibleVisualFiles.All.ToArray();
        AssertEqual(24, exported.Length, "Collectible original export count");
        InstalledRoomObjectCollectibleVisuals? pictures = field == 2 ? InstalledRoomObjectCollectibleVisuals.Stock() : null;
        if (field == 0)
            for (int raw = 0; raw <= ushort.MaxValue; raw++)
            {
                bool owned = expected.Any(entry => entry.Pointer == raw);
                AssertEqual(owned, RoomObjectCollectibleDrawDefinitionsTooling.TryGet(NativeObjectDraws.At<CollectibleDraw>((ushort)raw), out var value), "Collectible complete pointer domain");
                AssertEqual(owned, CollectibleObjectDraws.TryGetCollision(NativeObjectDraws.At<CollectibleDraw>((ushort)raw), out RoomCollisionType collision), "Collectible runtime block domain");
                AssertEqual(owned && expected.Single(entry => entry.Pointer == raw).Id.EndsWith("-frame", StringComparison.Ordinal),
                    CollectibleObjectDraws.TryGetBlockPairTerrain(NativeObjectDraws.At<CollectibleDraw>((ushort)raw), out RoomTerrainBlock pairTerrain),
                    "Collectible block-pair terrain domain");
                if (!owned)
                {
                    AssertEqual(default(RoomObjectCollectibleDrawFrame), value, "Collectible rejected output");
                    AssertEqual(default(RoomCollisionType), collision, "Collectible rejected runtime output");
                    AssertEqual(default(RoomTerrainBlock), pairTerrain, "Collectible rejected block-pair output");
                }
            }
        for (int index = 0; index < expected.Length; index++)
        {
            var entry = expected[index];
            AssertTrue(RoomObjectCollectibleDrawDefinitionsTooling.TryGet(NativeObjectDraws.At<CollectibleDraw>(entry.Pointer), out var direct), "Collectible direct identity");
            RoomObjectCollectibleDrawFrame[] values;
            if (RoomObjectCollectibleVisualFiles.IsEditable(direct.Draw))
            {
                RoomObjectCollectibleDrawFrame named = RoomObjectCollectibleVisualFiles.Editable.Single(frame =>
                    RoomObjectCollectibleVisualFiles.Family.Ids[RoomObjectDraw.Of(frame.Draw)] == entry.Id);
                values = [direct, named, exported[index]];
            }
            else
            {
                AssertTrue(!RoomObjectCollectibleVisualFiles.Family.Ids.Values.Contains(entry.Id, StringComparer.Ordinal), "Collectible block-pair frame is not an installed identity");
                values = [direct, exported[index]];
            }
            foreach (var value in values)
            {
                AssertEqual(entry.Pointer, RoomObjectDrawAddressCodec.Of(RoomObjectDraw.Of(value.Draw)), "Collectible original pointer and export order");
                if (field == 0)
                {
                    AssertEqual((ushort)1, ReadCollectibleDrawWord(rom, entry.Pointer), "Collectible original horizontal one-cell geometry");
                    AssertEqual((ushort)0, ReadCollectibleDrawWord(rom, (ushort)(entry.Pointer + 4)), "Collectible original terminator");
                }
                else if (field == 3) AssertEqual(entry.Id, value.Id, "Collectible original published name");
                else
                {
                    RoomObjectDraw draw = NativeObjectDraws.At<CollectibleDraw>(entry.Pointer);
                    RoomTerrainBlock native = RoomLevelWordCodec.Decode(ReadCollectibleDrawWord(rom, (ushort)(entry.Pointer + 2)));
                    AssertTrue(CollectibleObjectDraws.TryGetCollision(draw, out RoomCollisionType runtimeCollision), "Collectible runtime word ownership");
                    if (field == 1)
                    {
                        AssertEqual(native.Collision, runtimeCollision, "Collectible original runtime collision");
                        AssertEqual(native.Collision, value.Collision, "Collectible original physical collision");
                    }
                    else if (RoomObjectCollectibleVisualFiles.IsEditable(value.Draw))
                        // Core holds no appearance: what the frame shows is its installed picture.
                        AssertEqual(native.Appearance, pictures!.Image(draw).At(Offset16<RoomBlocks>.Zero), "Collectible installed picture");
                    else
                    {
                        // A block pair's frame writes its block number as gameplay terrain under the item's own picture (#1538).
                        AssertTrue(CollectibleObjectDraws.TryGetBlockPairTerrain(draw, out RoomTerrainBlock pairWord), "Collectible block-pair terrain");
                        AssertEqual(native, pairWord, "Collectible original block-pair terrain");
                    }

                }
            }
            if (field == 3)
                foreach (string bad in new[] {entry.Id.ToUpperInvariant(), " " + entry.Id, entry.Id + " "})
                    AssertTrue(!RoomObjectCollectibleVisualFiles.Family.Ids.Values.Contains(bad, StringComparer.Ordinal), "Collectible ordinal identity");
        }
        if (field == 3)
            foreach (string bad in new[] {"","energy-tank-2","dynamic-slot-0-frame-0","first-block-pair-first-frame","chozo-orb-0","chozo-orb-3","energy-tank-0","shot-reveal-0","block-pair-0-frame-0"})
                AssertTrue(!RoomObjectCollectibleVisualFiles.Family.Ids.Values.Contains(bad, StringComparer.Ordinal), "Collectible unsupported ID");
    }
}
