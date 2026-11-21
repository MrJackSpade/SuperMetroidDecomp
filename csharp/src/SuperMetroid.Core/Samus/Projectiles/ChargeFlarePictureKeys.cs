namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// The installed key of each main-flare and spark picture (#2520, #2553), written out so renaming a member never changes a
/// name the installation, an installed document or a save spells it by.
/// </summary>
internal static class ChargeFlarePictureKeys
{
    /// <summary>Each MainFlarePicture member's installed key.</summary>
    internal static InstalledKeys<MainFlarePicture> MainFlare { get; } = new(new Dictionary<MainFlarePicture, string>
    {
        [MainFlarePicture.Ember] = "Ember",
        [MainFlarePicture.Glow] = "Glow",
        [MainFlarePicture.WhiteBall] = "WhiteBall",
        [MainFlarePicture.Starburst] = "Starburst",
    });

    /// <summary>Each SparkPicture member's installed key.</summary>
    internal static InstalledKeys<SparkPicture> Sparks { get; } = new(new Dictionary<SparkPicture, string>
    {
        [SparkPicture.Widest] = "SparksWidest",
        [SparkPicture.Wider] = "SparksWider",
        [SparkPicture.Wide] = "SparksWide",
        [SparkPicture.Narrow] = "SparksNarrow",
        [SparkPicture.Narrower] = "SparksNarrower",
        [SparkPicture.Narrowest] = "SparksNarrowest",
    });
}
