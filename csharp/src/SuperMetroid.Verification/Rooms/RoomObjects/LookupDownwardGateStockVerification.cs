using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Verification.Areas.Escape;
using static SuperMetroid.Verification.Rooms.RoomObjects.SamusEaterObjectDefinitionsVerification;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Rooms.RoomObjects;

/// <summary>Checks downward gate column and colored trigger draw lists against bank-$84 PLM data.</summary>
internal static class LookupDownwardGateStockVerification
{
    /// <summary>Checks fourteen stock frames, run shapes, content identity, and rejection bounds.</summary>
    internal static void VerifyDownwardGateStockMapping(SuperMetroidAddressSpace rom,
        InstalledRoomObjectDownwardGateVisuals installed)
    {
        LookupEscapeGateStockFrame[] frames = [new LookupEscapeGateStockFrame(0xa517,"column-frame-0"),new LookupEscapeGateStockFrame(0xa525,"column-frame-1"),
            new LookupEscapeGateStockFrame(0xa533,"column-frame-2"),new LookupEscapeGateStockFrame(0xa541,"column-frame-3"),new LookupEscapeGateStockFrame(0xa54f,"column-frame-4"),new LookupEscapeGateStockFrame(0xa55d,"column-frame-5"),
            new LookupEscapeGateStockFrame(0xa5d7,"blue-left-trigger"),new LookupEscapeGateStockFrame(0xa5e3,"blue-right-trigger"),new LookupEscapeGateStockFrame(0xa5eb,"green-left-trigger"),
            new LookupEscapeGateStockFrame(0xa5f7,"green-right-trigger"),new LookupEscapeGateStockFrame(0xa5ff,"red-left-trigger"),new LookupEscapeGateStockFrame(0xa60b,"red-right-trigger"),
            new LookupEscapeGateStockFrame(0xa613,"yellow-left-trigger"),new LookupEscapeGateStockFrame(0xa61f,"yellow-right-trigger")];
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
                byte x = DownwardGateStockMappingRomByte(rom, 0x840000 | cursor++), y = DownwardGateStockMappingRomByte(rom, 0x840000 | cursor++);
                if (x == 0 && y == 0) break;
            }
            native.Add(frame.Pointer, runs.ToArray());
        }
        var entries = frames.Select(frame => new RoomObjectVisualRunEntry(frame.Id,
            native[frame.Pointer].Select(run => run.ToArray()).ToArray())).ToArray();
        var stock = InstalledRoomObjectDownwardGateVisuals.Stock();
        var imported = RoomObjectDownwardGateVisualFiles.Create(entries.Reverse());
        string Hash(Dictionary<ushort, RoomBlockAppearance[][]> words) => SuperMetroid.AssetExtraction.SelectedPresentationHash.FromAppearanceFrames(
            nameof(InstalledRoomObjectDownwardGateVisuals), words.ToDictionary(pair => RoomObjectDownwardGateVisualFiles.VisualId(NativeObjectDraws.At<DownwardGateDraw>(pair.Key)), pair => pair.Value));
        foreach (var catalog in new[] {stock, imported, installed})
        {
            AssertEqual(Hash(native), catalog.ContentIdentity, "Gate original run-framed hash");
            foreach (var frame in frames)
            {
                var runs = native[frame.Pointer];
                for (int run = 0; run < runs.Length; run++)
                {
                    for (int word = 0; word < runs[run].Length; word++)
                        AssertEqual(runs[run][word], catalog.GetAppearance(NativeObjectDraws.At<DownwardGateDraw>(frame.Pointer),run,word), "Gate original stock word");
                    foreach (int bad in new[] {int.MinValue,-1,runs[run].Length,int.MaxValue})
                        AssertThrows<ArgumentOutOfRangeException>(() => catalog.GetAppearance(NativeObjectDraws.At<DownwardGateDraw>(frame.Pointer),run,bad), "Gate stock word bounds");
                }
                foreach (int bad in new[] {int.MinValue,-1,runs.Length,int.MaxValue})
                    AssertThrows<ArgumentOutOfRangeException>(() => catalog.GetAppearance(NativeObjectDraws.At<DownwardGateDraw>(frame.Pointer),bad,0), "Gate stock run bounds");
            }
        }
        for (int pointer = 0; pointer <= ushort.MaxValue; pointer++)
            if (!native.ContainsKey((ushort)pointer))
                AssertThrows<InvalidDataException>(() => stock.GetAppearance(NativeObjectDraws.At<DownwardGateDraw>((ushort)pointer),0,0), "Gate stock unknown pointer");
        AssertThrows<InvalidDataException>(() => RoomObjectDownwardGateVisualFiles.Create(entries[..13]), "Gate missing frame");
        AssertThrows<InvalidDataException>(() => RoomObjectDownwardGateVisualFiles.Create(entries.Append(entries[0])), "Gate duplicate frame");
        var invalid = entries.ToArray();
        invalid[0] = new("COLUMN-FRAME-0",entries[0].Runs);
        AssertThrows<InvalidDataException>(() => RoomObjectDownwardGateVisualFiles.Create(invalid), "Gate ordinal identity");
        invalid[0] = new(entries[0].Id,[new RoomBlockAppearance[4]]);
        AssertThrows<InvalidDataException>(() => RoomObjectDownwardGateVisualFiles.Create(invalid), "Gate exact word shape");
        invalid[0] = new(entries[0].Id,[new RoomBlockAppearance[5],new RoomBlockAppearance[1]]);
        AssertThrows<InvalidDataException>(() => RoomObjectDownwardGateVisualFiles.Create(invalid), "Gate exact run shape");
    }

    /// <summary>Reads a native downward-gate draw-list byte for the stock oracle.</summary>
    private static byte DownwardGateStockMappingRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);
}
