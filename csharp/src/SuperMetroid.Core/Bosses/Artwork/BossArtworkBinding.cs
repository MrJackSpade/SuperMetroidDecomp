using SuperMetroid.Core.Rooms.Artwork;

namespace SuperMetroid.Core.Bosses.Artwork;

/// <summary>
/// The boss artwork the host has installed, rebound when it reloads artwork (#1395). The runtime owns one for the
/// session; each room's bosses read through it, so a rebinding reaches bosses already in the room.
/// </summary>
public sealed class BossArtworkBinding
{
    /// <summary>The installed boss artwork, or null before any is installed.</summary>
    public BossArtworkCatalog? Artwork { get; set; }

    /// <summary>The installed boss artwork; a boss that needs it fails when none is installed.</summary>
    internal BossArtworkCatalog Installed =>
        Artwork ?? throw new InvalidOperationException("No boss artwork is installed.");

    /// <summary>The installed Mother Brain corpse tiles; the bound artwork must provide them.</summary>
    internal RoomCharacterAtlas MotherBrainCorpse =>
        Artwork?.MotherBrainCorpse ?? throw new InvalidDataException(
            "Installed boss artwork is missing mother-brain-corpse-tiles.png.");
}
