using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.CartridgeTables;
namespace SuperMetroid.Verification.Samus;
/// <summary>Checks Samus's horizontal speeds by medium and movement against the cartridge records they come from.</summary>
internal static class SamusIndexedSpeedsVerification
{
    /// <summary>
    /// Compares every medium's and movement's speeds with the cartridge record the cartridge's selection reads for them, and
    /// confirms that Draygon's hold and special movement in air take water's standing and running speeds (#1734).
    /// </summary>
    internal static void VerifySamusIndexedSpeeds(CartridgeImportAddressSpace rom)
    {
        SamusHorizontalSpeeds Read(int a)
        {
            SignedSixteenSixteen Pair(int offset) => SignedSixteenSixteen.FromParts(
                unchecked((short)CartridgeImportSource.Require(rom).ReadCartridgeWord(a + offset)),
                CartridgeImportSource.Require(rom).ReadCartridgeWord(a + offset + 2));
            return new(Pair(0), Pair(4), Pair(8));
        }
        var speed = new SamusHorizontalSpeedState(SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus().WaterSpeedsInAir);
        int compared = 0;
        int readFromWater = 0;
        int waterList = SamusSourceDefinitions.MovementBank.Value | SamusMovementDefinitions.HorizontalMotion.WaterSpeedTable;
        foreach (SamusLiquidMedium medium in Enum.GetValues<SamusLiquidMedium>())
        {
            speed.SelectSpeedsFor(medium);
            foreach (SamusMovementType movement in Enum.GetValues<SamusMovementType>())
            {
                int address = speed.ResolveEntryAddress(movement);
                AssertEqual(Read(address), speed.Speeds(movement), $"{medium} {movement} speeds match the cartridge record its selection reads");
                if (medium == SamusLiquidMedium.Air && address >= waterList)
                    readFromWater++;
                compared++;
            }
        }
        AssertEqual(3 * Enum.GetValues<SamusMovementType>().Length, compared, "every medium's movements are compared");
        AssertEqual(2, readFromWater, "only Draygon's hold and special movement read past the air speeds");

        speed.SelectSpeedsFor(SamusLiquidMedium.Water);
        SamusHorizontalSpeeds waterStanding = speed.Speeds(SamusMovementType.Standing);
        SamusHorizontalSpeeds waterRunning = speed.Speeds(SamusMovementType.Running);
        speed.SelectAirSpeeds();
        AssertEqual(waterStanding, speed.Speeds(SamusMovementType.DraygonHeld), "Draygon's hold in air takes water's standing speeds");
        AssertEqual(waterRunning, speed.Speeds(SamusMovementType.Special), "special movement in air takes water's running speeds");
        AssertTrue(speed.Speeds(SamusMovementType.Running) != waterRunning, "running in air keeps its own speeds");
        Console.WriteLine($"Indexed Samus speeds: {compared} medium and movement speeds match the cartridge records they are read from; Draygon's hold and special movement in air take water's standing and running speeds.");
    }
}
