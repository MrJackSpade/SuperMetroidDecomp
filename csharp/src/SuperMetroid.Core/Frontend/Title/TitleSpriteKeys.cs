namespace SuperMetroid.Core.Frontend.Title;

/// <summary>
/// The installed key of each <see cref="TitleSprite"/> (#2520), written out so renaming a member never changes a name
/// the installation, an installed document or a save spells it by.
/// </summary>
internal static partial class TitleSpriteKeys
{
    /// <summary>Each TitleSprite member's installed key.</summary>
    internal static InstalledKeys<TitleSprite> Installed { get; } = new(new Dictionary<TitleSprite, string>
    {
        [TitleSprite.SuperMetroidLogo] = "SuperMetroidLogo",
        [TitleSprite.NintendoCopyright] = "NintendoCopyright",
    });
}
