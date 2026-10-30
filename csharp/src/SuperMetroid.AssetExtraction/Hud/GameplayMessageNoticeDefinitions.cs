using SuperMetroid.Core.Rooms;

namespace SuperMetroid.AssetExtraction.Hud;

/// <summary>Schema and stock text-region geometry for completion and save notices.</summary>
public static class GameplayMessageNoticeDefinitions
{
    /// <summary>Supported revision of the editable station-completion and save-confirmation notice JSON schema.</summary>
    public const int Version = 2;
    /// <summary>Asset filename for the five editable gameplay notice templates and bounded text regions.</summary>
    public const string FileName = "gameplay-message-notices.json";

    /// <summary>The five notices whose text regions may be edited in the asset schema.</summary>
    internal static readonly GameplayMessageId[] SupportedMessageIds =
    [
        GameplayMessageId.MapDataAccessCompleted,
        GameplayMessageId.EnergyRechargeCompleted,
        GameplayMessageId.MissileRechargeCompleted,
        GameplayMessageId.SaveConfirmation,
        GameplayMessageId.GunshipSaveConfirmation,
    ];

    /// <summary>The five supported notice identities in schema order: map download, energy recharge, missile recharge, station save confirmation, and gunship save confirmation; excludes pickup messages and the save-completed notice.</summary>
    public static ReadOnlySpan<GameplayMessageId> MessageIds => SupportedMessageIds;
}
