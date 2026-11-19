namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>
/// The installed key of each <see cref="SamusArmCannonCoverFrame"/> (#2520), written out so renaming a member never
/// changes a name the installation, an installed document or a save spells it by.
/// </summary>
internal static class SamusArmCannonCoverFrameKeys
{
    /// <summary>Each SamusArmCannonCoverFrame member's installed key.</summary>
    internal static InstalledKeys<SamusArmCannonCoverFrame> Installed { get; } = new(new Dictionary<SamusArmCannonCoverFrame, string>
    {
        [SamusArmCannonCoverFrame.SlightlyOpen] = "SlightlyOpen",
        [SamusArmCannonCoverFrame.HalfOpen] = "HalfOpen",
        [SamusArmCannonCoverFrame.FullyOpen] = "FullyOpen",
    });
}
