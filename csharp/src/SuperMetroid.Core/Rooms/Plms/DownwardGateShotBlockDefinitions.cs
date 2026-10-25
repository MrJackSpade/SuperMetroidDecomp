using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>
/// Compiled mechanics definitions consumed by the downward-gate shot-block setup routine.
/// </summary>
internal static class DownwardGateShotBlockDefinitions
{

    /// <summary>The final even room argument: the eighth color and side.</summary>
    private const ushort LastRoomArgument = 14;

    /// <summary>
    /// The blue-left trigger block: a shootable block showing visual block $46. Consecutive color/side variants show
    /// visual blocks $46..$4D.
    /// </summary>
    private static RoomLevelWord BlueLeftBlockWord { get; } = RoomLevelWord.Create(0x46, LevelBlockFlipFlags.None, RoomCollisionType.ShootableBlock);

    /// <summary>
    /// For an even room argument 0..14, let i = argument/2 in color/side order (blue, red, green, yellow; left then
    /// right): i selects the trigger program, and the selected side receives the trigger block showing visual block
    /// $46+i while the opposite side receives nothing. Even i selects left and odd i selects right. Every other argument
    /// is rejected.
    /// </summary>
    internal static DownwardGateShotBlockDefinition Resolve(int roomArgument)
    {
        if ((roomArgument & 1) != 0 || roomArgument > LastRoomArgument)
        {
            throw new InvalidDataException(
                $"Downward gate shot-block argument ${roomArgument:X4} does not name one of the eight colors and sides.");
        }

        int index = roomArgument / sizeof(ushort);
        ushort blockWord = BlueLeftBlockWord.WithVisualBlockIndex((ushort)(BlueLeftBlockWord.VisualBlockIndex + index)).Raw;
        bool left = (index & 1) == 0;
        return new(
            DownwardGatePlmProgramDefinitions.TriggerPrograms[index],
            left ? blockWord : (ushort)0,
            left ? (ushort)0 : blockWord);
    }
}
