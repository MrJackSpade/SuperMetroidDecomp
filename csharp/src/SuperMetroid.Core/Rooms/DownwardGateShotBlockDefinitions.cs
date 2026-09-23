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
    /// <remarks>
    /// Issue #1041: for an even room argument 0..14, row i = argument / 2
    /// selects the exact pointer $BCAF + 6*i. All eight little-endian words
    /// in the pinned NTSC J/U v1.0 ROM at $84:C70A..C719 match, from
    /// $BCAF through $BCD9. The six-byte stride follows consecutive native
    /// blue, red, green, and yellow left/right instruction lists, not a
    /// general pointer rule. Eleven retail gate PLMs use only in-range
    /// arguments 0, 2, 8, and 10; Resolve rejects odd or above-14 values.
    /// The independent gate verifier checks all eight rows against ROM and
    /// runs production setup without source-table reads. Keep the named
    /// list identities in the compiled records while documenting this
    /// exact bounded arithmetic relation.
    /// </remarks>
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
