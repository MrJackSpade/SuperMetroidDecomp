using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using static SamusEaterPlmDefinitionsVerification;
using static SuiteTimingVerification;

/// <summary>Represents lookup speed escape stages verification.</summary>
internal static class LookupSpeedEscapeStagesVerification
{
    /// <summary>verify speed escape stage targets.</summary>
    internal static void VerifySpeedEscapeStageTargets(SuperMetroidAddressSpace rom) =>
        Suite(nameof(VerifySpeedEscapeStageField), () => VerifySpeedEscapeStageField(rom, 0));

    /// <summary>verify speed escape stage heights.</summary>
    internal static void VerifySpeedEscapeStageHeights(SuperMetroidAddressSpace rom) =>
        Suite(nameof(VerifySpeedEscapeStageField), () => VerifySpeedEscapeStageField(rom, 1));

    /// <summary>verify speed escape stage velocities.</summary>
    internal static void VerifySpeedEscapeStageVelocities(SuperMetroidAddressSpace rom) =>
        Suite(nameof(VerifySpeedEscapeStageField), () => VerifySpeedEscapeStageField(rom, 2));

    /// <summary>verify speed escape stage field.</summary>
    private static void VerifySpeedEscapeStageField(SuperMetroidAddressSpace rom, int field)
    {
        // Original record locations, independent of the production stride and selectors.
        int[] records = [0x84b876, 0x84b87c, 0x84b882];
        for (int raw = 0; raw <= ushort.MaxValue; raw++)
        {
            ushort offset = (ushort)raw;
            int index = raw switch { 0 => 0, 6 => 1, 12 => 2, _ => -1 };
            if (raw == 18)
            {
                AssertEqual((ushort)0x8000, ReadSamusEaterPlmWord(rom, 0x84b888),
                    "Speed escape original completion sentinel");
                AssertTrue(SpeedBoosterEscapeStageDefinitions.Resolve(offset) is null,
                    "Speed escape terminal action");
            }
            else if (index < 0)
                AssertThrows<InvalidDataException>(() => SpeedBoosterEscapeStageDefinitions.Resolve(offset),
                    "Speed escape rejects every unsupported timer offset");
            else
            {
                SpeedBoosterEscapeStageDefinition stage = SpeedBoosterEscapeStageDefinitions.Resolve(offset)!.Value;
                ushort actual = field switch
                {
                    0 => stage.TargetSamusX,
                    1 => stage.MaximumFxY,
                    _ => stage.PackedYVelocity,
                };
                AssertEqual(ReadSamusEaterPlmWord(rom, records[index] + field * 2), actual,
                    $"Speed escape stage {index} original field {field}");
            }
        }
    }
}
