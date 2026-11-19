namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>
/// The installed key of each <see cref="GrappleRopeOrientation"/> (#2520), written out so renaming a member never
/// changes a name the installation, an installed document or a save spells it by.
/// </summary>
internal static class GrappleRopeOrientationKeys
{
    /// <summary>Each GrappleRopeOrientation member's installed key.</summary>
    internal static InstalledKeys<GrappleRopeOrientation> Installed { get; } = new(new Dictionary<GrappleRopeOrientation, string>
    {
        [GrappleRopeOrientation.Horizontal] = "Horizontal",
        [GrappleRopeOrientation.Diagonal] = "Diagonal",
        [GrappleRopeOrientation.Vertical] = "Vertical",
    });
}
