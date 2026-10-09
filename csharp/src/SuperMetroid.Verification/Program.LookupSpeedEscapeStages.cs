using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    /// <summary>Checks all three stage target-X words against their native bank-$84 records.</summary>
    /// <param name="rom">Address space containing the speed-booster escape program.</param>
    private static void VerifySpeedEscapeStageTargets(SuperMetroidAddressSpace rom) =>
        Suite(nameof(VerifySpeedEscapeStageField), () => VerifySpeedEscapeStageField(rom, 0));

    /// <summary>Checks all three maximum FX-height words against their native bank-$84 records.</summary>
    /// <param name="rom">Address space containing the speed-booster escape program.</param>
    private static void VerifySpeedEscapeStageHeights(SuperMetroidAddressSpace rom) =>
        Suite(nameof(VerifySpeedEscapeStageField), () => VerifySpeedEscapeStageField(rom, 1));

    /// <summary>Checks all three packed Y-velocity words against their native bank-$84 records.</summary>
    /// <param name="rom">Address space containing the speed-booster escape program.</param>
    private static void VerifySpeedEscapeStageVelocities(SuperMetroidAddressSpace rom) =>
        Suite(nameof(VerifySpeedEscapeStageField), () => VerifySpeedEscapeStageField(rom, 2));

    /// <summary>Compares one selected field from each supported speed-escape stage with the original records.</summary>
    /// <param name="rom">Address space used to read the native stage words.</param>
    /// <param name="field">Selects target X (0), maximum FX Y (1), or packed Y velocity (2).</param>
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
