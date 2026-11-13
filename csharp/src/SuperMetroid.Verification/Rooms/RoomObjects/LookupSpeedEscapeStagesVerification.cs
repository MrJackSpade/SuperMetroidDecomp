using SuperMetroid.AssetExtraction.Cartridge;
using static SuperMetroid.Verification.Rooms.RoomObjects.SamusEaterObjectDefinitionsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Verification.Rooms.RoomObjects;

/// <summary>Checks the Speed Booster escape's lava rises against the original stage records.</summary>
internal static class LookupSpeedEscapeStagesVerification
{
    /// <summary>Compares each rise's target Samus X coordinate with the cartridge.</summary>
    internal static void VerifySpeedEscapeStageTargets(CartridgeImportAddressSpace rom) =>
        Suite(nameof(VerifySpeedEscapeStageField), () => VerifySpeedEscapeStageField(rom, 0));

    /// <summary>Compares each rise's maximum FX Y height with the cartridge.</summary>
    internal static void VerifySpeedEscapeStageHeights(CartridgeImportAddressSpace rom) =>
        Suite(nameof(VerifySpeedEscapeStageField), () => VerifySpeedEscapeStageField(rom, 1));

    /// <summary>Compares each rise's packed Y velocity with the cartridge.</summary>
    internal static void VerifySpeedEscapeStageVelocities(CartridgeImportAddressSpace rom) =>
        Suite(nameof(VerifySpeedEscapeStageField), () => VerifySpeedEscapeStageField(rom, 2));

    /// <summary>
    /// Checks one field of every rise, in order, and that the original's record after the last rise is its completion
    /// mark, so the escape has exactly these rises.
    /// </summary>
    private static void VerifySpeedEscapeStageField(CartridgeImportAddressSpace rom, int field)
    {
        // Original record locations, independent of the production catalog.
        int[] records = [0x84b876, 0x84b87c, 0x84b882];
        AssertEqual(records.Length, SpeedBoosterEscapeStageDefinitions.Stages.Length, "Speed escape rise count");
        AssertEqual((ushort)0x8000, ReadSamusEaterObjectWord(rom, 0x84b888),
            "Speed escape original completion mark follows the last rise");
        for (int index = 0; index < records.Length; index++)
        {
            SpeedBoosterEscapeStageDefinition stage = SpeedBoosterEscapeStageDefinitions.Stages[index];
            int actual = field switch
            {
                0 => stage.TargetSamusX,
                1 => stage.MaximumFxY,
                _ => stage.YVelocity.RawValue,
            };
            AssertEqual(ReadSamusEaterObjectWord(rom, records[index] + field * 2), actual,
                $"Speed escape rise {index} original field {field}");
        }
    }
}
