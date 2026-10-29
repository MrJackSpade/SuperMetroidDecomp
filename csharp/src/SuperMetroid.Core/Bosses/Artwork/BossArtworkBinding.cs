using SuperMetroid.Core.Escape.Artwork;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.Core.Bosses.Ridley.Artwork;

namespace SuperMetroid.Core.Bosses.Artwork;

/// <summary>
/// The boss artwork the host has installed, rebound when it reloads artwork (#1395). The runtime owns one for the
/// session; each room's bosses read through it, so a rebinding reaches bosses already in the room.
/// </summary>
[ArchitectureReview("context-bag", "The session\'s installed boss artwork and presentation catalogs; its members are data the host binds, not services.")]
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
    public CeresRidleyFloorColorCatalog? CeresRidleyFloorColors { get; set; }

    /// <summary>Where Ceres Ridley's getaway floor runs take their tile numbers from, bound with the map presentation.</summary>
    public CeresRidleyFloorUpdateSources? CeresRidleyFloorUpdates { get; set; }

    /// <summary>The escape typewriters' installed text, bound with the map presentation.</summary>
    public EscapeTypewriterPresentation? EscapeTypewriterPresentation { get; set; }

    /// <summary>The installed boss artwork; a boss that needs it fails when none is installed.</summary>
    internal BossArtworkCatalog Installed =>
        Artwork ?? throw new InvalidOperationException("No boss artwork is installed.");
}
