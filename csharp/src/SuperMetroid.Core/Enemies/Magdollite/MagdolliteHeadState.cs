using SuperMetroid.Core.Animation;

namespace SuperMetroid.Core.Enemies.Magdollite;

/// <summary>A Magdollite head's state: what it is doing. The speed its arm moves at is its spawn settings' (#2474).</summary>
public sealed partial class MagdolliteHeadState : MagdollitePartState
{
    /// <summary>Creates the head's state, starting in <paramref name="action"/>.</summary>
    internal MagdolliteHeadState(MagdolliteHeadAction action)
    {
        Action = action;
    }

    /// <summary>The animation the head plays and where it stands in it (#1600, #2554).</summary>
    internal AnimationPlayback<MagdolliteHeadSegment> Playback { get; set; }

    /// <summary>What the head is doing.</summary>
    public MagdolliteHeadAction Action { get; internal set; }

    /// <summary>
    /// Whether the head is submerging: its submerge animation plays or has just finished. Its overlay starts tracking the arm
    /// once it is.
    /// </summary>
    internal bool IsSubmerging => Action is MagdolliteHeadAction.WaitingForAttackAnimation or MagdolliteHeadAction.Submerged;

    /// <summary>The arm has landed back at its origin (#2439): the head moves into its landed phase, and emerges on its next update.</summary>
    internal void ArmLanded() => Action = MagdolliteHeadAction.ArmLanded;

    /// <summary>The submerge's finishing cue (#2439): the head has submerged, and its next update releases its arm.</summary>
    internal void FinishSubmerge() => Action = MagdolliteHeadAction.Submerged;

    /// <summary>The emergence's finishing cue (#2439): the head has surfaced, and its next update idles it.</summary>
    internal void FinishEmerge() => Action = MagdolliteHeadAction.Surfaced;
}
