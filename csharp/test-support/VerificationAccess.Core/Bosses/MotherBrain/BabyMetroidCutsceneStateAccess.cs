namespace SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Enemies;

/// <summary>Verification access to <see cref="BabyMetroidCutsceneState"/> members production does not use.</summary>
internal static class BabyMetroidCutsceneStateAccess
{
    /// <summary>Reads whether the Baby is hidden without exposing mutable cutscene state.</summary>
    extension(BabyMetroidCutsceneState self)
    {
        /// <summary>True when the Baby's presence hides it from drawing.</summary>
        internal bool IsInvisible => self.Presence.HasAny(EnemyPresence.Hidden);
    }
}
