using SuperMetroid.Verification.Areas.Escape;
using static SuperMetroid.Verification.Rooms.Plms.SamusEaterPlmDefinitionsVerification;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Verification.Rooms.Plms;

/// <summary>Checks downward gate column and colored trigger draw lists against bank-$84 PLM data.</summary>
internal static class LookupDownwardGateStockVerification
{
    /// <summary>Checks fourteen stock frames, run shapes, edited words, content identity, and rejection bounds.</summary>
    internal static void VerifyDownwardGateStockMapping(SuperMetroidAddressSpace rom,
        RoomPlmDownwardGateVisualCatalog installed)
    {
        LookupEscapeGateStockFrame[] frames = [new LookupEscapeGateStockFrame(0xa517,"column-frame-0"),new LookupEscapeGateStockFrame(0xa525,"column-frame-1"),
            new LookupEscapeGateStockFrame(0xa533,"column-frame-2"),new LookupEscapeGateStockFrame(0xa541,"column-frame-3"),new LookupEscapeGateStockFrame(0xa54f,"column-frame-4"),new LookupEscapeGateStockFrame(0xa55d,"column-frame-5"),
            new LookupEscapeGateStockFrame(0xa5d7,"blue-left-trigger"),new LookupEscapeGateStockFrame(0xa5e3,"blue-right-trigger"),new LookupEscapeGateStockFrame(0xa5eb,"green-left-trigger"),
            new LookupEscapeGateStockFrame(0xa5f7,"green-right-trigger"),new LookupEscapeGateStockFrame(0xa5ff,"red-left-trigger"),new LookupEscapeGateStockFrame(0xa60b,"red-right-trigger"),
            new LookupEscapeGateStockFrame(0xa613,"yellow-left-trigger"),new LookupEscapeGateStockFrame(0xa61f,"yellow-right-trigger")];
        var native = new Dictionary<ushort, ushort[][]>();
        foreach (var frame in frames)
        {
            var runs = new List<ushort[]>();
            int cursor = frame.Pointer;
            while (true)
            {
                int count = ReadSamusEaterPlmWord(rom, 0x840000 | cursor) & 0x7fff;
                cursor += 2;
                var words = new ushort[count];
                for (int word = 0; word < count; word++, cursor += 2)
                    words[word] = (ushort)(ReadSamusEaterPlmWord(rom, 0x840000 | cursor) & 0xfff);
                runs.Add(words);
                byte x = DownwardGateStockMappingRomByte(rom, 0x840000 | cursor++), y = DownwardGateStockMappingRomByte(rom, 0x840000 | cursor++);
                if (x == 0 && y == 0) break;
            }
            native.Add(frame.Pointer, runs.ToArray());
        }
        var entries = frames.Select(frame => new RoomPlmVisualRunEntry(frame.Id,
            native[frame.Pointer].Select(run => run.ToArray()).ToArray())).ToArray();
        var stock = RoomPlmDownwardGateVisualCatalog.Stock();
        var imported = new RoomPlmDownwardGateVisualCatalog(entries.Reverse());
        string Hash(Dictionary<ushort, ushort[][]> words) => SuperMetroid.Core.Assets.SelectedPresentationHash.FromWordFrames(
            nameof(RoomPlmDownwardGateVisualCatalog), words.ToDictionary(pair => DownwardGatePlmDrawDefinitions.VisualId(NativePlmDraws.At<DownwardGateDraw>(pair.Key)), pair => pair.Value));
        foreach (var catalog in new[] {stock, imported, installed})
        {
            AssertEqual(Hash(native), catalog.ContentIdentity, "Gate original run-framed hash");
            foreach (var frame in frames)
            {
                var runs = native[frame.Pointer];
                for (int run = 0; run < runs.Length; run++)
                {
                    for (int word = 0; word < runs[run].Length; word++)
                        AssertEqual(runs[run][word], catalog.GetWord(NativePlmDraws.At<DownwardGateDraw>(frame.Pointer),run,word), "Gate original stock word");
                    foreach (int bad in new[] {int.MinValue,-1,runs[run].Length,int.MaxValue})
                        AssertThrows<ArgumentOutOfRangeException>(() => catalog.GetWord(NativePlmDraws.At<DownwardGateDraw>(frame.Pointer),run,bad), "Gate stock word bounds");
                }
                foreach (int bad in new[] {int.MinValue,-1,runs.Length,int.MaxValue})
                    AssertThrows<ArgumentOutOfRangeException>(() => catalog.GetWord(NativePlmDraws.At<DownwardGateDraw>(frame.Pointer),bad,0), "Gate stock run bounds");
            }
        }
        for (int pointer = 0; pointer <= ushort.MaxValue; pointer++)
            if (!native.ContainsKey((ushort)pointer))
                AssertThrows<InvalidDataException>(() => stock.GetWord(NativePlmDraws.At<DownwardGateDraw>((ushort)pointer),0,0), "Gate stock unknown pointer");
        entries[6].Runs[1][0] = 0xc58;
        entries[5].Runs[0][4] = 0x59;
        var mixed = new RoomPlmDownwardGateVisualCatalog(entries);
        var expected = frames.ToDictionary(frame => frame.Pointer,
            frame => entries.Single(entry => entry.Id == frame.Id).Runs.Select(run => run.ToArray()).ToArray());
        AssertEqual(Hash(expected), mixed.ContentIdentity, "Gate mixed stock/custom hash");
        entries[6].Runs[1][0] = 0x5a;
        entries[0].Runs[0][0] = 0x5b;
        foreach (var frame in frames)
        for (int run = 0; run < expected[frame.Pointer].Length; run++)
        for (int word = 0; word < expected[frame.Pointer][run].Length; word++)
            AssertEqual(expected[frame.Pointer][run][word], mixed.GetWord(NativePlmDraws.At<DownwardGateDraw>(frame.Pointer),run,word), "Gate clone isolation and stock fallback");
        AssertThrows<InvalidDataException>(() => new RoomPlmDownwardGateVisualCatalog(entries[..13]), "Gate missing frame");
        AssertThrows<InvalidDataException>(() => new RoomPlmDownwardGateVisualCatalog(entries.Append(entries[0])), "Gate duplicate frame");
        var invalid = entries.ToArray();
        invalid[0] = new("COLUMN-FRAME-0",entries[0].Runs);
        AssertThrows<InvalidDataException>(() => new RoomPlmDownwardGateVisualCatalog(invalid), "Gate ordinal identity");
        invalid[0] = new(entries[0].Id,[new ushort[4]]);
        AssertThrows<InvalidDataException>(() => new RoomPlmDownwardGateVisualCatalog(invalid), "Gate exact word shape");
        invalid[0] = new(entries[0].Id,[new ushort[5],new ushort[1]]);
        AssertThrows<InvalidDataException>(() => new RoomPlmDownwardGateVisualCatalog(invalid), "Gate exact run shape");
        entries[0].Runs[0][0] = 0xf058;
        AssertThrows<InvalidDataException>(() => new RoomPlmDownwardGateVisualCatalog(entries), "Gate visual bits only");
    }

    /// <summary>Reads a native downward-gate draw-list byte for the stock oracle.</summary>
    private static byte DownwardGateStockMappingRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);
}
