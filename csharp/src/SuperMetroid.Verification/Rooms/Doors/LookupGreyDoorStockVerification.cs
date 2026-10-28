using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Verification.Areas.Escape;
using SuperMetroid.AssetExtraction.Cartridge;
using static SuperMetroid.Verification.Rooms.Plms.SamusEaterPlmDefinitionsVerification;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Verification.Rooms.Doors;

/// <summary>Checks grey-door stock visuals and catalog validation against native draw frames.</summary>
internal static class LookupGreyDoorStockVerification
{
    /// <summary>Compares twenty four-row frames, content identity, clone isolation, and rejected pointers and document shapes.</summary>
    internal static void VerifyGreyDoorStockMapping(SuperMetroidAddressSpace rom,
        RoomPlmGreyDoorVisualCatalog installed)
    {
        LookupEscapeGateStockFrame[] frames = [new LookupEscapeGateStockFrame(0xa677,"clear-left"),new LookupEscapeGateStockFrame(0xa683,"clear-right"),new LookupEscapeGateStockFrame(0xa68f,"clear-up"),new LookupEscapeGateStockFrame(0xa69b,"clear-down"),
            new LookupEscapeGateStockFrame(0xa6a7,"grey-left-frame-0"),new LookupEscapeGateStockFrame(0xa6b3,"grey-left-frame-1"),new LookupEscapeGateStockFrame(0xa6bf,"grey-left-frame-2"),new LookupEscapeGateStockFrame(0xa6cb,"grey-left-frame-3"),
            new LookupEscapeGateStockFrame(0xa6d7,"grey-right-frame-0"),new LookupEscapeGateStockFrame(0xa6e3,"grey-right-frame-1"),new LookupEscapeGateStockFrame(0xa6ef,"grey-right-frame-2"),new LookupEscapeGateStockFrame(0xa6fb,"grey-right-frame-3"),
            new LookupEscapeGateStockFrame(0xa707,"grey-up-frame-0"),new LookupEscapeGateStockFrame(0xa713,"grey-up-frame-1"),new LookupEscapeGateStockFrame(0xa71f,"grey-up-frame-2"),new LookupEscapeGateStockFrame(0xa72b,"grey-up-frame-3"),
            new LookupEscapeGateStockFrame(0xa737,"grey-down-frame-0"),new LookupEscapeGateStockFrame(0xa743,"grey-down-frame-1"),new LookupEscapeGateStockFrame(0xa74f,"grey-down-frame-2"),new LookupEscapeGateStockFrame(0xa75b,"grey-down-frame-3")];
        var native = new Dictionary<ushort, ushort[]>();
        foreach (var frame in frames)
        {
            var words = new ushort[4];
            for (int row = 0; row < words.Length; row++)
                words[row] = (ushort)(ReadSamusEaterPlmWord(rom, 0x840000 | (frame.Pointer + 2 + row * 2)) & 0xfff);
            native.Add(frame.Pointer, words);
        }
        var entries = frames.Select(frame => new RoomPlmVisualEntry(frame.Id, native[frame.Pointer].ToArray())).ToArray();
        var stock = RoomPlmGreyDoorVisualCatalog.Stock();
        var imported = new RoomPlmGreyDoorVisualCatalog(entries.Reverse());
        string Hash(Dictionary<ushort, ushort[]> words) => SuperMetroid.Core.Assets.SelectedPresentationHash.FromWordFrames(
            nameof(RoomPlmGreyDoorVisualCatalog), words.ToDictionary(pair => GreyDoorCaps.VisualId(NativePlmDraws.At<GreyDoorCaps.Draw>(pair.Key)), pair => pair.Value));
        foreach (var catalog in new[] {stock,imported,installed})
        {
            AssertEqual(Hash(native), catalog.ContentIdentity, "Grey door original stock identity");
            foreach (var frame in frames)
            {
                for (int row = 0; row < 4; row++)
                    AssertEqual(native[frame.Pointer][row], catalog.GetWord(NativePlmDraws.At<GreyDoorCaps.Draw>(frame.Pointer),row), "Grey door native stock visual");
                foreach (int bad in new[] {int.MinValue,-1,4,int.MaxValue})
                    AssertThrows<ArgumentOutOfRangeException>(() => catalog.GetWord(NativePlmDraws.At<GreyDoorCaps.Draw>(frame.Pointer),bad), "Grey door stock row bounds");
            }
        }
        for (int raw = 0; raw <= ushort.MaxValue; raw++)
            if (!native.ContainsKey((ushort)raw))
                AssertThrows<InvalidDataException>(() => stock.GetWord(NativePlmDraws.At<GreyDoorCaps.Draw>((ushort)raw),0), "Grey door unknown stock pointer");
        entries[19].Blocks[1] = 0xc58;
        entries[8].Blocks[3] = 0x59;
        var mixed = new RoomPlmGreyDoorVisualCatalog(entries);
        var expected = frames.ToDictionary(frame => frame.Pointer,
            frame => entries.Single(entry => entry.Id == frame.Id).Blocks.ToArray());
        AssertEqual(Hash(expected), mixed.ContentIdentity, "Grey door custom/stock hash");
        entries[19].Blocks[1] = 0x5a;
        entries[0].Blocks[0] = 0x5b;
        foreach (var frame in frames)
        for (int row = 0; row < 4; row++)
            AssertEqual(expected[frame.Pointer][row], mixed.GetWord(NativePlmDraws.At<GreyDoorCaps.Draw>(frame.Pointer),row), "Grey door custom clone and stock isolation");
        AssertThrows<InvalidDataException>(() => new RoomPlmGreyDoorVisualCatalog(entries[..19]), "Grey door missing frame");
        AssertThrows<InvalidDataException>(() => new RoomPlmGreyDoorVisualCatalog(entries.Append(entries[0])), "Grey door duplicate frame");
        var invalid = entries.ToArray();
        invalid[0] = new("CLEAR-LEFT", entries[0].Blocks);
        AssertThrows<InvalidDataException>(() => new RoomPlmGreyDoorVisualCatalog(invalid), "Grey door ordinal identity");
        invalid[0] = new(entries[0].Id, new ushort[3]);
        AssertThrows<InvalidDataException>(() => new RoomPlmGreyDoorVisualCatalog(invalid), "Grey door exact shape");
        entries[0].Blocks[0] = 0xf058;
        AssertThrows<InvalidDataException>(() => new RoomPlmGreyDoorVisualCatalog(entries), "Grey door visual bits only");
    }
}
