using SuperMetroid.Core.Animation;

namespace SuperMetroid.Core.Enemies.Animals;

/// <summary>One cue of the escaping Dachora's animations: what it does, with the values it acts on.</summary>
internal abstract record EscapeDachoraCue
{
    /// <summary>Closes the hierarchy to the cues declared here.</summary>
    private EscapeDachoraCue()
    {
    }

    /// <summary>Continues at the cue's continuation once its condition holds (#2061).</summary>
    /// <param name="Kind">The condition the branch waits for.</param>
    /// <param name="Then">Where the branch continues the play when it is taken.</param>
    internal sealed record Branch(EscapeDachoraCueKind Kind, AnimationContinuation<EscapeDachoraSegment> Then) : EscapeDachoraCue;

    /// <summary>Moves the Dachora by the cue's pixels (#2151).</summary>
    /// <param name="Offset">The cue's signed pixel offset.</param>
    internal sealed record Move(int Offset) : EscapeDachoraCue;
}
