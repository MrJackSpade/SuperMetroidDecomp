namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>
/// The installed key of each <see cref="GrappleEndpointFrame"/> (#2520), written out so renaming a member never changes
/// a name the installation, an installed document or a save spells it by.
/// </summary>
internal static class GrappleEndpointFrameKeys
{
    /// <summary>Each GrappleEndpointFrame member's installed key.</summary>
    internal static InstalledKeys<GrappleEndpointFrame> Installed { get; } = new(new Dictionary<GrappleEndpointFrame, string>
    {
        [GrappleEndpointFrame.Point] = "Point",
        [GrappleEndpointFrame.Orb] = "Orb",
        [GrappleEndpointFrame.Ring] = "Ring",
        [GrappleEndpointFrame.SparklingRing] = "SparklingRing",
    });
}
