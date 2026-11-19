namespace SuperMetroid.Core.Escape.Artwork;

/// <summary>
/// The installed key of each <see cref="EscapeTypewriterMessageId"/> (#2520), written out so renaming a member never
/// changes a name the installation, an installed document or a save spells it by.
/// </summary>
internal static class EscapeTypewriterMessageIdKeys
{
    /// <summary>Each EscapeTypewriterMessageId member's installed key.</summary>
    internal static InstalledKeys<EscapeTypewriterMessageId> Installed { get; } = new(new Dictionary<EscapeTypewriterMessageId, string>
    {
        [EscapeTypewriterMessageId.Ceres] = "Ceres",
        [EscapeTypewriterMessageId.Zebes] = "Zebes",
    });
}
