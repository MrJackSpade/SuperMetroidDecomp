using SuperMetroid.TestSupport.RepositoryInstallation;
using SuperMetroid.Verification.Areas.Escape;
using static SuperMetroid.Verification.Rooms.RoomObjects.InstalledFramePictureAssertions;
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
    /// <summary>Compares the nine-cell clear's installed picture with the cartridge, then the import and its rejections (#1557).</summary>
    internal static void VerifyBotwoonWallStockMapping(CartridgeImportAddressSpace rom)
    {
        LookupEscapeGateStockFrame[] frames = [new LookupEscapeGateStockFrame(0x930f, "clear-wall")];
        InstalledRoomObjectBotwoonWallVisuals stock = InstalledRoomObjectBotwoonWallVisuals.Stock();
        AssertPicturedAsNative(rom, stock.Images, RoomObjectBotwoonWallVisualFiles.Family, frames, WrittenCells(RoomObjectBotwoonWallVisualFiles.Family));
        foreach (ushort pointer in new ushort[] {0,0x930e,0x9310,0xffff})
            AssertThrows<InvalidDataException>(() => stock.Image(NativeObjectDraws.At<BotwoonWallDraw>(pointer)), "Botwoon visual pointer domain");
        AssertImportAndRejections(rom, stock.Images, RoomObjectBotwoonWallVisualFiles.Family, RepositoryInstallation.Installation.RoomObjectBotwoonWallVisualDirectory);
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
        CartridgeImportAddressSpace rom = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(
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
                    stock.Image(BotwoonWallObjectDrawDefinitions.Clear).At(new Offset16<RoomBlocks>(0, block)),
                    $"stock Botwoon wall clear block {block} matches ROM");

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
