using SuperMetroid.Core.Escape.Artwork;
using SuperMetroid.Core.Enemies.BossBodies;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.Core.Bosses.Ridley.Artwork;
using SuperMetroid.Core.Bosses.Kraid;

namespace SuperMetroid.Core.Bosses.Artwork;

/// <summary>
/// The boss artwork the host has installed, rebound when it reloads artwork (#1395). The runtime owns one for the
/// session; each room's bosses read through it, so a rebinding reaches bosses already in the room.
/// </summary>
/// <param name="ceresRidleyGetawayBoostDelays">The installed delays Ceres Ridley's getaway wing frames leave behind.</param>
/// <param name="escapeTypewriterPresentation">The escape typewriters' installed text.</param>
public sealed class BossArtworkBinding(CeresRidleyGetawayBoostDelays ceresRidleyGetawayBoostDelays,
    EscapeTypewriterPresentation escapeTypewriterPresentation)
{
    /// <summary>
    /// Kraid's installed eye colours, which his eye glow reads back (#1512), or null before any are installed; the rest of
    /// the bosses' installed artwork is pre-coloured into their sheets by the import (#1583).
    /// </summary>
    public KraidEyeColours? KraidEyes { get; set; }

    /// <summary>Mother Brain's installed rainbow-beam colors, bound with the map presentation.</summary>
    public MotherBrainRainbowBeamColours? MotherBrainRainbowColors { get; set; }

    /// <summary>Mother Brain's installed rainbow-beam colours; a beam that shows fails when none are installed.</summary>
    /// <exception cref="InvalidOperationException">No rainbow-beam colours are installed.</exception>
    internal MotherBrainRainbowBeamColours RainbowBeamColours() =>
        MotherBrainRainbowColors ?? throw new InvalidOperationException("No Mother Brain rainbow-beam colours are installed to show her beam.");

    /// <summary>
    /// The Speed Booster delays Ceres Ridley's getaway wing frames leave behind, installed from the start and rebound with
    /// the map presentation.
    /// </summary>
    public CeresRidleyGetawayBoostDelays CeresRidleyGetawayBoostDelays
    {
        get;
        set => field = Ensure.NotNull(value);
    } = Ensure.NotNull(ceresRidleyGetawayBoostDelays);

    /// <summary>The escape typewriters' installed text, installed from the start and rebound with the map presentation.</summary>
    public EscapeTypewriterPresentation EscapeTypewriterPresentation
    {
        get;
        set => field = Ensure.NotNull(value);
    } = Ensure.NotNull(escapeTypewriterPresentation);

    /// <summary>Every boss's installed body sheet, or null before any is installed (#1466).</summary>
    public BossBodySheets? BodySheets { get; set; }

    /// <summary><paramref name="owner"/>'s installed body sheet; a boss that draws its body fails when none is installed.</summary>
    /// <exception cref="InvalidOperationException">No body sheets are installed.</exception>
    internal BossBodySheet Body(BossBodyOwner owner) =>
        (BodySheets ?? throw new InvalidOperationException($"No boss body sheets are installed to draw {owner}.")).Of(owner);
}
