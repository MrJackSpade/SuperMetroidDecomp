using SuperMetroid.Core.Samus.Movement;

namespace SuperMetroid.Core.Samus.Poses;

/// <summary>A pose from which Samus's shots and Grapple start, at its projectile height (#1928).</summary>
/// <param name="Facing">The way the pose faces; the front views face neither way.</param>
/// <param name="Movement">How she moves in the pose.</param>
/// <param name="NoInput">What the pose falls back to when no input transition applies.</param>
/// <param name="Animation">The animation the pose plays.</param>
/// <param name="Inputs">The input list that chooses her next pose from the held and newly pressed actions.</param>
/// <param name="VerticalRadius">The pose's vertical collision radius, in pixels.</param>
/// <param name="Aim">What the pose does with the arm cannon.</param>
/// <param name="ProjectileHeight">How far above her centre the pose's shots and Grapple start, in pixels.</param>
/// <param name="NoTransitionSetup">The setup the pose update runs when her input chooses no new pose from this one.</param>
internal sealed record SamusArmedPoseDefinition(
    SamusFacingDirection Facing,
    SamusMovementType Movement,
    SamusNoInputFallback NoInput,
    SamusPoseAnimation Animation,
    SamusPoseInputList Inputs,
    int VerticalRadius,
    SamusPoseAim Aim,
    int ProjectileHeight,
    SamusNoTransitionSetup NoTransitionSetup = SamusNoTransitionSetup.None)
    : SamusPoseDefinition(Facing, Movement, NoInput, Animation, Inputs, VerticalRadius, Aim, NoTransitionSetup);
