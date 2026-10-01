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
        ("SamusFullBodyCycleColorCatalog", "Apply" or "Resolve", "pointer") => FullBodyPointers(),
        ("SamusSuitColorCatalog", "Apply" or "Resolve", "suitTableOffset") => [0, 2, 4],
        // Contains is deliberately absent: querying an unowned ID is valid and
        // returns false rather than looking up missing artwork.
        _ => null,
    };

    private static int[] FullBodyPointers() =>
        new[] { SamusFullBodyCycleFamily.SpeedBooster, SamusFullBodyCycleFamily.ScrewAttack,
            SamusFullBodyCycleFamily.StoredShine, SamusFullBodyCycleFamily.ActiveShinespark }
        .SelectMany(family => Enumerable.Range(0, SamusFullBodyCycleColorFormat.SuitCount)
            .SelectMany(suit => Enumerable.Range(0, SamusFullBodyCycleColorFormat.ShadesPerSuit)
                .Select(shade => (int)SamusFullBodyCycleColorFormat.Pointer(family, suit, shade))))
        .Distinct().ToArray();
}
