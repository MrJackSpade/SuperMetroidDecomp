namespace SuperMetroid.Core.Enemies.Magdollite;

/// <summary>A Magdollite head's state: what it is doing. The speed its arm moves at is its spawn settings' (#2474).</summary>
public sealed class MagdolliteHeadState : MagdollitePartState
{
    /// <summary>Creates the head's state, starting in <paramref name="action"/>.</summary>
    internal MagdolliteHeadState(MagdolliteHeadAction action)
    {
        Action = action;
    }

    /// <summary>What the head is doing.</summary>
    public MagdolliteHeadAction Action { get; internal set; }

    /// <summary>
    /// Whether the head is submerging: its submerge animation plays or has just finished. Its overlay starts tracking the arm
    /// once it is.
    /// </summary>
    internal bool IsSubmerging => Action is MagdolliteHeadAction.WaitingForAttackAnimation or MagdolliteHeadAction.Submerged;

    /// <summary>The arm has landed back at its origin (#2439): the head emerges on its next update.</summary>
    /// <exception cref="InvalidOperationException">The head is not waiting for its arm.</exception>
    internal void ArmLanded() => Action = Action == MagdolliteHeadAction.WaitingForBodyLanding
        ? MagdolliteHeadAction.ArmLanded
        : throw new InvalidOperationException($"A Magdollite's arm landed while its head was {Action}.");

    /// <summary>The submerge's finishing cue (#2439): the head has submerged, and its next update releases its arm.</summary>
    /// <exception cref="InvalidOperationException">The head is not waiting for its submerge.</exception>
    internal void FinishSubmerge() => Action = Action == MagdolliteHeadAction.WaitingForAttackAnimation
        ? MagdolliteHeadAction.Submerged
        : throw new InvalidOperationException($"A Magdollite head finished submerging while it was {Action}.");

    /// <summary>The emergence's finishing cue (#2439): the head has surfaced, and its next update idles it.</summary>
    /// <exception cref="InvalidOperationException">The head is not waiting for its emergence.</exception>
    internal void FinishEmerge() => Action = Action == MagdolliteHeadAction.WaitingForReturnAnimation
        ? MagdolliteHeadAction.Surfaced
        : throw new InvalidOperationException($"A Magdollite head finished emerging while it was {Action}.");
}
