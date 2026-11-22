namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// How far Mother Brain's neck angles turn each update while her neck moves (#1951): both the lower and the upper angle
/// turn by the rate her current motion sets.
/// </summary>
internal static partial class MotherBrainNeckTurnDefinitions
{
    /// <summary>
    /// A quarter of a step an update: the slow sway of her stretching, her rainbow beam's aim, her lowering after the
    /// Baby's drain and her revival.
    /// </summary>
    internal static readonly RotationAngle Slow = RotationAngle.FromStepFraction(1, 4);

    /// <summary>Five sixteenths of a step an update: her neck as she rises from the fake death and as her late fight starts.</summary>
    internal static readonly RotationAngle Rising = RotationAngle.FromStepFraction(5, 16);

    /// <summary>Half a step an update: her ordinary bob in the third phase, between its recoils.</summary>
    internal static readonly RotationAngle Bobbing = RotationAngle.FromStepFraction(1, 2);

    /// <summary>A step an update: her neck once the fake death lands her, and as she fires her mouth laser.</summary>
    internal static readonly RotationAngle Steady = RotationAngle.FromSteps(1);

    /// <summary>Two steps an update: her head swinging quickly into place for the mouth laser.</summary>
    internal static readonly RotationAngle Quick = RotationAngle.FromSteps(2);

    /// <summary>
    /// Five steps an update: the fast throws of her neck as she stands from the fake death, fires the rainbow beam,
    /// finishes Samus off, revives, recovers from a recoil and dies.
    /// </summary>
    internal static readonly RotationAngle Fast = RotationAngle.FromSteps(5);

    /// <summary>Seven steps an update: her neck thrown back as the Baby Metroid's drain takes her aback.</summary>
    internal static readonly RotationAngle TakenAback = RotationAngle.FromSteps(7);

    /// <summary>Nine steps an update: her neck thrown up by a Hyper Beam hit's recoil.</summary>
    internal static readonly RotationAngle HyperBeamRecoil = RotationAngle.FromSteps(9);

    /// <summary>
    /// The neck turn from which her drool is the dying drool: a neck turning half a step an update or faster drools it, a
    /// slower one drools the ordinary drool.
    /// </summary>
    internal static readonly RotationAngle DyingDrool = RotationAngle.FromStepFraction(1, 2);
}
