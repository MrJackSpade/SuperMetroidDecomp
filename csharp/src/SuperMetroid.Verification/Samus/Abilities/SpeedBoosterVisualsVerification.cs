using SuperMetroid.AssetExtraction;
using static SuperMetroid.Verification.Bosses.MotherBrain.BabyMetroidRouteDefinitionsVerification;
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
    /// <summary>Checks native visual identity, exact pointer membership, and authored shape.</summary>
    private static void VerifySpeedBoosterStockVisualMapping(SuperMetroidAddressSpace rom)
    {
        const ushort pointer = 0xa4f3;
        RoomBlockAppearance native = RoomLevelWordCodec.DecodeAppearance(ReadRomWord(rom,0x84a4f5));
        var stock = InstalledRoomObjectSpeedBoosterVisuals.Stock();
        var imported = RoomObjectSpeedBoosterVisualFiles.Create([new("bomb-reveal",[native])]);
        string Hash(RoomBlockAppearance appearance) => SuperMetroid.AssetExtraction.SelectedPresentationHash.Create(
            nameof(InstalledRoomObjectSpeedBoosterVisuals), content => content.AppendAppearances("block",[appearance]));
        AssertEqual(native,stock.GetAppearance(NativeObjectDraws.At<SpeedBoosterBlockObjectDrawDefinitions.Draw>(pointer),0,0),"speed stock visual matches original native draw");
        AssertEqual(native,imported.GetAppearance(NativeObjectDraws.At<SpeedBoosterBlockObjectDrawDefinitions.Draw>(pointer),0,0),"speed imported visual matches original native draw");
        AssertEqual(Hash(native),stock.ContentIdentity,"speed stock scalar identity");
        AssertEqual(Hash(native),imported.ContentIdentity,"speed imported scalar identity");
        for (int address = 0; address <= ushort.MaxValue; address++)
            if (address != pointer)
                AssertThrows<InvalidDataException>(() => stock.GetAppearance(NativeObjectDraws.At<SpeedBoosterBlockObjectDrawDefinitions.Draw>((ushort)address),0,0),"speed visual full pointer rejection");
        foreach (int invalid in new[] {int.MinValue,-1,1,2,int.MaxValue})
        {
            AssertThrows<InvalidDataException>(() => stock.GetAppearance(NativeObjectDraws.At<SpeedBoosterBlockObjectDrawDefinitions.Draw>(pointer),invalid,0),"speed visual run bounds");
            AssertThrows<InvalidDataException>(() => stock.GetAppearance(NativeObjectDraws.At<SpeedBoosterBlockObjectDrawDefinitions.Draw>(pointer),0,invalid),"speed visual block bounds");
        }
        AssertThrows<InvalidDataException>(() => RoomObjectSpeedBoosterVisualFiles.Create([]),"speed visual requires entry");
        AssertThrows<InvalidDataException>(() => RoomObjectSpeedBoosterVisualFiles.Create([new("bomb-reveal",[native]),new("bomb-reveal",[native])]),"speed visual rejects duplicate entry");
        AssertThrows<InvalidDataException>(() => RoomObjectSpeedBoosterVisualFiles.Create([new("Bomb-reveal",[native])]),"speed visual ID is ordinal");
        AssertThrows<InvalidDataException>(() => RoomObjectSpeedBoosterVisualFiles.Create([new("bomb-reveal",[native,native])]),"speed visual rejects altered shape");
    }

    /// <summary>Checks the extracted stock visuals and that a malformed installed visual file fails loudly where it is read.</summary>
    internal static void VerifySpeedBoosterVisuals()
    {
        SuperMetroidAddressSpace rom = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(
            Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(VerifySpeedBoosterStockVisualMapping), () => VerifySpeedBoosterStockVisualMapping(rom));
        using PrivateInstallationCopy copy = InstalledFixtures.CreatePrivateCopyOf(layout => layout.RoomObjectSpeedBoosterVisualDirectory);
        GameInstallation installation = copy.Installation;
        RoomObjectSpeedBoosterVisualFilesConstants.ValidateStock(
            installation.RoomObjectSpeedBoosterVisualDirectory);
        RoomObjectDraw drawPointer = SpeedBoosterBlockObjectDrawDefinitions.BombRevealDraw;
        AssertEqual(RoomBlockAppearance.Of(0x0b6),
            installation.LoadRoomObjectSpeedBoosterVisuals()
                .GetAppearance(drawPointer, 0, 0),
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
