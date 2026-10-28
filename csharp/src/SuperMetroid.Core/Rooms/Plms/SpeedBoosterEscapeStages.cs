using static SuperMetroid.Core.Rooms.Plms.SpeedBoosterEscapeStageDefinitions;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>Which stage of the Speed Booster escape a PLM resolves to.</summary>
internal static class SpeedBoosterEscapeStages
{
    /// <summary>
    /// Selects the three position-triggered actions at $84:B846, in east-to-west order.
    /// Each case supplies an unsigned X threshold, unsigned maximum lava Y and packed
    /// signed 8.8 rise velocity for the supported NTSC revision. These are checkpoint
    /// actions, not samples to interpolate: the caller executes at most one per frame
    /// and advances its timer by six. Offset eighteen completes the event; every other
    /// ushort offset is invalid. No stage data or generated cache is stored.
    /// </summary>
    internal static SpeedBoosterEscapeStageDefinition? Resolve(int tableByteOffset)
    {
        if ((uint)tableByteOffset > TerminatorOffset || tableByteOffset % RecordByteCount != 0)
        {
            throw new InvalidDataException(
                $"Speed Booster escape PLM timer ${tableByteOffset:X4} is not a valid " +
                "offset into table $84:B876.");
        }

        return Stages[tableByteOffset / RecordByteCount];
    }
}
