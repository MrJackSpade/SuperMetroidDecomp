namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>
/// The selected appearance of Samus's bank-$90 atmospheric effects (#1463). Gameplay draws each effect's frame from the
/// pre-coloured atmosphere sheet by name; the appearances the sheet is composed from are the import pipeline's, and the
/// body catalog fingerprints the selection by this identity.
/// </summary>
public abstract class InstalledSamusAtmosphericArtwork
{
    /// <summary>SHA-256 of both selected atmospheric appearance lists.</summary>
    public abstract string ContentIdentity { get; }
}
