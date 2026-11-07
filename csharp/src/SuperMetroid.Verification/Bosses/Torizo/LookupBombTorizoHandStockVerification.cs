using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Core.Bosses.Torizo;
using SuperMetroid.Verification.Areas.Escape;
using static SuperMetroid.Verification.Rooms.RoomObjects.SamusEaterObjectDefinitionsVerification;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Bosses.Torizo.Artwork;

using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.AssetExtraction;
namespace SuperMetroid.Verification.Bosses.Torizo;

/// <summary>Checks the intact and cleared Bomb Torizo hand draw lists against native PLM words.</summary>
internal static class LookupBombTorizoHandStockVerification
{
    /// <summary>Checks stock words, identity hashes, and invalid shapes.</summary>
    internal static void VerifyBombTorizoHandStockMapping(SuperMetroidAddressSpace rom,
        RoomObjectBombTorizoHandVisualCatalog installed)
    {
        LookupEscapeGateStockFrame[] frames = [new LookupEscapeGateStockFrame(0x9877,"intact"),new LookupEscapeGateStockFrame(0x989d,"cleared")];
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
                byte x = BombTorizoHandStockMappingRomByte(rom, 0x840000 | cursor++), y = BombTorizoHandStockMappingRomByte(rom, 0x840000 | cursor++);
                if (x == 0 && y == 0) break;
            }
            native.Add(frame.Pointer, runs.ToArray());
        }
        var entries = frames.Select(frame => new RoomObjectVisualEntry(frame.Id,
            native[frame.Pointer].SelectMany(run => run).ToArray())).ToArray();
        var flat = frames.ToDictionary(frame => frame.Pointer, frame => native[frame.Pointer].SelectMany(run => run).ToArray());
        var stock = RoomObjectBombTorizoHandVisualCatalog.Stock();
        var imported = RoomObjectBombTorizoHandVisualFiles.Create(entries.Reverse());
        string Hash(Dictionary<ushort, RoomBlockAppearance[]> appearances) => SuperMetroid.AssetExtraction.SelectedPresentationHash.FromAppearanceFrames(
            nameof(RoomObjectBombTorizoHandVisualCatalog), appearances.ToDictionary(pair => RoomObjectBombTorizoHandVisualFiles.VisualId(NativeObjectDraws.At<BombTorizoHandDraw>(pair.Key)), pair => pair.Value));
        foreach (var catalog in new[] {stock,imported,installed})
        {
            AssertEqual(Hash(flat), catalog.ContentIdentity, "Hand original flattened identity");
            foreach (var frame in frames)
            {
                var runs = native[frame.Pointer];
                for (int run = 0; run < runs.Length; run++)
                {
                    for (int word = 0; word < runs[run].Length; word++)
                        AssertEqual(runs[run][word], catalog.GetAppearance(NativeObjectDraws.At<BombTorizoHandDraw>(frame.Pointer),run,word), "Hand original stock appearance");
                    foreach (int bad in new[] {int.MinValue,-1,runs[run].Length,int.MaxValue})
                        AssertThrows<ArgumentOutOfRangeException>(() => catalog.GetAppearance(NativeObjectDraws.At<BombTorizoHandDraw>(frame.Pointer),run,bad), "Hand stock cell bounds");
                }
                foreach (int bad in new[] {int.MinValue,-1,runs.Length,int.MaxValue})
                    AssertThrows<ArgumentOutOfRangeException>(() => catalog.GetAppearance(NativeObjectDraws.At<BombTorizoHandDraw>(frame.Pointer),bad,0), "Hand stock run bounds");
            }
        }
        for (int pointer = 0; pointer <= ushort.MaxValue; pointer++)
            if (!native.ContainsKey((ushort)pointer))
                AssertThrows<InvalidDataException>(() => stock.GetAppearance(NativeObjectDraws.At<BombTorizoHandDraw>((ushort)pointer),0,0), "Hand stock unknown pointer");
        AssertThrows<InvalidDataException>(() => RoomObjectBombTorizoHandVisualFiles.Create(entries[..1]), "Hand missing frame");
        AssertThrows<InvalidDataException>(() => RoomObjectBombTorizoHandVisualFiles.Create(entries.Append(entries[0])), "Hand duplicate frame");
        var invalid = entries.ToArray();
        invalid[0] = new("INTACT", entries[0].Blocks);
        AssertThrows<InvalidDataException>(() => RoomObjectBombTorizoHandVisualFiles.Create(invalid), "Hand ordinal identity");
        invalid[0] = new(entries[0].Id, new RoomBlockAppearance[7]);
        AssertThrows<InvalidDataException>(() => RoomObjectBombTorizoHandVisualFiles.Create(invalid), "Hand exact flattened shape");
    }

    /// <summary>Reads a native hand draw-list byte for the stock oracle.</summary>
    private static byte BombTorizoHandStockMappingRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);
}
