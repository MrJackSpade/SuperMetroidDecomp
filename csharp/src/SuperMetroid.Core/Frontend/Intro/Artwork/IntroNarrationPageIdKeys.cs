namespace SuperMetroid.Core.Frontend.Intro.Artwork;

/// <summary>
/// The installed key of each <see cref="IntroNarrationPageId"/> (#2520), written out so renaming a member never changes
/// a name the installation, an installed document or a save spells it by.
/// </summary>
internal static class IntroNarrationPageIdKeys
{
    /// <summary>Each IntroNarrationPageId member's installed key.</summary>
    internal static InstalledKeys<IntroNarrationPageId> Installed { get; } = new(new Dictionary<IntroNarrationPageId, string>
    {
        [IntroNarrationPageId.ZebesBattle] = "ZebesBattle",
        [IntroNarrationPageId.HatchedLarva] = "HatchedLarva",
        [IntroNarrationPageId.DeliveryToCeres] = "DeliveryToCeres",
        [IntroNarrationPageId.ScientistsFindings] = "ScientistsFindings",
        [IntroNarrationPageId.DistressSignal] = "DistressSignal",
        [IntroNarrationPageId.CeresUnderAttack] = "CeresUnderAttack",
    });
}
