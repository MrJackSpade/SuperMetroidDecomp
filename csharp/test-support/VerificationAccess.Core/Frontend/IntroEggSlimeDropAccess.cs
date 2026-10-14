using SuperMetroid.Core.Frontend;

/// <summary>Verification access to <see cref="IntroEggSlimeDrop"/> members production does not use.</summary>
internal static class IntroEggSlimeDropAccess
{
    /// <summary>Reads whether an egg slime drop's sprite remains active.</summary>
    extension(IntroEggSlimeDrop self)
    {
        /// <summary>Whether the drop's internal discovery sprite is active.</summary>
        internal bool IsActive => self.sprite.IsActive;
    }
}
