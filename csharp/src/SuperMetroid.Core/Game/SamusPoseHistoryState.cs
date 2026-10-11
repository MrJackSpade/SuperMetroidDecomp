namespace SuperMetroid.Core.Game;

/// <summary>
/// Cartridge pose-history words shifted by $91:E719 and the common $91:EB88
/// transition epilogue. These are transition samples, not per-frame samples.
/// </summary>
public sealed class SamusPoseHistoryState
{
    /// <summary>WRAM $0A20: pose sampled at the preceding committed transition.</summary>
    public SamusPoseId PreviousPose { get; set; }

    /// <summary>WRAM $0A22: previous direction byte followed by movement-type byte.</summary>
    public ushort PreviousDirectionAndMovement { get; set; }

    /// <summary>WRAM $0A24: previous pose before the latest transition-history shift.</summary>
    public SamusPoseId LastDifferentPose { get; set; }

    /// <summary>WRAM $0A26: older direction byte followed by movement-type byte.</summary>
    public ushort LastDifferentDirectionAndMovement { get; set; }

    /// <summary>
    /// $90:9D35 admits only older spinjump/walljump movement. The cartridge name
    /// says "last different", but the producer also shifts on same-pose transitions.
    /// </summary>
    public bool AllowsWallJumpProbe
    {
        get
        {
            // $90:9D35 compares the raw movement byte, so a byte outside the movement-type
            // domain is simply not one of the two admitted movements.
            byte movement = unchecked((byte)(LastDifferentDirectionAndMovement >> 8));
            return Enum.IsDefined((SamusMovementType)movement) &&
                (SamusMovementType)movement is SamusMovementType.SpinJumping or SamusMovementType.WallJumping;
        }
    }

    /// <summary>
    /// Applies the four literal word stores in $91:E719. Call for a committed
    /// transition, including one that selects the previous pose again; do not call
    /// merely because a frame elapsed or an intermediate pose assignment occurred.
    /// </summary>
    public void CommitTransition(SamusPoseId pose, ushort directionAndMovement)
    {
        LastDifferentPose = PreviousPose;
        LastDifferentDirectionAndMovement = PreviousDirectionAndMovement;
        PreviousPose = pose;
        PreviousDirectionAndMovement = directionAndMovement;
    }
}
