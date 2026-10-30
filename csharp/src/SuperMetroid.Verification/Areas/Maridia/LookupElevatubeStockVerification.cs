using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.AssetExtraction.Cartridge;
using static SuperMetroid.Verification.Rooms.Plms.SamusEaterPlmDefinitionsVerification;
using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Verification.Areas.Maridia;

/// <summary>Checks the stock elevatube block against its native word and rejects every unsupported catalog coordinate and identity.</summary>
internal static class LookupElevatubeStockVerification
{
    /// <summary>Compares stock, imported, and installed visual words, including snapshot isolation from mutable input arrays.</summary>
    internal static void VerifyElevatubeStockMapping(SuperMetroidAddressSpace rom,
        InstalledRoomPlmMaridiaElevatubeVisuals installed)
    {
        RoomBlockAppearance original = RoomLevelWordCodec.DecodeAppearance(ReadSamusEaterPlmWord(rom, 0x849369));
        var entry = new RoomPlmVisualEntry("elevatube-block", [original]);
        var stock = InstalledRoomPlmMaridiaElevatubeVisuals.Stock();
        var imported = new InstalledRoomPlmMaridiaElevatubeVisuals([entry]);
        string Hash(RoomBlockAppearance appearance) => SuperMetroid.Core.Assets.SelectedPresentationHash.Create(
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
        entry.Blocks[0] = RoomBlockAppearance.Of(0x058, BlockMirroring.Horizontal | BlockMirroring.Vertical);
        var custom = new InstalledRoomPlmMaridiaElevatubeVisuals([entry]);
        entry.Blocks[0] = RoomBlockAppearance.Of(0x059);
        AssertEqual(original, imported.GetAppearance(NativePlmDraws.At<MaridiaElevatubePlmDefinitions.Draw>(0x9367),0,0), "Elevatube imported scalar detached from input");
        AssertEqual(RoomBlockAppearance.Of(0x058, BlockMirroring.Horizontal | BlockMirroring.Vertical), custom.GetAppearance(NativePlmDraws.At<MaridiaElevatubePlmDefinitions.Draw>(0x9367),0,0), "Elevatube custom scalar detached from input");
        AssertEqual(Hash(RoomBlockAppearance.Of(0x058, BlockMirroring.Horizontal | BlockMirroring.Vertical)), custom.ContentIdentity, "Elevatube custom scalar hash");
        AssertThrows<InvalidDataException>(() => new InstalledRoomPlmMaridiaElevatubeVisuals([]), "Elevatube missing entry");
        AssertThrows<InvalidDataException>(() => new InstalledRoomPlmMaridiaElevatubeVisuals([entry,entry]), "Elevatube duplicate entry");
        foreach (string invalid in new[] {"", "ELEVATUBE-BLOCK", "elevatube-block "})
            AssertThrows<InvalidDataException>(() => new InstalledRoomPlmMaridiaElevatubeVisuals([new(invalid,[original])]), "Elevatube ordinal identity");
        foreach (RoomBlockAppearance[] invalid in new RoomBlockAppearance[][] {[], [original,original]})
            AssertThrows<InvalidDataException>(() => new InstalledRoomPlmMaridiaElevatubeVisuals([new("elevatube-block",invalid)]), "Elevatube shape");
    }
}
