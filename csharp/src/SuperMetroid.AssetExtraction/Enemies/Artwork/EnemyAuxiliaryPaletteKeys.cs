using SuperMetroid.Core;

namespace SuperMetroid.AssetExtraction.Enemies.Artwork;

/// <summary>
/// The installed key of each <see cref="EnemyAuxiliaryPalette"/> (#2520), written out so renaming a member never
/// changes a name the installation, an installed document or a save spells it by.
/// </summary>
internal static class EnemyAuxiliaryPaletteKeys
{
    /// <summary>Each EnemyAuxiliaryPalette member's installed key.</summary>
    internal static InstalledKeys<EnemyAuxiliaryPalette> Installed { get; } = new(new Dictionary<EnemyAuxiliaryPalette, string>
    {
        [EnemyAuxiliaryPalette.FaceBlock] = "FaceBlock",
        [EnemyAuxiliaryPalette.DeadSidehopper] = "DeadSidehopper",
        [EnemyAuxiliaryPalette.GoldenTorizoBody] = "GoldenTorizoBody",
        [EnemyAuxiliaryPalette.GoldenTorizoBelly] = "GoldenTorizoBelly",
    });
}
