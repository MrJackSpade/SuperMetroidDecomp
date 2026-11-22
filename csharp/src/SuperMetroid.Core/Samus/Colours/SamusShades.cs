using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.Core.Samus.Death;

namespace SuperMetroid.Core.Samus.Colours;

/// <summary>
/// The shades the import pre-colours Samus's sheets in (#1512), one for each colouring her suit, visor, beams and
/// suitless body take. Her colour state names them; which inks each recolours, and to what, is the import's.
/// </summary>
public static class SamusShades
{
    /// <summary>
    /// Each suit's name in its shades' installed names (#2511), written out so renaming a suit's identity never changes a
    /// name on disk.
    /// </summary>
    private static readonly IdentityTable<SamusSuit, string> SuitKeys = new(new Dictionary<SamusSuit, string>
        {
            [SamusSuit.Power] = "Power",
            [SamusSuit.Varia] = "Varia",
            [SamusSuit.Gravity] = "Gravity",
        });

    /// <summary>
    /// Each charge's name in its pulse shades' installed names (#2511), written out so renaming a charge's identity never
    /// changes a name on disk.
    /// </summary>
    private static readonly IdentityTable<SamusChargeKind, string> ChargeKeys = new(new Dictionary<SamusChargeKind, string>
        {
            [SamusChargeKind.ChargedBeam] = "ChargedBeam",
            [SamusChargeKind.PseudoScrew] = "PseudoScrew",
        });

    /// <summary>
    /// Each full-body cycle's name in its shades' installed names (#2511), written out so renaming a cycle's identity never
    /// changes a name on disk.
    /// </summary>
    private static readonly IdentityTable<SamusFullBodyCycleFamily, string> FullBodyCycleKeys = new(new Dictionary<SamusFullBodyCycleFamily, string>
        {
            [SamusFullBodyCycleFamily.SpeedBooster] = "SpeedBooster",
            [SamusFullBodyCycleFamily.ScrewAttack] = "ScrewAttack",
            [SamusFullBodyCycleFamily.StoredShine] = "StoredShine",
            [SamusFullBodyCycleFamily.ActiveShinespark] = "ActiveShinespark",
        });

    /// <summary>
    /// The installed name of each hurt colouring's shade (#2511), written out so renaming a colouring's identity never
    /// changes a name on disk.
    /// </summary>
    private static readonly IdentityTable<SamusHurtColorVariant, string> HurtKeys = new(new Dictionary<SamusHurtColorVariant, string>
        {
            [SamusHurtColorVariant.Hurt] = "HurtHurt",
            [SamusHurtColorVariant.Intro] = "HurtIntro",
        });

