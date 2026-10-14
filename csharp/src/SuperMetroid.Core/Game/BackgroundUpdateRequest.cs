namespace SuperMetroid.Core.Game;

/// <summary>
/// The four coordinate words passed through WRAM <c>$16-$1C</c> immediately before one of
/// the update-level/background-data row/column calls at <c>$80:A413-$80:A4B5</c>.
/// </summary>
/// <param name="Layer">The tilemap producer that must satisfy the request.</param>
/// <param name="Axis">Whether a column or a row is transferred.</param>
/// <param name="SourceBlock">The first source level/background-data block of the strip.</param>
/// <param name="VramBlock">The circular-tilemap block the strip begins at.</param>
public readonly record struct BackgroundUpdateRequest(
    BackgroundLayer Layer,
    BackgroundUpdateAxis Axis,
    Position16<RoomBlocks> SourceBlock,
    Position16<TilemapBlocks> VramBlock);
