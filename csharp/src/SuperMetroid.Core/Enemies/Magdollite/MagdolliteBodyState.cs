namespace SuperMetroid.Core.Enemies.Magdollite;

/// <summary>A Magdollite's rising body's state, the arm that grows out of the lava: what it is doing and how far it has grown.</summary>
public sealed partial class MagdolliteBodyState : MagdollitePartState
{
    /// <summary>Creates the body's state, starting in <paramref name="action"/>.</summary>
    internal MagdolliteBodyState(MagdolliteBodyAction action)
    {
        Action = action;
    }

    /// <summary>What the arm is doing.</summary>
    public MagdolliteBodyAction Action { get; internal set; }

    /// <summary>
    /// The arm's stage of its rise, which selects its pillar stack, threshold and overlay offset. It steps up as the arm
    /// rises past each stage's threshold and back down as it sinks.
    /// </summary>
    public MagdolliteRiseStage BodyPhase { get; internal set; }

    /// <summary>
    /// The arm's whole-pixel vertical displacement from where it started rising, negative while the arm is above that
    /// point.
    /// </summary>
    public int RiseDisplacementY { get; internal set; }

    /// <summary>The head releases the dormant arm (#2439): the arm starts rising from its origin on its next update.</summary>
    /// <exception cref="InvalidOperationException">The arm is not dormant.</exception>
    internal void Release() => Action = Action == MagdolliteBodyAction.Dormant
        ? MagdolliteBodyAction.Released
        : throw new InvalidOperationException($"A Magdollite's head released its arm while the arm was {Action}.");
}
