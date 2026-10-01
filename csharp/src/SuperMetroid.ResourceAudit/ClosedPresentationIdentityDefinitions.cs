using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;

namespace SuperMetroid.ResourceAudit;

/// <summary>Sparse identities whose ownership cannot be expressed as a contiguous index range.</summary>
internal static class ClosedPresentationIdentityDefinitions
{
    internal static int[]? Get(string type, string method, string parameter) => (type, method, parameter) switch
    {
        ("GameplayMessageTitlePresentation", "Build", "messageId") =>
            GameplayMessageTitleDefinitions.MessageIds.ToArray().Select(id => (int)id).ToArray(),
        ("GameplayMessagePanelPresentation", "Build", "messageId") =>
            GameplayMessagePanelDefinitions.MessageIds.ToArray().Select(id => (int)id).ToArray(),
        ("GameplayMessageNoticePresentation", "Build", "messageId") =>
            GameplayMessageNoticeDefinitions.MessageIds.ToArray().Select(id => (int)id).ToArray(),
        ("GameplayMessageNoticePresentation", "ApplySelection", "messageId") =>
            [(int)GameplayMessageId.SaveConfirmation, (int)GameplayMessageId.GunshipSaveConfirmation],
        // Contains is deliberately absent: querying an unowned ID is valid and
        // returns false rather than looking up missing artwork.
        _ => null,
    };
}
