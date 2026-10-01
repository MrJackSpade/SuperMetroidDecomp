using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Frontend;

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
        ("EnemyAuxiliaryColorCatalog", "Resolve", "palette") => EnemyAuxiliaryColorDefinitions.All.ToArray()
            .Select(definition => (int)definition.Id).ToArray(),
        ("KraidColorCatalog", "Resolve", "source") => Enum.GetValues<KraidPaletteSource>().Select(id => (int)id).ToArray(),
        ("DachoraColorCatalog", "Resolve", "phase") => Enum.GetValues<DachoraPalettePhase>().Select(id => (int)id).ToArray(),
        ("ShitroidColorCatalog", "TargetColor", "target") => Enum.GetValues<ShitroidColorTarget>().Select(id => (int)id).ToArray(),
        ("EscapeTypewriterPresentation", "Get", "id") => Enum.GetValues<EscapeTypewriterProgramId>()
            .Where(id => id != EscapeTypewriterProgramId.None).Select(id => (int)id).ToArray(),
        ("IntroNarrationPresentation", "GetLines" or "Compile", "page") =>
            Enum.GetValues<IntroNarrationPageId>().Select(id => (int)id).ToArray(),
        ("EndingTextPresentation", "Compile", "sequence") => Enum.GetValues<EndingTextSequence>().Select(id => (int)id).ToArray(),
        ("MapSpriteCatalog", "Draw", "id") => MapSpriteDefinitions.Frames.ToArray().Select(frame => (int)frame.NativeId).ToArray(),
        ("PauseReserveTankPresentation", "Draw", "nativeIdentity") => PauseReserveTankDefinitions.Frames().Select(frame => (int)frame.Id).ToArray(),
        ("TitleGraphicsPresentation", "DrawSprite", "pointer") => TitleSpriteDefinitions.NativePointers.ToArray().Select(pointer => (int)pointer).ToArray(),
        ("IntroCaretSpritePresentation", "Draw", "pointer") => IntroCaretSpriteDefinitions.Frames.ToArray().Select(frame => (int)frame.Pointer).ToArray(),
        ("IntroMotherBrainSpritePresentation", "Draw", "pointer") => IntroMotherBrainSpriteDefinitions.Frames.ToArray().Select(frame => (int)frame.Pointer).ToArray(),
        ("IntroMotherBrainExplosionSpritePresentation", "Draw", "pointer") => IntroMotherBrainExplosionSpriteDefinitions.Frames.ToArray().Select(frame => (int)frame.Pointer).ToArray(),
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
