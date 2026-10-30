using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.AssetExtraction.Hud;

/// <summary>How each editable completion and save notice is shaped: its content height and the text regions of its stock picture.</summary>
public static class GameplayMessageNotices
{
    /// <summary>Determines whether a message is one of the two editable save prompts requiring independent YES/NO selection rows.</summary>
    /// <param name="messageId">Gameplay message identity to classify; other messages return false without being validated as editable notices.</param>
    /// <returns>True for station or gunship save confirmation; false otherwise.</returns>
    public static bool IsSaveConfirmation(GameplayMessageId messageId) =>
        messageId is GameplayMessageId.SaveConfirmation or
            GameplayMessageId.GunshipSaveConfirmation;

    /// <summary>Gets the fixed native content height of an editable notice, excluding the top and bottom border rows; rejects unsupported message identities.</summary>
    /// <param name="messageId">One of the five identities in <see cref="GameplayMessageNoticeDefinitions.MessageIds"/>.</param>
    /// <returns>Four 32-cell rows for a save confirmation or three for a station-completion notice.</returns>
    public static int ContentRows(GameplayMessageId messageId) =>
        IsSaveConfirmation(messageId) ? 4 : messageId is
            GameplayMessageId.MapDataAccessCompleted or
            GameplayMessageId.EnergyRechargeCompleted or
            GameplayMessageId.MissileRechargeCompleted
                ? 3
                : throw new ArgumentOutOfRangeException(nameof(messageId), messageId,
                    "Message is not an editable completion/save notice.");

    /// <summary>
    /// Derives stock notice rectangles from the imported tilemap's visible glyph runs.
    /// Single interword spaces belong to a phrase; wider gaps and nontext glyphs separate
    /// regions, including the independent YES/NO choices. Source text remains artwork.
    /// </summary>
    /// <param name="messageId">Supported notice identity determining the required three- or four-row content height.</param>
    /// <param name="template">Complete row-major 32-cell-wide content tilemap, excluding border rows.</param>
    /// <returns>Left-aligned text regions in row/column order, measured in tile cells rather than screen pixels.</returns>
    public static IEnumerable<GameplayMessageTextRegionDefinition> StockTextRegions(
        GameplayMessageId messageId, IReadOnlyList<BackgroundTile> template)
    {
        int rows = ContentRows(messageId);
        Ensure.NotNull(template);
        if (template.Count != rows * GameplayMessageBoxDefinitions.Layout.TilemapWidth)
            throw new InvalidDataException("Notice region derivation requires its complete native tilemap rows.");
        return Enumerate();

        IEnumerable<GameplayMessageTextRegionDefinition> Enumerate()
        {
            int width = GameplayMessageBoxDefinitions.Layout.TilemapWidth;
            for (int row = 0; row < rows; row++)
            {
                int column = 0;
                while (column < width)
                {
                    if (!IsText(template[row * width + column])) { column++; continue; }
                    int start = column++;
                    while (column < width)
                    {
                        BackgroundTile cell = template[row * width + column];
                        if (IsText(cell)) { column++; continue; }
                        if (cell.Tile == GameplayMessageTitleDefinitions.PunctuationTiles[' '] &&
                            column + 1 < width && IsText(template[row * width + column + 1]))
                        {
                            column += 2;
                            continue;
                        }
                        break;
                    }
                    yield return new(row, start, column - start, GameplayMessageTextAlignment.Left);
                }
            }
        }

        // Text is a letter or any punctuation but the space, which separates words.
        static bool IsText(BackgroundTile cell) =>
            GameplayMessageGlyphs.IsLetterTile(cell.Tile) ||
            GameplayMessageTitleDefinitions.PunctuationTiles.Any(glyph => glyph.Key != ' ' && glyph.Value == cell.Tile);
    }
}
