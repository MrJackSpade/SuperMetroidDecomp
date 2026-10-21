using SuperMetroid.Core.Hud.Artwork;
using SuperMetroid.Core.Escape.Artwork;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.Core.Bosses.Ridley.Artwork;
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

    /// <summary>Mother Brain's installed health colors, bound with the map presentation.</summary>
    public MotherBrainHealthPalettePresentation? MotherBrainHealthColors { get; set; }

    /// <summary>Mother Brain's installed rainbow-beam colors, bound with the map presentation.</summary>
    public MotherBrainRainbowPalettePresentation? MotherBrainRainbowColors { get; set; }

    /// <summary>Mother Brain's installed room colors, bound with the map presentation.</summary>
    public MotherBrainRoomColorPresentation? MotherBrainRoomColors { get; set; }

    /// <summary>Ceres Ridley's installed colors, bound with the map presentation.</summary>
    public CeresRidleyColorCatalog? CeresRidleyColors { get; set; }

    /// <summary>Ceres Ridley's installed Mode 7 colors, bound with the map presentation.</summary>
    public CeresRidleyMode7ColorCatalog? CeresRidleyMode7Colors { get; set; }

    /// <summary>The escape timer's installed tiles, bound with the map presentation.</summary>
    public EscapeTimerTileAtlas? EscapeTimerArtwork { get; set; }

    /// <summary>The escape typewriters' installed text, bound with the map presentation.</summary>
    public EscapeTypewriterPresentation? EscapeTypewriterPresentation { get; set; }

    /// <summary>The installed standard BG3 characters Kraid's death restores, bound with the map presentation.</summary>
    public HudTileAtlas? HudTileArtwork { get; set; }

    /// <summary>The installed boss artwork; a boss that needs it fails when none is installed.</summary>
    internal BossArtworkCatalog Installed =>
        Artwork ?? throw new InvalidOperationException("No boss artwork is installed.");

    /// <summary>The installed Mother Brain corpse tiles; the bound artwork must provide them.</summary>
    internal RoomCharacterAtlas MotherBrainCorpse =>
        Artwork?.MotherBrainCorpse ?? throw new InvalidDataException(
            "Installed boss artwork is missing mother-brain-corpse-tiles.png.");
}
