namespace SuperMetroid.Core.Samus.Poses;

/// <summary>
/// What a pose number past Samus's authored poses defines, as the game reads it from what follows the authored
/// definitions (<see cref="Compatibility.UnauthoredPoseReads{TPose, TDefinition}"/>).
/// </summary>
/// <param name="Facing">The way the pose faces.</param>
/// <param name="Aim">What the pose does with the arm cannon.</param>
/// <param name="VerticalRadius">The pose's vertical collision radius, in pixels.</param>
/// <param name="ProjectileYOffset">How far above Samus's centre the pose's projectiles and Grapple start, in pixels.</param>
internal sealed record SamusUnauthoredPoseDefinition(
    SamusFacingDirection Facing,
    SamusPoseAim Aim,
    int VerticalRadius,
    int ProjectileYOffset);
