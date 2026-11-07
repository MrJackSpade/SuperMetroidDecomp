using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Core.Bosses.Draygon;
using SuperMetroid.Verification.Areas.Escape;
using static SuperMetroid.Verification.Rooms.RoomObjects.SamusEaterObjectDefinitionsVerification;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Bosses.Draygon.Artwork;

using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.AssetExtraction;
namespace SuperMetroid.Verification.Bosses.Draygon;

/// <summary>Checks Draygon cannon visual catalogs against native draw runs and editable asset constraints.</summary>
internal static class LookupDraygonCannonStockVerification
{
    /// <summary>Compares twelve shield and damage frames with bank-$84 art, then checks invalid shapes.</summary>
    internal static void VerifyDraygonCannonStockMapping(SuperMetroidAddressSpace rom,
        RoomObjectDraygonCannonVisualCatalog installed)
    {
        LookupEscapeGateStockFrame[] frames = [new LookupEscapeGateStockFrame(0x9fcd,"right-shield-a"),new LookupEscapeGateStockFrame(0x9fdd,"right-shield-b"),new LookupEscapeGateStockFrame(0xa02d,"right-damaged-a"),new LookupEscapeGateStockFrame(0xa03d,"right-damaged-b"),new LookupEscapeGateStockFrame(0xa04d,"right-damaged-c"),new LookupEscapeGateStockFrame(0xa05d,"right-damaged-d"),new LookupEscapeGateStockFrame(0xa0ed,"left-shield-a"),new LookupEscapeGateStockFrame(0xa101,"left-shield-b"),new LookupEscapeGateStockFrame(0xa165,"left-damaged-a"),new LookupEscapeGateStockFrame(0xa179,"left-damaged-b"),new LookupEscapeGateStockFrame(0xa18d,"left-damaged-c"),new LookupEscapeGateStockFrame(0xa1a1,"left-damaged-d")];
        var native = new Dictionary<ushort, RoomBlockAppearance[][]>();
        foreach (var frame in frames)
        {
            var runs = new List<RoomBlockAppearance[]>();
            int cursor = frame.Pointer;
            while (true)
            {
                int count = ReadSamusEaterObjectWord(rom, 0x840000 | cursor) & 0x7fff;
                cursor += 2;
                var appearances = new RoomBlockAppearance[count];
                for (int cell = 0; cell < count; cell++, cursor += 2)
                    appearances[cell] = RoomLevelWordCodec.Decode(ReadSamusEaterObjectWord(rom, 0x840000 | cursor)).Appearance;
                runs.Add(appearances);
                byte x = DraygonCannonStockMappingRomByte(rom, 0x840000 | cursor++), y = DraygonCannonStockMappingRomByte(rom, 0x840000 | cursor++);
                if (x == 0 && y == 0) break;
            }
            native.Add(frame.Pointer, runs.ToArray());
        }
        var entries = frames.Select(frame => new RoomObjectVisualEntry(frame.Id,
            native[frame.Pointer].SelectMany(run => run).ToArray())).ToArray();
        var flat = frames.ToDictionary(frame => frame.Pointer, frame => native[frame.Pointer].SelectMany(run => run).ToArray());
        var stock = RoomObjectDraygonCannonVisualCatalog.Stock();
        var imported = RoomObjectDraygonCannonVisualFiles.Create(entries.Reverse());
        string Hash(Dictionary<ushort, RoomBlockAppearance[]> appearances) => SuperMetroid.AssetExtraction.SelectedPresentationHash.FromAppearanceFrames(
            nameof(RoomObjectDraygonCannonVisualCatalog), appearances.ToDictionary(pair => RoomObjectDraygonCannonVisualFiles.VisualId(NativeObjectDraws.At<DraygonCannonDraw>(pair.Key)), pair => pair.Value));
        foreach (var catalog in new[] {stock,imported,installed})
        {
            AssertEqual(Hash(flat), catalog.ContentIdentity, "Cannon original flattened identity");
            foreach (var frame in frames)
            {
                var runs = native[frame.Pointer];
                for (int run = 0; run < runs.Length; run++)
                {
                    for (int word = 0; word < runs[run].Length; word++)
                        AssertEqual(runs[run][word], catalog.GetAppearance(NativeObjectDraws.At<DraygonCannonDraw>(frame.Pointer),run,word), "Cannon original stock appearance");
                    foreach (int bad in new[] {int.MinValue,-1,runs[run].Length,int.MaxValue})
                        AssertThrows<ArgumentOutOfRangeException>(() => catalog.GetAppearance(NativeObjectDraws.At<DraygonCannonDraw>(frame.Pointer),run,bad), "Cannon stock cell bounds");
                }
                foreach (int bad in new[] {int.MinValue,-1,runs.Length,int.MaxValue})
                    AssertThrows<ArgumentOutOfRangeException>(() => catalog.GetAppearance(NativeObjectDraws.At<DraygonCannonDraw>(frame.Pointer),bad,0), "Cannon stock run bounds");
            }
        }
        for (int pointer = 0; pointer <= ushort.MaxValue; pointer++)
            if (!native.ContainsKey((ushort)pointer))
                AssertThrows<InvalidDataException>(() => stock.GetAppearance(NativeObjectDraws.At<DraygonCannonDraw>((ushort)pointer),0,0), "Cannon stock unknown pointer");
        AssertThrows<InvalidDataException>(() => RoomObjectDraygonCannonVisualFiles.Create(entries[..1]), "Cannon missing frame");
        AssertThrows<InvalidDataException>(() => RoomObjectDraygonCannonVisualFiles.Create(entries.Append(entries[0])), "Cannon duplicate frame");
        var invalid = entries.ToArray();
        invalid[0] = new("RIGHT-SHIELD-A", entries[0].Blocks);
        AssertThrows<InvalidDataException>(() => RoomObjectDraygonCannonVisualFiles.Create(invalid), "Cannon ordinal identity");
        invalid[0] = new(entries[0].Id, new RoomBlockAppearance[7]);
        AssertThrows<InvalidDataException>(() => RoomObjectDraygonCannonVisualFiles.Create(invalid), "Cannon exact flattened shape");
    }

    /// <summary>Reads one native draw-record byte for the cannon run-termination oracle.</summary>
    private static byte DraygonCannonStockMappingRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);
}
