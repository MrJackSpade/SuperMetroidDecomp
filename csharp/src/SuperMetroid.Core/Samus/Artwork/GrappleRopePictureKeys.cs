namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>
/// The installed key of each <see cref="GrappleRopePicture"/> (#2520), written out so renaming a member never changes a
/// name the installation, an installed document or a save spells it by.
/// </summary>
internal static partial class GrappleRopePictureKeys
{
    /// <summary>Each GrappleRopePicture member's installed key.</summary>
    internal static InstalledKeys<GrappleRopePicture> Installed { get; } = new(new Dictionary<GrappleRopePicture, string>
    {
        [GrappleRopePicture.Start] = "Start",
        [GrappleRopePicture.Quarter] = "Quarter",
        [GrappleRopePicture.Half] = "Half",
        [GrappleRopePicture.ThreeQuarters] = "ThreeQuarters",
    });
}
