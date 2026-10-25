using SuperMetroid.Core.Samus.Movement;
namespace SuperMetroid.Core.Samus.Poses;

/// <summary>
/// Cartridge pose-history words shifted by $91:E719 and the common $91:EB88
/// transition epilogue. These are transition samples, not per-frame samples.
/// </summary>
public sealed class SamusPoseHistoryState
{
    /// <summary>WRAM $0A20: pose sampled at the preceding committed transition.</summary>
    public SamusPoseId PreviousPose { get; set; }

    /// <summary>WRAM $0A22, 0 through 65535: previous direction byte followed by movement-type byte.</summary>
    public int PreviousDirectionAndMovement
    {
        get;
        set => field = Ensure.BetweenInclusive(0, ushort.MaxValue, value);
    }

    /// <summary>WRAM $0A24: previous pose before the latest transition-history shift.</summary>
    public SamusPoseId LastDifferentPose { get; set; }

    /// <summary>WRAM $0A26, 0 through 65535: older direction byte followed by movement-type byte.</summary>
    public int LastDifferentDirectionAndMovement
    {
        get;
        set => field = Ensure.BetweenInclusive(0, ushort.MaxValue, value);
    }

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
            return SamusPoseRecordCodec.HistoryMovement(checked((ushort)LastDifferentDirectionAndMovement)) is
                SamusMovementType.SpinJumping or SamusMovementType.WallJumping;
        }
    }

    /// <summary>
    /// Applies the four literal word stores in $91:E719. Call for a committed
    /// transition, including one that selects the previous pose again; do not call
    /// merely because a frame elapsed or an intermediate pose assignment occurred.
    /// </summary>
    public void CommitTransition(SamusPoseId pose, int directionAndMovement)
    {
        LastDifferentPose = PreviousPose;
        LastDifferentDirectionAndMovement = PreviousDirectionAndMovement;
        PreviousPose = pose;
        PreviousDirectionAndMovement = directionAndMovement;
    }
}
