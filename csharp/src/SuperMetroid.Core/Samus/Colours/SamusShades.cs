using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.Core.Samus.Death;
using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Core.Samus.Colours;

/// <summary>
/// The shades the import pre-colours Samus's sheets in (#1512), one for each colouring her suit, visor, beams and
/// suitless body take. Her colour state names them; which inks each recolours, and to what, is the import's.
/// </summary>
public static class SamusShades
{
    /// <summary>The suit's colours as gameplay starts, before any suit is loaded.</summary>
    public static PictureShade StartingSuit { get; } = new("SuitStarting", 0);

    /// <summary>The beams' colours as gameplay starts, before any beam is loaded.</summary>
    public static PictureShade StartingBeams { get; } = new("BeamsStarting", 0);

    /// <summary>The suit's colours in the opening's gameplay flashbacks, as the opening's own colours hold them.</summary>
    public static PictureShade IntroSuit { get; } = new("SuitIntro", 0);

    /// <summary>The beams' colours in the opening's gameplay flashbacks, as the opening's own colours hold them.</summary>
    public static PictureShade IntroBeams { get; } = new("BeamsIntro", 0);

    /// <summary>The suit's normal colours in <paramref name="suit"/>.</summary>
    public static PictureShade Suit(SamusSuit suit) => new($"Suit{suit}", 0);

    /// <summary>The suit under one step of a charged beam's pulse or the pseudo Screw Attack's hold.</summary>
    public static PictureShade Charge(SamusChargeShade shade) => new($"Charge{shade.Kind}{shade.Suit}", shade.Step);

    /// <summary>
    /// The suit under each step of <paramref name="kind"/>'s pulse in <paramref name="suit"/>, in the order the pulse shows
    /// them (#2447).
    /// </summary>
    public static IReadOnlyList<PictureShade> ChargePulse(SamusChargeKind kind, SamusSuit suit) =>
        [.. Enumerable.Range(0, SamusShadeDefinitions.Charge.PulseShades).Select(step => Charge(new(kind, suit, step)))];

    /// <summary>
    /// The suit under each frame of the Hyper Beam's full-body glow, in the order the drained Samus's rainbow shows them
    /// (#2387).
    /// </summary>
    public static IReadOnlyList<PictureShade> HyperBeamSuit { get; } =
        Run("HyperBeamSuit", SamusShadeDefinitions.FullBodyCycles.HyperBeamShadeCount);

    /// <summary>
    /// The suit under each frame of a Hyper shot's glow, which walks the charge colours' copy of the Hyper Beam glow (#2447):
    /// in the full-body glow's order, which the shot's glow plays from its last to its first.
    /// </summary>
    public static IReadOnlyList<PictureShade> HyperShot { get; } =
        Run("HyperShot", SamusShadeDefinitions.FullBodyCycles.HyperBeamShadeCount);

    /// <summary>The suit under one shade of a full-body cycle: Speed Booster, Screw Attack or a shinespark's glow.</summary>
    public static PictureShade FullBodyCycle(SamusFullBodyCycleShade shade) =>
        new($"{shade.Family}{shade.Suit}", shade.Shade);

    /// <summary>
    /// The suit under each of <paramref name="family"/>'s shades in <paramref name="suit"/>, from its first to its brightest
    /// (#2447).
    /// </summary>
    public static IReadOnlyList<PictureShade> FullBodyCycleShades(SamusFullBodyCycleFamily family, SamusSuit suit) =>
        [.. Enumerable.Range(0, SamusShadeDefinitions.FullBodyCycles.FamilyShades)
            .Select(shade => FullBodyCycle(new(family, suit, shade)))];

    /// <summary>The suit under the hurt flash's bright colours or its cinematic grey restoration.</summary>
    public static PictureShade Hurt(SamusHurtColorVariant variant) => new($"Hurt{variant}", 0);

    /// <summary>The suit's visible colours under an ordinary charged shot's yellow glow.</summary>
    public static PictureShade ChargedShotGlow { get; } = new("ChargedShotGlow", 0);

    /// <summary>The visor under each shade of the X-ray widening glow (#2285), in the order the glow shows them (#2387).</summary>
    public static IReadOnlyList<PictureShade> XrayWideningVisor { get; } =
        Run("VisorXrayWidening", SamusShadeDefinitions.Visor.XrayWideningShades);

    /// <summary>
    /// The visor under each shade of the steady visor cycle, which the full X-ray beam and the room both show (#2285), in the
    /// order the cycle shows them (#2387).
    /// </summary>
    public static IReadOnlyList<PictureShade> VisorCycle { get; } = Run("VisorCycle", SamusShadeDefinitions.Visor.CycleShades);

    /// <summary>The visor's green as a door transition finishes loading the next room.</summary>
    public static PictureShade DoorCompletionVisor { get; } = new("VisorDoorCompletion", 0);

    /// <summary>The grapple flare's colour once the grapple's firing colours are loaded.</summary>
    public static PictureShade GrappleFiringFlare { get; } = new("GrappleFlare", 0);

    /// <summary>
    /// The beams' colours for one beam combination: the shade it selects in the beam shade table. The Chainsaw beam,
    /// which the table does not list, shows the colours of the listed combination it shares its shade with (#1736).
    /// </summary>
    public static PictureShade Beam(SamusBeamCombination combination) => new($"Beam{ShownBeamShade(combination)}", 0);

    /// <summary>The beam combination whose shade <paramref name="combination"/> shows.</summary>
    internal static SamusBeamCombination ShownBeamShade(SamusBeamCombination combination) =>
        combination == ChainsawBeamGraphicsDefinitions.Selection ? ChainsawBeamGraphicsDefinitions.ShownBeamShade : combination;

    /// <summary>The Crystal Flash's body colours at each step of its pulse, in the order the pulse shows them (#2387).</summary>
    public static IReadOnlyList<PictureShade> CrystalFlashBody { get; } =
        Run("CrystalFlashBody", SamusShadeDefinitions.CrystalFlash.BodyStepCount);

    /// <summary>The Crystal Flash's bubble colours at each step of their rotation, in the order it shows them (#2387).</summary>
    public static IReadOnlyList<PictureShade> CrystalFlashBubble { get; } =
        Run("CrystalFlashBubble", SamusShadeDefinitions.CrystalFlash.BubbleShadeCount);

    /// <summary>The beams' colours under each frame of the Hyper Beam's own colour cycle, in the order it shows them (#2447).</summary>
    public static IReadOnlyList<PictureShade> HyperBeamCycle { get; } =
        Run("HyperBeamCycle", SamusShadeDefinitions.HyperBeamCycle.Frames);

    /// <summary>The suit as her death sequence shows it in <paramref name="suit"/> under <paramref name="shade"/>.</summary>
    public static PictureShade DeathSuit(SamusSuit suit, SamusDeathShade shade) => new($"Death{suit}", shade.Step);

    /// <summary>Her body without the suit as her death sequence shows it under <paramref name="shade"/>.</summary>
    public static PictureShade DeathSuitless(SamusDeathShade shade) => new("DeathSuitless", shade.Step);

    /// <summary>Her beams' and grapple flare's colours under <paramref name="shade"/> of the death sequence's whiteout.</summary>
    public static PictureShade DeathWhiteout(SamusWhiteoutShade shade) => new("DeathWhiteout", shade.Step);

    /// <summary>The <paramref name="count"/> shades of the run <paramref name="effect"/>, in the order the run shows them.</summary>
    private static PictureShade[] Run(string effect, int count) =>
        [.. Enumerable.Range(0, count).Select(step => new PictureShade(effect, step))];
}
