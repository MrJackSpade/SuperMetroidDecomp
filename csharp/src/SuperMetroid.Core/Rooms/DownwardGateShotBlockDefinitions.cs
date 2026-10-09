namespace SuperMetroid.Core.Rooms;

/// <summary>
/// One calculated downward-gate shot-block setup action for bank $84.
/// </summary>
/// <param name="InstructionList">Native instruction-list pointer for the room-argument variant.</param>
/// <param name="LeftBlockWord">BTS word to write for the left trigger block, or zero when that side is untouched.</param>
/// <param name="RightBlockWord">BTS word to write for the right trigger block, or zero when that side is untouched.</param>
internal readonly record struct DownwardGateShotBlockDefinition(
    ushort InstructionList,
    ushort LeftBlockWord,
    ushort RightBlockWord);

/// <summary>
/// Compiled mechanics definitions consumed by the downward-gate shot-block setup routine.
/// </summary>
internal static class DownwardGateShotBlockDefinitions
{

    /// <summary>Largest even room argument accepted by the compiled shot-block table.</summary>
    private const ushort LastRoomArgument = 14;

    /// <summary>
    /// First shootable trigger block at $84:C71A: type C and blue-left BTS 46.
    /// Consecutive color/side variants use BTS 46..4D.
    /// </summary>
    private const ushort BlueLeftBlockWord = 0xc046;

    /// <summary>
    /// For an even room argument 0..14, let i = argument/2 in native color/side order
    /// (blue, red, green, yellow; left then right). The eight native six-byte lists
    /// are contiguous, so the pointer is BCAF + 6*i. The selected side receives
    /// C046+i; the opposite side receives zero and is not written. Even i selects
    /// left and odd i selects right. All three fields independently match the supported
    /// NTSC original tables; no cached records remain. Every other argument is rejected.
    /// </summary>
    internal static DownwardGateShotBlockDefinition Resolve(ushort roomArgument)
    {
        if ((roomArgument & 1) != 0 || roomArgument > LastRoomArgument)
        {
            throw new InvalidDataException(
                $"Downward gate shot-block argument ${roomArgument:X4} is not an even table offset from $84:C70A.");
        }

        int index = roomArgument / sizeof(ushort);
        ushort blockWord = (ushort)(BlueLeftBlockWord + index);
        bool left = (index & 1) == 0;
        return new(
            (ushort)(RoomPlmInstructionLists.DownwardGateShotBlockBlueLeft + 6 * index),
            left ? blockWord : (ushort)0,
            left ? (ushort)0 : blockWord);
    }
}
