using static SuperMetroid.Core.Enemies.WorkRobots.WorkRobotPaletteTimingDefinitions;

namespace SuperMetroid.Core.Enemies.WorkRobots;

/// <summary>How long the Work Robot's palette cycle shows each record.</summary>
internal static class WorkRobotPaletteCycle
{
    /// <summary>Returns the duration for one aligned authored palette-record offset; offsets are validated before division.</summary>
    /// <exception cref="InvalidDataException"><paramref name="byteOffset"/> is not one of the six record offsets.</exception>
    public static ushort DurationForByteOffset(int byteOffset)
    {
        if (byteOffset % RecordByteCount != 0 ||
            byteOffset >= RecordCount * RecordByteCount)
        {
            throw new InvalidDataException(
                $"Work Robot palette timing offset {byteOffset} is not one of the " +
                $"{RecordCount} authored {RecordByteCount}-byte records.");
        }

        return Durations[byteOffset / RecordByteCount];
    }
}
