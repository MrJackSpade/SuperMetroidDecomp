using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Verification.Areas.Escape;
using SuperMetroid.AssetExtraction.Cartridge;
using static SuperMetroid.Verification.Rooms.Plms.SamusEaterPlmDefinitionsVerification;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Verification.Rooms.Doors;

/// <summary>Checks blue door visual catalogs against native frames, physical aliases, and editable asset constraints.</summary>
internal static class LookupBlueDoorStockVerification
{
    /// <summary>Compares all four orientations and their aliases with bank-$84 art, then checks custom-frame isolation and validation.</summary>
    internal static void VerifyBlueDoorStockMapping(SuperMetroidAddressSpace rom,
        InstalledRoomPlmBlueDoorVisuals installed)
    {
        LookupEscapeGateStockFrame[] frames = [
            new LookupEscapeGateStockFrame(0xa9b3,"left-frame-0"),new LookupEscapeGateStockFrame(0xa9bf,"left-frame-1"),new LookupEscapeGateStockFrame(0xa9cb,"left-frame-2"),new LookupEscapeGateStockFrame(0xa9d7,"left-frame-3"),
            new LookupEscapeGateStockFrame(0xa9ef,"right-frame-0"),new LookupEscapeGateStockFrame(0xa9fb,"right-frame-1"),new LookupEscapeGateStockFrame(0xaa07,"right-frame-2"),new LookupEscapeGateStockFrame(0xaa13,"right-frame-3"),
            new LookupEscapeGateStockFrame(0xaa2b,"up-frame-0"),new LookupEscapeGateStockFrame(0xaa37,"up-frame-1"),new LookupEscapeGateStockFrame(0xaa43,"up-frame-2"),new LookupEscapeGateStockFrame(0xaa4f,"up-frame-3"),
            new LookupEscapeGateStockFrame(0xaa67,"down-frame-0"),new LookupEscapeGateStockFrame(0xaa73,"down-frame-1"),new LookupEscapeGateStockFrame(0xaa7f,"down-frame-2"),new LookupEscapeGateStockFrame(0xaa8b,"down-frame-3")];
        ushort[] aliases = [0xa9a7,0xa9e3,0xaa1f,0xaa5b];
        var native = new Dictionary<ushort, ushort[]>();
        foreach (var frame in frames)
        {
            var words = new ushort[4];
            for (int row = 0; row < words.Length; row++)
                words[row] = (ushort)(ReadSamusEaterPlmWord(rom, 0x840000 | (frame.Pointer + 2 + row * 2)) & 0xfff);
            native.Add(frame.Pointer, words);
        }
        var entries = frames.Select(frame => new RoomPlmVisualEntry(frame.Id, native[frame.Pointer].ToArray())).ToArray();
        var stock = InstalledRoomPlmBlueDoorVisuals.Stock();
        var imported = new InstalledRoomPlmBlueDoorVisuals(entries.Reverse());
        string Hash(Dictionary<ushort, ushort[]> words) => SuperMetroid.Core.Assets.SelectedPresentationHash.FromWordFrames(
            nameof(InstalledRoomPlmBlueDoorVisuals), words.ToDictionary(pair => BlueDoorCaps.VisualId(NativePlmDraws.At<BlueDoorCaps.Draw>(pair.Key)), pair => pair.Value));
        foreach (var catalog in new[] {stock,imported,installed})
        {
            AssertEqual(Hash(native), catalog.ContentIdentity, "Blue door original stock identity");
            foreach (var frame in frames)
            {
                for (int row = 0; row < 4; row++)
                    AssertEqual(native[frame.Pointer][row], catalog.GetWord(NativePlmDraws.At<BlueDoorCaps.Draw>(frame.Pointer),row), "Blue door native stock visual");
                foreach (int bad in new[] {int.MinValue,-1,4,int.MaxValue})
                    AssertThrows<ArgumentOutOfRangeException>(() => catalog.GetWord(NativePlmDraws.At<BlueDoorCaps.Draw>(frame.Pointer),bad), "Blue door stock row bounds");
            }
        }
        foreach (var catalog in new[] {stock,imported,installed})
        foreach (ushort alias in aliases)
        {
            for (int row = 0; row < 4; row++)
                AssertEqual((ushort)(ReadSamusEaterPlmWord(rom, 0x840000 | (alias + 2 + row * 2)) & 0xfff),
                    catalog.GetWord(NativePlmDraws.At<BlueDoorCaps.Draw>(alias),row), "Blue physical alias original stock visuals");
            foreach (int bad in new[] {int.MinValue,-1,4,int.MaxValue})
                AssertThrows<ArgumentOutOfRangeException>(() => catalog.GetWord(NativePlmDraws.At<BlueDoorCaps.Draw>(alias),bad), "Blue alias cell bounds");
        }
        for (int raw = 0; raw <= ushort.MaxValue; raw++)
            if (!native.ContainsKey((ushort)raw) && !aliases.Contains((ushort)raw))
                AssertThrows<InvalidDataException>(() => stock.GetWord(NativePlmDraws.At<BlueDoorCaps.Draw>((ushort)raw),0), "Blue door unknown stock pointer");
        entries[12].Blocks[1] = 0xc58;
        entries[8].Blocks[3] = 0x59;
        var mixed = new InstalledRoomPlmBlueDoorVisuals(entries);
        var expected = frames.ToDictionary(frame => frame.Pointer,
            frame => entries.Single(entry => entry.Id == frame.Id).Blocks.ToArray());
        AssertEqual(Hash(expected), mixed.ContentIdentity, "Blue door custom/stock hash");
        entries[12].Blocks[1] = 0x5a;
        entries[0].Blocks[0] = 0x5b;
        foreach (var frame in frames)
        for (int row = 0; row < 4; row++)
            AssertEqual(expected[frame.Pointer][row], mixed.GetWord(NativePlmDraws.At<BlueDoorCaps.Draw>(frame.Pointer),row), "Blue door custom clone and stock isolation");
        for (int orientation = 0; orientation < aliases.Length; orientation++)
        for (int row = 0; row < 4; row++)
            AssertEqual(expected[frames[orientation * 4].Pointer][row], mixed.GetWord(NativePlmDraws.At<BlueDoorCaps.Draw>(aliases[orientation]),row),
                "Blue alias follows custom or stock canonical frame");
        AssertThrows<InvalidDataException>(() => new InstalledRoomPlmBlueDoorVisuals(entries[..15]), "Blue door missing frame");
        AssertThrows<InvalidDataException>(() => new InstalledRoomPlmBlueDoorVisuals(entries.Append(entries[0])), "Blue door duplicate frame");
        var invalid = entries.ToArray();
        invalid[0] = new("LEFT-FRAME-0", entries[0].Blocks);
        AssertThrows<InvalidDataException>(() => new InstalledRoomPlmBlueDoorVisuals(invalid), "Blue door ordinal identity");
        invalid[0] = new(entries[0].Id, new ushort[3]);
        AssertThrows<InvalidDataException>(() => new InstalledRoomPlmBlueDoorVisuals(invalid), "Blue door exact shape");
        entries[0].Blocks[0] = 0xf058;
        AssertThrows<InvalidDataException>(() => new InstalledRoomPlmBlueDoorVisuals(entries), "Blue door visual bits only");
    }
}
