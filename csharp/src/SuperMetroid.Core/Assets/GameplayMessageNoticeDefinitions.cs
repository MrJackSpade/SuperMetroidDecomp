using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Assets;

/// <summary>Schema and stock text-region geometry for completion and save notices.</summary>
public static class GameplayMessageNoticeDefinitions
{
    public const int Version = 1;
    public const string FileName = "gameplay-message-notices.json";
    public const string LeftAlignment = "Left";
    public const string CenterAlignment = "Center";

    private static readonly GameplayMessageId[] SupportedMessageIds =
    [
        GameplayMessageId.MapDataAccessCompleted,
        GameplayMessageId.EnergyRechargeCompleted,
        GameplayMessageId.MissileRechargeCompleted,
        GameplayMessageId.SaveConfirmation,
        GameplayMessageId.GunshipSaveConfirmation,
    ];

    public static ReadOnlySpan<GameplayMessageId> MessageIds => SupportedMessageIds;

    public static bool IsSaveConfirmation(GameplayMessageId messageId) =>
        messageId is GameplayMessageId.SaveConfirmation or
            GameplayMessageId.GunshipSaveConfirmation;

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
                        if ((word & 0x03ff) == GameplayMessageTitleDefinitions.SpaceCharacter &&
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

        static bool IsText(ushort word) => (word & 0x03ff) is
            >= GameplayMessageTitleDefinitions.UppercaseACharacter and <= GameplayMessageTitleDefinitions.UppercaseZCharacter or
            GameplayMessageTitleDefinitions.HyphenCharacter or GameplayMessageTitleDefinitions.PeriodCharacter or
            GameplayMessageTitleDefinitions.QuestionMarkCharacter;
    }
}

/// <summary>One stock text rectangle inside a gameplay notice's content rows.</summary>
public readonly record struct GameplayMessageTextRegionDefinition(
    int Row,
    int Column,
    int Width,
    string Alignment);
