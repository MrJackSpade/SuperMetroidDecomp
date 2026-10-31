using SuperMetroid.Core.Escape.Artwork;
using SuperMetroid.Core.Enemies.BossBodies;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.Core.Bosses.Ridley.Artwork;

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
    public CeresRidleyFloorColorCatalog? CeresRidleyFloorColors { get; set; }

    /// <summary>The Speed Booster delays Ceres Ridley's getaway wing frames leave behind, bound with the map presentation.</summary>
    public CeresRidleyGetawayBoostDelays? CeresRidleyGetawayBoostDelays { get; set; }

    /// <summary>The escape typewriters' installed text, bound with the map presentation.</summary>
    public EscapeTypewriterPresentation? EscapeTypewriterPresentation { get; set; }

    /// <summary>Every boss's installed body sheet, or null before any is installed (#1466).</summary>
    public BossBodySheets? BodySheets { get; set; }

    /// <summary><paramref name="owner"/>'s installed body sheet; a boss that draws its body fails when none is installed.</summary>
    /// <exception cref="InvalidOperationException">No body sheets are installed.</exception>
    internal BossBodySheet Body(BossBodyOwner owner) =>
        (BodySheets ?? throw new InvalidOperationException($"No boss body sheets are installed to draw {owner}.")).Of(owner);
}
