namespace SuperMetroid.Core.Frontend.Ending.Artwork;

/// <summary>
/// The installed key of each <see cref="EndingExplosionPicture"/> (#2520), written out so renaming a member never
/// changes a name the installation, an installed document or a save spells it by.
/// </summary>
internal static class EndingExplosionPictureKeys
{
    /// <summary>Each EndingExplosionPicture member's installed key.</summary>
    internal static InstalledKeys<EndingExplosionPicture> Installed { get; } = new(new Dictionary<EndingExplosionPicture, string>
    {
        [EndingExplosionPicture.WideExplosion] = "WideExplosion",
        [EndingExplosionPicture.ConcentricExplosion] = "ConcentricExplosion",
        [EndingExplosionPicture.Eclipse] = "Eclipse",
        [EndingExplosionPicture.BurstGlow] = "Blank",
    });
}
