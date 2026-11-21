namespace SuperMetroid.Core.Progression;

/// <summary>
/// The installed key of each <see cref="TextLanguage"/> (#2560), written out so renaming a member never changes a name an
/// installed document spells it by, and the order the options' artwork walks the languages in.
/// </summary>
internal static partial class TextLanguageKeys
{
    /// <summary>Each TextLanguage member's installed key.</summary>
    internal static InstalledKeys<TextLanguage> Installed { get; } = new(new Dictionary<TextLanguage, string>
    {
        [TextLanguage.English] = "English",
        [TextLanguage.Japanese] = "Japanese",
    });

    /// <summary>The languages in the order the options' artwork lists them: English, then Japanese.</summary>
    internal static IReadOnlyList<TextLanguage> InListedOrder { get; } = Ensure.EveryMemberOnce<TextLanguage>(
    [
        TextLanguage.English,
        TextLanguage.Japanese,
    ]);
}
