using System.Collections.Immutable;

namespace SuperMetroid.Core.Rooms.Artwork;

/// <summary>
/// One 16x16 cell of a room's background image whose picture changes with appearance inputs (#1392, #1466): while every
/// input shows the room's load the image shows the cell, otherwise the block sheet's picture of the cell for their values.
/// </summary>
/// <param name="Cell">The cell, sixteen to a pair of the image's tile rows.</param>
/// <param name="Picture">The name the block sheet's pictures of the cell begin with.</param>
/// <param name="Inputs">The inputs the cell's picture depends on.</param>
public sealed record RoomBackgroundCell(int Cell, string Picture, ImmutableArray<AppearanceInput> Inputs);
