namespace SuperMetroid.Core.Frontend;

/// <summary>
/// One colour of a frontend screen, named by its owner (#1464): colour <paramref name="Colour"/> of set
/// <paramref name="Set"/> of <paramref name="Owner"/>. Installed screen palettes key the relationships between their
/// colours and the colours a player edited by cell.
/// </summary>
/// <param name="Owner">The owner that holds the colour.</param>
/// <param name="Set">The owner's set, zero through seven.</param>
/// <param name="Colour">The colour within the set, zero through fifteen.</param>
public readonly record struct ScreenColourCell(ScreenColourOwner Owner, int Set, int Colour);
