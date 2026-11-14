using SuperMetroid.Core.Samus.Movement;

namespace SuperMetroid.Core.Samus.Poses;

/// <summary>
/// What a pose number past Samus's authored poses defines, as the game reads it from what follows the authored
/// definitions (<see cref="Compatibility.UnauthoredPoseReads{TPose, TDefinition}"/>).
/// </summary>
/// <param name="Facing">The way the pose faces.</param>
/// <param name="Aim">What the pose does with the arm cannon.</param>
/// <param name="VerticalRadius">The pose's vertical collision radius, in pixels.</param>
/// <param name="ProjectileYOffset">How far above Samus's centre the pose's projectiles and Grapple start, in pixels.</param>
/// <param name="Movement">
/// The movement type the pose selects its movement handler by, or none where it reads a movement beyond every movement
/// type, which no handler runs and which fails loudly.
/// </param>
/// <param name="NoInputPose">The pose the pose falls back to with no input.</param>
internal sealed record SamusUnauthoredPoseDefinition(
    SamusFacingDirection Facing,
    SamusPoseAim Aim,
    int VerticalRadius,
    int ProjectileYOffset,
    SamusMovementType? Movement,
    SamusPoseId NoInputPose);
