using SuperMetroid.AssetExtraction;
using static SuperMetroid.Verification.Bosses.MotherBrain.BabyMetroidRouteDefinitionsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Bosses.Botwoon;
using SuperMetroid.Verification.Harness;

/// <summary>Checks editable Botwoon wall-clear tiles remain separate from PLM collision and timing.</summary>
internal static class BotwoonWallVisualsVerification
{
    /// <summary>Compares the nine stock draw words with cartridge data and checks the catalog's coordinate domain.</summary>
    internal static void VerifyBotwoonWallStockMapping(SuperMetroidAddressSpace rom)
    {
        var native = new RoomBlockAppearance[9];
        for (int index = 0; index < native.Length; index++)
            native[index] = RoomLevelWordCodec.DecodeAppearance(ReadRomWord(rom, 0x849311 + 2 * index));
        var stock = InstalledRoomObjectBotwoonWallVisuals.Stock();
        var loaded = RoomObjectBotwoonWallVisualFiles.Create([new("clear-wall", native)]);
        string expectedIdentity = SuperMetroid.AssetExtraction.SelectedPresentationHash.Create(
            nameof(InstalledRoomObjectBotwoonWallVisuals), content => content.AppendAppearances("blocks", native));
        AssertEqual(expectedIdentity, stock.ContentIdentity, "Botwoon calculated stock preserves prior content identity");
        AssertEqual(expectedIdentity, loaded.ContentIdentity, "Botwoon loaded stock identity");
        for (int index = 0; index < native.Length; index++)
        {
            AssertEqual(native[index], stock.GetAppearance(NativeObjectDraws.At<BotwoonWallDraw>(0x930f), 0, index), "Botwoon stock appearance matches native");
            AssertEqual(native[index], loaded.GetAppearance(NativeObjectDraws.At<BotwoonWallDraw>(0x930f), 0, index), "Botwoon imported stock appearance matches native");
        }
        foreach (var catalog in new[] {stock, loaded})
        {
            foreach (int index in new[] {int.MinValue,-1,9,int.MaxValue})
                AssertThrows<ArgumentOutOfRangeException>(() => catalog.GetAppearance(NativeObjectDraws.At<BotwoonWallDraw>(0x930f), 0, index), "Botwoon visual block domain");
            foreach (int run in new[] {int.MinValue,-1,1,int.MaxValue})
                AssertThrows<InvalidDataException>(() => catalog.GetAppearance(NativeObjectDraws.At<BotwoonWallDraw>(0x930f), run, 0), "Botwoon visual run domain");
            foreach (ushort pointer in new ushort[] {0,0x930e,0x9310,0xffff})
                AssertThrows<InvalidDataException>(() => catalog.GetAppearance(NativeObjectDraws.At<BotwoonWallDraw>(pointer), 0, 0), "Botwoon visual pointer domain");
        }
    }

    /// <summary>Requires only the native clear-draw pointer to resolve to Botwoon's stable visual ID.</summary>
    internal static void VerifyBotwoonWallVisualIdMapping()
    {
        for (int pointer = 0; pointer <= ushort.MaxValue; pointer++)
            if (pointer == 0x930f)
                AssertEqual("clear-wall", RoomObjectBotwoonWallVisualFiles.VisualId(NativeObjectDraws.At<BotwoonWallDraw>((ushort)pointer)), "Botwoon stable export identity");
            else
                AssertThrows<InvalidDataException>(() => RoomObjectBotwoonWallVisualFiles.VisualId(NativeObjectDraws.At<BotwoonWallDraw>((ushort)pointer)), "Botwoon visual ID accepts only the clear draw");
    }

    /// <summary>Checks the extracted stock visuals and that a malformed installed visual file fails loudly where it is read.</summary>
    internal static void VerifyBotwoonWallVisuals()
    {
        SuperMetroidAddressSpace rom = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(
            Path.GetFullPath("Super Metroid.smc"));
        // A private installation copy, whose stock file is tampered below.
        using (PrivateInstallationCopy copy = InstalledFixtures.CreatePrivateCopyOf(layout => layout.RoomObjectBotwoonWallVisualDirectory))
        {
            GameInstallation installation = copy.Installation;
            RoomObjectBotwoonWallVisualFilesConstants.ValidateStock(
                installation.RoomObjectBotwoonWallVisualDirectory);
            InstalledRoomObjectBotwoonWallVisuals stock =
                installation.LoadRoomObjectBotwoonWallVisuals();
            for (int block = 0; block < 9; block++)
                AssertEqual(RoomBlockAppearance.Of(0x0ff),
                    stock.GetAppearance(BotwoonWallObjectDrawDefinitions.Clear,
                        0, block),
                    $"stock Botwoon wall clear block {block} matches ROM");
            AssertThrows<InvalidDataException>(
                () => RoomObjectBotwoonWallVisualFiles.Create([]),
                "Botwoon wall visual catalog rejects missing clear frame");

            string stockPath = Path.Combine(
                installation.RoomObjectBotwoonWallVisualDirectory,
                RoomObjectBotwoonWallVisualFiles.VisualFileName);
            File.WriteAllText(stockPath, "{}");
            AssertThrows<InvalidDataException>(
                () => RoomObjectBotwoonWallVisualFilesConstants.ValidateStock(
                    installation.RoomObjectBotwoonWallVisualDirectory),
                "tampered Botwoon wall stock fails loudly where it is read");
        }
        Console.WriteLine(
            "Botwoon wall visuals: native nine-tile clear and strict failures pass.");
    }

    /// <summary>Runs the installed Botwoon wall visual check from the command dispatcher.</summary>
    [VerificationCommand("--botwoon-wall-visuals")]
    internal static int BotwoonWallVisualsCommand()
    {
        Suite(nameof(VerifyBotwoonWallVisuals), () => VerifyBotwoonWallVisuals());
        return 0;
    }

}
