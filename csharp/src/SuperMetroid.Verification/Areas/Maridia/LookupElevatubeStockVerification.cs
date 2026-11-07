using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.AssetExtraction.Cartridge;
using static SuperMetroid.Verification.Rooms.RoomObjects.SamusEaterObjectDefinitionsVerification;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Verification.Areas.Maridia;

/// <summary>Checks the stock elevatube block against its native word and rejects every unsupported catalog coordinate and identity.</summary>
internal static class LookupElevatubeStockVerification
{
    /// <summary>Compares stock, imported, and installed visual words, and rejects invalid imports.</summary>
    internal static void VerifyElevatubeStockMapping(SuperMetroidAddressSpace rom,
        InstalledRoomObjectMaridiaElevatubeVisuals installed)
    {
        RoomBlockAppearance original = RoomLevelWordCodec.DecodeAppearance(ReadSamusEaterObjectWord(rom, 0x849369));
        var entry = new RoomObjectVisualEntry("elevatube-block", [original]);
        var stock = InstalledRoomObjectMaridiaElevatubeVisuals.Stock();
        var imported = RoomObjectMaridiaElevatubeVisualFiles.Create([entry]);
        string Hash(RoomBlockAppearance appearance) => SuperMetroid.AssetExtraction.SelectedPresentationHash.Create(
            nameof(InstalledRoomObjectMaridiaElevatubeVisuals), content => content.AppendAppearances("block", [appearance]));
        foreach (var catalog in new[] {stock, imported, installed})
        {
            AssertEqual(original, catalog.GetAppearance(NativeObjectDraws.At<MaridiaElevatubeObjectDefinitions.Draw>(0x9367),0,0), "Elevatube original stock appearance");
            AssertEqual(Hash(original), catalog.ContentIdentity, "Elevatube original scalar identity");
            foreach (int invalid in new[] {int.MinValue,-1,1,int.MaxValue})
            {
                AssertThrows<InvalidDataException>(() => catalog.GetAppearance(NativeObjectDraws.At<MaridiaElevatubeObjectDefinitions.Draw>(0x9367),invalid,0), "Elevatube run bounds");
                AssertThrows<InvalidDataException>(() => catalog.GetAppearance(NativeObjectDraws.At<MaridiaElevatubeObjectDefinitions.Draw>(0x9367),0,invalid), "Elevatube cell bounds");
            }
        }
        for (int pointer = 0; pointer <= ushort.MaxValue; pointer++)
            if (pointer != 0x9367)
                AssertThrows<InvalidDataException>(() => stock.GetAppearance(NativeObjectDraws.At<MaridiaElevatubeObjectDefinitions.Draw>((ushort)pointer),0,0), "Elevatube full pointer domain");
        AssertThrows<InvalidDataException>(() => RoomObjectMaridiaElevatubeVisualFiles.Create([]), "Elevatube missing entry");
        AssertThrows<InvalidDataException>(() => RoomObjectMaridiaElevatubeVisualFiles.Create([entry,entry]), "Elevatube duplicate entry");
        foreach (string invalid in new[] {"", "ELEVATUBE-BLOCK", "elevatube-block "})
            AssertThrows<InvalidDataException>(() => RoomObjectMaridiaElevatubeVisualFiles.Create([new(invalid,[original])]), "Elevatube ordinal identity");
        foreach (RoomBlockAppearance[] invalid in new RoomBlockAppearance[][] {[], [original,original]})
            AssertThrows<InvalidDataException>(() => RoomObjectMaridiaElevatubeVisualFiles.Create([new("elevatube-block",invalid)]), "Elevatube shape");
    }
}
