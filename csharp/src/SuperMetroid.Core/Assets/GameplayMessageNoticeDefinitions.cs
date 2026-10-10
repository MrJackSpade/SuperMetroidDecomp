using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Assets;

/// <summary>Schema and stock text-region geometry for completion and save notices.</summary>
public static class GameplayMessageNoticeDefinitions
{
    /// <summary>Supported revision of the editable station-completion and save-confirmation notice JSON schema.</summary>
    public const int Version = 1;
    /// <summary>Asset filename for the five editable gameplay notice templates and bounded text regions.</summary>
    public const string FileName = "gameplay-message-notices.json";
    /// <summary>Case-sensitive alignment token placing text at the first tile cell of its allocated region.</summary>
    public const string LeftAlignment = "Left";
    /// <summary>Case-sensitive alignment token centering text within its allocated tile-cell width using integer division of the remaining space.</summary>
    public const string CenterAlignment = "Center";

    private static readonly GameplayMessageId[] SupportedMessageIds =
    [
        GameplayMessageId.MapDataAccessCompleted,
        GameplayMessageId.EnergyRechargeCompleted,
        GameplayMessageId.MissileRechargeCompleted,
        GameplayMessageId.SaveConfirmation,
        GameplayMessageId.GunshipSaveConfirmation,
    ];

    /// <summary>The five supported notice identities in schema order: map download, energy recharge, missile recharge, station save confirmation, and gunship save confirmation; excludes pickup messages and the save-completed notice.</summary>
    public static ReadOnlySpan<GameplayMessageId> MessageIds => SupportedMessageIds;

    /// <summary>Determines whether a message is one of the two editable save prompts requiring independent YES/NO selection rows.</summary>
    /// <param name="messageId">Gameplay message identity to classify; other messages return false without being validated as editable notices.</param>
    /// <returns>True for station or gunship save confirmation; false otherwise.</returns>
    public static bool IsSaveConfirmation(GameplayMessageId messageId) =>
        messageId is GameplayMessageId.SaveConfirmation or
            GameplayMessageId.GunshipSaveConfirmation;

    /// <summary>Gets the fixed native content height of an editable notice, excluding the top and bottom border rows; rejects unsupported message identities.</summary>
    /// <param name="messageId">One of the five identities in <see cref="MessageIds"/>.</param>
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
        GameplayMessageId messageId, IReadOnlyList<GameplayMessageTitleCell> template)
    {
        int rows = ContentRows(messageId);
        Ensure.NotNull(template);
        if (template.Count != rows * GameplayMessageRomData.Layout.TilemapWidth)
            throw new InvalidDataException("Notice region derivation requires its complete native tilemap rows.");
        return Enumerate();

        IEnumerable<GameplayMessageTextRegionDefinition> Enumerate()
        {
            int width = GameplayMessageRomData.Layout.TilemapWidth;
            for (int row = 0; row < rows; row++)
            {
                int column = 0;
                while (column < width)
                {
                    if (!IsText(template[row * width + column].Raw)) { column++; continue; }
                    int start = column++;
                    while (column < width)
                    {
                        ushort word = template[row * width + column].Raw;
                        if (IsText(word)) { column++; continue; }
                        if ((GameplayMessageTitleGlyph)(word & 0x03ff) == GameplayMessageTitleGlyph.Space &&
                            column + 1 < width && IsText(template[row * width + column + 1].Raw))
                        {
                            column += 2;
                            continue;
                        }
                        break;
                    }
                    yield return new(row, start, column - start, LeftAlignment);
                }
            }
        }

        static bool IsText(ushort word) => (GameplayMessageTitleGlyph)(word & 0x03ff) is
            >= GameplayMessageTitleGlyph.A and <= GameplayMessageTitleGlyph.Z or
            GameplayMessageTitleGlyph.Hyphen or GameplayMessageTitleGlyph.Period or
            GameplayMessageTitleGlyph.QuestionMark;
    }
}

/// <summary>One stock text rectangle inside a gameplay notice's content rows.</summary>
/// <param name="Row">Zero-based content tile row, excluding the upper border.</param>
/// <param name="Column">Zero-based starting tile column within the 32-cell content row.</param>
/// <param name="Width">Region width in tile cells, including single interword spaces retained within the glyph run.</param>
/// <param name="Alignment">Text alignment token; stock region derivation supplies <see cref="GameplayMessageNoticeDefinitions.LeftAlignment"/>.</param>
public readonly record struct GameplayMessageTextRegionDefinition(
    int Row,
    int Column,
    int Width,
    string Alignment);
