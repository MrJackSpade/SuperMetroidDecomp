namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>Verification access to <see cref="EndingRewardJump"/> members production does not use.</summary>
internal static class EndingRewardJumpAccess
{
    /// <summary>Reads the reward jump's private vertical speed for timing verification.</summary>
    extension(EndingRewardJump self)
    {
        /// <summary>Current signed vertical velocity in the ending reward jump.</summary>
        internal int VerticalVelocity => self.velocity;
    }
}
