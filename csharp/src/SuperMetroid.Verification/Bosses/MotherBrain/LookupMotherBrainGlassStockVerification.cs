using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Verification.Areas.Escape;
using static SuperMetroid.Verification.Rooms.RoomObjects.SamusEaterObjectDefinitionsVerification;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;

using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.AssetExtraction;
namespace SuperMetroid.Verification.Bosses.MotherBrain;

/// <summary>Checks Mother Brain glass PLM visuals through damage and shatter frames.</summary>
internal static class LookupMotherBrainGlassStockVerification
{
    /// <summary>Compares eleven stock frames, flattened identity, and strict import validation.</summary>
    internal static void VerifyMotherBrainGlassStockMapping(SuperMetroidAddressSpace rom,
        RoomObjectMotherBrainGlassVisualCatalog installed)
    {
        LookupEscapeGateStockFrame[] frames = [new LookupEscapeGateStockFrame(0x9717,"initial"),new LookupEscapeGateStockFrame(0x971d,"pane-damage-1"),new LookupEscapeGateStockFrame(0x9731,"pane-damage-2"),new LookupEscapeGateStockFrame(0x9745,"pane-transition"),new LookupEscapeGateStockFrame(0x974f,"shifted-pane-1"),new LookupEscapeGateStockFrame(0x9769,"shifted-pane-2"),new LookupEscapeGateStockFrame(0x9781,"shifted-pane-3"),new LookupEscapeGateStockFrame(0x978f,"shatter-1"),new LookupEscapeGateStockFrame(0x97b7,"shatter-2"),new LookupEscapeGateStockFrame(0x97e7,"shatter-3"),new LookupEscapeGateStockFrame(0x9817,"cleared")];
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
                byte x = MotherBrainGlassStockMappingRomByte(rom, 0x840000 | cursor++), y = MotherBrainGlassStockMappingRomByte(rom, 0x840000 | cursor++);
                if (x == 0 && y == 0) break;
            }
            native.Add(frame.Pointer, runs.ToArray());
        }
        var entries = frames.Select(frame => new RoomObjectVisualEntry(frame.Id,
            native[frame.Pointer].SelectMany(run => run).ToArray())).ToArray();
        var flat = frames.ToDictionary(frame => frame.Pointer, frame => native[frame.Pointer].SelectMany(run => run).ToArray());
        var stock = RoomObjectMotherBrainGlassVisualCatalog.Stock();
        var imported = RoomObjectMotherBrainGlassVisualFiles.Create(entries.Reverse());
        string Hash(Dictionary<ushort, RoomBlockAppearance[]> appearances) => SuperMetroid.AssetExtraction.SelectedPresentationHash.FromAppearanceFrames(
            nameof(RoomObjectMotherBrainGlassVisualCatalog), appearances.ToDictionary(pair => RoomObjectMotherBrainGlassVisualFiles.VisualId(NativeObjectDraws.At<MotherBrainGlassDraw>(pair.Key)), pair => pair.Value));
        foreach (var catalog in new[] {stock,imported,installed})
        {
            AssertEqual(Hash(flat), catalog.ContentIdentity, "Glass original flattened identity");
            foreach (var frame in frames)
            {
                var runs = native[frame.Pointer];
                for (int run = 0; run < runs.Length; run++)
                {
                    for (int word = 0; word < runs[run].Length; word++)
                        AssertEqual(runs[run][word], catalog.GetAppearance(NativeObjectDraws.At<MotherBrainGlassDraw>(frame.Pointer),run,word), "Glass original stock appearance");
                    foreach (int bad in new[] {int.MinValue,-1,runs[run].Length,int.MaxValue})
                        AssertThrows<ArgumentOutOfRangeException>(() => catalog.GetAppearance(NativeObjectDraws.At<MotherBrainGlassDraw>(frame.Pointer),run,bad), "Glass stock cell bounds");
                }
                foreach (int bad in new[] {int.MinValue,-1,runs.Length,int.MaxValue})
                    AssertThrows<ArgumentOutOfRangeException>(() => catalog.GetAppearance(NativeObjectDraws.At<MotherBrainGlassDraw>(frame.Pointer),bad,0), "Glass stock run bounds");
            }
        }
        for (int pointer = 0; pointer <= ushort.MaxValue; pointer++)
            if (!native.ContainsKey((ushort)pointer))
                AssertThrows<InvalidDataException>(() => stock.GetAppearance(NativeObjectDraws.At<MotherBrainGlassDraw>((ushort)pointer),0,0), "Glass stock unknown pointer");
        AssertThrows<InvalidDataException>(() => RoomObjectMotherBrainGlassVisualFiles.Create(entries[..1]), "Glass missing frame");
        AssertThrows<InvalidDataException>(() => RoomObjectMotherBrainGlassVisualFiles.Create(entries.Append(entries[0])), "Glass duplicate frame");
        var invalid = entries.ToArray();
        invalid[0] = new("INITIAL", entries[0].Blocks);
        AssertThrows<InvalidDataException>(() => RoomObjectMotherBrainGlassVisualFiles.Create(invalid), "Glass ordinal identity");
        invalid[0] = new(entries[0].Id, new RoomBlockAppearance[7]);
        AssertThrows<InvalidDataException>(() => RoomObjectMotherBrainGlassVisualFiles.Create(invalid), "Glass exact flattened shape");
    }

    /// <summary>Reads native bank $84 glass draw-stream bytes as the stock reference.</summary>
    private static byte MotherBrainGlassStockMappingRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);
}
