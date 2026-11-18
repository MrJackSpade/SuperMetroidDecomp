using static SuperMetroid.Core.Samus.Poses.SamusAnimationSegmentName;

namespace SuperMetroid.Core.Samus.Poses;

/// <summary>
/// The named frames of Samus's animations that her movement, sounds and colours read (#2048). A reader compares the frame
/// Samus's animation is on with one of these as the game compares them, whatever animation she animates by
/// (<see cref="SamusState.AnimationHasReached"/>, <see cref="SamusState.AnimationIsOn"/>).
/// </summary>
internal static class SamusAnimationFrames
{
    /// <summary>The neutral jump's rising frame, which the animation keeps alive while she still rises.</summary>
    internal static SamusAnimationPlace NeutralJumpRise { get; } = SamusAnimationDelayDefinitions.NeutralJumpAndWindupAnimation.Place(SamusAnimationPlaceName.NeutralJumpRise, Main, 1);

    /// <summary>The Space Jump spin's first frame, on whose last tick the underwater Space Jump sound plays.</summary>
    internal static SamusAnimationPlace SpaceJumpSpinStart { get; } = SamusAnimationDelayDefinitions.SpaceJumpAnimation.Place(SamusAnimationPlaceName.SpaceJumpSpinStart, Spin, 0);

    /// <summary>The Space Jump spin's ninth frame, on whose last tick the underwater Space Jump sound plays again.</summary>
    internal static SamusAnimationPlace SpaceJumpSpinMiddle { get; } = SamusAnimationDelayDefinitions.SpaceJumpAnimation.Place(SamusAnimationPlaceName.SpaceJumpSpinMiddle, Spin, 8);

    /// <summary>The first frame of the ordinary wall-jump spin, from which a charged wall jump attacks as a pseudo Screw Attack.</summary>
    internal static SamusAnimationPlace WallJumpOrdinarySpinStart { get; } = SamusAnimationDelayDefinitions.WallJumpAnimation.Place(SamusAnimationPlaceName.WallJumpOrdinarySpinStart, OrdinarySpin, 0);

    /// <summary>The first frame of the Space Jump wall-jump spin, from which the spin sound is the Space Jump's.</summary>
    internal static SamusAnimationPlace WallJumpSpaceJumpSpinStart { get; } = SamusAnimationDelayDefinitions.WallJumpAnimation.Place(SamusAnimationPlaceName.WallJumpSpaceJumpSpinStart, SpaceJumpSpin, 0);

    /// <summary>The first frame of the Screw Attack wall-jump spin, which always attacks as the Screw Attack and sounds as it.</summary>
    internal static SamusAnimationPlace WallJumpScrewAttackSpinStart { get; } = SamusAnimationDelayDefinitions.WallJumpAnimation.Place(SamusAnimationPlaceName.WallJumpScrewAttackSpinStart, ScrewAttackSpin, 0);

    /// <summary>The first frame of the spin from which a wall jump can launch.</summary>
    internal static SamusAnimationPlace SpinJumpWallJumpReady { get; } = SamusAnimationDelayDefinitions.SpinJumpAnimation.Place(SamusAnimationPlaceName.SpinJumpWallJumpReady, WallJumpReady, 0);

    /// <summary>The first frame of the Screw Attack spin from which a wall jump can launch.</summary>
    internal static SamusAnimationPlace ScrewAttackWallJumpReady { get; } = SamusAnimationDelayDefinitions.ScrewAttackAnimation.Place(SamusAnimationPlaceName.ScrewAttackWallJumpReady, WallJumpReady, 0);

    /// <summary>The Screw Attack spin's first frame, which keeps her normal colours and restarts the glow's cycle.</summary>
    internal static SamusAnimationPlace ScrewAttackSpinStart { get; } = SamusAnimationDelayDefinitions.ScrewAttackAnimation.Place(SamusAnimationPlaceName.ScrewAttackSpinStart, Spin, 0);

    /// <summary>The fall's first fast-fall frame.</summary>
    internal static SamusAnimationFrame FastFall { get; } = new(SamusAnimationSegmentName.FastFall, 0);

    /// <summary>The fast fall's first frame as a place in the fall's animation, below which the movement starts the fast fall.</summary>
    internal static SamusAnimationPlace FastFallStart { get; } = SamusAnimationDelayDefinitions.FallingAnimation.Place(SamusAnimationPlaceName.FastFallStart, SamusAnimationSegmentName.FastFall, 0);

    /// <summary>The front-facing pose's appearance at the Zebes start.</summary>
    internal static SamusAnimationFrame ZebesAppearance { get; } = new(Appearance, 0);

    /// <summary>The Crystal Flash's glow, which its raise starts.</summary>
    internal static SamusAnimationFrame CrystalFlashGlow { get; } = new(Glow, 0);

    /// <summary>The Crystal Flash's finish, which its ammunition drain starts.</summary>
    internal static SamusAnimationFrame CrystalFlashFinish { get; } = new(Finish, 0);

    /// <summary>The X-ray scope's middle aim, which its activation starts on.</summary>
    internal static SamusAnimationFrame XrayActivation { get; } = Abilities.XrayScopeDefinitions.Aims.Level;

    /// <summary>A ground turn's last frame, whose last tick completes an X-ray turn.</summary>
    internal static SamusAnimationFrame TurnEnd { get; } = new(Main, 2);

    /// <summary>The run stride's first foot contact, on whose last tick a running footstep lands.</summary>
    internal static SamusAnimationFrame RunFirstFootContact { get; } = new(Run, 2);

    /// <summary>The run stride's second foot contact, on whose last tick the other running footstep lands.</summary>
    internal static SamusAnimationFrame RunSecondFootContact { get; } = new(Run, 7);
}
