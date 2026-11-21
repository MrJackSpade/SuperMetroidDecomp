using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Samus;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// Her brain played as the game plays it at her own draw (#2604): the head's play of the brain's animation
/// (<see cref="MotherBrainHead.PlayMotherBrainBrain"/>), which her draw runs and which carries out the animation's cues.
/// </summary>
internal static class MotherBrainHeadDraws
{
    /// <summary>Her head's draws.</summary>
    extension(MotherBrainHead head)
    {
        /// <summary>Plays her brain once, as her draw does, and returns the picture it shows, or null while her brain is not live.</summary>
        internal EnemyFrameId? DrawBrain(MotherBrainEnemyState state, SamusState samus) => head.PlayMotherBrainBrain(state, samus)?.Frame;
    }
}
