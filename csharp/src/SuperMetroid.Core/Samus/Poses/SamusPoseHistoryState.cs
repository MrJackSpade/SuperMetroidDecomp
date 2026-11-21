using SuperMetroid.Core.Samus.Movement;
namespace SuperMetroid.Core.Samus.Poses;

/// <summary>
/// The pose Samus's latest committed pose transition published, with the movement types her last two committed
/// transitions recorded (#1473). These are transition samples, not per-frame samples: every committed transition shifts
/// them, including one that selects the previous pose again.
/// </summary>
public sealed class SamusPoseHistoryState
{
    /// <summary>The pose the latest committed transition published.</summary>
    public SamusPoseId PreviousPose { get; private set; }

    /// <summary>The movement type the latest committed transition recorded.</summary>
    internal SamusMovementType PreviousMovement { get; set; }

    /// <summary>The movement type the committed transition before the latest one recorded.</summary>
    internal SamusMovementType MovementBeforePrevious { get; set; }

    /// <summary>
    /// Whether the wall-jump probe may run: only when the older committed transition recorded spin jumping or wall
    /// jumping. The history shifts on every committed transition, including one that selects the same pose again.
    /// </summary>
    public bool AllowsWallJumpProbe => MovementBeforePrevious is SamusMovementType.SpinJumping or SamusMovementType.WallJumping;

    /// <summary>
    /// Shifts the history for a committed transition, including one that selects the previous pose again; do not call
    /// merely because a frame elapsed or an intermediate pose assignment occurred.
    /// </summary>
    /// <param name="pose">The pose the transition published.</param>
    /// <param name="movement">The movement type the transition recorded.</param>
    public void CommitTransition(SamusPoseId pose, SamusMovementType movement)
    {
        MovementBeforePrevious = PreviousMovement;
        PreviousPose = pose;
        PreviousMovement = movement;
    }
}
