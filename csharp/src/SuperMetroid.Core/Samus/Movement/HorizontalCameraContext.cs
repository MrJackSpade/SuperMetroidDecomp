using SuperMetroid.Core.Rooms.Scrolling;

namespace SuperMetroid.Core.Samus.Movement;

/// <summary>
/// What horizontal camera target selection asks of Samus: whether her motion runs
/// against her facing (knockback, moonwalking, or a turn that keeps the old momentum), whether she faces
/// left, and how far ahead of her the camera sits.
/// </summary>
[IndependentFlag("independent facts about Samus the camera target reads")]
public readonly partial record struct HorizontalCameraContext
{
    /// <summary>A value of its parts; only its own operations make one.</summary>
    /// <param name="knockedBack">Whether a knockback is carrying Samus, which reverses the facing target.</param>
    /// <param name="movementType">Samus's movement type; moonwalking reverses the facing target.</param>
    /// <param name="turningAgainstMomentum">Whether a turn is still carrying Samus opposite her new facing, which reverses the facing target.</param>
    /// <param name="facesLeft">Whether Samus's pose faces left. Every other pose facing, front-view included, selects the right-facing target.</param>
    /// <param name="cameraDistance">How far ahead of Samus the camera target sits.</param>
    [IndependentFlag("independent facts about Samus the camera target reads")]
    internal HorizontalCameraContext(
        bool knockedBack,
        SamusMovementType movementType,
        bool turningAgainstMomentum,
        bool facesLeft,
        CameraDistanceMode cameraDistance)
    {
        KnockedBack = knockedBack;
        MovementType = movementType;
        TurningAgainstMomentum = turningAgainstMomentum;
        FacesLeft = facesLeft;
        CameraDistance = cameraDistance;
    }

    /// <summary>Whether a knockback is carrying Samus, which reverses the facing target.</summary>
    public bool KnockedBack { get; init; }

    /// <summary>Samus's movement type; moonwalking reverses the facing target.</summary>
    public SamusMovementType MovementType { get; init; }

    /// <summary>Whether a turn is still carrying Samus opposite her new facing, which reverses the facing target.</summary>
    public bool TurningAgainstMomentum { get; init; }

    /// <summary>Whether Samus's pose faces left. Every other pose facing, front-view included, selects the right-facing target.</summary>
    public bool FacesLeft { get; init; }

    /// <summary>How far ahead of Samus the camera target sits.</summary>
    public CameraDistanceMode CameraDistance { get; init; }

    /// <summary>
    /// Builds the camera context from Samus's knockback, acceleration mode and facing: the turning acceleration mode is
    /// the turn that keeps the old momentum.
    /// </summary>
    /// <param name="KnockedBack">Whether a knockback is pending or running.</param>
    /// <param name="MovementType">Samus's movement type.</param>
    /// <param name="XAccelerationMode">Samus's horizontal acceleration mode.</param>
    /// <param name="Facing">The way Samus's pose faces.</param>
    /// <param name="CameraDistance">How far ahead of Samus the camera target sits.</param>
    public HorizontalCameraContext(
        bool KnockedBack,
        SamusMovementType MovementType,
        SamusAccelerationMode XAccelerationMode,
        SamusFacingDirection Facing,
        CameraDistanceMode CameraDistance)
        : this(
            KnockedBack,
            MovementType,
            turningAgainstMomentum: XAccelerationMode == SamusAccelerationMode.Turning,
            facesLeft: Facing == SamusFacingDirection.Left,
            CameraDistance)
    {
    }
}
