using SuperMetroid.Core.Escape.Artwork;
using SuperMetroid.Core.Enemies.BossBodies;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.Core.Bosses.Ridley.Artwork;
using SuperMetroid.Core.Bosses.Kraid;

namespace SuperMetroid.Core.Bosses.Artwork;

/// <summary>
/// The boss artwork the host has installed, rebound when it reloads artwork (#1395). The runtime owns one for the
/// session; each room's bosses read through it, so a rebinding reaches bosses already in the room. Every member is
/// installed from the start and rebound only to other installed artwork (#2246, #2269).
/// </summary>
/// <param name="ceresRidleyGetawayBoostDelays">The installed delays Ceres Ridley's getaway wing frames leave behind.</param>
/// <param name="escapeTypewriterPresentation">The escape typewriters' installed text.</param>
/// <param name="motherBrainRainbowColors">Mother Brain's installed rainbow-beam colours.</param>
/// <param name="kraidEyes">Kraid's installed eye colours.</param>
/// <param name="bodySheets">Every boss's installed body sheet.</param>
public sealed class BossArtworkBinding(CeresRidleyGetawayBoostDelays ceresRidleyGetawayBoostDelays,
    EscapeTypewriterPresentation escapeTypewriterPresentation, MotherBrainRainbowBeamColours motherBrainRainbowColors,
    KraidEyeColours kraidEyes, BossBodySheets bodySheets)
{
    /// <summary>
    /// Kraid's installed eye colours, which his eye glow reads back (#1512), rebound with the bosses' artwork; the rest of
    /// the bosses' installed artwork is pre-coloured into their sheets by the import (#1583).
    /// </summary>
    public KraidEyeColours KraidEyes
    {
        get;
        set => field = Ensure.NotNull(value);
    } = Ensure.NotNull(kraidEyes);

    /// <summary>Mother Brain's installed rainbow-beam colours, rebound with the map presentation.</summary>
    public MotherBrainRainbowBeamColours MotherBrainRainbowColors
    {
        get;
        set => field = Ensure.NotNull(value);
    } = Ensure.NotNull(motherBrainRainbowColors);

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

    /// <summary>Every boss's installed body sheet (#1466), rebound with the bosses' artwork.</summary>
    public BossBodySheets BodySheets
    {
        get;
        set => field = Ensure.NotNull(value);
    } = Ensure.NotNull(bodySheets);

    /// <summary><paramref name="owner"/>'s installed body sheet.</summary>
    internal BossBodySheet Body(BossBodyOwner owner) => BodySheets.Of(owner);
}
