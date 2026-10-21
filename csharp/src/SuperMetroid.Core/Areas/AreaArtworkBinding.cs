namespace SuperMetroid.Core.Areas;

/// <summary>
/// The area artwork the host has installed, rebound when it reloads artwork (#1395). The runtime owns one for the
/// session; each room's area actors read through it, so a rebinding reaches actors already in the room.
/// </summary>
public sealed class AreaArtworkBinding
{
    /// <summary>The installed area artwork, or null before any is installed.</summary>
    public AreaArtworkCatalog? Artwork { get; set; }

    /// <summary>The installed area artwork; an actor that needs it fails when none is installed.</summary>
    internal AreaArtworkCatalog Installed =>
        Artwork ?? throw new InvalidOperationException("No area artwork is installed.");
}
