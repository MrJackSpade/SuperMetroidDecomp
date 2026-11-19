namespace SuperMetroid.Core.Assets;

/// <summary>
/// The installed key of each <see cref="EncounterArtwork"/> (#2520), written out so renaming a member never changes a
/// name the installation, an installed document or a save spells it by.
/// </summary>
internal static class EncounterArtworkKeys
{
    /// <summary>Each EncounterArtwork member's installed key.</summary>
    internal static InstalledKeys<EncounterArtwork> Installed { get; } = new(new Dictionary<EncounterArtwork, string>
    {
        [EncounterArtwork.Gunship] = "Gunship",
        [EncounterArtwork.Crocomire] = "Crocomire",
        [EncounterArtwork.BombTorizo] = "BombTorizo",
        [EncounterArtwork.GoldenTorizo] = "GoldenTorizo",
        [EncounterArtwork.CeresElevatorDoor] = "CeresElevatorDoor",
        [EncounterArtwork.CeresEscape] = "CeresEscape",
    });
}
