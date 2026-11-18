using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>One row of the mural behind Mother Brain's tank (#2211, #2258), which her fake death's ascent reveals.</summary>
/// <param name="Place">The row's place in the mural, counting from its top border row.</param>
public readonly record struct MotherBrainMuralRow(int Place)
{
    /// <summary>The mural's rows, top row first.</summary>
    internal static IReadOnlyList<MotherBrainMuralRow> All { get; } =
        [.. Enumerable.Range(0, MotherBrainMuralDefinitions.RowCount).Select(place => new MotherBrainMuralRow(place))];

    /// <summary>The mural's top row, which every reveal starts from.</summary>
    internal static ListStep<MotherBrainMuralRow> First => ListStep<MotherBrainMuralRow>.FirstOf(All);

    /// <summary>The reveal of each row, top row first: the row's draw, drawn once.</summary>
    private static readonly TerrainChange[] Reveals =
        [.. Enumerable.Range(0, MotherBrainMuralDefinitions.RowCount).Select(place => TerrainChange.DrawnOnce(MotherBrainFakeDeathDraws.MuralRow(place)))];

    /// <summary>The room block the row lies at: the mural's column, each row one block below the row above it.</summary>
    public Position16<RoomBlocks> Block => new(MotherBrainMuralDefinitions.Column, MotherBrainMuralDefinitions.TopBlockRow + Place);

    /// <summary>The terrain change revealing the row: its draw, drawn once.</summary>
    internal TerrainChange Reveal => Reveals[Place];
}
