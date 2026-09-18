namespace SuperMetroid.Core.Rooms;

/// <summary>
/// One immutable row from the downward-gate shot-block setup tables in bank $84.
/// </summary>
internal readonly record struct DownwardGateShotBlockDefinition(
    ushort InstructionList,
    ushort LeftBlockWord,
    ushort RightBlockWord);

/// <summary>
/// Compiled mechanics definitions consumed by the downward-gate shot-block setup routine.
/// </summary>
internal static class DownwardGateShotBlockDefinitions
{
    /// <summary>
    /// Native table $84:C70A-$C719, which selects the eight gate-trigger instruction lists.
    /// </summary>
    internal const int InstructionListTableAddress = 0x84c70a;

    /// <summary>
    /// Native table $84:C71A-$C729, which installs the left-side trigger block when nonzero.
    /// </summary>
    internal const int LeftBlockWordTableAddress = 0x84c71a;

    /// <summary>
    /// Native table $84:C72A-$C739, which installs the right-side trigger block when nonzero.
    /// </summary>
    internal const int RightBlockWordTableAddress = 0x84c72a;

    private const ushort LastRoomArgument = 14;

    private static readonly DownwardGateShotBlockDefinition[] Entries =
    [
        new(RoomPlmInstructionLists.DownwardGateShotBlockBlueLeft, 0xc046, 0),
        new(RoomPlmInstructionLists.DownwardGateShotBlockBlueRight, 0, 0xc047),
        new(RoomPlmInstructionLists.DownwardGateShotBlockRedLeft, 0xc048, 0),
        new(RoomPlmInstructionLists.DownwardGateShotBlockRedRight, 0, 0xc049),
        new(RoomPlmInstructionLists.DownwardGateShotBlockGreenLeft, 0xc04a, 0),
        new(RoomPlmInstructionLists.DownwardGateShotBlockGreenRight, 0, 0xc04b),
        new(RoomPlmInstructionLists.DownwardGateShotBlockYellowLeft, 0xc04c, 0),
        new(RoomPlmInstructionLists.DownwardGateShotBlockYellowRight, 0, 0xc04d),
    ];

    /// <summary>Resolves the native even byte offset stored in the room population record.</summary>
    internal static DownwardGateShotBlockDefinition Resolve(ushort roomArgument)
    {
        if ((roomArgument & 1) != 0 || roomArgument > LastRoomArgument)
        {
            throw new InvalidDataException(
                $"Downward gate shot-block argument ${roomArgument:X4} is not an even table offset from $84:C70A.");
        }

        return Entries[roomArgument / sizeof(ushort)];
    }
}
