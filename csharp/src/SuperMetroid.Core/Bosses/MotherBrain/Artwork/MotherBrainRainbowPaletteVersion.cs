namespace SuperMetroid.Core.Bosses.MotherBrain.Artwork;

/// <summary>Schema revisions of the editable Mother Brain rainbow palette document.</summary>
public enum MotherBrainRainbowPaletteVersion
{
    /// <summary>Legacy revision lacking fake-death colors, accepted only when loading with a verified current-stock fallback.</summary>
    PreFakeDeath = 2,
    /// <summary>Current palette schema revision, requiring the brain-only fake-death sequence alongside all previous color families.</summary>
    Current = 3,
}
