using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    private static void VerifyElevatubeStockMapping(SuperMetroidAddressSpace rom,
        RoomPlmMaridiaElevatubeVisualCatalog installed)
    {
        ushort original = (ushort)(ReadSamusEaterPlmWord(rom, 0x849369) & 0xfff);
        var entry = new RoomPlmMaridiaElevatubeVisualEntry("elevatube-block", [original]);
        var stock = RoomPlmMaridiaElevatubeVisualCatalog.Stock();
        var imported = new RoomPlmMaridiaElevatubeVisualCatalog([entry]);
        string Hash(ushort word) => SuperMetroid.Core.Assets.SelectedPresentationHash.Create(
            nameof(RoomPlmMaridiaElevatubeVisualCatalog), content => content.Append("visual word", word));
        foreach (var catalog in new[] {stock, imported, installed})
        {
            AssertEqual(original, catalog.GetWord(0x9367,0,0), "Elevatube original stock appearance");
            AssertEqual(Hash(original), catalog.ContentIdentity, "Elevatube original scalar identity");
            foreach (int invalid in new[] {int.MinValue,-1,1,int.MaxValue})
            {
                AssertThrows<InvalidDataException>(() => catalog.GetWord(0x9367,invalid,0), "Elevatube run bounds");
                AssertThrows<InvalidDataException>(() => catalog.GetWord(0x9367,0,invalid), "Elevatube cell bounds");
            }
        }
        for (int pointer = 0; pointer <= ushort.MaxValue; pointer++)
            if (pointer != 0x9367)
                AssertThrows<InvalidDataException>(() => stock.GetWord((ushort)pointer,0,0), "Elevatube full pointer domain");
        entry.Blocks[0] = 0x0c58;
        var custom = new RoomPlmMaridiaElevatubeVisualCatalog([entry]);
        entry.Blocks[0] = 0x59;
        AssertEqual(original, imported.GetWord(0x9367,0,0), "Elevatube imported scalar detached from input");
        AssertEqual((ushort)0x0c58, custom.GetWord(0x9367,0,0), "Elevatube custom scalar detached from input");
        AssertEqual(Hash(0x0c58), custom.ContentIdentity, "Elevatube custom scalar hash");
        AssertThrows<InvalidDataException>(() => new RoomPlmMaridiaElevatubeVisualCatalog([]), "Elevatube missing entry");
        AssertThrows<InvalidDataException>(() => new RoomPlmMaridiaElevatubeVisualCatalog([entry,entry]), "Elevatube duplicate entry");
        foreach (string invalid in new[] {"", "ELEVATUBE-BLOCK", "elevatube-block "})
            AssertThrows<InvalidDataException>(() => new RoomPlmMaridiaElevatubeVisualCatalog([new(invalid,[original])]), "Elevatube ordinal identity");
        foreach (ushort[] invalid in new ushort[][] {[], [original,original], [0xf058]})
            AssertThrows<InvalidDataException>(() => new RoomPlmMaridiaElevatubeVisualCatalog([new("elevatube-block",invalid)]), "Elevatube shape and visual bits");
    }
}
