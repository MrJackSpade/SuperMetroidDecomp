using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Verification.Areas.Escape;
using static SuperMetroid.Verification.Rooms.Plms.SamusEaterPlmDefinitionsVerification;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Verification.Rooms.Plms;

/// <summary>Checks stock and installed Noob Tube visual words against native PLM draw lists.</summary>
internal static class LookupNoobTubeStockVerification
{
    /// <summary>Compares seven flattened frame shapes and identities, then checks copied edits and rejection of invalid visual entries.</summary>
    internal static void VerifyNoobTubeStockMapping(SuperMetroidAddressSpace rom,
        InstalledRoomPlmNoobTubeVisuals installed)
    {
        LookupEscapeGateStockFrame[] frames = [new LookupEscapeGateStockFrame(0x98d1,"intact"),new LookupEscapeGateStockFrame(0x98d7,"damaged"),new LookupEscapeGateStockFrame(0x98dd,"opened"),new LookupEscapeGateStockFrame(0x98e3,"cleared"),new LookupEscapeGateStockFrame(0x9953,"broken-late"),new LookupEscapeGateStockFrame(0x9991,"opened-rows"),new LookupEscapeGateStockFrame(0x99e5,"broken-full")];
        var native = new Dictionary<ushort, RoomBlockAppearance[][]>();
        foreach (var frame in frames)
        {
            var runs = new List<RoomBlockAppearance[]>();
            int cursor = frame.Pointer;
            while (true)
            {
                int count = ReadSamusEaterPlmWord(rom, 0x840000 | cursor) & 0x7fff;
                cursor += 2;
                var words = new RoomBlockAppearance[count];
                for (int word = 0; word < count; word++, cursor += 2)
                    words[word] = RoomLevelWordCodec.DecodeAppearance(ReadSamusEaterPlmWord(rom, 0x840000 | cursor));
                runs.Add(words);
                byte x = NoobTubeStockMappingRomByte(rom, 0x840000 | cursor++), y = NoobTubeStockMappingRomByte(rom, 0x840000 | cursor++);
                if (x == 0 && y == 0) break;
            }
            native.Add(frame.Pointer, runs.ToArray());
        }
        var entries = frames.Select(frame => new RoomPlmVisualEntry(frame.Id,
            native[frame.Pointer].SelectMany(run => run).ToArray())).ToArray();
        var flat = frames.ToDictionary(frame => frame.Pointer, frame => native[frame.Pointer].SelectMany(run => run).ToArray());
        var stock = InstalledRoomPlmNoobTubeVisuals.Stock();
        var imported = new InstalledRoomPlmNoobTubeVisuals(entries.Reverse());
        string Hash(Dictionary<ushort, RoomBlockAppearance[]> words) => SuperMetroid.Core.Assets.SelectedPresentationHash.FromAppearanceFrames(
            nameof(InstalledRoomPlmNoobTubeVisuals), words.ToDictionary(pair => NoobTubePlmDraws.VisualId(NativePlmDraws.At<NoobTubeDraw>(pair.Key)), pair => pair.Value));
        foreach (var catalog in new[] {stock,imported,installed})
        {
            AssertEqual(Hash(flat), catalog.ContentIdentity, "Tube original flattened identity");
            foreach (var frame in frames)
            {
                var runs = native[frame.Pointer];
                for (int run = 0; run < runs.Length; run++)
                {
                    for (int word = 0; word < runs[run].Length; word++)
                        AssertEqual(runs[run][word], catalog.GetAppearance(NativePlmDraws.At<NoobTubeDraw>(frame.Pointer),run,word), "Tube original stock word");
                    foreach (int bad in new[] {int.MinValue,-1,runs[run].Length,int.MaxValue})
                        AssertThrows<ArgumentOutOfRangeException>(() => catalog.GetAppearance(NativePlmDraws.At<NoobTubeDraw>(frame.Pointer),run,bad), "Tube stock word bounds");
                }
                foreach (int bad in new[] {int.MinValue,-1,runs.Length,int.MaxValue})
                    AssertThrows<ArgumentOutOfRangeException>(() => catalog.GetAppearance(NativePlmDraws.At<NoobTubeDraw>(frame.Pointer),bad,0), "Tube stock run bounds");
            }
        }
        for (int pointer = 0; pointer <= ushort.MaxValue; pointer++)
            if (!native.ContainsKey((ushort)pointer))
                AssertThrows<InvalidDataException>(() => stock.GetAppearance(NativePlmDraws.At<NoobTubeDraw>((ushort)pointer),0,0), "Tube stock unknown pointer");
        entries[3].Blocks[24] = RoomBlockAppearance.Of(0x058, BlockMirroring.Horizontal | BlockMirroring.Vertical);
        var mixed = new InstalledRoomPlmNoobTubeVisuals(entries);
        var expected = frames.ToDictionary(frame => frame.Pointer, frame => entries.Single(entry => entry.Id == frame.Id).Blocks.ToArray());
        AssertEqual(Hash(expected), mixed.ContentIdentity, "Tube mixed custom/stock identity");
        entries[3].Blocks[24] = RoomBlockAppearance.Of(0x05a);
        entries[6].Blocks[36] = RoomBlockAppearance.Of(0x05b);
        foreach (var frame in frames)
        {
            int index = 0;
            for (int run = 0; run < native[frame.Pointer].Length; run++)
            for (int word = 0; word < native[frame.Pointer][run].Length; word++)
                AssertEqual(expected[frame.Pointer][index++], mixed.GetAppearance(NativePlmDraws.At<NoobTubeDraw>(frame.Pointer),run,word), "Tube cloned custom and unchanged stock isolation");
        }
        AssertThrows<InvalidDataException>(() => new InstalledRoomPlmNoobTubeVisuals(entries[..1]), "Tube missing frame");
        AssertThrows<InvalidDataException>(() => new InstalledRoomPlmNoobTubeVisuals(entries.Append(entries[0])), "Tube duplicate frame");
        var invalid = entries.ToArray();
        invalid[0] = new("INTACT", entries[0].Blocks);
        AssertThrows<InvalidDataException>(() => new InstalledRoomPlmNoobTubeVisuals(invalid), "Tube ordinal identity");
        invalid[0] = new(entries[0].Id, new RoomBlockAppearance[7]);
        AssertThrows<InvalidDataException>(() => new InstalledRoomPlmNoobTubeVisuals(invalid), "Tube exact flattened shape");
    }

    /// <summary>Reads native PLM run displacement bytes for the stock visual comparison.</summary>
    private static byte NoobTubeStockMappingRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);
}
