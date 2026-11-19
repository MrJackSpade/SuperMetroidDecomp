using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Samus.Grapple;
using SuperMetroid.Core.Samus.Hud;
using SuperMetroid.Core.Samus.Movement;

namespace SuperMetroid.Core.Samus.Poses;

/// <summary>
/// What one way Samus moves does (#2323, #2445): when her input changes nothing, when a bomb jump is published, when the
/// X-ray starts, when she enters water, when she fires the Grapple, which way the HUD's selected item fires, and how she
/// takes a hit's knockback.
/// </summary>
/// <param name="NoTransitionResponse">What her momentum does when her input finds no new pose.</param>
/// <param name="BombJump">How a published bomb jump is set up.</param>
/// <param name="Xray">The visor pose the X-ray takes, or whether it starts at all.</param>
/// <param name="Splash">The splash her entry into water raises.</param>
/// <param name="GrappleFiring">Whether firing the Grapple fires or is cancelled.</param>
/// <param name="HudSelection">How the HUD's selected item fires.</param>
/// <param name="Knockback">How she takes a hit's knockback.</param>
/// <param name="NoTransitionSetup">
/// The setup the pose update runs when her input chooses no new pose and her pose names none
/// (<see cref="SamusNoTransitionSetups"/>).
/// </param>
internal sealed record SamusMovementTypeDefinition(
    SamusMomentumResponse NoTransitionResponse,
    SamusBombJumpSetup BombJump,
    XrayPosture Xray,
    WaterSplashKind Splash,
    GrappleFiringResponse GrappleFiring,
    SamusHudSelectionMode HudSelection,
    SamusKnockbackResponse Knockback,
    SamusNoTransitionSetup NoTransitionSetup = SamusNoTransitionSetup.None);
