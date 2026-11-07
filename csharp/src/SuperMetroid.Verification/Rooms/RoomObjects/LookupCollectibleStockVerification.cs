using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Verification.Areas.Escape;
using SuperMetroid.AssetExtraction.Cartridge;
using static SuperMetroid.Verification.Rooms.RoomObjects.CollectibleVisualsVerification;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Verification.Rooms.RoomObjects;

/// <summary>Checks stock collectible visual words and content identity against the native PLM frame table.</summary>
internal static class LookupCollectibleStockVerification
{
    /// <summary>
    /// Compares 24 native frame words with stock, imported, and installed catalogs, the 16 editable frames forming the
    /// installed file and its identity (#1538), then checks unknown pointers and invalid imports.
    /// </summary>
    internal static void VerifyCollectibleStockMapping(SuperMetroidAddressSpace rom,
        InstalledRoomObjectCollectibleVisuals installed)
    {
        LookupEscapeGateStockFrame[] frames = [new LookupEscapeGateStockFrame(0xa2b5,"empty"),new LookupEscapeGateStockFrame(0xa2c7,"chozo-orb-0"),new LookupEscapeGateStockFrame(0xa2cd,"chozo-orb-1"),new LookupEscapeGateStockFrame(0xa2d3,"chozo-orb-2"),new LookupEscapeGateStockFrame(0xa2d9,"chozo-orb-burst"),new LookupEscapeGateStockFrame(0xa2df,"energy-tank-0"),new LookupEscapeGateStockFrame(0xa2e5,"energy-tank-1"),new LookupEscapeGateStockFrame(0xa2eb,"missile-tank-0"),new LookupEscapeGateStockFrame(0xa2f1,"missile-tank-1"),new LookupEscapeGateStockFrame(0xa2f7,"super-missile-tank-0"),new LookupEscapeGateStockFrame(0xa2fd,"super-missile-tank-1"),new LookupEscapeGateStockFrame(0xa303,"power-bomb-tank-0"),new LookupEscapeGateStockFrame(0xa309,"power-bomb-tank-1"),new LookupEscapeGateStockFrame(0xa30f,"block-pair-0-frame-0"),new LookupEscapeGateStockFrame(0xa315,"block-pair-0-frame-1"),new LookupEscapeGateStockFrame(0xa31b,"block-pair-1-frame-0"),new LookupEscapeGateStockFrame(0xa321,"block-pair-1-frame-1"),new LookupEscapeGateStockFrame(0xa327,"block-pair-2-frame-0"),new LookupEscapeGateStockFrame(0xa32d,"block-pair-2-frame-1"),new LookupEscapeGateStockFrame(0xa333,"block-pair-3-frame-0"),new LookupEscapeGateStockFrame(0xa339,"block-pair-3-frame-1"),new LookupEscapeGateStockFrame(0xa3dd,"shot-reveal-0"),new LookupEscapeGateStockFrame(0xa3e3,"shot-reveal-1"),new LookupEscapeGateStockFrame(0xa3e9,"shot-reveal-2")];
        var native = frames.ToDictionary(frame => frame.Pointer,
            frame => RoomLevelWordCodec.DecodeAppearance(ReadCollectibleDrawWord(rom, (ushort)(frame.Pointer + 2))));
        bool Editable(LookupEscapeGateStockFrame frame) => RoomObjectCollectibleVisualFiles.IsEditable(
            RoomObjectDraw.FrameOf<RoomObjectCollectibleDrawDefinitions.Draw>(NativeObjectDraws.At<RoomObjectCollectibleDrawDefinitions.Draw>(frame.Pointer), "collectible"));
        var entries = frames.Where(Editable).Select(frame => new RoomObjectVisualBlockEntry(frame.Id, native[frame.Pointer])).ToArray();
        AssertEqual(16, entries.Length, "Collectible installed file holds the editable frames");
        var editableNative = frames.Where(Editable).ToDictionary(frame => frame.Pointer, frame => native[frame.Pointer]);
        var stock = InstalledRoomObjectCollectibleVisuals.Stock();
        var imported = RoomObjectCollectibleVisualFiles.Create(entries.Reverse());
        string Hash(Dictionary<ushort, RoomBlockAppearance> words) => SuperMetroid.AssetExtraction.SelectedPresentationHash.FromAppearanceFrames(
            nameof(InstalledRoomObjectCollectibleVisuals), words.ToDictionary(pair => RoomObjectCollectibleVisualFiles.VisualId(RoomObjectDraw.FrameOf<RoomObjectCollectibleDrawDefinitions.Draw>(NativeObjectDraws.At<RoomObjectCollectibleDrawDefinitions.Draw>(pair.Key), "collectible")), pair => pair.Value));
        foreach (var catalog in new[] {stock, imported, installed})
        {
            AssertEqual(Hash(editableNative), catalog.ContentIdentity, "Collectible original single-word framed identity");
            foreach (var frame in frames)
                AssertEqual(native[frame.Pointer], catalog.GetAppearance(NativeObjectDraws.At<RoomObjectCollectibleDrawDefinitions.Draw>(frame.Pointer)), "Collectible original stock word");
        }
        for (int pointer = 0; pointer <= ushort.MaxValue; pointer++)
            if (!native.ContainsKey((ushort)pointer))
                AssertThrows<InvalidDataException>(() => stock.GetAppearance(NativeObjectDraws.At<RoomObjectCollectibleDrawDefinitions.Draw>((ushort)pointer)), "Collectible stock unknown pointer");
        AssertThrows<InvalidDataException>(() => RoomObjectCollectibleVisualFiles.Create(entries[..1]), "Collectible missing frame");
        AssertThrows<InvalidDataException>(() => RoomObjectCollectibleVisualFiles.Create(entries.Append(entries[0])), "Collectible duplicate frame");
        foreach (RoomObjectVisualBlockEntry bad in new RoomObjectVisualBlockEntry[]
            { new("EMPTY", RoomTerrainBlocks.BlankAir.Appearance), new("unknown", RoomTerrainBlocks.BlankAir.Appearance) })
        {
            var invalid = entries.ToArray();
            invalid[0] = bad;
            AssertThrows<InvalidDataException>(() => RoomObjectCollectibleVisualFiles.Create(invalid), "Collectible invalid import");
        }
    }
}
