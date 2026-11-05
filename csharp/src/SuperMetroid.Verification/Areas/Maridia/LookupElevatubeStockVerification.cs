using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.AssetExtraction.Cartridge;
using static SuperMetroid.Verification.Rooms.Plms.SamusEaterPlmDefinitionsVerification;
using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Verification.Areas.Maridia;

/// <summary>Checks the stock elevatube block against its native word and rejects every unsupported catalog coordinate and identity.</summary>
internal static class LookupElevatubeStockVerification
{
    /// <summary>Compares stock, imported, and installed visual words, and rejects invalid imports.</summary>
    internal static void VerifyElevatubeStockMapping(SuperMetroidAddressSpace rom,
        InstalledRoomPlmMaridiaElevatubeVisuals installed)
    {
        RoomBlockAppearance original = RoomLevelWordCodec.DecodeAppearance(ReadSamusEaterPlmWord(rom, 0x849369));
        var entry = new RoomPlmVisualEntry("elevatube-block", [original]);
        var stock = InstalledRoomPlmMaridiaElevatubeVisuals.Stock();
        var imported = RoomPlmMaridiaElevatubeVisualFiles.Create([entry]);
        string Hash(RoomBlockAppearance appearance) => SuperMetroid.AssetExtraction.SelectedPresentationHash.Create(
            nameof(InstalledRoomPlmMaridiaElevatubeVisuals), content => content.AppendAppearances("block", [appearance]));
        foreach (var catalog in new[] {stock, imported, installed})
        {
            AssertEqual(original, catalog.GetAppearance(NativePlmDraws.At<MaridiaElevatubePlmDefinitions.Draw>(0x9367),0,0), "Elevatube original stock appearance");
            AssertEqual(Hash(original), catalog.ContentIdentity, "Elevatube original scalar identity");
            foreach (int invalid in new[] {int.MinValue,-1,1,int.MaxValue})
            {
                AssertThrows<InvalidDataException>(() => catalog.GetAppearance(NativePlmDraws.At<MaridiaElevatubePlmDefinitions.Draw>(0x9367),invalid,0), "Elevatube run bounds");
                AssertThrows<InvalidDataException>(() => catalog.GetAppearance(NativePlmDraws.At<MaridiaElevatubePlmDefinitions.Draw>(0x9367),0,invalid), "Elevatube cell bounds");
            }
        }
        for (int pointer = 0; pointer <= ushort.MaxValue; pointer++)
            if (pointer != 0x9367)
                AssertThrows<InvalidDataException>(() => stock.GetAppearance(NativePlmDraws.At<MaridiaElevatubePlmDefinitions.Draw>((ushort)pointer),0,0), "Elevatube full pointer domain");
        AssertThrows<InvalidDataException>(() => RoomPlmMaridiaElevatubeVisualFiles.Create([]), "Elevatube missing entry");
        AssertThrows<InvalidDataException>(() => RoomPlmMaridiaElevatubeVisualFiles.Create([entry,entry]), "Elevatube duplicate entry");
        foreach (string invalid in new[] {"", "ELEVATUBE-BLOCK", "elevatube-block "})
            AssertThrows<InvalidDataException>(() => RoomPlmMaridiaElevatubeVisualFiles.Create([new(invalid,[original])]), "Elevatube ordinal identity");
        foreach (RoomBlockAppearance[] invalid in new RoomBlockAppearance[][] {[], [original,original]})
            AssertThrows<InvalidDataException>(() => RoomPlmMaridiaElevatubeVisualFiles.Create([new("elevatube-block",invalid)]), "Elevatube shape");
    }
}
