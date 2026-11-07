using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Verification.Areas.Escape;
using static SuperMetroid.Verification.Rooms.RoomObjects.SamusEaterObjectDefinitionsVerification;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Rooms.RoomObjects;

/// <summary>Checks stock and installed Noob Tube visual words against native PLM draw lists.</summary>
internal static class LookupNoobTubeStockVerification
{
    /// <summary>Compares seven flattened frame shapes and identities, then checks rejection of invalid visual entries.</summary>
    internal static void VerifyNoobTubeStockMapping(SuperMetroidAddressSpace rom,
        InstalledRoomObjectNoobTubeVisuals installed)
    {
        LookupEscapeGateStockFrame[] frames = [new LookupEscapeGateStockFrame(0x98d1,"intact"),new LookupEscapeGateStockFrame(0x98d7,"damaged"),new LookupEscapeGateStockFrame(0x98dd,"opened"),new LookupEscapeGateStockFrame(0x98e3,"cleared"),new LookupEscapeGateStockFrame(0x9953,"broken-late"),new LookupEscapeGateStockFrame(0x9991,"opened-rows"),new LookupEscapeGateStockFrame(0x99e5,"broken-full")];
        var native = new Dictionary<ushort, RoomBlockAppearance[][]>();
        foreach (var frame in frames)
        {
            var runs = new List<RoomBlockAppearance[]>();
            int cursor = frame.Pointer;
            while (true)
            {
                int count = ReadSamusEaterObjectWord(rom, 0x840000 | cursor) & 0x7fff;
                cursor += 2;
                var words = new RoomBlockAppearance[count];
                for (int word = 0; word < count; word++, cursor += 2)
                    words[word] = RoomLevelWordCodec.DecodeAppearance(ReadSamusEaterObjectWord(rom, 0x840000 | cursor));
                runs.Add(words);
                byte x = NoobTubeStockMappingRomByte(rom, 0x840000 | cursor++), y = NoobTubeStockMappingRomByte(rom, 0x840000 | cursor++);
                if (x == 0 && y == 0) break;
            }
            native.Add(frame.Pointer, runs.ToArray());
        }
        var entries = frames.Select(frame => new RoomObjectVisualEntry(frame.Id,
            native[frame.Pointer].SelectMany(run => run).ToArray())).ToArray();
        var flat = frames.ToDictionary(frame => frame.Pointer, frame => native[frame.Pointer].SelectMany(run => run).ToArray());
        var stock = InstalledRoomObjectNoobTubeVisuals.Stock();
        var imported = RoomObjectNoobTubeVisualFiles.Create(entries.Reverse());
        string Hash(Dictionary<ushort, RoomBlockAppearance[]> words) => SuperMetroid.AssetExtraction.SelectedPresentationHash.FromAppearanceFrames(
            nameof(InstalledRoomObjectNoobTubeVisuals), words.ToDictionary(pair => RoomObjectNoobTubeVisualFiles.VisualId(NativeObjectDraws.At<NoobTubeDraw>(pair.Key)), pair => pair.Value));
        foreach (var catalog in new[] {stock,imported,installed})
        {
            AssertEqual(Hash(flat), catalog.ContentIdentity, "Tube original flattened identity");
            foreach (var frame in frames)
            {
                var runs = native[frame.Pointer];
                for (int run = 0; run < runs.Length; run++)
                {
                    for (int word = 0; word < runs[run].Length; word++)
                        AssertEqual(runs[run][word], catalog.GetAppearance(NativeObjectDraws.At<NoobTubeDraw>(frame.Pointer),run,word), "Tube original stock word");
                    foreach (int bad in new[] {int.MinValue,-1,runs[run].Length,int.MaxValue})
                        AssertThrows<ArgumentOutOfRangeException>(() => catalog.GetAppearance(NativeObjectDraws.At<NoobTubeDraw>(frame.Pointer),run,bad), "Tube stock word bounds");
                }
                foreach (int bad in new[] {int.MinValue,-1,runs.Length,int.MaxValue})
                    AssertThrows<ArgumentOutOfRangeException>(() => catalog.GetAppearance(NativeObjectDraws.At<NoobTubeDraw>(frame.Pointer),bad,0), "Tube stock run bounds");
            }
        }
        for (int pointer = 0; pointer <= ushort.MaxValue; pointer++)
            if (!native.ContainsKey((ushort)pointer))
                AssertThrows<InvalidDataException>(() => stock.GetAppearance(NativeObjectDraws.At<NoobTubeDraw>((ushort)pointer),0,0), "Tube stock unknown pointer");
        AssertThrows<InvalidDataException>(() => RoomObjectNoobTubeVisualFiles.Create(entries[..1]), "Tube missing frame");
        AssertThrows<InvalidDataException>(() => RoomObjectNoobTubeVisualFiles.Create(entries.Append(entries[0])), "Tube duplicate frame");
        var invalid = entries.ToArray();
        invalid[0] = new("INTACT", entries[0].Blocks);
        AssertThrows<InvalidDataException>(() => RoomObjectNoobTubeVisualFiles.Create(invalid), "Tube ordinal identity");
        invalid[0] = new(entries[0].Id, new RoomBlockAppearance[7]);
        AssertThrows<InvalidDataException>(() => RoomObjectNoobTubeVisualFiles.Create(invalid), "Tube exact flattened shape");
    }

    /// <summary>Reads native PLM run displacement bytes for the stock visual comparison.</summary>
    private static byte NoobTubeStockMappingRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);
}
