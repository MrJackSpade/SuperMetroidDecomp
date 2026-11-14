using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Core.Samus.Palettes;

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
    public static PictureShade Charge(SamusChargePalette palette) => new($"Charge{palette.Kind}{palette.Suit}", palette.Step);

    /// <summary>The suit under one frame of the Hyper Beam's full-body glow, which the drained Samus and the Hyper shot show.</summary>
    public static PictureShade HyperBeamSuit(int frame) => new("HyperBeamSuit", frame);

    /// <summary>The suit under one frame of a Hyper shot's glow, which walks the charge colours' copy of the Hyper Beam glow.</summary>
    public static PictureShade HyperShot(int frame) => new("HyperShot", frame);

    /// <summary>The suit under one shade of a full-body cycle: Speed Booster, Screw Attack or a shinespark's glow.</summary>
    public static PictureShade FullBodyCycle(SamusFullBodyCyclePalette palette) =>
        new($"{palette.Family}{palette.Suit}", palette.Shade);

    /// <summary>The suit under the hurt flash's bright colours or its cinematic grey restoration.</summary>
    public static PictureShade Hurt(SamusHurtColorVariant variant) => new($"Hurt{variant}", 0);

    /// <summary>The suit's visible colours under an ordinary charged shot's yellow glow.</summary>
    public static PictureShade ChargedShotGlow { get; } = new("ChargedShotGlow", 0);

    /// <summary>The visor under one of the X-ray and room visor cycle's shades.</summary>
    public static PictureShade Visor(int shade) => new("Visor", shade);

    /// <summary>The visor's green as a door transition finishes loading the next room.</summary>
    public static PictureShade DoorCompletionVisor { get; } = new("VisorDoorCompletion", 0);

    /// <summary>The grapple flare's colour once the grapple's firing colours are loaded.</summary>
    public static PictureShade GrappleFiringFlare { get; } = new("GrappleFlare", 0);

    /// <summary>
    /// The beams' colours for one beam combination: the palette it selects in the beam palette table. The Chainsaw beam,
    /// which the table does not list, shows the colours of the listed combination it shares its palette with (#1736).
    /// </summary>
    public static PictureShade Beam(SamusBeamCombination combination) => new($"Beam{ShownPalette(combination)}", 0);

    /// <summary>The beam combination whose palette <paramref name="combination"/> shows.</summary>
    internal static SamusBeamCombination ShownPalette(SamusBeamCombination combination) =>
        combination == ChainsawBeamGraphicsDefinitions.Selection ? ChainsawBeamGraphicsDefinitions.ShownPalette : combination;

    /// <summary>The Crystal Flash's body colours at one step of its pulse.</summary>
    public static PictureShade CrystalFlashBody(int step) => new("CrystalFlashBody", step);

    /// <summary>The Crystal Flash's bubble colours at one step of their rotation.</summary>
    public static PictureShade CrystalFlashBubble(int step) => new("CrystalFlashBubble", step);

    /// <summary>The beams' colours under one frame of the Hyper Beam's own colour cycle.</summary>
    public static PictureShade HyperBeamCycle(int frame) => new("HyperBeamCycle", frame);

    /// <summary>The suit as her death sequence shows it in <paramref name="suit"/> at one of its steps.</summary>
    public static PictureShade DeathSuit(SamusSuit suit, int step) => new($"Death{suit}", step);

    /// <summary>Her body without the suit as her death sequence shows it at one of its steps.</summary>
    public static PictureShade DeathSuitless(int step) => new("DeathSuitless", step);

    /// <summary>Her beams' and grapple flare's colours under one step of the death sequence's whiteout.</summary>
    public static PictureShade DeathWhiteout(int step) => new("DeathWhiteout", step);
}
