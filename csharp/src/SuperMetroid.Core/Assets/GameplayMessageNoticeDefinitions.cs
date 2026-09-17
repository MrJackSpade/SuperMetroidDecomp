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

    private static readonly GameplayMessageTextRegionDefinition[] MapAndEnergyRegions =
    [
        new(0, 8, 15, LeftAlignment),
        new(2, 10, 10, LeftAlignment),
    ];

    private static readonly GameplayMessageTextRegionDefinition[] MissileRegions =
    [
        new(0, 8, 14, LeftAlignment),
        new(2, 10, 10, LeftAlignment),
    ];

    private static readonly GameplayMessageTextRegionDefinition[] SaveRegions =
    [
        new(0, 8, 14, LeftAlignment),
        new(1, 8, 8, LeftAlignment),
        new(3, 10, 3, LeftAlignment),
        new(3, 19, 2, LeftAlignment),
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

    public static ReadOnlySpan<GameplayMessageTextRegionDefinition> StockTextRegions(
        GameplayMessageId messageId) => messageId switch
        {
            GameplayMessageId.SaveConfirmation or
            GameplayMessageId.GunshipSaveConfirmation => SaveRegions,
            GameplayMessageId.MapDataAccessCompleted or
            GameplayMessageId.EnergyRechargeCompleted => MapAndEnergyRegions,
            GameplayMessageId.MissileRechargeCompleted => MissileRegions,
            _ => throw new ArgumentOutOfRangeException(nameof(messageId), messageId,
                "Message is not an editable completion/save notice."),
        };
}

/// <summary>One stock text rectangle inside a gameplay notice's content rows.</summary>
public readonly record struct GameplayMessageTextRegionDefinition(
    int Row,
    int Column,
    int Width,
    string Alignment);
