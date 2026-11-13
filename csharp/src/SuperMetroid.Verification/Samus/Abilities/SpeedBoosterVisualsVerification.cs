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
namespace SuperMetroid.Verification.Samus.Abilities;
using SuperMetroid.Verification.Harness;

/// <summary>Checks the bomb-revealed Speed Booster block's installed image.</summary>
internal static class SpeedBoosterVisualsVerification
{
    /// <summary>Compares the reveal's installed picture with the cartridge, rejects every other draw, then checks the import and its rejections (#1557).</summary>
    private static void VerifySpeedBoosterStockVisualMapping(CartridgeImportAddressSpace rom)
    {
        const ushort pointer = 0xa4f3;
        LookupEscapeGateStockFrame[] frames = [new LookupEscapeGateStockFrame(pointer, "bomb-reveal")];
        InstalledRoomObjectSpeedBoosterVisuals stock = InstalledRoomObjectSpeedBoosterVisuals.Stock();
        AssertPicturedAsNative(rom, stock.Images, RoomObjectSpeedBoosterVisualFiles.Family, frames, WrittenCells(RoomObjectSpeedBoosterVisualFiles.Family));
        for (int address = 0; address <= ushort.MaxValue; address++)
            if (address != pointer)
                AssertThrows<InvalidDataException>(() => stock.Image(NativeObjectDraws.At<SpeedBoosterBlockObjectDrawDefinitions.Draw>((ushort)address)),"speed visual full pointer rejection");
        AssertImportAndRejections(rom, stock.Images, RoomObjectSpeedBoosterVisualFiles.Family, RepositoryInstallation.Installation.RoomObjectSpeedBoosterVisualDirectory);
    }

    /// <summary>Checks the extracted stock visuals and that a malformed installed visual file fails loudly where it is read.</summary>
    internal static void VerifySpeedBoosterVisuals()
    {
        CartridgeImportAddressSpace rom = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(
            Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(VerifySpeedBoosterStockVisualMapping), () => VerifySpeedBoosterStockVisualMapping(rom));
        using PrivateInstallationCopy copy = InstalledFixtures.CreatePrivateCopyOf(layout => layout.RoomObjectSpeedBoosterVisualDirectory);
        GameInstallation installation = copy.Installation;
        RoomObjectSpeedBoosterVisualFilesConstants.ValidateStock(
            installation.RoomObjectSpeedBoosterVisualDirectory);
        RoomObjectDraw drawPointer = SpeedBoosterBlockObjectDrawDefinitions.BombRevealDraw;
        AssertEqual(RoomBlockAppearance.Of(0x0b6),
            installation.LoadRoomObjectSpeedBoosterVisuals()
                .Image(drawPointer).At(Offset16<RoomBlocks>.Zero),
            "stock bomb-reveal visual matches the cartridge");

        string stockPath = Path.Combine(
            installation.RoomObjectSpeedBoosterVisualDirectory,
            RoomObjectSpeedBoosterVisualFiles.VisualFileName);
        File.WriteAllText(stockPath, "{}");
        AssertThrows<InvalidDataException>(
            () => RoomObjectSpeedBoosterVisualFilesConstants.ValidateStock(
                installation.RoomObjectSpeedBoosterVisualDirectory),
            "stock tampering fails validation");
        Console.WriteLine(
            "Speed Booster visuals: native reveal parity and strict failures pass.");
    }

    /// <summary>Runs the focused Speed Booster reveal artwork check against the retail ROM.</summary>
    [VerificationCommand("--speed-booster-visuals")]
    internal static int SpeedBoosterVisualsCommand()
    {
        Suite(nameof(VerifySpeedBoosterVisuals), () => VerifySpeedBoosterVisuals());
        return 0;
    }
}
