using SuperMetroid.AssetExtraction;
using SuperMetroid.Verification.Rooms.Plms;
using SuperMetroid.Verification.Areas.Escape;
using SuperMetroid.Tooling;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using System.Text.Json.Nodes;
using static SuperMetroid.Verification.Rooms.Plms.SamusEaterPlmDefinitionsVerification;
using static SuperMetroid.Verification.Areas.Tourian.TourianAccessPlmProgramsVerification;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;
using SuperMetroid.Core.Areas.Tourian;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.TestSupport.RepositoryInstallation;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Areas.Tourian;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;
using SuperMetroid.Core.Areas.Tourian.Artwork;

/// <summary>Checks Tourian entrance crumble and clear art independently of its physical PLM blocks.</summary>
internal static class TourianAccessVisualsVerification
{
    /// <summary>Checks the extracted stock visuals and that a malformed installed visual file fails loudly where it is read.</summary>
    internal static void VerifyTourianAccessVisuals()
    {
        var rom = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(
            Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Tourian stock native oracle revision");
        // A private installation copy, whose stock file is tampered below.
        using (PrivateInstallationCopy copy = InstalledFixtures.CreatePrivateCopyOf(layout => layout.RoomPlmTourianAccessVisualDirectory))
        {
            GameInstallation installation = copy.Installation;
            RoomPlmTourianAccessVisualFilesConstants.ValidateStock(
                installation.RoomPlmTourianAccessVisualDirectory);
            VerifyTourianAccessStockMapping(rom, installation.LoadRoomPlmTourianAccessVisuals());

            string stockPath = Path.Combine(
                installation.RoomPlmTourianAccessVisualDirectory,
                RoomPlmTourianAccessVisualFiles.VisualFileName);
            File.WriteAllText(stockPath, "{}");
            AssertThrows<InvalidDataException>(
                () => RoomPlmTourianAccessVisualFilesConstants.ValidateStock(
                    installation.RoomPlmTourianAccessVisualDirectory),
                "tampered Tourian stock fails loudly where it is read");
        }
        Console.WriteLine(
            "Tourian access visuals: five native layouts and strict failures pass.");
    }

    /// <summary>Compares every stock visual word and content identity with cartridge draw lists.</summary>
    private static void VerifyTourianAccessStockMapping(SuperMetroidAddressSpace rom,
        RoomPlmTourianAccessVisuals installed)
    {
        LookupEscapeGateStockFrame[] frames = [new LookupEscapeGateStockFrame(0x9297,"crumble-empty-row"), new LookupEscapeGateStockFrame(0x92a3,"crumble-frame-0"),
            new LookupEscapeGateStockFrame(0x92af,"crumble-frame-1"), new LookupEscapeGateStockFrame(0x92bb,"crumble-frame-2"), new LookupEscapeGateStockFrame(0x92c7,"clear-six-rows")];
        var native = new Dictionary<ushort, RoomBlockAppearance[]>();
        foreach (var frame in frames)
        {
            var words = new RoomBlockAppearance[frame.Pointer == 0x92c7 ? 24 : 4];
            for (int index = 0; index < words.Length; index++)
                words[index] = RoomLevelWordCodec.DecodeAppearance(ReadSamusEaterPlmWord(rom,
                    0x840000 | (frame.Pointer + index / 4 * 12 + index % 4 * 2 + 2)));
            native.Add(frame.Pointer, words);
        }
        var entries = frames.Select(frame => new RoomPlmVisualEntry(frame.Id, native[frame.Pointer].ToArray())).ToArray();
        var stock = RoomPlmTourianAccessVisuals.Stock();
        var imported = RoomPlmTourianAccessVisualFiles.Create(entries.Reverse());
        string Hash(Dictionary<ushort, RoomBlockAppearance[]> words) => SuperMetroid.AssetExtraction.SelectedPresentationHash.FromAppearanceFrames(
            "RoomPlmTourianAccessVisualCatalog", words.ToDictionary(pair => RoomPlmTourianAccessVisualFiles.VisualId(NativePlmDraws.At<TourianAccessDraw>(pair.Key)), pair => pair.Value));
        foreach (var catalog in new[] {stock, imported, installed})
        {
            AssertEqual(Hash(native), catalog.ContentIdentity, "Tourian access original flattened hash");
            foreach (var frame in frames)
            {
                for (int index = 0; index < native[frame.Pointer].Length; index++)
                    AssertEqual(native[frame.Pointer][index], catalog.GetAppearance(NativePlmDraws.At<TourianAccessDraw>(frame.Pointer),index / 4,index % 4), "Tourian access native stock word");
                foreach (int bad in new[] {int.MinValue,-1,native[frame.Pointer].Length / 4,int.MaxValue})
                    AssertThrows<ArgumentOutOfRangeException>(() => catalog.GetAppearance(NativePlmDraws.At<TourianAccessDraw>(frame.Pointer),bad,0), "Tourian access run bounds");
                foreach (int bad in new[] {int.MinValue,-1,4,int.MaxValue})
                for (int run = 0; run < native[frame.Pointer].Length / 4; run++)
                    AssertThrows<ArgumentOutOfRangeException>(() => catalog.GetAppearance(NativePlmDraws.At<TourianAccessDraw>(frame.Pointer),run,bad), "Tourian access block bounds");
            }
        }
        for (int pointer = 0; pointer <= ushort.MaxValue; pointer++)
            if (!native.ContainsKey((ushort)pointer))
                AssertThrows<InvalidDataException>(() => stock.GetAppearance(NativePlmDraws.At<TourianAccessDraw>((ushort)pointer),0,0), "Tourian access unknown pointer");
        AssertThrows<InvalidDataException>(() => RoomPlmTourianAccessVisualFiles.Create(entries[..4]), "Tourian access missing frame");
        AssertThrows<InvalidDataException>(() => RoomPlmTourianAccessVisualFiles.Create(entries.Append(entries[0])), "Tourian access duplicate frame");
        var invalid = entries.ToArray();
        invalid[0] = new("CRUMBLE-EMPTY-ROW", entries[0].Blocks);
        AssertThrows<InvalidDataException>(() => RoomPlmTourianAccessVisualFiles.Create(invalid), "Tourian access ordinal identity");
        invalid[0] = new(entries[0].Id, new RoomBlockAppearance[3]);
        AssertThrows<InvalidDataException>(() => RoomPlmTourianAccessVisualFiles.Create(invalid), "Tourian access exact frame shape");
    }

    /// <summary>Runs Tourian access visual extraction and PLM isolation checks.</summary>
    [VerificationCommand("--tourian-access-visuals")]
    internal static int TourianAccessVisualsCommand()
    {
        Suite(nameof(VerifyTourianAccessVisuals), () => VerifyTourianAccessVisuals());
        return 0;
    }

}
