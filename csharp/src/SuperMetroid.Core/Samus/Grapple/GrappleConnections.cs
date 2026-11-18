using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Samus.Projectiles;
using static SuperMetroid.Core.Samus.Grapple.GrappleConnectionDefinitions;
using static SuperMetroid.Core.Samus.Poses.SamusPoseId;

namespace SuperMetroid.Core.Samus.Grapple;

/// <summary>How the grapple connects at each special angle: the stop or its reflection, firing cancellation and the pose a release drops into.</summary>
internal static class GrappleConnections
{
    /// <summary>
    /// The special connections in the order a swing compares them: each authored stop followed by its left/right
    /// reflection.
    /// </summary>
    internal static IReadOnlyList<SpecialConnection> SpecialConnections { get; } =
        [.. AuthoredStops.SelectMany(stop => new[] { stop, Reflection(stop) })];

    /// <summary>
    /// The mirror of <paramref name="stop"/> on the other side: its angle reflected as <see cref="ReflectionTurn"/>
    /// less its own, its X offset negated and its pose facing the other way; its Y offset and connection kind are shared.
    /// </summary>
    internal static SpecialConnection Reflection(SpecialConnection stop) =>
        new(ReflectionTurn - stop.Angle, SamusPoseMirror.Of(stop.Pose), stop.SamusOffset.MirroredX(), stop.Kind);

    /// <summary>Whether <paramref name="movement"/> cancels firing the Grapple (<see cref="SamusMovementTypeDefinitions"/>).</summary>
    /// <exception cref="ArgumentOutOfRangeException">The movement type has no definition.</exception>
    internal static bool CancelsFiring(SamusMovementType movement) =>
        SamusMovementTypeDefinitions.Movements.TryGetValue(movement, out SamusMovementTypeDefinition? definition)
            ? definition.GrappleFiring == GrappleFiringResponse.Cancelled
            : throw new ArgumentOutOfRangeException(nameof(movement));

    /// <summary>Directional standing/crouching poses after the rope drops.</summary>
    internal static SamusPoseId DroppedPose(SamusProjectileDirection direction, bool compact) => direction switch
    {
        SamusProjectileDirection.UpFacingRight => compact ? CrouchingAimUpRightPose : StandingAimUpRightPose,
        SamusProjectileDirection.UpRight => compact ? CrouchingAimDiagonalUpRightPose : StandingAimDiagonalUpRightPose,
        SamusProjectileDirection.Right or SamusProjectileDirection.DownFacingRight => compact ? CrouchingRightPose : FacingRightNormalPose,
        SamusProjectileDirection.DownRight => compact ? CrouchingAimDiagonalDownRightPose : StandingAimDiagonalDownRightPose,
        SamusProjectileDirection.DownFacingLeft or SamusProjectileDirection.Left => compact ? CrouchingLeftPose : FacingLeftNormalPose,
        SamusProjectileDirection.DownLeft => compact ? CrouchingAimDiagonalDownLeftPose : StandingAimDiagonalDownLeftPose,
        SamusProjectileDirection.UpLeft => compact ? CrouchingAimDiagonalUpLeftPose : StandingAimDiagonalUpLeftPose,
        SamusProjectileDirection.UpFacingLeft => compact ? CrouchingAimUpLeftPose : StandingAimUpLeftPose,
        _ => throw new ArgumentOutOfRangeException(nameof(direction)),
    };

    /// <summary>The connection <paramref name="policy"/> installs for a grapple fired in <paramref name="fireDirection"/>.</summary>
    /// <exception cref="KeyNotFoundException">The policy or direction is not a defined member.</exception>
    internal static GrappleConnection ResolveConnection(GrappleConnectionPolicy policy, SamusProjectileDirection fireDirection) =>
        Directional[policy][fireDirection];
}
