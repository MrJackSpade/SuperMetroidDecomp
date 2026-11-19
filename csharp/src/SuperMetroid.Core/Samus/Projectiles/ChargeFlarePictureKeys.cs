namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// The installed key of each <see cref="ChargeFlarePicture"/> (#2520), written out so renaming a member never changes a
/// name the installation, an installed document or a save spells it by.
/// </summary>
internal static class ChargeFlarePictureKeys
{
    /// <summary>Each ChargeFlarePicture member's installed key.</summary>
    internal static InstalledKeys<ChargeFlarePicture> Installed { get; } = new(new Dictionary<ChargeFlarePicture, string>
    {
        [ChargeFlarePicture.Ember] = "Ember",
        [ChargeFlarePicture.Glow] = "Glow",
        [ChargeFlarePicture.WhiteBall] = "WhiteBall",
        [ChargeFlarePicture.Starburst] = "Starburst",
        [ChargeFlarePicture.SparksWidest] = "SparksWidest",
        [ChargeFlarePicture.SparksWider] = "SparksWider",
        [ChargeFlarePicture.SparksWide] = "SparksWide",
        [ChargeFlarePicture.SparksNarrow] = "SparksNarrow",
        [ChargeFlarePicture.SparksNarrower] = "SparksNarrower",
        [ChargeFlarePicture.SparksNarrowest] = "SparksNarrowest",
    });
}
