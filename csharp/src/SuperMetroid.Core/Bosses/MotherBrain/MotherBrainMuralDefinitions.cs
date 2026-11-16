namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// The mural behind Mother Brain's tank (#2211), which her fake death's ascent reveals two rows an update from the top.
/// </summary>
internal static class MotherBrainMuralDefinitions
{
    /// <summary>The mural's rows, top to bottom.</summary>
    internal static IReadOnlyList<MotherBrainRoomMutation> Rows { get; } =
    [
        MotherBrainRoomMutation.MuralRow1, MotherBrainRoomMutation.MuralRow2, MotherBrainRoomMutation.MuralRow3,
        MotherBrainRoomMutation.MuralRow4, MotherBrainRoomMutation.MuralRow5, MotherBrainRoomMutation.MuralRow6,
        MotherBrainRoomMutation.MuralRow7, MotherBrainRoomMutation.MuralRow8, MotherBrainRoomMutation.MuralRow9,
        MotherBrainRoomMutation.MuralRow10, MotherBrainRoomMutation.MuralRow11, MotherBrainRoomMutation.MuralRow12,
    ];

    /// <summary>The rows the reveal draws each update.</summary>
    internal const int RowsEachUpdate = 2;

    /// <summary>The block column every row is drawn from.</summary>
    internal const int Column = 2;

    /// <summary>The block row the mural's top row is drawn at; each later row lies one below.</summary>
    internal const int TopBlockRow = 2;
}
