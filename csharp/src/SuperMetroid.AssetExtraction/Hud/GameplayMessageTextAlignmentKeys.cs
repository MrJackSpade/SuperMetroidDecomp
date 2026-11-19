using SuperMetroid.Core;

namespace SuperMetroid.AssetExtraction.Hud;

/// <summary>
/// The installed key of each <see cref="GameplayMessageTextAlignment"/> (#2520), written out so renaming a member never
/// changes a name the installation, an installed document or a save spells it by.
/// </summary>
internal static class GameplayMessageTextAlignmentKeys
{
    /// <summary>Each GameplayMessageTextAlignment member's installed key.</summary>
    internal static InstalledKeys<GameplayMessageTextAlignment> Installed { get; } = new(new Dictionary<GameplayMessageTextAlignment, string>
    {
        [GameplayMessageTextAlignment.Left] = "Left",
        [GameplayMessageTextAlignment.Center] = "Center",
    });
}
