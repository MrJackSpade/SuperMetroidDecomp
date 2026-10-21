using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Verification.Areas.Escape;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Rooms.Plms.SamusEaterPlmDefinitionsVerification;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Verification.Rooms.Doors;

/// <summary>Checks the 48 colored-door draw frames, editable overlays, and stock isolation.</summary>
internal static class LookupColoredDoorStockVerification
{
    /// <summary>Compares each four-row colored-door frame with ROM and validates lookup, hashing, and malformed edits.</summary>
    internal static void VerifyColoredDoorStockMapping(SuperMetroidAddressSpace rom,
        RoomPlmColoredDoorVisualCatalog installed)
    {
        LookupColoredCapFamily[] families = [
            new LookupColoredCapFamily(0xa767,"yellow-left"),new LookupColoredCapFamily(0xa797,"yellow-right"),new LookupColoredCapFamily(0xa7c7,"yellow-up"),new LookupColoredCapFamily(0xa7f7,"yellow-down"),
            new LookupColoredCapFamily(0xa827,"green-left"),new LookupColoredCapFamily(0xa857,"green-right"),new LookupColoredCapFamily(0xa887,"green-up"),new LookupColoredCapFamily(0xa8b7,"green-down"),
            new LookupColoredCapFamily(0xa8e7,"red-left"),new LookupColoredCapFamily(0xa917,"red-right"),new LookupColoredCapFamily(0xa947,"red-up"),new LookupColoredCapFamily(0xa977,"red-down")];
        var frames = families.SelectMany(family => Enumerable.Range(0,4).Select(frame =>
            new LookupEscapeGateStockFrame(Pointer:(ushort)(family.First + 12 * frame), Id:$"{family.Name}-frame-{frame}"))).ToArray();
        var native = new Dictionary<ushort, ushort[]>();
        foreach (var frame in frames)
        {
            var words = new ushort[4];
            for (int row = 0; row < words.Length; row++)
                words[row] = (ushort)(ReadSamusEaterPlmWord(rom, 0x840000 | (frame.Pointer + 2 + row * 2)) & 0xfff);
            native.Add(frame.Pointer, words);
        }
        var entries = frames.Select(frame => new RoomPlmVisualEntry(frame.Id, native[frame.Pointer].ToArray())).ToArray();
        var stock = RoomPlmColoredDoorVisualCatalog.Stock();
        var imported = new RoomPlmColoredDoorVisualCatalog(entries.Reverse());
        string Hash(Dictionary<ushort, ushort[]> words) => SuperMetroid.Core.Assets.SelectedPresentationHash.FromWordFrames(
            nameof(RoomPlmColoredDoorVisualCatalog), words.ToDictionary(pair => ColoredDoorPlmDrawDefinitions.VisualId(NativePlmDraws.At<ColoredDoorPlmDrawDefinitions.Draw>(pair.Key)), pair => pair.Value));
        foreach (var catalog in new[] {stock,imported,installed})
        {
            AssertEqual(Hash(native), catalog.ContentIdentity, "Colored door original stock identity");
            foreach (var frame in frames)
            {
                for (int row = 0; row < 4; row++)
                    AssertEqual(native[frame.Pointer][row], catalog.GetWord(NativePlmDraws.At<ColoredDoorPlmDrawDefinitions.Draw>(frame.Pointer),row), "Colored door native stock visual");
                foreach (int bad in new[] {int.MinValue,-1,4,int.MaxValue})
                    AssertThrows<ArgumentOutOfRangeException>(() => catalog.GetWord(NativePlmDraws.At<ColoredDoorPlmDrawDefinitions.Draw>(frame.Pointer),bad), "Colored door stock row bounds");
            }
        }
        for (int raw = 0; raw <= ushort.MaxValue; raw++)
            if (!native.ContainsKey((ushort)raw))
                AssertThrows<InvalidDataException>(() => stock.GetWord(NativePlmDraws.At<ColoredDoorPlmDrawDefinitions.Draw>((ushort)raw),0), "Colored door unknown stock pointer");
        entries[47].Blocks[1] = 0xc58;
        entries[16].Blocks[3] = 0x59;
        var mixed = new RoomPlmColoredDoorVisualCatalog(entries);
        var expected = frames.ToDictionary(frame => frame.Pointer,
            frame => entries.Single(entry => entry.Id == frame.Id).Blocks.ToArray());
        AssertEqual(Hash(expected), mixed.ContentIdentity, "Colored door custom/stock hash");
        entries[47].Blocks[1] = 0x5a;
        entries[0].Blocks[0] = 0x5b;
        foreach (var frame in frames)
        for (int row = 0; row < 4; row++)
            AssertEqual(expected[frame.Pointer][row], mixed.GetWord(NativePlmDraws.At<ColoredDoorPlmDrawDefinitions.Draw>(frame.Pointer),row), "Colored door custom clone and stock isolation");
        AssertThrows<InvalidDataException>(() => new RoomPlmColoredDoorVisualCatalog(entries[..47]), "Colored door missing frame");
        AssertThrows<InvalidDataException>(() => new RoomPlmColoredDoorVisualCatalog(entries.Append(entries[0])), "Colored door duplicate frame");
        var invalid = entries.ToArray();
        invalid[0] = new("YELLOW-LEFT-FRAME-0", entries[0].Blocks);
        AssertThrows<InvalidDataException>(() => new RoomPlmColoredDoorVisualCatalog(invalid), "Colored door ordinal identity");
        invalid[0] = new(entries[0].Id, new ushort[3]);
        AssertThrows<InvalidDataException>(() => new RoomPlmColoredDoorVisualCatalog(invalid), "Colored door exact shape");
        entries[0].Blocks[0] = 0xf058;
        AssertThrows<InvalidDataException>(() => new RoomPlmColoredDoorVisualCatalog(entries), "Colored door visual bits only");
    }
}
