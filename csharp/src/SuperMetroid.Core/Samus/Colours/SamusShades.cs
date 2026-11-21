using System.Collections.Frozen;
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
public static partial class SamusShades
{
    /// <summary>
    /// Each suit's name in its shades' installed names (#2511), written out so renaming a suit's identity never changes a
    /// name on disk.
    /// </summary>
    private static readonly FrozenDictionary<SamusSuit, string> SuitKeys =
        new Dictionary<SamusSuit, string>
        {
            [SamusSuit.Power] = "Power",
            [SamusSuit.Varia] = "Varia",
            [SamusSuit.Gravity] = "Gravity",
        }.ToFrozenDictionary();

    /// <summary>
    /// Each charge's name in its pulse shades' installed names (#2511), written out so renaming a charge's identity never
    /// changes a name on disk.
    /// </summary>
    private static readonly FrozenDictionary<SamusChargeKind, string> ChargeKeys =
        new Dictionary<SamusChargeKind, string>
        {
            [SamusChargeKind.ChargedBeam] = "ChargedBeam",
            [SamusChargeKind.PseudoScrew] = "PseudoScrew",
        }.ToFrozenDictionary();

    /// <summary>
    /// Each full-body cycle's name in its shades' installed names (#2511), written out so renaming a cycle's identity never
    /// changes a name on disk.
    /// </summary>
    private static readonly FrozenDictionary<SamusFullBodyCycleFamily, string> FullBodyCycleKeys =
        new Dictionary<SamusFullBodyCycleFamily, string>
        {
            [SamusFullBodyCycleFamily.SpeedBooster] = "SpeedBooster",
            [SamusFullBodyCycleFamily.ScrewAttack] = "ScrewAttack",
            [SamusFullBodyCycleFamily.StoredShine] = "StoredShine",
            [SamusFullBodyCycleFamily.ActiveShinespark] = "ActiveShinespark",
        }.ToFrozenDictionary();

    /// <summary>
    /// The installed name of each hurt colouring's shade (#2511), written out so renaming a colouring's identity never
    /// changes a name on disk.
    /// </summary>
    private static readonly FrozenDictionary<SamusHurtColorVariant, string> HurtKeys =
        new Dictionary<SamusHurtColorVariant, string>
        {
            [SamusHurtColorVariant.Hurt] = "HurtHurt",
            [SamusHurtColorVariant.Intro] = "HurtIntro",
        }.ToFrozenDictionary();

    /// <summary>
    /// The installed name of each beam combination's shade (#2511), written out so renaming a combination's identity never
    /// changes a name on disk.
    /// </summary>
    private static readonly FrozenDictionary<SamusBeamCombination, string> BeamKeys =
        new Dictionary<SamusBeamCombination, string>
        {
            [SamusBeamCombination.Power] = "BeamPower",
            [SamusBeamCombination.Wave] = "BeamWave",
            [SamusBeamCombination.Ice] = "BeamIce",
            [SamusBeamCombination.IceWave] = "BeamIceWave",
            [SamusBeamCombination.Spazer] = "BeamSpazer",
            [SamusBeamCombination.SpazerWave] = "BeamSpazerWave",
            [SamusBeamCombination.SpazerIce] = "BeamSpazerIce",
            [SamusBeamCombination.SpazerIceWave] = "BeamSpazerIceWave",
            [SamusBeamCombination.Plasma] = "BeamPlasma",
            [SamusBeamCombination.PlasmaWave] = "BeamPlasmaWave",
            [SamusBeamCombination.PlasmaIce] = "BeamPlasmaIce",
            [SamusBeamCombination.PlasmaIceWave] = "BeamPlasmaIceWave",
            [SamusBeamCombination.SpazerPlasma] = "BeamSpazerPlasma",
            [SamusBeamCombination.SpazerPlasmaWave] = "BeamSpazerPlasmaWave",
            [SamusBeamCombination.SpazerPlasmaIce] = "BeamSpazerPlasmaIce",
            [SamusBeamCombination.SpazerPlasmaIceWave] = "BeamSpazerPlasmaIceWave",
        }.ToFrozenDictionary();

    /// <summary>The suit's colours as gameplay starts.</summary>
    public static PictureShade StartingSuit { get; } = new("SuitStarting", 0);

