using SuperMetroid.Core.Samus.Movement;

namespace SuperMetroid.Core.Samus.Poses;

/// <summary>
/// A pose the Baby's drain leaves Samus in (#1928): she can neither shoot nor fire the Grapple from it, so it has no
/// projectile height.
/// </summary>
/// <param name="Facing">The way the pose faces.</param>
/// <param name="Movement">How she moves in the pose.</param>
/// <param name="NoInput">What the pose falls back to when no input transition applies.</param>
/// <param name="Animation">The animation the pose plays.</param>
/// <param name="Inputs">The input list that chooses her next pose from the held and newly pressed actions.</param>
/// <param name="VerticalRadius">The pose's vertical collision radius, in pixels.</param>
internal sealed record SamusDrainedPoseDefinition(
    SamusFacingDirection Facing,
    SamusMovementType Movement,
    SamusNoInputFallback NoInput,
    SamusPoseAnimation Animation,
    SamusPoseInputList Inputs,
    int VerticalRadius)
    : SamusPoseDefinition(Facing, Movement, NoInput, Animation, Inputs, VerticalRadius, new SamusPoseAim.ShootingDisabled(),
        SamusNoTransitionSetup.None);
