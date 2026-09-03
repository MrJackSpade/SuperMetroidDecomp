using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>
/// The two-stage Samus handler installed by Ceres Ridley's Mode-7 getaway at
/// <c>$90:E119-$E21B</c>.
/// </summary>
/// <remarks>
/// The native request is made from room-main code, after Samus has already moved for that
/// frame. <see cref="Request"/> therefore publishes a pending handler and
/// <see cref="BeginFrame"/> promotes it on the following gameplay frame. This explicit
/// seam is important in the current runtime because Ridley's translated visual owner runs
/// during EnemyMain; immediately changing pose or input there would be one frame early.
/// </remarks>
public sealed class SamusCeresRidleyEjectionState
{
    private const ushort TerminalDownwardSpeed = 5;

    /// <summary>True between `$A6:AAF8`'s request and the following handler frame.</summary>
    public bool IsPending { get; private set; }

    /// <summary>True while `$90:E12E/$E1C8` owns Samus movement.</summary>
    public bool IsActive { get; private set; }

    /// <summary>True until the first `$90:E12E` call installs pose and velocity.</summary>
    public bool InitializationPending { get; private set; }

    /// <summary>
    /// Native <c>samus_var62</c>: one pushes toward the left wall, two toward the right.
    /// </summary>
    public ushort PushDirection { get; private set; }

    /// <summary>Publishes `$90:E119` without executing its next-frame gamma handler early.</summary>
    public void Request()
    {
        if (!IsActive)
            IsPending = true;
    }

    /// <summary>
    /// Promotes a room-main request at the beginning of the following gameplay call.
    /// </summary>
    public void BeginFrame(SamusState samus)
    {
        ArgumentNullException.ThrowIfNull(samus);
        if (!IsPending)
            return;

        IsPending = false;
        IsActive = true;
        InitializationPending = true;

        // `$90:E119` replaces only MovementHandler. The ordinary pose-input handler stays
        // installed: held controller chords are still matched, projectile/HUD input still
        // runs, and `$90:E1C8` specifically discards a prospective `$4F/$50` damage boost
        // while the shove remains active. Runtime owns that narrow prospective-pose clear.
        // Do not use the broader host InputLocked flag here; doing so changes controller
        // semantics and made this cutscene indistinguishable from an elevator/message lock.
    }

    /// <summary>Executes one `$90:E12E` or `$90:E1C8` handler call.</summary>
    public CeresRidleyEjectionResult Step(
        ISnesAddressSpace bus,
        RoomLevelData level,
        SamusState samus,
        ushort layer1X,
        ushort nmiFrameCounter)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(level);
        ArgumentNullException.ThrowIfNull(samus);
        if (!IsActive)
            throw new InvalidOperationException("Ceres Ridley ejection is not active.");

        if (InitializationPending)
        {
            InitializationPending = false;

            // `$90:E12E` selects the ordinary hurt pose from the PREVIOUS pose direction,
            // refreshes radius/animation, and only then applies `21 - radius` alignment.
            bool facingLeft = samus.ReadPoseXDirection(bus) == 4;
            samus.Pose = facingLeft
                ? SamusPoseIds.KnockbackLeftPose
                : SamusPoseIds.KnockbackRightPose;
            samus.RefreshCollisionRadii(bus);
            samus.InitializeAnimation(bus);
            samus.YPosition = unchecked((ushort)(
                samus.YPosition - (21 - samus.Kinematics.YRadius)));

            // The side is selected from screen X, not world X. Exact center belongs to the
            // rightward branch because the native BMI tests a signed `(screenX - 128)`.
            ushort screenX = unchecked((ushort)(samus.XPosition - layer1X));
            PushDirection = unchecked((short)(screenX - 128)) < 0
                ? (ushort)1
                : (ushort)2;
            samus.Kinematics.YSpeed = TerminalDownwardSpeed;
            samus.Kinematics.YSubspeed = 0;
            samus.KnockbackXDirection = PushDirection == 1 ? (ushort)0 : (ushort)1;

            // `$90:E12E` initializes only handler state on this call. `$90:E1C8` does not
            // perform the first collision/movement pass until the following frame.
            return new CeresRidleyEjectionResult(
                Initialized: true,
                Horizontal: null,
                Vertical: null,
                Ended: false);
        }

