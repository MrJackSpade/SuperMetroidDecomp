namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// The installed key of each <see cref="SamusProjectileDirection"/> (#2520), written out so renaming a member never
/// changes a name the installation, an installed document or a save spells it by.
/// </summary>
internal static class SamusProjectileDirectionKeys
{
    /// <summary>Each SamusProjectileDirection member's installed key.</summary>
    internal static InstalledKeys<SamusProjectileDirection> Installed { get; } = new(new Dictionary<SamusProjectileDirection, string>
    {
        [SamusProjectileDirection.UpFacingRight] = "UpFacingRight",
        [SamusProjectileDirection.UpRight] = "UpRight",
        [SamusProjectileDirection.Right] = "Right",
        [SamusProjectileDirection.DownRight] = "DownRight",
        [SamusProjectileDirection.DownFacingRight] = "DownFacingRight",
        [SamusProjectileDirection.DownFacingLeft] = "DownFacingLeft",
        [SamusProjectileDirection.DownLeft] = "DownLeft",
        [SamusProjectileDirection.Left] = "Left",
        [SamusProjectileDirection.UpLeft] = "UpLeft",
        [SamusProjectileDirection.UpFacingLeft] = "UpFacingLeft",
    });
}
