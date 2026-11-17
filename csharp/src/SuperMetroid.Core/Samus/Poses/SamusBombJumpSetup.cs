namespace SuperMetroid.Core.Samus.Poses;

/// <summary>How a published bomb jump is set up from the way Samus moves (#2323).</summary>
internal enum SamusBombJumpSetup
{
    /// <summary>She takes the forward jump of the way she faces, which then rises from the blast.</summary>
    ForwardJump,

    /// <summary>She takes the forward jump of the way she faces, unless time is frozen, which turns the jump down.</summary>
    ForwardJumpUnlessTimeFrozen,

    /// <summary>She keeps her pose, which already rises from the blast.</summary>
    KeepPose,

    /// <summary>The jump is turned down and the request dropped.</summary>
    TurnedDown,
}