    /// <summary>The beams' colours as gameplay starts.</summary>
    public static PictureShade StartingBeams { get; } = new("BeamsStarting", 0);

    /// <summary>The suit's colours in the opening's gameplay flashbacks, as the opening's own colours hold them.</summary>
    public static PictureShade IntroSuit { get; } = new("SuitIntro", 0);

    /// <summary>The beams' colours in the opening's gameplay flashbacks, as the opening's own colours hold them.</summary>
    public static PictureShade IntroBeams { get; } = new("BeamsIntro", 0);

    /// <summary>The suit's normal colours in <paramref name="suit"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The suit has no installed name.</exception>
    public static PictureShade Suit(SamusSuit suit) => new($"Suit{Ensure.Listed(SuitKeys, suit)}", 0);

    /// <summary>The suit under one step of a charged beam's pulse or the pseudo Screw Attack's hold.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The charge or the suit has no installed name.</exception>
    public static PictureShade Charge(SamusChargeShade shade) =>
        new($"Charge{Ensure.Listed(ChargeKeys, shade.Kind)}{Ensure.Listed(SuitKeys, shade.Suit)}", shade.Step);

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
    /// <exception cref="ArgumentOutOfRangeException">The cycle or the suit has no installed name.</exception>
    public static PictureShade FullBodyCycle(SamusFullBodyCycleShade shade) =>
        new($"{Ensure.Listed(FullBodyCycleKeys, shade.Family)}{Ensure.Listed(SuitKeys, shade.Suit)}", shade.Shade);

    /// <summary>
    /// The suit under each of <paramref name="family"/>'s shades in <paramref name="suit"/>, from its first to its brightest
    /// (#2447).
    /// </summary>
    public static IReadOnlyList<PictureShade> FullBodyCycleShades(SamusFullBodyCycleFamily family, SamusSuit suit) =>
        [.. Enumerable.Range(0, SamusShadeDefinitions.FullBodyCycles.FamilyShades)
            .Select(shade => FullBodyCycle(new(family, suit, shade)))];

    /// <summary>The suit under the hurt flash's bright colours or its cinematic grey restoration.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The colouring has no installed name.</exception>
    public static PictureShade Hurt(SamusHurtColorVariant variant) => new(Ensure.Listed(HurtKeys, variant), 0);

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

    /// <summary>The grapple flare's colour once the grapple has fired.</summary>
    public static PictureShade GrappleFiringFlare { get; } = new("GrappleFlare", 0);

    /// <summary>
    /// The beams' colours for one beam combination: the shade that combination shows. The Chainsaw beam, which has no
    /// shade of its own, shows the colours of the combination it shares its shade with (#1736).
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">The combination has no installed name.</exception>
    public static PictureShade Beam(SamusBeamCombination combination) => new(Ensure.Listed(BeamKeys, ShownBeamShade(combination)), 0);

    /// <summary>The beam combination whose shade <paramref name="combination"/> shows.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The combination has no beam graphics.</exception>
    internal static SamusBeamCombination ShownBeamShade(SamusBeamCombination combination) =>
        Ensure.Listed(SamusBeamGraphicsDefinitions.ShownShades, combination);

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
    /// <exception cref="ArgumentOutOfRangeException">The suit has no installed name.</exception>
    public static PictureShade DeathSuit(SamusSuit suit, SamusDeathShade shade) => new($"Death{Ensure.Listed(SuitKeys, suit)}", shade.Step);

    /// <summary>Her body without the suit as her death sequence shows it under <paramref name="shade"/>.</summary>
    public static PictureShade DeathSuitless(SamusDeathShade shade) => new("DeathSuitless", shade.Step);

    /// <summary>Her beams' and grapple flare's colours under <paramref name="shade"/> of the death sequence's whiteout.</summary>
    public static PictureShade DeathWhiteout(SamusWhiteoutShade shade) => new("DeathWhiteout", shade.Step);

    /// <summary>The <paramref name="count"/> shades of the run <paramref name="effect"/>, in the order the run shows them.</summary>
    private static PictureShade[] Run([InstalledKey] string effect, int count) =>
        [.. Enumerable.Range(0, count).Select(step => new PictureShade(effect, step))];
}