    /// <summary>
    /// The installed name of each beam combination's shade (#2511), written out so renaming a combination's identity never
    /// changes a name on disk.
    /// </summary>
    private static readonly IdentityTable<SamusBeamCombination, string> BeamKeys = new(new Dictionary<SamusBeamCombination, string>
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
        });

    /// <summary>
    /// The combination whose shade each beam combination shows. A combination the game has a shade of its own for, and
    /// the Spazer, Ice and Wave beam with its defined black (#1318), shows its own. Spazer with Plasma, the Chainsaw beam (Spazer,
    /// Plasma and Wave, #1736) and the Murder beam (Spazer, Plasma, Ice and Wave), which the pause screen's category quirk
    /// equips, have no beam shade of their own in the game, so each shows the shade of its beams without Spazer, the
    /// combination the pause screen otherwise equips.
    /// </summary>
    private static readonly IdentityTable<SamusBeamCombination, SamusBeamCombination> ShownShades = new(new Dictionary<SamusBeamCombination, SamusBeamCombination>
        {
            [SamusBeamCombination.Power] = SamusBeamCombination.Power,
            [SamusBeamCombination.Wave] = SamusBeamCombination.Wave,
            [SamusBeamCombination.Ice] = SamusBeamCombination.Ice,
            [SamusBeamCombination.IceWave] = SamusBeamCombination.IceWave,
            [SamusBeamCombination.Spazer] = SamusBeamCombination.Spazer,
            [SamusBeamCombination.SpazerWave] = SamusBeamCombination.SpazerWave,
            [SamusBeamCombination.SpazerIce] = SamusBeamCombination.SpazerIce,
            [SamusBeamCombination.SpazerIceWave] = SamusBeamCombination.SpazerIceWave,
            [SamusBeamCombination.Plasma] = SamusBeamCombination.Plasma,
            [SamusBeamCombination.PlasmaWave] = SamusBeamCombination.PlasmaWave,
            [SamusBeamCombination.PlasmaIce] = SamusBeamCombination.PlasmaIce,
            [SamusBeamCombination.PlasmaIceWave] = SamusBeamCombination.PlasmaIceWave,
            [SamusBeamCombination.SpazerPlasma] = SamusBeamCombination.Plasma,
            [SamusBeamCombination.SpazerPlasmaWave] = SamusBeamCombination.PlasmaWave,
            [SamusBeamCombination.SpazerPlasmaIce] = SamusBeamCombination.SpazerPlasmaIce,
            [SamusBeamCombination.SpazerPlasmaIceWave] = SamusBeamCombination.PlasmaIceWave,
        });

    /// <summary>The suit's colours as gameplay starts.</summary>
    public static PictureShade StartingSuit { get; } = new("SuitStarting", 0);

    /// <summary>The beams' colours as gameplay starts.</summary>
    public static PictureShade StartingBeams { get; } = new("BeamsStarting", 0);

    /// <summary>The suit's colours in the opening's gameplay flashbacks, as the opening's own colours hold them.</summary>
    public static PictureShade IntroSuit { get; } = new("SuitIntro", 0);

    /// <summary>The beams' colours in the opening's gameplay flashbacks, as the opening's own colours hold them.</summary>
    public static PictureShade IntroBeams { get; } = new("BeamsIntro", 0);

    /// <summary>The suit's normal colours in <paramref name="suit"/>.</summary>
    public static PictureShade Suit(SamusSuit suit) => new($"Suit{SuitKeys[suit]}", 0);

    /// <summary>The suit under one step of a charged beam's pulse or the pseudo Screw Attack's hold.</summary>
    public static PictureShade Charge(SamusChargeShade shade) =>
        new($"Charge{ChargeKeys[shade.Kind]}{SuitKeys[shade.Suit]}", shade.Step);

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
        new($"{FullBodyCycleKeys[shade.Family]}{SuitKeys[shade.Suit]}", shade.Shade);

    /// <summary>
    /// The suit under each of <paramref name="family"/>'s shades in <paramref name="suit"/>, from its first to its brightest
    /// (#2447).
    /// </summary>
    public static IReadOnlyList<PictureShade> FullBodyCycleShades(SamusFullBodyCycleFamily family, SamusSuit suit) =>
        [.. Enumerable.Range(0, SamusShadeDefinitions.FullBodyCycles.FamilyShades)
            .Select(shade => FullBodyCycle(new(family, suit, shade)))];

    /// <summary>The suit under the hurt flash's bright colours or its cinematic grey restoration.</summary>
    public static PictureShade Hurt(SamusHurtColorVariant variant) => new(HurtKeys[variant], 0);

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
    /// The beams' colours for one beam combination: the shade that combination shows (<see cref="ShownShades"/>).
    /// </summary>
    public static PictureShade Beam(SamusBeamCombination combination) => new(BeamKeys[ShownBeamShade(combination)], 0);

    /// <summary>The beam combination whose shade <paramref name="combination"/> shows.</summary>
    internal static SamusBeamCombination ShownBeamShade(SamusBeamCombination combination) => ShownShades[combination];

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
    public static PictureShade DeathSuit(SamusSuit suit, SamusDeathShade shade) => new($"Death{SuitKeys[suit]}", shade.Step);

    /// <summary>Her body without the suit as her death sequence shows it under <paramref name="shade"/>.</summary>
    public static PictureShade DeathSuitless(SamusDeathShade shade) => new("DeathSuitless", shade.Step);

    /// <summary>Her beams' and grapple flare's colours under <paramref name="shade"/> of the death sequence's whiteout.</summary>
    public static PictureShade DeathWhiteout(SamusWhiteoutShade shade) => new("DeathWhiteout", shade.Step);

    /// <summary>The <paramref name="count"/> shades of the run <paramref name="effect"/>, in the order the run shows them.</summary>
    private static PictureShade[] Run([InstalledKey] string effect, int count) =>
        [.. Enumerable.Range(0, count).Select(step => new PictureShade(effect, step))];
}
