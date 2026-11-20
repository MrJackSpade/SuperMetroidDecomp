using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// Mother Brain's fake-death room block draws (#2034): the wall fill, the escape door and the six tube clears, and the one
/// draw of a row of the mural behind her tank (#2258), the block shape each draws, from
/// <see cref="MotherBrainFakeDeathObjectDrawDefinitions"/>, and the installed picture (#1557) each shows.
/// </summary>
internal static class MotherBrainFakeDeathDraws
{
    /// <summary>The fake-death room's frames other than the mural's rows, pictured by the installed fake-death room frames.</summary>
    internal static RoomObjectDrawFamily<MotherBrainFakeDeathDraw> Family { get; } =
        new RoomObjectDrawFamily<MotherBrainFakeDeathDraw>("Mother Brain fake-death room",
            frame => RoomObjectDrawFamily<MotherBrainFakeDeathDraw>.Defined(MotherBrainFakeDeathObjectDrawDefinitions.Shapes, frame));

    /// <summary>
    /// The rows of the mural behind her tank, pictured by the installed fake-death room frames: its top and bottom rows are
    /// its slope border, every row between is open.
    /// </summary>
    internal static RoomObjectDrawFamily<MuralRowDraw> MuralRowFamily { get; } =
        new RoomObjectDrawFamily<MuralRowDraw>("Mother Brain mural",
            frame => frame.Row is 0 or MotherBrainMuralDefinitions.RowCount - 1
                ? MotherBrainFakeDeathObjectDrawDefinitions.MuralBorderRow
                : MotherBrainFakeDeathObjectDrawDefinitions.MuralInteriorRow);

    /// <summary>The draw of the mural's row <paramref name="row"/>, counted from its top row.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The mural has no such row.</exception>
    internal static RoomObjectFrameDraw<MuralRowDraw> MuralRow(int row) =>
        MuralRowFamily.Of(new MuralRowDraw(Ensure.BetweenInclusive(0, MotherBrainMuralDefinitions.RowCount - 1, row)));

    /// <summary>One row of the mural behind her tank.</summary>
    /// <param name="Row">The row, counted from the mural's top row.</param>
    internal readonly record struct MuralRowDraw(int Row);
}
