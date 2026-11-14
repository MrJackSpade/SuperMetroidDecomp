using SuperMetroid.Core.Samus.Movement;

namespace SuperMetroid.Core.Samus.Poses;

/// <summary>
/// What one of Samus's unauthored poses defines, as the game reads it
/// (<see cref="Compatibility.UnauthoredPoseReads{TPose, TDefinition}"/>).
/// </summary>
/// <param name="Facing">The way the pose faces.</param>
/// <param name="Aim">What the pose does with the arm cannon.</param>
/// <param name="VerticalRadius">The pose's vertical collision radius, in pixels.</param>
/// <param name="ProjectileYOffset">How far above Samus's centre the pose's projectiles and Grapple start, in pixels.</param>
/// <param name="Movement">
/// The movement type the pose selects its movement by, or none where the pose has no movement type, which no
/// movement runs and which fails loudly.
/// </param>
/// <param name="NoInputPose">The pose the pose falls back to with no input.</param>
internal sealed record SamusUnauthoredPoseDefinition(
    SamusFacingDirection Facing,
    SamusPoseAim Aim,
    int VerticalRadius,
    int ProjectileYOffset,
    SamusMovementType? Movement,
    SamusPoseId NoInputPose);
