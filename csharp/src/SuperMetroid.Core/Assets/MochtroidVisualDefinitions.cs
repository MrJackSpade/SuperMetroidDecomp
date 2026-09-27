namespace SuperMetroid.Core.Assets;

/// <summary>
/// The six OAM identities selected by Mochtroid's free-flight and attached
/// programs. Their native durations and movement/attachment AI stay compiled.
/// </summary>
internal static class MochtroidVisualDefinitions
{
    /// <summary>Mochtroid's instruction and spritemap bank, $A3.</summary>
    internal const byte Bank = 0xa3;
    internal const int FrameCount = 6;

    private static readonly (ushort Operand, ushort Frame)[] Selectors =
    [
        (0xa747, 0xa9b0), (0xa74b, 0xa9d0),
        (0xa74f, 0xa9f0), (0xa753, 0xa9d0),
        (0xa75b, 0xaa06), (0xa75f, 0xaa1c),
        (0xa763, 0xaa32), (0xa767, 0xaa1c),
    ];

    internal static EnemySpritemapDefinition[] Frames() =>
    [
        new(Bank, 0xa9b0, "mochtroid_flight_0"),
        new(Bank, 0xa9d0, "mochtroid_flight_1"),
        new(Bank, 0xa9f0, "mochtroid_flight_2"),
        new(Bank, 0xaa06, "mochtroid_attached_0"),
        new(Bank, 0xaa1c, "mochtroid_attached_1"),
        new(Bank, 0xaa32, "mochtroid_attached_2"),
    ];

    internal static ushort FrameAt(ushort operandAddress)
    {
        foreach ((ushort operand, ushort frame) in Selectors)
        {
            if (operand == operandAddress)
                return frame;
        }
        throw new InvalidDataException(
            $"Mochtroid visual selector $A3:{operandAddress:X4} is not compiled.");
    }
}
