using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Verification.Areas.Escape;
using static SuperMetroid.Verification.Rooms.Plms.SamusEaterPlmDefinitionsVerification;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Verification.Rooms.Plms;

/// <summary>Checks map, save, and recharge station visual frames against native PLM draw lists.</summary>
internal static class LookupStationStockVerification
{
    /// <summary>Compares twenty stock frames and selected hashes, then checks copied edits and invalid entry rejection.</summary>
    internal static void VerifyStationStockMapping(SuperMetroidAddressSpace rom,
        InstalledRoomPlmStationVisuals installed)
    {
        LookupEscapeGateStockFrame[] frames = [new LookupEscapeGateStockFrame(0x9f25,"map-frame-0"),new LookupEscapeGateStockFrame(0x9f6d,"energy-frame-0"),new LookupEscapeGateStockFrame(0x9f91,"missile-frame-0"),
            new LookupEscapeGateStockFrame(0x9f31,"map-frame-1"),new LookupEscapeGateStockFrame(0x9f79,"energy-frame-1"),new LookupEscapeGateStockFrame(0x9f9d,"missile-frame-1"),
            new LookupEscapeGateStockFrame(0x9f3d,"map-frame-2"),new LookupEscapeGateStockFrame(0x9f85,"energy-frame-2"),new LookupEscapeGateStockFrame(0x9fa9,"missile-frame-2"),
            new LookupEscapeGateStockFrame(0x9a3f,"save-idle"),new LookupEscapeGateStockFrame(0x9a9f,"save-active-a"),new LookupEscapeGateStockFrame(0x9a6f,"save-active-b"),
            new LookupEscapeGateStockFrame(0x9f49,"map-right-retracted"),new LookupEscapeGateStockFrame(0x9f55,"map-right-extended"),new LookupEscapeGateStockFrame(0x9f5b,"map-left-retracted"),new LookupEscapeGateStockFrame(0x9f67,"map-left-extended"),
            new LookupEscapeGateStockFrame(0x9fb5,"resource-right-retracted"),new LookupEscapeGateStockFrame(0x9fbb,"resource-right-extended"),new LookupEscapeGateStockFrame(0x9fc1,"resource-left-retracted"),new LookupEscapeGateStockFrame(0x9fc7,"resource-left-extended")];
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
                byte x = StationStockMappingRomByte(rom, 0x840000 | cursor++), y = StationStockMappingRomByte(rom, 0x840000 | cursor++);
                if (x == 0 && y == 0) break;
            }
            native.Add(frame.Pointer, runs.ToArray());
        }
        var entries = frames.Select(frame => new RoomPlmVisualRunEntry(frame.Id,
            native[frame.Pointer].Select(run => run.ToArray()).ToArray())).ToArray();
        var original = frames.ToDictionary(frame => frame.Pointer, frame => native[frame.Pointer].Select(run => run.ToArray()).ToArray());
        var stock = InstalledRoomPlmStationVisuals.Stock();
        var imported = new InstalledRoomPlmStationVisuals(entries.Reverse());
        string Hash(Dictionary<ushort, RoomBlockAppearance[][]> words) => SuperMetroid.Core.Assets.SelectedPresentationHash.FromAppearanceFrames(
            nameof(InstalledRoomPlmStationVisuals), words.ToDictionary(pair => StationPlmDraws.VisualId(NativePlmDraws.At<StationDraw>(pair.Key)), pair => pair.Value));
        foreach (var catalog in new[] {stock,imported,installed})
        {
            AssertEqual(Hash(original), catalog.ContentIdentity, "Station original run-framed identity");
            foreach (var frame in frames)
            {
                var runs = native[frame.Pointer];
                for (int run = 0; run < runs.Length; run++)
                {
                    for (int word = 0; word < runs[run].Length; word++)
                        AssertEqual(runs[run][word], catalog.GetAppearance(NativePlmDraws.At<StationDraw>(frame.Pointer),run,word), "Station original stock word");
                    foreach (int bad in new[] {int.MinValue,-1,runs[run].Length,int.MaxValue})
                        AssertThrows<ArgumentOutOfRangeException>(() => catalog.GetAppearance(NativePlmDraws.At<StationDraw>(frame.Pointer),run,bad), "Station stock word bounds");
                }
                foreach (int bad in new[] {int.MinValue,-1,runs.Length,int.MaxValue})
                    AssertThrows<ArgumentOutOfRangeException>(() => catalog.GetAppearance(NativePlmDraws.At<StationDraw>(frame.Pointer),bad,0), "Station stock run bounds");
            }
        }
        for (int pointer = 0; pointer <= ushort.MaxValue; pointer++)
            if (!native.ContainsKey((ushort)pointer))
                AssertThrows<InvalidDataException>(() => stock.GetAppearance(NativePlmDraws.At<StationDraw>((ushort)pointer),0,0), "Station stock unknown pointer");
        entries[10].Runs[3][1] = 0xc58;
        var mixed = new InstalledRoomPlmStationVisuals(entries);
        var expected = frames.ToDictionary(frame => frame.Pointer, frame => entries.Single(entry => entry.Id == frame.Id).Runs.Select(run => run.ToArray()).ToArray());
        AssertEqual(Hash(expected), mixed.ContentIdentity, "Station mixed custom/stock identity");
        entries[10].Runs[3][1] = 0x5a;
        entries[11].Runs[5][0] = 0x5b;
        foreach (var frame in frames)
        {
            for (int run = 0; run < native[frame.Pointer].Length; run++)
            for (int word = 0; word < native[frame.Pointer][run].Length; word++)
                AssertEqual(expected[frame.Pointer][run][word], mixed.GetAppearance(NativePlmDraws.At<StationDraw>(frame.Pointer),run,word), "Station cloned custom and unchanged stock isolation");
        }
        AssertThrows<InvalidDataException>(() => new InstalledRoomPlmStationVisuals(entries[..1]), "Station missing frame");
        AssertThrows<InvalidDataException>(() => new InstalledRoomPlmStationVisuals(entries.Append(entries[0])), "Station duplicate frame");
        var invalid = entries.ToArray();
        invalid[0] = new("MAP-FRAME-0", entries[0].Runs);
        AssertThrows<InvalidDataException>(() => new InstalledRoomPlmStationVisuals(invalid), "Station ordinal identity");
        invalid[0] = new(entries[0].Id, new RoomBlockAppearance[][] { new RoomBlockAppearance[7] });
        AssertThrows<InvalidDataException>(() => new InstalledRoomPlmStationVisuals(invalid), "Station exact run shape");
    }

    /// <summary>Reads native run-displacement bytes as the stock station draw oracle.</summary>
    private static byte StationStockMappingRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);
}
