namespace SuperMetroid.Core.Frontend.Options.Artwork;

/// <summary>Whether an options anchor may lie off screen horizontally.</summary>
public enum OffscreenX
{
    /// <summary>X must be on screen.</summary>
    Rejected,

    /// <summary>X may extend into the OAM X field.</summary>
    Allowed,
}
