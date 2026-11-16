using A = SuperMetroid.Core.Bosses.Torizo.TorizoAnimation;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>
/// The animation each of a walking Torizo's decisions continues in when its test passes (#2175): the dodge or attack for
/// the way its last step faced and the foot it stepped on, which the step's cue records in its posture.
/// </summary>
internal static class TorizoBranches
{
    /// <summary>The animation a passing <paramref name="kind"/> decision continues in, from the start, for <paramref name="posture"/>.</summary>
    /// <param name="kind">The decision.</param>
    /// <param name="posture">The posture the Torizo's last step left it in.</param>
    /// <exception cref="InvalidOperationException"><paramref name="kind"/> is no decision.</exception>
    internal static TorizoAnimation Target(TorizoBranchKind kind, TorizoPostureFlags posture)
    {
        bool facingRight = posture.HasFlag(TorizoPostureFlags.FacingRight);
        bool onRightFoot = TorizoFacing.SteppedOnRightFoot(posture);
        return kind switch
        {
            // A jump back lands on the foot it stepped on.
            TorizoBranchKind.BombJumpBackFromSamusInFront or TorizoBranchKind.GoldenJumpBackFromSamusInFront => facingRight
                ? onRightFoot ? A.FacingRightJumpingBackwardToRightFoot : A.FacingRightJumpingBackwardToLeftFoot
                : onRightFoot ? A.FacingLeftJumpingBackwardToRightFoot : A.FacingLeftJumpingBackwardToLeftFoot,
            TorizoBranchKind.GoldenLeapAtDistantSamus => facingRight ? A.FacingRightJumpingForward : A.FacingLeftJumpingForward,
            TorizoBranchKind.GoldenSitOnSamusMorphedBehind => facingRight ? A.GoldenFacingRightSittingDownAttack : A.GoldenFacingLeftSittingDownAttack,
            _ => throw new InvalidOperationException($"Undefined {nameof(TorizoBranchKind)} {kind}."),
        };
    }
}