        // `Samus_BombJumpFallingXMovement_` deliberately uses the current pose's movement
        // type ($0A for $53/$54) and the persistent base/extra-run words. Direction comes
        // from samus_var62 instead of pose facing, so Ridley always throws toward a wall.
        SamusHorizontalSpeedState horizontalSpeed = samus.HorizontalSpeed;
        horizontalSpeed.SelectEnvironmentSpeedTable(
            samus.LiquidPhysics.DetermineMovementMedium(samus));
        uint baseSpeed = horizontalSpeed.CalculateBaseSpeed(
            bus,
            samus.ReadMovementType(bus));
        int requestedX = PushDirection == 1
            ? horizontalSpeed.CalculateLeftDisplacement(baseSpeed, samus.Kinematics.ExtraXFixed)
            : horizontalSpeed.CalculateRightDisplacement(baseSpeed, samus.Kinematics.ExtraXFixed);
        BlockMoveResult horizontal = SamusBlockCollision.MoveHorizontal(
            bus,
            level,
            samus.Kinematics,
            requestedX);

        if (horizontal.Collided)
        {
            // `$90:E1FD/$E21C` restore the ordinary handler, then Samus_ClearMoveVars sees
            // the still-set collision flag and clears every movement word. The subsequent
            // `$90:DDE9` hit-interruption pass observes a completed type-$0A reaction and
            // routes through `$90:DE20-$DE73` / `$91:F31D`: `$53/$54` becomes ordinary
            // falling `$29/$2A`, with its shorter radius aligned to the same feet. Calling
            // that shared owner here is the host equivalent of the native same-frame tail;
            // selecting standing directly would skip real fall/ground collision behavior.
            IsActive = false;
            PushDirection = 0;
            samus.HorizontalSpeed.BaseSpeed = 0;
            samus.HorizontalSpeed.BaseSubspeed = 0;
            samus.HorizontalSpeed.AccelerationMode = 0;
            samus.Kinematics.YSpeed = 0;
            samus.Kinematics.YSubspeed = 0;
            samus.Kinematics.YDirection = 0;
            SamusKnockbackMovement.FinishHumanoidToFalling(bus, samus);
            return new CeresRidleyEjectionResult(
                Initialized: false,
                Horizontal: horizontal,
                Vertical: null,
                Ended: true);
        }

        // `$90:8F5A` accelerates only while the signed whole speed is below five. The
        // resulting 16.16 word is passed directly to MoveDown; unlike the common falling
        // wrapper, this path adds no extra one-pixel downward bias.
        if (unchecked((short)(samus.Kinematics.YSpeed - TerminalDownwardSpeed)) < 0)
        {
            uint accelerated = unchecked(
                samus.Kinematics.VerticalSpeedFixed +
                (((uint)samus.Kinematics.YAcceleration << 16) |
                 samus.Kinematics.YSubacceleration));
            samus.Kinematics.YSpeed = unchecked((ushort)(accelerated >> 16));
            samus.Kinematics.YSubspeed = unchecked((ushort)accelerated);
        }
        BlockMoveResult vertical = SamusBlockCollision.MoveVertical(
            bus,
            level,
            samus.Kinematics,
            unchecked((int)samus.Kinematics.VerticalSpeedFixed),
            scanLeftToRight: (nmiFrameCounter & 1) == 0);

        return new CeresRidleyEjectionResult(
            Initialized: false,
            Horizontal: horizontal,
            Vertical: vertical,
            Ended: false);
    }
}

/// <summary>Debugger-visible work performed by one Ceres ejection handler call.</summary>
public readonly record struct CeresRidleyEjectionResult(
    bool Initialized,
    BlockMoveResult? Horizontal,
    BlockMoveResult? Vertical,
    bool Ended);
