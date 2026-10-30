using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Verification.Areas.Escape;
using static SuperMetroid.Verification.Rooms.Plms.SamusEaterPlmDefinitionsVerification;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;

using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.AssetExtraction;
namespace SuperMetroid.Verification.Bosses.MotherBrain;

/// <summary>Checks Mother Brain glass PLM visuals through damage and shatter frames.</summary>
internal static class LookupMotherBrainGlassStockVerification
{
    /// <summary>Compares eleven stock frames, flattened identity, edited ownership, and strict import validation.</summary>
    internal static void VerifyMotherBrainGlassStockMapping(SuperMetroidAddressSpace rom,
        RoomPlmMotherBrainGlassVisualCatalog installed)
    {
        LookupEscapeGateStockFrame[] frames = [new LookupEscapeGateStockFrame(0x9717,"initial"),new LookupEscapeGateStockFrame(0x971d,"pane-damage-1"),new LookupEscapeGateStockFrame(0x9731,"pane-damage-2"),new LookupEscapeGateStockFrame(0x9745,"pane-transition"),new LookupEscapeGateStockFrame(0x974f,"shifted-pane-1"),new LookupEscapeGateStockFrame(0x9769,"shifted-pane-2"),new LookupEscapeGateStockFrame(0x9781,"shifted-pane-3"),new LookupEscapeGateStockFrame(0x978f,"shatter-1"),new LookupEscapeGateStockFrame(0x97b7,"shatter-2"),new LookupEscapeGateStockFrame(0x97e7,"shatter-3"),new LookupEscapeGateStockFrame(0x9817,"cleared")];
        var native = new Dictionary<ushort, RoomBlockAppearance[][]>();
        foreach (var frame in frames)
        {
            var runs = new List<RoomBlockAppearance[]>();
            int cursor = frame.Pointer;
            while (true)
            {
                int count = ReadSamusEaterPlmWord(rom, 0x840000 | cursor) & 0x7fff;
                cursor += 2;
                var appearances = new RoomBlockAppearance[count];
                for (int cell = 0; cell < count; cell++, cursor += 2)
                    appearances[cell] = RoomLevelWordCodec.Decode(ReadSamusEaterPlmWord(rom, 0x840000 | cursor)).Appearance;
                runs.Add(appearances);
                byte x = MotherBrainGlassStockMappingRomByte(rom, 0x840000 | cursor++), y = MotherBrainGlassStockMappingRomByte(rom, 0x840000 | cursor++);
                if (x == 0 && y == 0) break;
            }
            native.Add(frame.Pointer, runs.ToArray());
        }
        var entries = frames.Select(frame => new RoomPlmVisualEntry(frame.Id,
            native[frame.Pointer].SelectMany(run => run).ToArray())).ToArray();
        var flat = frames.ToDictionary(frame => frame.Pointer, frame => native[frame.Pointer].SelectMany(run => run).ToArray());
        var stock = RoomPlmMotherBrainGlassVisualCatalog.Stock();
        var imported = new RoomPlmMotherBrainGlassVisualCatalog(entries.Reverse());
        string Hash(Dictionary<ushort, RoomBlockAppearance[]> appearances) => SuperMetroid.Core.Assets.SelectedPresentationHash.FromAppearanceFrames(
            nameof(RoomPlmMotherBrainGlassVisualCatalog), appearances.ToDictionary(pair => MotherBrainGlassDraws.VisualId(NativePlmDraws.At<MotherBrainGlassDraw>(pair.Key)), pair => pair.Value));
        foreach (var catalog in new[] {stock,imported,installed})
        {
            AssertEqual(Hash(flat), catalog.ContentIdentity, "Glass original flattened identity");
            foreach (var frame in frames)
            {
                var runs = native[frame.Pointer];
                for (int run = 0; run < runs.Length; run++)
                {
                    for (int word = 0; word < runs[run].Length; word++)
                        AssertEqual(runs[run][word], catalog.GetAppearance(NativePlmDraws.At<MotherBrainGlassDraw>(frame.Pointer),run,word), "Glass original stock appearance");
                    foreach (int bad in new[] {int.MinValue,-1,runs[run].Length,int.MaxValue})
                        AssertThrows<ArgumentOutOfRangeException>(() => catalog.GetAppearance(NativePlmDraws.At<MotherBrainGlassDraw>(frame.Pointer),run,bad), "Glass stock cell bounds");
                }
                foreach (int bad in new[] {int.MinValue,-1,runs.Length,int.MaxValue})
                    AssertThrows<ArgumentOutOfRangeException>(() => catalog.GetAppearance(NativePlmDraws.At<MotherBrainGlassDraw>(frame.Pointer),bad,0), "Glass stock run bounds");
            }
        }
        for (int pointer = 0; pointer <= ushort.MaxValue; pointer++)
            if (!native.ContainsKey((ushort)pointer))
                AssertThrows<InvalidDataException>(() => stock.GetAppearance(NativePlmDraws.At<MotherBrainGlassDraw>((ushort)pointer),0,0), "Glass stock unknown pointer");
        entries[8].Blocks[12] = RoomBlockAppearance.Of(0x58, BlockMirroring.Horizontal | BlockMirroring.Vertical);
        var mixed = new RoomPlmMotherBrainGlassVisualCatalog(entries);
        var expected = frames.ToDictionary(frame => frame.Pointer, frame => entries.Single(entry => entry.Id == frame.Id).Blocks.ToArray());
        AssertEqual(Hash(expected), mixed.ContentIdentity, "Glass mixed custom/stock identity");
        entries[8].Blocks[12] = RoomBlockAppearance.Of(0x5a);
        entries[10].Blocks[15] = RoomBlockAppearance.Of(0x5b);
        foreach (var frame in frames)
        {
            int index = 0;
            for (int run = 0; run < native[frame.Pointer].Length; run++)
            for (int word = 0; word < native[frame.Pointer][run].Length; word++)
                AssertEqual(expected[frame.Pointer][index++], mixed.GetAppearance(NativePlmDraws.At<MotherBrainGlassDraw>(frame.Pointer),run,word), "Glass cloned custom and unchanged stock isolation");
        }
        AssertThrows<InvalidDataException>(() => new RoomPlmMotherBrainGlassVisualCatalog(entries[..1]), "Glass missing frame");
        AssertThrows<InvalidDataException>(() => new RoomPlmMotherBrainGlassVisualCatalog(entries.Append(entries[0])), "Glass duplicate frame");
        var invalid = entries.ToArray();
        invalid[0] = new("INITIAL", entries[0].Blocks);
        AssertThrows<InvalidDataException>(() => new RoomPlmMotherBrainGlassVisualCatalog(invalid), "Glass ordinal identity");
        invalid[0] = new(entries[0].Id, new RoomBlockAppearance[7]);
        AssertThrows<InvalidDataException>(() => new RoomPlmMotherBrainGlassVisualCatalog(invalid), "Glass exact flattened shape");
    }

    /// <summary>Reads native bank $84 glass draw-stream bytes as the stock reference.</summary>
    private static byte MotherBrainGlassStockMappingRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);
}
